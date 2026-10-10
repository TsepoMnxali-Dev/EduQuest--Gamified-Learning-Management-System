/* =============================================================
   EduQuest - profile.js  (used by profile.html)

   Loads the signed-in learner's real data:

     GET /api/learners/me                    name, email, grade, school, province
     GET /api/learners/{id}/subjects         chosen subjects
     GET /api/learners/{id}/achievements     earned achievements (IDs)
     GET /api/achievements                   achievement names / badges
     GET /api/statistics/learners/{id}       quizzes completed
     GET /api/leaderboards                   total points + rank

   Buttons:
     Edit Profile     PUT  /api/learners/me                  name, grade, school
     Change Password  POST /api/auth/change-password
     Manage Subjects  POST/DELETE /api/learners/{id}/subjects/{subjectId}
     Help & Support   opens a help dialog (set SUPPORT_EMAIL below)

   Needs js/api.js (EduQuestAPI) loaded first.
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    /* Shown in the Help & Support dialog. Leave empty to hide the contact line. */
    var SUPPORT_EMAIL = "";

    var currentProfile = null;   /* GET /learners/me */
    var currentSubjects = [];    /* [{ subjectID, subjectName }] */

    function byId(id) { return document.getElementById(id); }

    function setText(id, value) {
        var el = byId(id);
        if (el) { el.textContent = value; }
    }

    /* Values come from the database, so escape before using innerHTML. */
    function esc(value) {
        return String(value === null || value === undefined ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function showError(message) {
        var box = byId("profileError");
        if (!box) { return; }
        box.textContent = message;
        box.style.display = "block";
    }

    function formatMonthYear(iso) {
        var date = new Date(iso);
        if (isNaN(date.getTime())) { return "-"; }
        return date.toLocaleDateString("en-ZA", { month: "long", year: "numeric" });
    }

    /* A failed secondary request shouldn't blank the whole page. */
    function safe(promise, fallback) {
        return promise.catch(function (err) {
            console.error("Profile request failed:", err);
            return fallback;
        });
    }

    function renderBadge(achievement) {
        var image = String(achievement.badgeImage || "").trim();
        var isImage = /^(https?:)?\/\//i.test(image) || /^\/|\.(png|jpe?g|gif|svg|webp)$/i.test(image);
        var icon = isImage
            ? '<img src="' + esc(image) + '" alt="" style="width:40px;height:40px;object-fit:contain;">'
            : esc(image || "🏆");

        return '<div class="badge" title="' + esc(achievement.description) + '">' +
            '<div class="badge-icon">' + icon + "</div>" +
            "<p>" + esc(achievement.name) + "</p></div>";
    }

    function renderAchievements(earned, allAchievements) {
        var grid = byId("badgeGrid");
        if (!grid) { return; }

        var earnedIds = {};
        earned.forEach(function (e) { earnedIds[e.achievementID] = true; });

        var mine = allAchievements.filter(function (a) { return earnedIds[a.achievementID]; });

        grid.innerHTML = mine.length
            ? mine.map(renderBadge).join("")
            : "<p>No achievements earned yet. Complete quizzes to start earning badges!</p>";
    }

    async function loadProfile() {
        var profile;
        try {
            profile = await EduQuestAPI.get("/learners/me");
        } catch (err) {
            setText("profileName", "Profile unavailable");
            setText("profileSummary", "");
            setText("statScore", "0");
            setText("statRank", "-");
            setText("statQuizzes", "0");
            byId("badgeGrid").innerHTML = "";
            if (/session has expired/i.test(err.message)) {
                window.location.href = "login.html";
                return;
            }
            showError(err.message);
            return;
        }

        currentProfile = profile;

        var fullName = (profile.firstName + " " + profile.lastName).trim();
        var initials = ((profile.firstName || "").charAt(0) + (profile.lastName || "").charAt(0)).toUpperCase();

        setText("profileAvatar", initials || "?");
        setText("profileName", fullName);
        setText("profileSummary", [profile.gradeName, profile.schoolName, profile.provinceName].join(" · "));

        setText("infoName", fullName);
        setText("infoEmail", profile.email);
        setText("infoGrade", profile.gradeName);
        setText("infoSchool", profile.schoolName);
        setText("infoProvince", profile.provinceName);
        setText("infoEnrolled", formatMonthYear(profile.dateCreated));

        var id = profile.learnerID;
        var results = await Promise.all([
            safe(EduQuestAPI.get("/learners/" + id + "/subjects"), []),
            safe(EduQuestAPI.get("/learners/" + id + "/achievements"), []),
            safe(EduQuestAPI.get("/achievements"), []),
            safe(EduQuestAPI.get("/statistics/learners/" + id), null),
            safe(EduQuestAPI.get("/leaderboards"), [])
        ]);

        var subjects = results[0];
        currentSubjects = subjects;
        setText("infoSubjects", subjects.length
            ? subjects.map(function (s) { return s.subjectName; }).join(", ")
            : "None selected yet");

        renderAchievements(results[1], results[2]);

        setText("statQuizzes", results[3] ? results[3].totalQuizAttempts : 0);

        /* Use the learner's most recently updated leaderboard entry. */
        var entries = results[4]
            .filter(function (e) { return e.learnerID === id; })
            .sort(function (a, b) { return new Date(b.lastUpdated) - new Date(a.lastUpdated); });

        if (entries.length) {
            var rank = String(entries[0].rank);
            setText("statScore", entries[0].totalPoints);
            setText("statRank", /^\d+$/.test(rank) ? "#" + rank : rank);
        } else {
            setText("statScore", 0);
            setText("statRank", "Unranked");
        }
    }


    /* ---------- dialogs ---------- */

    var modalSubmit = null;   /* function called when the dialog form is saved */

    function openModal(title, bodyHtml, saveLabel, onSubmit) {
        byId("pfTitle").textContent = title;
        byId("pfBody").innerHTML = bodyHtml;
        hideModalError();
        modalSubmit = onSubmit;

        var save = byId("pfSave");
        save.hidden = !onSubmit;
        save.disabled = false;
        save.textContent = saveLabel || "Save";
        byId("pfCancel").textContent = onSubmit ? "Cancel" : "Close";

        byId("pfModal").hidden = false;
        var first = byId("pfBody").querySelector("input, select");
        if (first) { first.focus(); }
    }

    function closeModal() {
        byId("pfModal").hidden = true;
        byId("pfBody").innerHTML = "";
        modalSubmit = null;
    }

    function showModalError(message) {
        var box = byId("pfError");
        box.textContent = message;
        box.hidden = false;
    }

    function hideModalError() {
        var box = byId("pfError");
        box.textContent = "";
        box.hidden = true;
    }

    async function handleModalSubmit(event) {
        event.preventDefault();
        if (!modalSubmit) { return; }

        hideModalError();
        var save = byId("pfSave");
        var label = save.textContent;
        save.disabled = true;
        save.textContent = "Saving...";

        try {
            await modalSubmit();
        } catch (err) {
            if (/session has expired/i.test(err.message)) {
                window.location.href = "login.html";
                return;
            }
            showModalError(err.message);
        } finally {
            save.disabled = false;
            save.textContent = label;
        }
    }

    /* ----- Edit Profile ----- */

    async function openEditProfile() {
        if (!currentProfile) { return; }

        var grades, schools;
        try {
            var lists = await Promise.all([EduQuestAPI.get("/grades"), EduQuestAPI.get("/schools")]);
            grades = lists[0].slice().sort(function (a, b) { return a.gradeID - b.gradeID; });
            schools = lists[1].slice().sort(function (a, b) { return a.schoolName.localeCompare(b.schoolName); });
        } catch (err) {
            showError("Could not load grades and schools: " + err.message);
            return;
        }

        function schoolLabel(s) { return s.schoolName + " (" + s.provinceName + ")"; }

        var gradeOptions = grades.map(function (g) {
            return '<option value="' + g.gradeID + '"' + (g.gradeID === currentProfile.gradeID ? " selected" : "") +
                ">" + esc(g.gradeName) + "</option>";
        }).join("");

        var schoolOptions = schools.map(function (s) {
            return '<option value="' + esc(schoolLabel(s)) + '"></option>';
        }).join("");

        var current = schools.filter(function (s) { return s.schoolID === currentProfile.schoolID; })[0];

        openModal("Edit Profile",
            '<div class="pf-field"><label for="pfFirst">First name</label>' +
                '<input id="pfFirst" maxlength="100" value="' + esc(currentProfile.firstName) + '"></div>' +
            '<div class="pf-field"><label for="pfLast">Surname</label>' +
                '<input id="pfLast" maxlength="100" value="' + esc(currentProfile.lastName) + '"></div>' +
            '<div class="pf-field"><label for="pfGrade">Grade</label>' +
                '<select id="pfGrade">' + gradeOptions + "</select></div>" +
            '<div class="pf-field"><label for="pfSchool">School</label>' +
                '<input id="pfSchool" list="pfSchoolList" autocomplete="off" placeholder="Type to search schools..." ' +
                    'value="' + esc(current ? schoolLabel(current) : "") + '">' +
                '<datalist id="pfSchoolList">' + schoolOptions + "</datalist>" +
                '<span class="pf-hint">Start typing, then pick your school from the list.</span></div>',
            "Save Changes",
            async function () {
                var first = byId("pfFirst").value.trim();
                var last = byId("pfLast").value.trim();
                var typed = byId("pfSchool").value.trim().toLowerCase();

                if (!first || !last) { throw new Error("Please enter your first name and surname."); }

                var school = schools.filter(function (s) {
                    return schoolLabel(s).toLowerCase() === typed;
                })[0];
                if (!school) { throw new Error("Please pick your school from the list."); }

                await EduQuestAPI.put("/learners/me", {
                    firstName: first,
                    lastName: last,
                    gradeID: Number(byId("pfGrade").value),
                    schoolID: school.schoolID
                });

                closeModal();
                await loadProfile();
            });
    }

    /* ----- Change Password ----- */

    function openChangePassword() {
        openModal("Change Password",
            '<div class="pf-field"><label for="pfCurrent">Current password</label>' +
                '<input id="pfCurrent" type="password" autocomplete="current-password"></div>' +
            '<div class="pf-field"><label for="pfNew">New password</label>' +
                '<input id="pfNew" type="password" autocomplete="new-password">' +
                '<span class="pf-hint">At least 8 characters.</span></div>' +
            '<div class="pf-field"><label for="pfConfirm">Confirm new password</label>' +
                '<input id="pfConfirm" type="password" autocomplete="new-password"></div>',
            "Update Password",
            async function () {
                var current = byId("pfCurrent").value;
                var next = byId("pfNew").value;

                if (!current) { throw new Error("Please enter your current password."); }
                if (next.length < 8) { throw new Error("The new password must be at least 8 characters."); }
                if (next !== byId("pfConfirm").value) { throw new Error("The new passwords do not match."); }

                await EduQuestAPI.post("/auth/change-password", {
                    currentPassword: current,
                    newPassword: next
                });

                byId("pfBody").innerHTML = '<p class="pf-success">Your password has been updated.</p>';
                byId("pfSave").hidden = true;
                byId("pfCancel").textContent = "Close";
                modalSubmit = null;
            });
    }

    /* ----- Manage Subjects ----- */

    async function openManageSubjects() {
        if (!currentProfile) { return; }

        var all;
        try {
            all = await EduQuestAPI.get("/subjects");
        } catch (err) {
            showError("Could not load subjects: " + err.message);
            return;
        }

        var have = {};
        currentSubjects.forEach(function (s) { have[s.subjectID] = true; });

        var checks = all.length
            ? '<div class="pf-checks">' + all.map(function (s) {
                return '<label class="pf-check"><input type="checkbox" value="' + s.subjectID + '"' +
                    (have[s.subjectID] ? " checked" : "") + "> " + esc(s.subjectName) + "</label>";
            }).join("") + "</div>"
            : "<p>No subjects are available yet.</p>";

        openModal("Manage Subjects",
            '<p class="pf-hint" style="margin:0 0 12px;">Choose the subjects you are studying.</p>' + checks,
            "Save Subjects",
            async function () {
                var wanted = {};
                byId("pfBody").querySelectorAll("input[type=checkbox]:checked").forEach(function (box) {
                    wanted[box.value] = true;
                });

                var base = "/learners/" + currentProfile.learnerID + "/subjects/";
                var jobs = [];

                all.forEach(function (s) {
                    if (wanted[s.subjectID] && !have[s.subjectID]) {
                        jobs.push(EduQuestAPI.post(base + s.subjectID, {}));
                    } else if (!wanted[s.subjectID] && have[s.subjectID]) {
                        jobs.push(EduQuestAPI.del(base + s.subjectID));
                    }
                });

                await Promise.all(jobs);
                closeModal();
                await loadProfile();
            });
    }

    /* ----- Help & Support ----- */

    function openHelp() {
        var contact = SUPPORT_EMAIL
            ? '<p><strong>Still stuck?</strong>Email <a href="mailto:' + esc(SUPPORT_EMAIL) + '">' +
                esc(SUPPORT_EMAIL) + "</a> and we will get back to you.</p>"
            : "";

        openModal("Help & Support",
            '<div class="pf-help">' +
                "<p><strong>Update your details</strong>Use Edit Profile to change your name, grade or school.</p>" +
                "<p><strong>Forgot your password?</strong>Use Change Password while signed in, or the reset link on the sign-in page.</p>" +
                "<p><strong>Missing subjects or points?</strong>Use Manage Subjects to pick your subjects. Your score and rank update after you complete quizzes.</p>" +
                contact +
            "</div>",
            null, null);
    }

    byId("editProfileBtn").addEventListener("click", openEditProfile);
    byId("changePasswordBtn").addEventListener("click", openChangePassword);
    byId("manageSubjectsBtn").addEventListener("click", openManageSubjects);
    byId("helpBtn").addEventListener("click", openHelp);

    byId("pfForm").addEventListener("submit", handleModalSubmit);
    byId("pfClose").addEventListener("click", closeModal);
    byId("pfCancel").addEventListener("click", closeModal);
    byId("pfModal").addEventListener("mousedown", function (e) {
        if (e.target === byId("pfModal")) { closeModal(); }
    });
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && !byId("pfModal").hidden) { closeModal(); }
    });

    var logoutBtn = byId("logoutBtn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", function () { EduQuestAPI.logout(); });
    }

    loadProfile();
})();
