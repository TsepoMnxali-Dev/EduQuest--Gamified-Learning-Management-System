
/* =============================================================
   EduQuest - Learner Dashboard

   CHANGED:
   1. Displays the learner's real full name in the welcome message.
   2. Displays only the first name in the top-right profile.
   3. Retrieves the learner's registered grade from the backend.
   4. Uses GET /api/learners/me.
   5. Preserves authentication and logout.
   6. Does not generate fictional statistics.
============================================================= */

(function () {
    "use strict";

    // ==========================================
    // AUTHENTICATION
    // ==========================================

    if (!window.EduQuestAPI) {
        console.error("EduQuestAPI is missing.");
        return;
    }

    // Only authenticated learners can access this page.
    if (!EduQuestAPI.requireLearner()) {
        return;
    }

    const currentUser = EduQuestAPI.getCurrentUser();

    if (!currentUser) {
        return;
    }

    // ==========================================
    // HTML ELEMENTS
    // ==========================================

    const heading =
        document.getElementById("welcomeHeading");

    const label =
        document.getElementById("profileLabel");

    const avatar =
        document.getElementById("profileAvatar");

    // NEW: Grade element from dashboard.html
    const gradeLabel =
        document.getElementById("profileGrade");

    const logoutLink =
        document.getElementById("logoutLink");

    // ==========================================
    // LEARNER INFORMATION
    // ==========================================

    const firstName =
        String(currentUser.firstName || "").trim();

    const lastName =
        String(currentUser.lastName || "").trim();

    const fullName =
        [firstName, lastName]
            .filter(Boolean)
            .join(" ");

    const displayName = fullName || "Learner";

    // ==========================================
    // WELCOME MESSAGE
    // ==========================================

    // Displays: Welcome back, Thabo Mfazwe!
    if (heading) {
        heading.textContent =
            "Welcome back, " + displayName + "!";
    }

    // ==========================================
    // TOP-RIGHT PROFILE
    // ==========================================

    // CHANGED: Displays first name only.
    if (label) {
        label.textContent =
            firstName || "Learner";
    }

    // CHANGED: Displays first-name initial.
    // Example: Thabo -> T
    if (avatar) {
        avatar.textContent =
            firstName
                ? firstName.charAt(0).toUpperCase()
                : "L";
    }

    // ==========================================
    // NEW: RETRIEVE LEARNER'S GRADE
    // ==========================================

    async function loadLearnerGrade() {

        if (!gradeLabel) {
            return;
        }

        // Do not display a fictional grade.
        gradeLabel.textContent = "";

        try {

            // NEW: Fetch logged-in learner profile.
            const learner =
                await EduQuestAPI.get("/learners/me");

            if (!learner) {
                return;
            }

            // CHANGED: Support normal camelCase JSON
            // and PascalCase JSON configurations.
            const rawGrade =
                learner.gradeName ??
                learner.GradeName;

            if (rawGrade === null ||
                rawGrade === undefined) {
                return;
            }

            const gradeName =
                String(rawGrade).trim();

            if (!gradeName) {
                return;
            }

            // Prevent "Grade Grade 12".
            const formattedGrade =
                /^grade\b/i.test(gradeName)
                    ? gradeName
                    : "Grade " + gradeName;

            // Example: Thabo · Grade 12
            gradeLabel.textContent =
                "· " + formattedGrade;

        } catch (error) {

            console.error(
                "Failed to retrieve learner grade:",
                error
            );

            // Leave the grade blank if the
            // backend request fails.
            gradeLabel.textContent = "";
        }
    }

    // ==========================================
    // LOGOUT
    // ==========================================

    if (logoutLink) {

        logoutLink.addEventListener(
            "click",
            function (event) {

                event.preventDefault();

                EduQuestAPI.logout();
            }
        );
    }

    // ==========================================
    // INITIALIZE DASHBOARD
    // ==========================================

    // NEW: Retrieve the learner's real grade.
    loadLearnerGrade();

    // Existing empty states remain unchanged
    // until the statistics APIs are connected.

})();
