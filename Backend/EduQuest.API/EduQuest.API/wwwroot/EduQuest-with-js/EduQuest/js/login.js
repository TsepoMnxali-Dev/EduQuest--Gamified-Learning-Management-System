/*
========================================================
EduQuest - Login

CHANGED:
1. Saves both JWT and user profile information.
2. Displays the learner's real name on the dashboard.
3. Redirects Admins and Learners correctly.
4. Preserves login validation and error messages.
5. Preserves show/hide password.
6. Prevents repeated login submissions.
7. Preserves existing authenticated sessions.
========================================================
*/

(function () {
    "use strict";

    // ==========================================
    // HTML ELEMENTS
    // ==========================================

    const form = document.getElementById("loginForm");

    if (!form) {
        return;
    }

    const emailInput =
        document.getElementById("email");

    const passwordInput =
        document.getElementById("password");

    const messageBox =
        document.getElementById("loginMessage") ||
        document.getElementById("error");

    const togglePassword =
        document.getElementById("togglePassword");

    const submitButton =
        document.getElementById("loginButton") ||
        form.querySelector("button[type='submit']");

    // ==========================================
    // API CHECK
    // ==========================================

    if (!window.EduQuestAPI) {
        console.error("EduQuestAPI is missing.");

        if (messageBox) {
            messageBox.textContent =
                "Unable to load login services.";
            messageBox.style.display = "block";
        }

        return;
    }

    // ==========================================
    // DASHBOARD PATHS
    // ==========================================

    const ADMIN_DASHBOARD =
        "../../pages/admin/dashboard.html";

    const LEARNER_DASHBOARD =
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

    async function redirectUser(role) {
        const normalizedRole = String(role || "").trim().toLowerCase();
        if (normalizedRole === "admin") {
            window.location.replace(ADMIN_DASHBOARD);
            return true;
        }
        if (normalizedRole === "learner") {
            if (window.EduQuestProfileGate && typeof EduQuestProfileGate.redirectAfterLogin === "function") {
                await EduQuestProfileGate.redirectAfterLogin();
            } else {
                window.location.replace(LEARNER_DASHBOARD);
            }
            return true;
        }
        return false;
    }

    // ==========================================
    // SHOW / HIDE PASSWORD
    // ==========================================

    if (togglePassword && passwordInput) {
        togglePassword.addEventListener(
            "click",
            function () {
                const isHidden =
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

    if (emailInput) {
        emailInput.addEventListener(
            "input",
            clearMessage
        );
    }

    if (passwordInput) {
        passwordInput.addEventListener(
            "input",
            clearMessage
        );
    }

    // ==========================================
    // EXISTING LOGIN SESSION
    // ==========================================

    if (EduQuestAPI.getToken()) {
        const currentUser = EduQuestAPI.getCurrentUser();
        if (currentUser && ["admin", "learner"].includes(String(currentUser.role).toLowerCase())) {
            redirectUser(currentUser.role).catch(function (error) {
                showMessage(error.message || "Unable to verify your profile.", "error");
            });
        } else {
            EduQuestAPI.clearToken();
        }
    }

    // ==========================================
    // LOGIN FORM
    // ==========================================

    let isSubmitting = false;

    form.addEventListener(
        "submit",
        async function (event) {
            event.preventDefault();

            if (isSubmitting) {
                return;
            }

            clearMessage();

            const email =
                emailInput.value.trim();

            const password =
                passwordInput.value;

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

            isSubmitting = true;

            const originalButtonText =
                submitButton
                    ? submitButton.textContent
                    : "Sign In";

            if (submitButton) {
                submitButton.disabled = true;
                submitButton.textContent =
                    "Signing in...";
            }

            // ==================================
            // BACKEND LOGIN REQUEST
            // ==================================

            try {
                const result =
                    await EduQuestAPI.post(
                        "/auth/login",
                        {
                            email: email,
                            password: password
                        },
                        {
                            auth: false
                        }
                    );

                // ==================================
                // VALIDATE BACKEND RESPONSE
                // ==================================

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

                const role = String(
                    result.user.role
                ).trim().toLowerCase();

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

                EduQuestAPI.saveToken(
                    result.token,
                    result.user
                );

                // ==================================
                // VERIFY SAVED SESSION
                // ==================================

                const currentUser =
                    EduQuestAPI.getCurrentUser();

                if (!currentUser) {
                    throw new Error(
                        "Unable to establish your login session."
                    );
                }

                if (
                    String(currentUser.role)
                        .trim()
                        .toLowerCase() !== role
                ) {
                    EduQuestAPI.clearToken();

                    throw new Error(
                        "Authentication role mismatch."
                    );
                }

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

                await redirectUser(role);

            } catch (error) {
                let message =
                    error.message ||
                    "Could not sign in. Please try again.";

                if (
                    /unauthorized|invalid email|incorrect email|401/i
                        .test(message)
                ) {
                    message =
                        "Incorrect email or password. " +
                        "Please try again.";
                }

                showMessage(message, "error");

            } finally {
                isSubmitting = false;

                if (submitButton) {
                    submitButton.disabled = false;

                    submitButton.textContent =
                        originalButtonText;
                }
            }
        }
    );

})();