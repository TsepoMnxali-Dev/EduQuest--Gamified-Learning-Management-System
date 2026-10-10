
/*
========================================================
EduQuest - Login

CHANGED:
- Admins go to the admin dashboard.
- Learners go to the learner dashboard.
- Incorrect login details show an error message.
- Supports show/hide password.
- Prevents repeated login submissions.
- Uses the role returned by the backend.
========================================================
*/

(function () {
    "use strict";

    // ==========================================
    // HTML ELEMENTS
    // ==========================================

    var form = document.getElementById("loginForm");

    if (!form) {
        return;
    }

    var emailInput = document.getElementById("email");
    var passwordInput = document.getElementById("password");

    // NEW: Supports the updated login HTML.
    var messageBox =
        document.getElementById("loginMessage") ||
        document.getElementById("error");

    var togglePassword =
        document.getElementById("togglePassword");

    var submitButton =
        document.getElementById("loginButton") ||
        form.querySelector("button[type='submit']");

    // ==========================================
    // DASHBOARD PATHS
    // ==========================================

    var ADMIN_DASHBOARD =
        "../../pages/admin/dashboard.html";

    var LEARNER_DASHBOARD =
        "dashboard.html";

    // ==========================================
    // NOTIFICATIONS
    // ==========================================

    function showMessage(message, type) {

        if (!messageBox) {
            return;
        }

        messageBox.textContent = message;

        messageBox.className =
            "login-message " + type;

        messageBox.style.display = "block";

        messageBox.setAttribute(
            "role",
            type === "error" ? "alert" : "status"
        );
    }

    function clearMessage() {

        if (!messageBox) {
            return;
        }

        messageBox.textContent = "";
        messageBox.className = "login-message";
        messageBox.style.display = "none";
    }

    // ==========================================
    // ROLE-BASED REDIRECTION
    // ==========================================

    function redirectUser(role) {

        var normalizedRole =
            String(role || "").trim().toLowerCase();

        if (normalizedRole === "admin") {

            window.location.replace(
                ADMIN_DASHBOARD
            );

            return true;
        }

        if (normalizedRole === "learner") {

            window.location.replace(
                LEARNER_DASHBOARD
            );

            return true;
        }

        return false;
    }

    // ==========================================
    // SHOW / HIDE PASSWORD
    // ==========================================

    if (togglePassword) {

        togglePassword.addEventListener(
            "click",
            function () {

                var isHidden =
                    passwordInput.type === "password";

                passwordInput.type =
                    isHidden ? "text" : "password";

                togglePassword.textContent =
                    isHidden ? "Hide" : "Show";

                togglePassword.setAttribute(
                    "aria-label",
                    isHidden
                        ? "Hide password"
                        : "Show password"
                );
            }
        );
    }

    // ==========================================
    // CLEAR ERRORS WHEN TYPING
    // ==========================================

    emailInput.addEventListener(
        "input",
        clearMessage
    );

    passwordInput.addEventListener(
        "input",
        clearMessage
    );

    // ==========================================
    // CHANGED: EXISTING LOGIN SESSION
    // ==========================================

    // Previously, every logged-in user was
    // redirected to the learner dashboard.
    //
    // Now we check their role first.

    if (
        window.EduQuestAPI &&
        EduQuestAPI.getToken()
    ) {

        var currentUser =
            EduQuestAPI.getCurrentUser();

        if (
            currentUser &&
            redirectUser(currentUser.role)
        ) {
            return;
        }

        // If a token exists but the role cannot
        // be determined, clear the invalid session.
        // The user can then sign in again.

        if (EduQuestAPI.clearToken) {
            EduQuestAPI.clearToken();
        }
    }

    // ==========================================
    // LOGIN FORM
    // ==========================================

    form.addEventListener(
        "submit",
        function (event) {

            event.preventDefault();

            clearMessage();

            var email = emailInput.value.trim();
            var password = passwordInput.value;

            // ==================================
            // INPUT VALIDATION
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

            submitButton.disabled = true;
            submitButton.textContent = "Signing in...";

            // ==================================
            // BACKEND LOGIN REQUEST
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

                // Check backend response.
                if (
                    !result ||
                    !result.token ||
                    !result.user ||
                    !result.user.role
                ) {

                    throw new Error(
                        "Login response is incomplete."
                    );
                }

                var role = String(
                    result.user.role
                ).trim().toLowerCase();

                // Reject unsupported roles.
                if (
                    role !== "admin" &&
                    role !== "learner"
                ) {

                    throw new Error(
                        "Your account role is not supported."
                    );
                }

                // ==================================
                // SAVE AUTHENTICATION
                // ==================================

                // Keep using the existing API helper
                // so token storage stays consistent.

                EduQuestAPI.saveToken(result.token);

                // IMPORTANT:
                // We will verify getCurrentUser()
                // in api.js next. It should read
                // the actual role from the JWT.

                // ==================================
                // SUCCESS NOTIFICATION
                // ==================================

                showMessage(
                    "Login successful! Redirecting...",
                    "success"
                );

                // ==================================
                // REDIRECT BY ROLE
                // ==================================

                redirectUser(role);
            })

            .catch(function (error) {

                var message =
                    error.message ||
                    "Could not sign in. Please try again.";

                // Use a consistent message for
                // invalid login credentials.
                if (
                    /unauthorized|invalid email|incorrect email|401/i
                        .test(message)
                ) {

                    message =
                        "Incorrect email or password. " +
                        "Please try again.";
                }

                showMessage(message, "error");
            })

            .finally(function () {

                submitButton.disabled = false;
                submitButton.textContent = "Sign In";
            });
        }
    );

})();
