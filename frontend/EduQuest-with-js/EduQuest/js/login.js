/* =============================================================
   EduQuest - login.js  (used by login.html)
============================================================= */
(function () {
    "use strict";

    var form = document.getElementById("loginForm");
    if (!form) { return; }

    var errorBox = document.getElementById("error");

    function showError(message) {
        if (!errorBox) { alert(message); return; }
        errorBox.textContent = message;
        errorBox.style.display = "block";
    }

    function clearError() {
        if (!errorBox) { return; }
        errorBox.style.display = "none";
    }

    /* Admins go to the admin portal, everyone else to the learner dashboard. */
    var LEARNER_HOME = "dashboard.html";
    var ADMIN_HOME = "../../pages/admin/dashboard.html";

    function homeFor(user) {
        return user && user.role === "Admin" ? ADMIN_HOME : LEARNER_HOME;
    }

    /* If someone is already signed in, skip straight to their dashboard. */
    if (window.EduQuestAPI && EduQuestAPI.getToken()) {
        window.location.href = homeFor(EduQuestAPI.getCurrentUser());
        return;
    }

    form.addEventListener("submit", function (event) {
        event.preventDefault();
        clearError();

        var email = document.getElementById("email").value.trim();
        var password = document.getElementById("password").value;
        var submitButton = form.querySelector("button[type='submit']");

        if (submitButton) {
            submitButton.disabled = true;
            submitButton.textContent = "Signing in...";
        }

        EduQuestAPI.post("/auth/login", { email: email, password: password }, { auth: false })
            .then(function (result) {
                EduQuestAPI.saveToken(result.token);
                window.location.href = homeFor(EduQuestAPI.getCurrentUser());
            })
            .catch(function (err) {
                showError(err.message || "Could not sign in. Please try again.");
            })
            .finally(function () {
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.textContent = "Sign In";
                }
            });
    });
})();