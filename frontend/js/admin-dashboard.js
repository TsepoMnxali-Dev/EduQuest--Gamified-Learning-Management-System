
/* =============================================================
   EduQuest - Admin Dashboard
============================================================= */

(function () {
    "use strict";

    // NEW: Verify that the API helper is loaded.
    if (!window.EduQuestAPI) {
        console.error("EduQuestAPI is missing.");
        return;
    }

    // CHANGED: Only Admin accounts may continue.
    if (!EduQuestAPI.requireAdmin()) {
        return;
    }

    const currentUser = EduQuestAPI.getCurrentUser();

    // ==========================================
    // ADMIN PROFILE
    // ==========================================

    const heading = document.getElementById("adminWelcome");
    const label = document.getElementById("adminProfileName");
    const avatar = document.getElementById("adminAvatar");

    const email = currentUser.email || "Admin";

    const displayName = email.includes("@")
        ? email.split("@")[0]
        : email;

    if (heading) {
        heading.textContent = "Welcome back, " + displayName + "!";
    }

    if (label) {
        label.textContent = displayName;
    }

    if (avatar) {
        avatar.textContent = displayName.charAt(0).toUpperCase();
    }

    // ==========================================
    // LOGOUT
    // ==========================================

    const logoutButton =
        document.getElementById("adminLogout");

    if (logoutButton) {
        logoutButton.addEventListener("click", function () {
            EduQuestAPI.logout();
        });
    }

    // ==========================================
    // NEW: UPDATE STATISTICS
    // ==========================================

    function updateStatistic(id, value) {

        const element = document.getElementById(id);

        if (!element) {
            return;
        }

        if (
            typeof value === "number" &&
            Number.isFinite(value)
        ) {
            element.textContent = value.toLocaleString();
        } else {
            element.textContent = "—";
        }
    }

    function updateStatus(message) {

        const status = document.getElementById("adminActivity");

        if (status) {
            status.textContent = message;
        }
    }

    // ==========================================
    // NEW: FETCH REAL DATA
    // ==========================================

    async function loadStatistics() {

        updateStatus("Loading statistics from EduQuest...");

        try {

            const data = await EduQuestAPI.get(
                "/Statistics/overview"
            );

            if (!data || typeof data !== "object") {
                throw new Error("Invalid statistics response.");
            }

            // CHANGED: Exact overview DTO properties.
            updateStatistic("totalLearners", data.totalLearners);
            updateStatistic("activeLearners", data.activeLearners);
            updateStatistic("inactiveLearners", data.inactiveLearners);

            updateStatistic(
                "totalQuizAttempts",
                data.totalQuizAttempts
            );

            updateStatistic(
                "totalCompetitionParticipants",
                data.totalCompetitionParticipants
            );

            updateStatistic(
                "totalAchievementsEarned",
                data.totalAchievementsEarned
            );

            updateStatus("Statistics are up to date.");

        } catch (error) {

            console.error("Dashboard statistics error:", error);

            updateStatus(
                error.message || "Unable to load statistics."
            );
        }
    }

    loadStatistics();

})();
