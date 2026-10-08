/* =============================================================
   EduQuest - dashboard.js  (used by dashboard.html)
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    var currentUser = EduQuestAPI.getCurrentUser();

    if (currentUser && currentUser.email) {
        var heading = document.getElementById("welcomeHeading");
        var label = document.getElementById("profileLabel");
        var avatar = document.getElementById("profileAvatar");

        if (heading) heading.textContent = "Welcome back, " + currentUser.email + "!";
        if (label) label.textContent = currentUser.role || "Learner";
        if (avatar) avatar.textContent = currentUser.email.charAt(0).toUpperCase();
    }

    var logoutLink = document.getElementById("logoutLink");
    if (logoutLink) {
        logoutLink.addEventListener("click", function (event) {
            event.preventDefault();
            EduQuestAPI.logout();
        });
    }
})();
