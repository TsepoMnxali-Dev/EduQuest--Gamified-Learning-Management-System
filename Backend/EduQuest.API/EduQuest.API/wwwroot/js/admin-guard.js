/* =============================================================
   EduQuest - admin-guard.js  (shared by every admin page)
   - Sends anyone who is not a signed-in Admin to the login page.
   - Makes the "Admin" button in the top bar log the user out.
   Load AFTER api.js and BEFORE the page's own script:
       <script src="../../EduQuest-with-js/EduQuest/js/api.js"></script>
       <script src="../../js/admin-guard.js"></script>
============================================================= */
(function () {
    "use strict";

    if (!window.EduQuestAPI) {
        console.error("admin-guard.js: api.js must be loaded before this script.");
        return;
    }

    /* The shared login page lives in the learner frontend. */
    var LOGIN_URL = "../../EduQuest-with-js/EduQuest/login.html";

    var me = EduQuestAPI.getCurrentUser();
    if (!me || me.role !== "Admin") {
        window.location.replace(LOGIN_URL);
        return;
    }

    function signOut() {
        /* Not EduQuestAPI.logout(): it redirects to a login.html relative to this folder. */
        EduQuestAPI.clearToken();
        window.location.href = LOGIN_URL;
    }

    function wireProfileButton() {
        var btn = document.querySelector(".profile-btn");
        if (!btn) { return; }
        btn.title = (me.email ? me.email + " - " : "") + "click to log out";
        btn.addEventListener("click", function () {
            if (window.confirm("Log out of the admin portal?")) { signOut(); }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", wireProfileButton);
    } else {
        wireProfileButton();
    }
})();
