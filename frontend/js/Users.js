/* =============================================================
   EduQuest - users.js  (used by pages/admin/users.html)

   Admin "Manage Users" page, wired to the backend:

     GET    /api/users            list every account
     GET    /api/learners         grade / school / province per learner
     GET    /api/grades           grade dropdowns
     GET    /api/schools          school + province dropdowns
     POST   /api/users            create an account
     POST   /api/learners         create the learner profile
     PUT    /api/users/{id}       edit an account (also suspend / reactivate)
     PUT    /api/learners/{id}    edit a learner's grade / school
     DELETE /api/users/{id}       deactivate (soft delete - IsActive = false)

   Needs js/api.js (EduQuestAPI) loaded first.
============================================================= */
(function () {
    "use strict";

    if (!window.EduQuestAPI) {
        console.error("users.js: api.js must be loaded before this script.");
        return;
    }

    /* The shared login page lives in the learner frontend. */
    var LOGIN_URL = "../../EduQuest-with-js/EduQuest/login.html";

    /* Seeded in ApplicationDBContext (Role HasData). */
    var ROLE_IDS = { learner: 1, admin: 2, sponsor: 3 };

    /* Everything on this page is Admin-only (UsersController is [Authorize(Roles = "Admin")]). */
    var me = EduQuestAPI.getCurrentUser();
    if (!me || me.role !== "Admin") {
        window.location.href = LOGIN_URL;
        return;
    }

    var users = [];        /* merged User + Learner rows used by the table */
    var grades = [];       /* [{ gradeID, gradeName }] */
    var schools = [];      /* [{ schoolID, schoolName, provinceID, provinceName }] */
    var editingUser = null; /* the row being edited, or null while adding */

    /* ---------- small helpers ---------- */

    function byId(id) {
        return document.getElementById(id);
    }

    function setText(id, value) {
        var el = byId(id);
        if (el) { el.textContent = value; }
    }

    /* Names and emails are typed in by users (learners can self-register),
       so everything is escaped before it goes into innerHTML. */
    function esc(value) {
        return String(value === null || value === undefined ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    /* The API sends UTC timestamps; EF can drop the trailing "Z", so add it back. */
    function parseDate(value) {
        if (!value) { return null; }
        var text = String(value);
        if (!/(Z|[+-]\d\d:?\d\d)$/.test(text)) { text += "Z"; }
        var date = new Date(text);
        return isNaN(date.getTime()) ? null : date;
    }

    function formatDate(date) {
        if (!date) { return "—"; }
        return date.toLocaleDateString("en-GB", { day: "numeric", month: "long", year: "numeric" });
    }

    function fullName(user) {
        return user.firstName + " " + user.surname;
    }

    function initials(user) {
        return ((user.firstName || "").charAt(0) + (user.surname || "").charAt(0)).toUpperCase();
    }

    /* api.js clears the token on a 401. If it is gone, send the admin back to sign in. */
    function handleAuthError() {
        if (!EduQuestAPI.getToken()) {
            window.location.href = LOGIN_URL;
            return true;
        }
        return false;
    }

    /* ---------- hover cards ---------- */

    function detailItem(label, value) {
        return '<div class="user-detail-item"><span>' + esc(label) + "</span><strong>" +
            esc(value || "—") + "</strong></div>";
    }

    function createUserDetails(user) {
        var html =
            '<div class="user-details-card">' +
                '<div class="user-details-header">' +
                    '<div class="large-avatar">' + esc(initials(user)) + "</div>" +
                    "<div><h3>" + esc(fullName(user)) + "</h3><p>" + esc(user.email) + "</p></div>" +
                "</div>";

        if (user.role === "Learner") {
            html +=
                '<div class="user-details-section"><h4>Academic Information</h4>' +
                    detailItem("Grade", user.grade) +
                    detailItem("School", user.school) +
                    detailItem("Region", user.region) +
                "</div>";
        }

        html +=
            '<div class="user-details-section"><h4>Account</h4>' +
                detailItem("Role", user.role) +
                detailItem("Registered", formatDate(user.createdAt)) +
            "</div></div>";

        return html;
    }

    /* ---------- table ---------- */

    function setTableMessage(message) {
        var body = byId("usersTableBody");
        if (!body) { return; }
        body.innerHTML = '<tr><td colspan="8">' + esc(message) + "</td></tr>";
    }

    function rowHtml(user) {
        var toggleLabel = user.isActive ? "Deactivate" : "Reactivate";
        /* Admin accounts can't be deactivated (the API enforces this too). */
        var toggleDisabled = user.isActive && user.role === "Admin";
        var toggleTitle = toggleDisabled
            ? "Admin accounts can't be deactivated"
            : toggleLabel + " user";

        return (
            "<tr>" +
                '<td class="user-name-column">' +
                    '<div class="user-name-cell user-hover-trigger">' +
                        '<div class="avatar">' + esc(initials(user)) + "</div>" +
                        '<div class="user-name-information">' +
                            "<strong>" + esc(fullName(user)) + "</strong>" +
                            '<div class="user-email">' + esc(user.email) + "</div>" +
                        "</div>" +
                        createUserDetails(user) +
                    "</div>" +
                "</td>" +
                "<td>" + esc(user.email) + "</td>" +
                "<td>" + esc(user.role) + "</td>" +
                "<td>" + esc(user.grade || "—") + "</td>" +
                "<td>" + esc(user.school || "—") + "</td>" +
                "<td>" + esc(user.region || "—") + "</td>" +
                '<td><span class="status-badge ' + esc(user.status.toLowerCase()) + '">' +
                    esc(user.status) + "</span></td>" +
                "<td>" +
                    '<div class="row-actions">' +
                        '<button type="button" class="icon-action edit-action" data-action="edit" ' +
                            'data-id="' + user.id + '" title="Edit user">Edit</button>' +
                        '<button type="button" class="icon-action delete-action" data-action="toggle" ' +
                            'data-id="' + user.id + '" title="' + esc(toggleTitle) + '"' +
                            (toggleDisabled ? " disabled" : "") + ">" + toggleLabel + "</button>" +
                    "</div>" +
                "</td>" +
            "</tr>"
        );
    }

    function displayUsers(list) {
        var body = byId("usersTableBody");
        if (!body) { return; }

        list = list || users;

        if (list.length === 0) {
            setTableMessage(users.length === 0 ? "No users found." : "No users match your filters.");
            return;
        }

        body.innerHTML = list.map(rowHtml).join("");
    }

    /* ---------- overview cards ---------- */

    function updateOverview() {
        var total = users.length;
        var learners = users.filter(function (u) { return u.role === "Learner"; }).length;
        var admins = users.filter(function (u) { return u.role === "Admin"; }).length;
        var others = total - learners - admins;
        var active = users.filter(function (u) { return u.isActive; }).length;
        var suspended = total - active;

        var now = new Date();
        var newThisMonth = users.filter(function (u) {
            return u.createdAt &&
                u.createdAt.getFullYear() === now.getFullYear() &&
                u.createdAt.getMonth() === now.getMonth();
        }).length;

        var breakdown = learners + " Learners. " + admins + " Admins.";
        if (others > 0) { breakdown += " " + others + " Other."; }

        setText("statTotal", total);
        setText("statTotalSub", breakdown);
        setText("statActive", active);
        setText("statActiveSub", suspended + " suspended account" + (suspended === 1 ? "" : "s"));
        setText("statNew", newThisMonth);
        setText("statNewSub", newThisMonth === 0
            ? "No new accounts this month"
            : newThisMonth + " new account" + (newThisMonth === 1 ? "" : "s") + " this month");
    }

    /* ---------- search + filters ---------- */

    function applyFilters() {
        var search = (byId("userSearch") ? byId("userSearch").value : "").toLowerCase().trim();
        var role = byId("roleFilter") ? byId("roleFilter").value : "all";
        var grade = byId("gradeFilter") ? byId("gradeFilter").value : "all";
        var status = byId("statusFilter") ? byId("statusFilter").value : "all";

        var filtered = users.filter(function (user) {
            var matchesSearch =
                fullName(user).toLowerCase().indexOf(search) !== -1 ||
                user.email.toLowerCase().indexOf(search) !== -1;
            var matchesRole = role === "all" || user.role.toLowerCase() === role;
            var matchesGrade = grade === "all" || String(user.gradeId) === grade;
            var matchesStatus = status === "all" || user.status.toLowerCase() === status;

            return matchesSearch && matchesRole && matchesGrade && matchesStatus;
        });

        displayUsers(filtered);
    }

    /* ---------- loading data ---------- */

    async function loadUsers() {
        setTableMessage("Loading users...");

        try {
            var results = await Promise.all([
                EduQuestAPI.get("/users"),
                EduQuestAPI.get("/learners")
            ]);

            var learnersByUserId = {};
            results[1].forEach(function (learner) {
                learnersByUserId[learner.userID] = learner;
            });

            users = results[0].map(function (u) {
                var learner = learnersByUserId[u.userID] || null;
                var createdAt = parseDate(u.dateCreated);

                return {
                    id: u.userID,
                    firstName: u.firstName,
                    surname: u.lastName,
                    email: u.email,
                    roleId: u.roleID,
                    role: u.roleName,
                    isActive: u.isActive,
                    status: u.isActive ? "Active" : "Suspended",
                    createdAt: createdAt,
                    learnerId: learner ? learner.learnerID : null,
                    gradeId: learner ? learner.gradeID : null,
                    grade: learner ? learner.gradeName : null,
                    schoolId: learner ? learner.schoolID : null,
                    school: learner ? learner.schoolName : null,
                    region: learner ? learner.provinceName : null
                };
            });

            updateOverview();
            applyFilters();
        } catch (err) {
            if (handleAuthError()) { return; }
            console.error("Failed to load users:", err);
            setTableMessage("Failed to load users: " + err.message);
        }
    }

    /* Grades and schools feed the dropdowns (filter bar + Add/Edit form). */
    async function loadLookups() {
        try {
            var results = await Promise.all([
                EduQuestAPI.get("/grades"),
                EduQuestAPI.get("/schools")
            ]);

            grades = results[0].slice().sort(function (a, b) { return a.gradeID - b.gradeID; });
            schools = results[1];

            fillGradeOptions();
            fillProvinceOptions();
            return true;
        } catch (err) {
            if (!handleAuthError()) { console.error("Failed to load grades/schools:", err); }
            return false;
        }
    }

    function fillGradeOptions() {
        var filter = byId("gradeFilter");
        var formSelect = byId("grade");

        if (filter) {
            filter.innerHTML = '<option value="all">All grades</option>' +
                grades.map(function (g) {
                    return '<option value="' + g.gradeID + '">' + esc(g.gradeName) + "</option>";
                }).join("");
        }

        if (formSelect) {
            formSelect.innerHTML = '<option value="">Select grade</option>' +
                grades.map(function (g) {
                    return '<option value="' + g.gradeID + '">' + esc(g.gradeName) + "</option>";
                }).join("");
        }
    }

    function fillProvinceOptions() {
        var select = byId("province");
        if (!select) { return; }

        var seen = {};
        var provinces = [];
        schools.forEach(function (s) {
            if (!seen[s.provinceID]) {
                seen[s.provinceID] = true;
                provinces.push({ id: s.provinceID, name: s.provinceName });
            }
        });
        provinces.sort(function (a, b) { return a.name.localeCompare(b.name); });

        select.innerHTML = '<option value="">All provinces</option>' +
            provinces.map(function (p) {
                return '<option value="' + p.id + '">' + esc(p.name) + "</option>";
            }).join("");
    }

    /* ---------- Searchable school picker ----------
       Lists every school from /api/schools. The province select is only an
       optional filter, and typing narrows the list by school or province name.
       The chosen school's ID lives in the hidden #school input. */

    var schoolMatches = [];   /* schools currently shown in the list */
    var schoolActive = -1;    /* highlighted row (keyboard navigation) */

    function schoolList() { return byId("schoolList"); }

    function openSchoolList() {
        schoolList().hidden = false;
        byId("schoolSearch").setAttribute("aria-expanded", "true");
    }

    function closeSchoolList() {
        schoolList().hidden = true;
        byId("schoolSearch").setAttribute("aria-expanded", "false");
        schoolActive = -1;
    }

    function updateSchoolHint() {
        var hint = byId("schoolHint");
        if (!hint) { return; }
        var total = schools.length;
        var provinceId = byId("province").value;
        var inScope = provinceId
            ? schools.filter(function (s) { return String(s.provinceID) === String(provinceId); }).length
            : total;
        hint.textContent = inScope + " school" + (inScope === 1 ? "" : "s") +
            (provinceId ? " in this province" : " available") + " - type to search";
    }

    function renderSchoolList() {
        var list = schoolList();
        var provinceId = byId("province").value;
        /* Once a school is picked, show the full list again instead of only the chosen name. */
        var query = byId("school").value ? "" : byId("schoolSearch").value.trim().toLowerCase();

        schoolMatches = schools
            .filter(function (s) {
                if (provinceId && String(s.provinceID) !== String(provinceId)) { return false; }
                if (!query) { return true; }
                return s.schoolName.toLowerCase().indexOf(query) !== -1 ||
                    String(s.provinceName || "").toLowerCase().indexOf(query) !== -1;
            })
            .sort(function (a, b) { return a.schoolName.localeCompare(b.schoolName); });

        if (schoolMatches.length === 0) {
            list.innerHTML = '<li class="empty">No schools match "' + esc(byId("schoolSearch").value.trim()) + '"</li>';
            schoolActive = -1;
            return;
        }

        var selectedId = byId("school").value;
        schoolActive = -1;
        list.innerHTML = schoolMatches.map(function (s, i) {
            var isSelected = String(s.schoolID) === selectedId;
            if (isSelected) { schoolActive = i; }
            return '<li role="option" data-index="' + i + '"' +
                (isSelected ? ' class="active" aria-selected="true"' : "") + ">" +
                "<span>" + esc(s.schoolName) + "</span><small>" + esc(s.provinceName || "") + "</small></li>";
        }).join("");
    }

    function highlightSchool(index) {
        var items = schoolList().querySelectorAll("li[data-index]");
        if (items.length === 0) { return; }
        schoolActive = (index + items.length) % items.length;
        Array.prototype.forEach.call(items, function (li, i) {
            li.classList.toggle("active", i === schoolActive);
        });
        items[schoolActive].scrollIntoView({ block: "nearest" });
    }

    /* Select a school by ID (or clear with null) and keep the province filter in step. */
    function setSchool(schoolId) {
        var school = schoolId ? schools.filter(function (s) {
            return s.schoolID === Number(schoolId);
        })[0] : null;

        byId("school").value = school ? String(school.schoolID) : "";
        byId("schoolSearch").value = school ? school.schoolName : "";

        if (school) { byId("province").value = String(school.provinceID); }
        updateSchoolHint();
        closeSchoolList();
    }

    function resetSchoolPicker() {
        byId("school").value = "";
        byId("schoolSearch").value = "";
        byId("province").value = "";
        closeSchoolList();
        updateSchoolHint();
    }

    function initSchoolPicker() {
        var search = byId("schoolSearch");
        var list = schoolList();
        if (!search || !list) { return; }

        search.addEventListener("focus", function () {
            renderSchoolList();
            openSchoolList();
        });
        search.addEventListener("click", function () {
            renderSchoolList();
            openSchoolList();
        });

        search.addEventListener("input", function () {
            byId("school").value = "";   /* typing invalidates any earlier pick */
            renderSchoolList();
            openSchoolList();
        });

        search.addEventListener("keydown", function (e) {
            if (e.key === "ArrowDown" || e.key === "ArrowUp") {
                e.preventDefault();
                if (list.hidden) { renderSchoolList(); openSchoolList(); }
                highlightSchool(schoolActive + (e.key === "ArrowDown" ? 1 : -1));
            } else if (e.key === "Enter") {
                if (!list.hidden && schoolMatches.length > 0) {
                    e.preventDefault();
                    /* A lone match is accepted even without arrowing to it. */
                    var pick = schoolMatches[schoolActive >= 0 ? schoolActive : (schoolMatches.length === 1 ? 0 : -1)];
                    if (pick) { setSchool(pick.schoolID); }
                }
            } else if (e.key === "Escape") {
                if (!list.hidden) { e.stopPropagation(); closeSchoolList(); }
            }
        });

        /* mousedown (not click) so the pick lands before the input loses focus. */
        list.addEventListener("mousedown", function (e) {
            e.preventDefault();
            var li = e.target.closest("li[data-index]");
            if (li) { setSchool(schoolMatches[Number(li.getAttribute("data-index"))].schoolID); }
        });

        search.addEventListener("blur", function () {
            closeSchoolList();
            /* Unpicked free text isn't a valid school - drop it so it can't look selected. */
            if (!byId("school").value) { search.value = ""; }
        });

        byId("province").addEventListener("change", function () {
            var picked = byId("school").value;
            if (picked) {
                var school = schools.filter(function (s) { return s.schoolID === Number(picked); })[0];
                if (school && byId("province").value &&
                    String(school.provinceID) !== byId("province").value) {
                    setSchool(null);
                }
            }
            updateSchoolHint();
        });
    }

    /* ---------- Add / Edit modal ---------- */

    function showFormError(message) {
        var box = byId("userFormError");
        if (!box) { alert(message); return; }
        box.textContent = message;
        box.style.display = "block";
    }

    function clearFormError() {
        var box = byId("userFormError");
        if (box) { box.style.display = "none"; box.textContent = ""; }
    }

    /* Grade / province / school only apply to learners. */
    function toggleLearnerFields() {
        var isLearner = byId("role").value === "learner";
        byId("learnerFields").style.display = isLearner ? "" : "none";
        byId("grade").required = isLearner;
        byId("schoolSearch").required = isLearner;
    }

    /* Admins can't be suspended, so lock the status to Active for them.
       An already-suspended admin stays editable so they can be reactivated. */
    function syncStatusField() {
        var status = byId("status");
        var roleKey = editingUser ? editingUser.role.toLowerCase() : byId("role").value;
        var lock = roleKey === "admin" && (!editingUser || editingUser.isActive);

        if (lock) { status.value = "active"; }
        status.disabled = lock;
    }

    async function openModal(user) {
        var modal = byId("addUserModal");
        var form = byId("userForm");
        if (!modal || !form) { return; }

        if (grades.length === 0 || schools.length === 0) {
            await loadLookups();
        }

        editingUser = user || null;
        form.reset();
        clearFormError();

        byId("userModalTitle").textContent = editingUser ? "Edit User" : "Add User";
        byId("saveUserBtn").textContent = editingUser ? "Save Changes" : "Save User";

        /* Passwords are only set when creating an account. */
        byId("passwordFields").style.display = editingUser ? "none" : "";
        byId("password").required = !editingUser;
        byId("confirmPassword").required = !editingUser;

        /* Changing role after creation would orphan the learner profile. */
        byId("role").disabled = !!editingUser;

        /* An admin can change their own email, but not another admin's (the API enforces this too). */
        var lockEmail = !!editingUser && editingUser.role === "Admin" && editingUser.id !== me.userId;
        byId("email").readOnly = lockEmail;
        byId("email").title = lockEmail ? "You can't change another admin's email" : "";

        resetSchoolPicker();

        if (editingUser) {
            byId("firstName").value = editingUser.firstName;
            byId("surname").value = editingUser.surname;
            byId("email").value = editingUser.email;
            byId("role").value = editingUser.role.toLowerCase();
            byId("status").value = editingUser.isActive ? "active" : "suspended";

            if (editingUser.role === "Learner" && editingUser.gradeId) {
                var school = schools.filter(function (s) {
                    return s.schoolID === editingUser.schoolId;
                })[0];

                byId("grade").value = String(editingUser.gradeId);
                if (school) { setSchool(school.schoolID); }
            }
        }

        toggleLearnerFields();
        syncStatusField();
        modal.style.display = "flex";
    }

    function closeModal() {
        var modal = byId("addUserModal");
        if (modal) { modal.style.display = "none"; }
        editingUser = null;
    }

    function setSaving(saving) {
        var button = byId("saveUserBtn");
        if (!button) { return; }
        button.disabled = saving;
        if (saving) {
            button.textContent = "Saving...";
        } else {
            button.textContent = editingUser ? "Save Changes" : "Save User";
        }
    }

    async function createUser(values) {
        var created = await EduQuestAPI.post("/users", {
            roleID: ROLE_IDS[values.role],
            firstName: values.firstName,
            lastName: values.lastName,
            email: values.email,
            password: values.password
        });

        /* The backend always creates accounts as active, so suspend afterwards if asked. */
        if (!values.isActive) {
            await EduQuestAPI.put("/users/" + created.userID, {
                roleID: ROLE_IDS[values.role],
                firstName: values.firstName,
                lastName: values.lastName,
                email: values.email,
                isActive: false
            });
        }

        if (values.role === "learner") {
            try {
                await EduQuestAPI.post("/learners", {
                    userID: created.userID,
                    gradeID: values.gradeId,
                    schoolID: values.schoolId
                });
            } catch (err) {
                var partial = new Error(
                    "The account was created, but the learner profile could not be saved (" +
                    err.message + "). Edit the user to add their grade and school."
                );
                partial.partial = true;
                throw partial;
            }
        }
    }

    async function updateUser(values) {
        await EduQuestAPI.put("/users/" + editingUser.id, {
            roleID: editingUser.roleId,
            firstName: values.firstName,
            lastName: values.lastName,
            email: values.email,
            isActive: values.isActive
        });

        if (editingUser.role === "Learner") {
            var learnerBody = { gradeID: values.gradeId, schoolID: values.schoolId };

            if (editingUser.learnerId) {
                await EduQuestAPI.put("/learners/" + editingUser.learnerId, learnerBody);
            } else {
                learnerBody.userID = editingUser.id;
                await EduQuestAPI.post("/learners", learnerBody);
            }
        }
    }

    async function handleSubmit(event) {
        event.preventDefault();
        clearFormError();

        var role = editingUser ? editingUser.role.toLowerCase() : byId("role").value;

        var values = {
            firstName: byId("firstName").value.trim(),
            lastName: byId("surname").value.trim(),
            email: byId("email").value.trim(),
            role: role,
            isActive: byId("status").value === "active",
            gradeId: Number(byId("grade").value),
            schoolId: Number(byId("school").value),
            password: byId("password").value
        };

        if (role === "learner" && !values.schoolId) {
            showFormError("Please search for and select a school from the list.");
            byId("schoolSearch").focus();
            return;
        }

        if (!editingUser) {
            if (values.password.length < 8) {
                showFormError("Password must be at least 8 characters.");
                return;
            }
            if (values.password !== byId("confirmPassword").value) {
                showFormError("Passwords do not match.");
                return;
            }
        }

        setSaving(true);

        try {
            if (editingUser) {
                await updateUser(values);
            } else {
                await createUser(values);
            }

            var savedId = editingUser ? editingUser.id : null;
            var statusChanged = editingUser && editingUser.isActive !== values.isActive;

            closeModal();
            await loadUsers();

            if (statusChanged && !statusWasApplied(savedId, values.isActive)) {
                alert(STALE_BACKEND_MESSAGE);
            }
        } catch (err) {
            if (handleAuthError()) { return; }

            if (err.partial) {
                /* The account exists now, so don't leave the form open to be re-submitted. */
                closeModal();
                await loadUsers();
                alert(err.message);
                return;
            }

            showFormError(err.message);
        } finally {
            setSaving(false);
        }
    }

    /* ---------- row actions ---------- */

    async function toggleActive(user) {
        var action = user.isActive ? "deactivate" : "reactivate";
        var message = "Are you sure you want to " + action + " " + fullName(user) + "?";
        if (user.isActive) { message += "\nThey won't be able to sign in until reactivated."; }

        if (!confirm(message)) { return; }

        try {
            if (user.isActive) {
                /* The API soft-deletes: it sets IsActive = false rather than removing the row. */
                await EduQuestAPI.del("/users/" + user.id);
            } else {
                await EduQuestAPI.put("/users/" + user.id, {
                    roleID: user.roleId,
                    firstName: user.firstName,
                    lastName: user.surname,
                    email: user.email,
                    isActive: true
                });
            }

            await loadUsers();

            if (!statusWasApplied(user.id, !user.isActive)) {
                alert(STALE_BACKEND_MESSAGE);
            }
        } catch (err) {
            if (handleAuthError()) { return; }
            alert("Could not " + action + " the user: " + err.message);
        }
    }

    var STALE_BACKEND_MESSAGE =
        "The server accepted the request but the account's status didn't change. " +
        "The running API is probably out of date: copy the updated UsersController.cs, " +
        "UserDto.cs and UpdateUserDto.cs into the backend, then rebuild and restart it.";

    /* After a save, confirm the server really applied the status we sent. */
    function statusWasApplied(userId, expectedActive) {
        var reloaded = findUser(userId);
        return !reloaded || reloaded.isActive === expectedActive;
    }

    function findUser(id) {
        return users.filter(function (u) { return u.id === id; })[0];
    }

    /* ---------- wiring ---------- */

    function init() {
        var body = byId("usersTableBody");
        if (body) {
            body.addEventListener("click", function (event) {
                var button = event.target.closest("button[data-action]");
                if (!button || button.disabled) { return; }

                var user = findUser(Number(button.dataset.id));
                if (!user) { return; }

                if (button.dataset.action === "edit") { openModal(user); }
                if (button.dataset.action === "toggle") { toggleActive(user); }
            });
        }

        var openButton = byId("openAddUser");
        if (openButton) { openButton.addEventListener("click", function () { openModal(null); }); }

        var closeButton = byId("closeAddUser");
        if (closeButton) { closeButton.addEventListener("click", closeModal); }

        var form = byId("userForm");
        if (form) { form.addEventListener("submit", handleSubmit); }

        byId("role").addEventListener("change", function () {
            toggleLearnerFields();
            syncStatusField();
        });
        initSchoolPicker();

        byId("userSearch").addEventListener("input", applyFilters);
        ["roleFilter", "gradeFilter", "statusFilter"].forEach(function (id) {
            byId(id).addEventListener("change", applyFilters);
        });

        loadLookups();
        loadUsers();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();