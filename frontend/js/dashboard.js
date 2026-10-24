
/* =============================================================
   EduQuest - Admin Dashboard
   File: frontend/js/admin-dashboard.js

   Displays real platform statistics using existing API data.

   Dashboard:
   - Total Learners
   - Active Learners
   - Inactive Learners
   - Quiz Attempts
   - Competition Participants
   - Achievements Earned

   Requires:
   - api.js
   - admin-guard.js

============================================================= */

(function () {
    "use strict";

    // =========================================================
    // API AND ADMIN AUTHENTICATION
    // =========================================================

    const API = window.EduQuestAPI;

    if (!API) {
        console.error(
            "Admin Dashboard: EduQuestAPI is missing."
        );
        return;
    }

    const LOGIN_URL =
        "../../EduQuest-with-js/EduQuest/login.html";

    const currentUser =
        typeof API.getCurrentUser === "function"
            ? API.getCurrentUser()
            : null;

    if (
        !currentUser ||
        String(currentUser.role || "").toLowerCase() !== "admin"
    ) {
        window.location.href = LOGIN_URL;
        return;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    function byId(id) {
        return document.getElementById(id);
    }

    function setText(id, value) {
        const element = byId(id);

        if (element) {
            element.textContent =
                value === null || value === undefined
                    ? "—"
                    : String(value);
        }
    }

    function getField(object, ...names) {
        if (!object || typeof object !== "object") {
            return undefined;
        }

        for (const name of names) {
            if (object[name] !== undefined) {
                return object[name];
            }
        }

        return undefined;
    }

    function getArray(response) {
        if (Array.isArray(response)) {
            return response;
        }

        if (!response || typeof response !== "object") {
            return null;
        }

        for (const key of [
            "items",
            "data",
            "results",
            "users",
            "learners",
            "quizAttempts",
            "competitions",
            "achievements"
        ]) {
            if (Array.isArray(response[key])) {
                return response[key];
            }
        }

        return null;
    }

    function formatNumber(value) {
        const number = Number(value);

        if (!Number.isFinite(number)) {
            return "—";
        }

        return number.toLocaleString("en-ZA");
    }

    function setStat(id, value) {
        setText(id, formatNumber(value));
    }

    function setStatus(message) {
        setText("adminActivity", message);
    }

    function isLearner(user) {
        const role = getField(
            user,
            "role",
            "Role",
            "roleName",
            "RoleName"
        );

        return String(role || "").toLowerCase() === "learner";
    }

    function getUserStatus(user) {
        const active = getField(
            user,
            "isActive",
            "IsActive",
            "active",
            "Active"
        );

        if (typeof active === "boolean") {
            return active;
        }

        if (active === 1 || active === "1") {
            return true;
        }

        if (active === 0 || active === "0") {
            return false;
        }

        const status = getField(
            user,
            "status",
            "Status"
        );

        if (typeof status === "string") {
            const normalized = status.toLowerCase();

            if (normalized === "active") {
                return true;
            }

            if (normalized === "inactive") {
                return false;
            }
        }

        return null;
    }

    // =========================================================
    // ADMIN PROFILE
    // =========================================================

    function loadAdminProfile() {
        const firstName = String(
            getField(
                currentUser,
                "firstName",
                "FirstName"
            ) || ""
        ).trim();

        const lastName = String(
            getField(
                currentUser,
                "lastName",
                "LastName"
            ) || ""
        ).trim();

        const displayName =
            [firstName, lastName].filter(Boolean).join(" ") ||
            getField(currentUser, "name", "Name") ||
            "Administrator";

        const firstLetter =
            String(displayName).charAt(0).toUpperCase();

        setText(
            "adminWelcome",
            "Welcome back, " + displayName + "!"
        );

        setText("adminProfileName", displayName);
        setText("adminAvatar", firstLetter);
    }

    // =========================================================
    // LEARNER STATISTICS
    // =========================================================

    function renderLearnerStatistics(response) {
        const users = getArray(response);

        if (!users) {
            throw new Error(
                "The users endpoint did not return a recognized list."
            );
        }

        const learners = users.filter(isLearner);

        const active = learners.filter(
            user => getUserStatus(user) === true
        ).length;

        const inactive = learners.filter(
            user => getUserStatus(user) === false
        ).length;

        const unknown = learners.length - active - inactive;

        setStat("totalLearners", learners.length);

        // Unknown statuses are not automatically considered
        // active or inactive.
        if (unknown === 0) {
            setStat("activeLearners", active);
            setStat("inactiveLearners", inactive);
        } else {
            setText("activeLearners", "—");
            setText("inactiveLearners", "—");
        }

        return {
            total: learners.length,
            active: unknown === 0 ? active : null,
            inactive: unknown === 0 ? inactive : null,
            unknown: unknown
        };
    }

    // =========================================================
    // OPTIONAL PLATFORM STATISTICS
    // =========================================================

    async function loadOptionalCount(
        endpoint,
        elementId,
        label
    ) {
        try {
            const response = await API.get(endpoint);

            const items = getArray(response);

            if (items) {
                setStat(elementId, items.length);
                return true;
            }

            const count = getField(
                response,
                "count",
                "Count",
                "total",
                "Total",
                "totalCount",
                "TotalCount"
            );

            if (
                count !== undefined &&
                count !== null &&
                Number.isFinite(Number(count))
            ) {
                setStat(elementId, Number(count));
                return true;
            }

            console.warn(
                label + ": Unexpected API response.",
                response
            );

            return false;

        } catch (error) {
            console.warn(
                label + " is unavailable:",
                error
            );

            return false;
        }
    }

    // =========================================================
    // DASHBOARD DATA
    // =========================================================

    async function loadDashboard() {
        setStatus("Loading platform statistics...");

        // Load users from the endpoint already used
        // by the original Admin Dashboard.
        let learnerStats = null;

        try {
            const usersResponse = await API.get("/users");

            learnerStats =
                renderLearnerStatistics(usersResponse);

        } catch (error) {
            console.error(
                "Unable to load learner statistics:",
                error
            );
        }

        // The following endpoints must exist and return
        // the required records to populate their cards.
        // A missing endpoint leaves the value as "—".
        

        const results = await Promise.all([
            loadOptionalCount(
                "/quizattempts",
                "totalQuizAttempts",
                "Quiz attempts"
            ),

            loadOptionalCount(
                "/competitionparticipants",
                "totalCompetitionParticipants",
                "Competition participants"
            ),

            loadOptionalCount(
                "/achievements",
                "totalAchievementsEarned",
                "Achievements earned"
            )
        ]);

        const availableCounts =
            results.filter(Boolean).length;

        if (learnerStats) {
            const unknownStatusMessage =
                learnerStats.unknown > 0
                    ? " Active/inactive counts are unavailable because some learner records do not expose a recognized status."
                    : "";

            if (availableCounts === 3) {
                setStatus(
                    "Dashboard statistics loaded successfully." +
                    unknownStatusMessage
                );
            } else {
                setStatus(
                    "Learner statistics loaded. Some additional " +
                    "statistics are unavailable from the current API." +
                    unknownStatusMessage
                );
            }

        } else {
            setStatus(
                "Unable to load learner statistics. " +
                "Check the backend connection and API permissions."
            );
        }
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    function setupLogout() {
        const logoutButton = byId("adminLogout");

        if (!logoutButton) return;

        // Only attach a listener if admin-guard.js
        // does not already handle this button.
        // The existing admin-guard.js should remain
        // responsible for the admin sign-out flow.
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    function initialize() {
        loadAdminProfile();
        setupLogout();
        loadDashboard();
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initialize,
            { once: true }
        );
    } else {
        initialize();
    }

})();
