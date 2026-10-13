
/* ============================================================
   EDUQUEST - ADMIN USER MANAGEMENT
   File: frontend/js/main.js

   NEW:
   - Load real users from SQL Server via the API
   - Search and filter users
   - View user details
   - Create users
   - Edit users
   - Deactivate users
   - Populate role dropdowns from backend user data
   - Display API validation errors

   No hardcoded users or role IDs.
============================================================ */

"use strict";

// ============================================================
// CONFIGURATION AND STATE
// ============================================================

const api = window.EduQuestAPI;

let users = [];
let availableRoles = [];
let selectedUserId = null;


// ============================================================
// DOM HELPERS
// ============================================================

function byId(id) {
    return document.getElementById(id);
}

function setText(id, value) {
    const element = byId(id);

    if (!element) return;

    element.textContent =
        value === null ||
        value === undefined ||
        value === ""
            ? "—"
            : String(value);
}

function showError(id, message) {
    const element = byId(id);

    if (!element) return;

    element.textContent = message;
    element.hidden = false;
}

function clearError(id) {
    const element = byId(id);

    if (!element) return;

    element.textContent = "";
    element.hidden = true;
}

function openModal(id) {
    const modal = byId(id);

    if (!modal) return;

    modal.style.display = "flex";
}

function closeModal(id) {
    const modal = byId(id);

    if (!modal) return;

    modal.style.display = "none";
}

function getFullName(user) {
    return [
        user.firstName,
        user.lastName
    ].filter(Boolean).join(" ") || "Unknown User";
}

function getInitials(user) {
    const first = user.firstName?.charAt(0) || "";
    const last = user.lastName?.charAt(0) || "";

    return (first + last).toUpperCase() || "U";
}

function normalizeUser(data) {
    return {
        id: data.userID ?? data.userId ?? data.UserID,
        firstName: data.firstName ?? data.FirstName ?? "",
        lastName: data.lastName ?? data.LastName ?? "",
        email: data.email ?? data.Email ?? "",
        roleId: data.roleID ?? data.roleId ?? data.RoleID,
        roleName: data.roleName ?? data.RoleName ?? "",
        isActive: data.isActive ?? data.IsActive ?? false
    };
}

function setButtonLoading(buttonId, loading, loadingText) {
    const button = byId(buttonId);

    if (!button) return;

    if (loading) {
        button.dataset.originalText = button.textContent;
        button.textContent = loadingText;
        button.disabled = true;
    } else {
        button.textContent =
            button.dataset.originalText || button.textContent;

        button.disabled = false;
    }
}


// ============================================================
// ADMIN PROFILE
// ============================================================

function displayAdminProfile() {
    const admin = api.getCurrentUser();

    if (!admin) return;

    const name = [
        admin.firstName,
        admin.lastName
    ].filter(Boolean).join(" ");

    setText(
        "adminProfileName",
        name || admin.email || "Administrator"
    );

    const initials = [
        admin.firstName?.charAt(0),
        admin.lastName?.charAt(0)
    ].filter(Boolean).join("").toUpperCase();

    setText("adminAvatar", initials || "A");
}


// ============================================================
// GET USERS FROM BACKEND
// ============================================================

async function loadUsers() {
    showTableMessage("Loading registered users...");

    try {
        const response = await api.get("/Users");

        if (!Array.isArray(response)) {
            throw new Error(
                "Unexpected response from the Users API."
            );
        }

        users = response.map(normalizeUser);

        // CHANGED: Role IDs come from real backend records.
        buildAvailableRoles();

        populateRoleDropdowns();

        setText(
            "usersCount",
            `${users.length} registered users`
        );

        applyUserFilters();

    } catch (error) {
        console.error("Error loading users:", error);

        showTableMessage(
            error.message || "Unable to load users."
        );
    }
}


// ============================================================
// ROLE OPTIONS FROM BACKEND
// ============================================================

function buildAvailableRoles() {
    const rolesById = new Map();

    users.forEach(function (user) {
        const roleId = Number(user.roleId);

        if (
            Number.isInteger(roleId) &&
            roleId > 0 &&
            user.roleName
        ) {
            rolesById.set(roleId, {
                id: roleId,
                name: user.roleName
            });
        }
    });

    availableRoles = Array.from(rolesById.values());

    availableRoles.sort(function (a, b) {
        return a.name.localeCompare(b.name);
    });
}

function populateRoleDropdowns() {
    ["addRole", "editRole"].forEach(function (id) {
        const select = byId(id);

        if (!select) return;

        const previousValue = select.value;

        select.replaceChildren();

        const placeholder = document.createElement("option");
        placeholder.value = "";
        placeholder.textContent = "Select a role";

        select.appendChild(placeholder);

        availableRoles.forEach(function (role) {
            const option = document.createElement("option");

            option.value = String(role.id);
            option.textContent = role.name;

            select.appendChild(option);
        });

        if (
            availableRoles.some(
                role => String(role.id) === previousValue
            )
        ) {
            select.value = previousValue;
        }
    });
}


// ============================================================
// TABLE HELPERS
// ============================================================

function showTableMessage(message) {
    const body = byId("usersTableBody");

    if (!body) return;

    body.replaceChildren();

    const row = document.createElement("tr");
    const cell = document.createElement("td");

    cell.colSpan = 5;
    cell.textContent = message;
    cell.style.textAlign = "center";
    cell.style.padding = "25px";

    row.appendChild(cell);
    body.appendChild(row);
}

function createCell(value) {
    const cell = document.createElement("td");

    cell.textContent =
        value === null ||
        value === undefined ||
        value === ""
            ? "—"
            : String(value);

    return cell;
}

function createActionButton(label, className, handler) {
    const button = document.createElement("button");

    button.type = "button";
    button.className = className;
    button.textContent = label;

    button.addEventListener("click", handler);

    return button;
}


// ============================================================
// DISPLAY USERS
// ============================================================

function displayUsers(userList = users) {
    const body = byId("usersTableBody");

    if (!body) return;

    body.replaceChildren();

    if (userList.length === 0) {
        showTableMessage("No users found.");
        return;
    }

    userList.forEach(function (user) {
        const row = document.createElement("tr");

        // NAME
        const nameCell = document.createElement("td");
        const nameWrapper = document.createElement("div");

        nameWrapper.className = "user-list-name";

        const avatar = document.createElement("span");
        avatar.className = "user-list-avatar";
        avatar.textContent = getInitials(user);

        const name = document.createElement("strong");
        name.textContent = getFullName(user);

        nameWrapper.append(avatar, name);
        nameCell.appendChild(nameWrapper);
        row.appendChild(nameCell);

        // EMAIL
        row.appendChild(createCell(user.email));

        // ROLE
        row.appendChild(createCell(user.roleName));

        // STATUS
        const statusCell = document.createElement("td");
        const status = document.createElement("span");

        status.className = user.isActive
            ? "user-status user-status-active"
            : "user-status user-status-inactive";

        status.textContent = user.isActive
            ? "Active"
            : "Inactive";

        statusCell.appendChild(status);
        row.appendChild(statusCell);

        // ACTIONS
        const actionsCell = document.createElement("td");
        const actions = document.createElement("div");

        actions.className = "user-row-actions";

        actions.appendChild(
            createActionButton(
                "View",
                "btn btn-outline",
                function () {
                    viewUser(user.id);
                }
            )
        );

        actions.appendChild(
            createActionButton(
                "Edit",
                "btn btn-outline",
                function () {
                    openEditUser(user.id);
                }
            )
        );

        if (user.isActive) {
            actions.appendChild(
                createActionButton(
                    "Deactivate",
                    "btn btn-danger",
                    function () {
                        deactivateUser(user.id);
                    }
                )
            );
        }

        actionsCell.appendChild(actions);
        row.appendChild(actionsCell);

        body.appendChild(row);
    });
}


// ============================================================
// SEARCH AND FILTER USERS
// ============================================================

function applyUserFilters() {
    const search =
        byId("userSearch")?.value.trim().toLowerCase() || "";

    const role =
        byId("roleFilter")?.value.trim().toLowerCase() || "";

    const status =
        byId("statusFilter")?.value.trim().toLowerCase() || "";

    const filtered = users.filter(function (user) {
        const matchesSearch =
            getFullName(user).toLowerCase().includes(search) ||
            user.email.toLowerCase().includes(search);

        const matchesRole =
            !role ||
            user.roleName.toLowerCase() === role;

        const currentStatus =
            user.isActive ? "active" : "inactive";

        const matchesStatus =
            !status ||
            currentStatus === status;

        return matchesSearch &&
               matchesRole &&
               matchesStatus;
    });

    displayUsers(filtered);
}


// ============================================================
// VIEW USER DETAILS
// ============================================================

async function viewUser(userId) {
    openModal("viewUserModal");

    setText("viewUserName", "Loading...");
    setText("viewUserEmail", "—");
    setText("viewUserAvatar", "U");
    setText("viewUserStatus", "—");
    setText("viewUserId", "—");
    setText("viewUserRole", "—");
    setText("viewUserRoleId", "—");
    setText("viewUserAccountStatus", "—");

    try {
        const response = await api.get(`/Users/${userId}`);
        const user = normalizeUser(response);

        setText("viewUserName", getFullName(user));
        setText("viewUserEmail", user.email);
        setText("viewUserAvatar", getInitials(user));
        setText("viewUserId", user.id);
        setText("viewUserRole", user.roleName);
        setText("viewUserRoleId", user.roleId);

        const accountStatus = user.isActive
            ? "Active"
            : "Inactive";

        setText("viewUserStatus", accountStatus);
        setText("viewUserAccountStatus", accountStatus);

    } catch (error) {
        console.error("Error loading user details:", error);

        setText(
            "viewUserName",
            "Unable to load user details"
        );

        setText("viewUserEmail", error.message);
    }
}


// ============================================================
// ADD USER
// ============================================================

function openAddUser() {
    byId("addUserForm")?.reset();

    clearError("addUserError");

    populateRoleDropdowns();

    openModal("addUserModal");
}

async function addUser(event) {
    event.preventDefault();

    clearError("addUserError");

    const roleId = Number(byId("addRole").value);

    if (!Number.isInteger(roleId) || roleId <= 0) {
        showError(
            "addUserError",
            "Please select a valid role."
        );
        return;
    }

    const payload = {
        roleID: roleId,
        firstName: byId("addFirstName").value.trim(),
        lastName: byId("addLastName").value.trim(),
        email: byId("addEmail").value.trim(),
        password: byId("addPassword").value
    };

    if (
        !payload.firstName ||
        !payload.lastName ||
        !payload.email ||
        payload.password.length < 8
    ) {
        showError(
            "addUserError",
            "Please complete all required fields. " +
            "The password must have at least 8 characters."
        );
        return;
    }

    setButtonLoading("saveAddUser", true, "Creating...");

    try {
        await api.post("/Users", payload);

        closeModal("addUserModal");

        byId("addUserForm").reset();

        await loadUsers();

    } catch (error) {
        console.error("Error creating user:", error);

        showError(
            "addUserError",
            error.message || "Unable to create user."
        );

    } finally {
        setButtonLoading("saveAddUser", false);
    }
}


// ============================================================
// EDIT USER - LOAD DATA FROM BACKEND
// ============================================================

async function openEditUser(userId) {
    selectedUserId = userId;

    clearError("editUserError");

    const form = byId("editUserForm");

    if (form) form.reset();

    openModal("editUserModal");

    try {
        const response = await api.get(`/Users/${userId}`);
        const user = normalizeUser(response);

        selectedUserId = user.id;

        byId("editUserId").value = user.id;
        byId("editFirstName").value = user.firstName;
        byId("editLastName").value = user.lastName;
        byId("editEmail").value = user.email;

        populateRoleDropdowns();

        const roleSelect = byId("editRole");

        // Preserve the user's actual role even if it
        // is missing from the derived role list.
        if (
            !Array.from(roleSelect.options).some(
                option => option.value === String(user.roleId)
            )
        ) {
            const option = document.createElement("option");

            option.value = String(user.roleId);
            option.textContent = user.roleName;

            roleSelect.appendChild(option);
        }

        roleSelect.value = String(user.roleId);

    } catch (error) {
        console.error("Error loading user for editing:", error);

        showError(
            "editUserError",
            error.message || "Unable to load user."
        );
    }
}


// ============================================================
// SAVE EDITED USER
// ============================================================

async function saveEditedUser(event) {
    event.preventDefault();

    clearError("editUserError");

    const userId = Number(byId("editUserId").value);
    const roleId = Number(byId("editRole").value);

    if (
        !Number.isInteger(userId) ||
        userId <= 0 ||
        !Number.isInteger(roleId) ||
        roleId <= 0
    ) {
        showError(
            "editUserError",
            "Invalid user or role selection."
        );
        return;
    }

    const payload = {
        roleID: roleId,
        firstName: byId("editFirstName").value.trim(),
        lastName: byId("editLastName").value.trim(),
        email: byId("editEmail").value.trim()
    };

    if (
        !payload.firstName ||
        !payload.lastName ||
        !payload.email
    ) {
        showError(
            "editUserError",
            "Please complete all required fields."
        );
        return;
    }

    setButtonLoading(
        "saveEditUser",
        true,
        "Saving..."
    );

    try {
        await api.put(`/Users/${userId}`, payload);

        closeModal("editUserModal");

        selectedUserId = null;

        await loadUsers();

    } catch (error) {
        console.error("Error updating user:", error);

        showError(
            "editUserError",
            error.message || "Unable to update user."
        );

    } finally {
        setButtonLoading("saveEditUser", false);
    }
}


// ============================================================
// DEACTIVATE USER
// ============================================================

async function deactivateUser(userId) {
    const user = users.find(function (item) {
        return String(item.id) === String(userId);
    });

    if (!user) return;

    const confirmed = window.confirm(
        `Deactivate ${getFullName(user)}?\n\n` +
        "This will mark the account as inactive."
    );

    if (!confirmed) return;

    try {
        // Backend performs a soft delete:
        // IsActive = false
        await api.del(`/Users/${userId}`);

        await loadUsers();

    } catch (error) {
        console.error("Error deactivating user:", error);

        window.alert(
            error.message || "Unable to deactivate user."
        );
    }
}


// ============================================================
// MODAL EVENT HANDLERS
// ============================================================

function setupModalEvents() {
    // VIEW MODAL
    byId("closeViewUser")?.addEventListener(
        "click",
        function () {
            closeModal("viewUserModal");
        }
    );

    byId("doneViewUser")?.addEventListener(
        "click",
        function () {
            closeModal("viewUserModal");
        }
    );

    // ADD MODAL
    byId("openAddUser")?.addEventListener(
        "click",
        openAddUser
    );

    byId("closeAddUser")?.addEventListener(
        "click",
        function () {
            closeModal("addUserModal");
        }
    );

    byId("cancelAddUser")?.addEventListener(
        "click",
        function () {
            closeModal("addUserModal");
        }
    );

    byId("addUserForm")?.addEventListener(
        "submit",
        addUser
    );

    // EDIT MODAL
    byId("closeEditUser")?.addEventListener(
        "click",
        function () {
            closeModal("editUserModal");
        }
    );

    byId("cancelEditUser")?.addEventListener(
        "click",
        function () {
            closeModal("editUserModal");
        }
    );

    byId("editUserForm")?.addEventListener(
        "submit",
        saveEditedUser
    );

    // CLOSE WHEN CLICKING MODAL BACKDROP
    [
        "viewUserModal",
        "addUserModal",
        "editUserModal"
    ].forEach(function (id) {
        const modal = byId(id);

        if (!modal) return;

        modal.addEventListener("click", function (event) {
            if (event.target === modal) {
                closeModal(id);
            }
        });
    });

    // ESCAPE KEY
    document.addEventListener("keydown", function (event) {
        if (event.key !== "Escape") return;

        [
            "viewUserModal",
            "addUserModal",
            "editUserModal"
        ].forEach(closeModal);
    });
}


// ============================================================
// FILTER EVENTS
// ============================================================

function setupFilterEvents() {
    byId("userSearch")?.addEventListener(
        "input",
        applyUserFilters
    );

    byId("roleFilter")?.addEventListener(
        "change",
        applyUserFilters
    );

    byId("statusFilter")?.addEventListener(
        "change",
        applyUserFilters
    );
}


// ============================================================
// INITIALIZE PAGE
// ============================================================

document.addEventListener("DOMContentLoaded", async function () {
    if (!api) {
        showTableMessage(
            "EduQuest API helper is not available."
        );
        return;
    }

    // Only authenticated Admin users may access this page.
    if (!api.requireAdmin()) {
        return;
    }

    displayAdminProfile();

    setupModalEvents();
    setupFilterEvents();

    byId("adminLogout")?.addEventListener(
        "click",
        function () {
            api.logout();
        }
    );

    await loadUsers();
});
