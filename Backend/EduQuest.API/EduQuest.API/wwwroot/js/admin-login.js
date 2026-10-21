
/*
====================================================
EduQuest - Admin Login

NEW FILE:
- Authenticates admin accounts.
- Rejects non-admin accounts.
- Displays login notifications.
- Supports show/hide password.
- Redirects to admin dashboard.
====================================================
*/

(function () {
    "use strict";

    var form = document.getElementById("adminLoginForm");

    if (!form) {
        return;
    }

    var emailInput =
        document.getElementById("adminEmail");

    var passwordInput =
        document.getElementById("adminPassword");

    var messageBox =
        document.getElementById("adminLoginMessage");

    var loginButton =
        document.getElementById("adminLoginButton");

    var togglePassword =
        document.getElementById("adminTogglePassword");

    // Admin dashboard is in the same directory.
    var ADMIN_DASHBOARD = "dashboard.html";

    // ==========================================
    // NOTIFICATIONS
    // ==========================================

    function showMessage(message, type) {

        messageBox.textContent = message;

        messageBox.className =
            "admin-message " + type;

        messageBox.style.display = "block";

        messageBox.setAttribute(
            "role",
            type === "error" ? "alert" : "status"
        );
    }

    function clearMessage() {

        messageBox.textContent = "";

        messageBox.className = "admin-message";

        messageBox.style.display = "none";
    }

    // ==========================================
    // SHOW / HIDE PASSWORD
    // ==========================================

    togglePassword.addEventListener(
        "click",
        function () {

            var hidden =
                passwordInput.type === "password";

            passwordInput.type =
                hidden ? "text" : "password";

            togglePassword.textContent =
                hidden ? "Hide" : "Show";
        }
    );

    emailInput.addEventListener("input", clearMessage);
    passwordInput.addEventListener("input", clearMessage);

    // ==========================================
    // EXISTING SESSION
    // ==========================================

    if (
        window.EduQuestAPI &&
        EduQuestAPI.getToken()
    ) {

        var currentUser =
            EduQuestAPI.getCurrentUser();

        if (
            currentUser &&
            String(currentUser.role).toLowerCase()
                === "admin"
        ) {

            window.location.replace(
                ADMIN_DASHBOARD
            );

            return;
        }
    }

    // ==========================================
    // ADMIN LOGIN
    // ==========================================

    form.addEventListener(
        "submit",
        function (event) {

            event.preventDefault();

            clearMessage();

            var email = emailInput.value.trim();

            var password = passwordInput.value;

            // ==================================
            // VALIDATION
            // ==================================

            if (!email) {

                showMessage(
                    "Please enter your email address.",
                    "error"
                );

                emailInput.focus();
                return;
            }

            if (!emailInput.checkValidity()) {

                showMessage(
                    "Please enter a valid email address.",
                    "error"
                );

                emailInput.focus();
                return;
            }

            if (!password) {

                showMessage(
                    "Please enter your password.",
                    "error"
                );

                passwordInput.focus();
                return;
            }

            // ==================================
            // LOADING STATE
            // ==================================

            loginButton.disabled = true;

            loginButton.textContent =
                "Signing in...";

            // ==================================
            // BACKEND AUTHENTICATION
            // ==================================

            EduQuestAPI.post(
                "/auth/login",
                {
                    email: email,
                    password: password
                },
                {
                    auth: false
                }
            )

            .then(function (result) {

                if (
                    !result ||
                    !result.token ||
                    !result.user ||
                    !result.user.role
                ) {

                    throw new Error(
                        "Invalid login response from server."
                    );
                }

                var role = String(
                    result.user.role
                ).trim().toLowerCase();

                // ==================================
                // ADMIN ROLE VERIFICATION
                // ==================================

                if (role !== "admin") {

                    showMessage(
                        "This account does not have " +
                        "administrator access. " +
                        "Please use the learner login.",
                        "error"
                    );

                    return;
                }

                // ==================================
                // SAVE TOKEN
                // ==================================

                EduQuestAPI.saveToken(
                    result.token
                );

                // ==================================
                // LOGIN SUCCESS
                // ==================================

                showMessage(
                    "Admin login successful! Redirecting...",
                    "success"
                );

                window.location.replace(
                    ADMIN_DASHBOARD
                );
            })

            .catch(function (error) {

                var message =
                    error.message ||
                    "Unable to sign in. Please try again.";

                if (
                    /401|unauthorized|invalid email|incorrect email/i
                        .test(message)
                ) {

                    message =
                        "Incorrect email or password. " +
                        "Please try again.";
                }

                showMessage(message, "error");
            })

            .finally(function () {

                loginButton.disabled = false;

                loginButton.textContent =
                    "Sign In as Admin";
            });
        }
    );

})();
