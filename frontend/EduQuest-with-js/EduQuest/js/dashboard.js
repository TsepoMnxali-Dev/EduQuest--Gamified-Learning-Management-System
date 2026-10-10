
/* =============================================================
   EduQuest - Learner Dashboard

   CHANGED:
   1. Checks the Learner role.
   2. Prevents dashboard logic after failed authentication.
   3. Preserves welcome message and logout.
   4. Does not insert fictional data.
============================================================= */

(function () {
    "use strict";

    if (!window.EduQuestAPI) {
        console.error("EduQuestAPI is missing.");
        return;
    }

    // CHANGED: Learners only.
    if (!EduQuestAPI.requireLearner()) {
        return;
    }

    const currentUser = EduQuestAPI.getCurrentUser();

    if (!currentUser) {
        return;
    }

    // ==========================================
    // PROFILE
    // ==========================================

    const heading =
        document.getElementById("welcomeHeading");

    const label =
        document.getElementById("profileLabel");

    const avatar =
        document.getElementById("profileAvatar");

    const email = currentUser.email || "";

    const displayName = email.includes("@")
        ? email.split("@")[0]
        : email || "Learner";

    if (heading) {
        heading.textContent =
            "Welcome back, " + displayName + "!";
    }

    if (label) {
        label.textContent = "Learner";
    }

    if (avatar) {
        avatar.textContent =
            displayName.charAt(0).toUpperCase();
    }

    // ==========================================
    // LOGOUT
    // ==========================================

    const logoutLink =
        document.getElementById("logoutLink");

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
    // LEARNER STATISTICS
    // ==========================================

    // No hardcoded scores, rankings or activity.
    //
    // These sections will be connected to the
    // actual learner statistics and leaderboard
    // endpoints once their response structures
    // are confirmed.

})();
