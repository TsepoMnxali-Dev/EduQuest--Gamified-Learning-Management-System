/* =============================================================
   EduQuest - reset-password.js  (used by reset-password.html)

   Two modes, chosen from the page address:
   - No token in the address: show the "enter your email" form and
     call POST /auth/forgot-password.
   - ?email=...&token=... in the address (from the emailed link):
     show the "new password" form and call POST /auth/reset-password.
============================================================= */
(function () {
    "use strict";

    function byId(id) {
        return document.getElementById(id);
    }

    function showError(message) {
        byId("success").style.display = "none";
        var box = byId("error");
        box.textContent = message;
        box.style.display = "block";
    }

    function showSuccess(message) {
        byId("error").style.display = "none";
        var box = byId("success");
        box.textContent = message;
        box.style.display = "block";
    }

    function clearMessages() {
        byId("error").style.display = "none";
        byId("success").style.display = "none";
    }

    /* Read the link parameters, then remove them from the address bar
       so the token isn't left in the browser history. */
    var params = new URLSearchParams(window.location.search);
    var resetEmail = params.get("email");
    var resetToken = params.get("token");

    if (resetEmail && resetToken) {
        window.history.replaceState({}, document.title, window.location.pathname);
        byId("resetPanel").classList.add("active");
    } else {
        byId("requestPanel").classList.add("active");
    }

    /* Mode 1: request the reset email. */
    byId("requestForm").addEventListener("submit", function (event) {
        event.preventDefault();
        clearMessages();

        var emailInput = byId("email");
        if (!emailInput.checkValidity()) {
            emailInput.reportValidity();
            return;
        }

        var button = byId("requestSubmit");
        button.disabled = true;
        button.textContent = "Sending...";

        EduQuestAPI.post("/auth/forgot-password", {
            email: emailInput.value.trim()
        }, { auth: false })
            .then(function (result) {
                showSuccess(
                    (result && result.message) ||
                    "If an account exists for that email, a password reset link has been sent."
                );
                button.textContent = "Link Sent";
            })
            .catch(function (err) {
                showError(err.message || "Could not send the reset link. Please try again.");
                button.disabled = false;
                button.textContent = "Send Reset Link";
            });
    });

    /* Mode 2: save the new password. */
    byId("resetForm").addEventListener("submit", function (event) {
        event.preventDefault();
        clearMessages();

        var newPassword = byId("newPassword").value;
        var confirmPassword = byId("confirmPassword").value;

        if (newPassword.length < 8) {
            showError("Your password must be at least 8 characters.");
            return;
        }
        if (newPassword !== confirmPassword) {
            showError("Passwords do not match.");
            return;
        }

        var button = byId("resetSubmit");
        button.disabled = true;
        button.textContent = "Saving...";

        EduQuestAPI.post("/auth/reset-password", {
            email: resetEmail,
            token: resetToken,
            newPassword: newPassword
        }, { auth: false })
            .then(function (result) {
                showSuccess(
                    ((result && result.message) || "Your password has been reset.") +
                    " Taking you to sign in..."
                );
                button.textContent = "Done";
                setTimeout(function () {
                    window.location.href = "login.html";
                }, 2000);
            })
            .catch(function (err) {
                showError(err.message || "Could not reset your password. Please request a new link.");
                button.disabled = false;
                button.textContent = "Reset Password";
            });
    });
})();