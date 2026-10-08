/* =============================================================
   EduQuest - reset-password.js  (used by reset-password.html)
============================================================= */
(function () {
    "use strict";

    var form = document.getElementById("resetForm");
    if (!form) { return; }

    form.addEventListener("submit", function (event) {
        event.preventDefault();

        /* TODO (backend): ask the server to send the password-reset email here,
           then show a confirmation message. */
        alert("The password-reset email will be connected to the backend later.");
    });
})();
