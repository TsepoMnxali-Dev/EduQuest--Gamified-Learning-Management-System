
/* =============================================================
   EduQuest - Shared API Helper

   CHANGED:
   1. Saves JWT and authenticated user information.
   2. Returns the learner's first and last names.
   3. Preserves JWT expiration checks.
   4. Preserves Admin and Learner role protection.
   5. Preserves API requests and file downloads.
   6. Clears user information on logout.
============================================================= */

window.EduQuestAPI = (function () {
    "use strict";

    // ==========================================
    // CONFIGURATION
    // ==========================================

    const BASE_URL = "https://localhost:7021/api";

    const TOKEN_KEY = "eduquest_token";

    // NEW: Storage key for authenticated user details.
    const USER_KEY = "eduquest_user";

    // Capture script location.
    const scriptUrl = document.currentScript.src;

    const HOME_PAGE = new URL(
        "../index.html",
        scriptUrl
    ).href;

    const LOGIN_PAGE = new URL(
        "../login.html",
        scriptUrl
    ).href;

    const LEARNER_DASHBOARD = new URL(
        "../dashboard.html",
        scriptUrl
    ).href;

    const ADMIN_DASHBOARD = new URL(
        "../../../pages/admin/dashboard.html",
        scriptUrl
    ).href;

    // ==========================================
    // TOKEN MANAGEMENT
    // ==========================================

    // CHANGED:
    // Saves the token and optional user details
    // returned by the backend.
    function saveToken(token, user) {
        if (!token || typeof token !== "string") {
            throw new Error("Invalid authentication token.");
        }

        localStorage.setItem(TOKEN_KEY, token);

        if (user) {
            const claims = decodeToken(token);

            const tokenUserId = claims
                ? getUserIdFromClaims(claims)
                : null;

            // NEW: Only associate profile details
            // with the user identified by the JWT.
            if (
                tokenUserId !== null &&
                Number(user.userId) === tokenUserId
            ) {
                const profile = {
                    userId: tokenUserId,
                    firstName: user.firstName || "",
                    lastName: user.lastName || "",
                    email: user.email || ""
                };

                localStorage.setItem(
                    USER_KEY,
                    JSON.stringify(profile)
                );
            } else {
                localStorage.removeItem(USER_KEY);
            }
        } else {
            localStorage.removeItem(USER_KEY);
        }
    }

    // CHANGED:
    // Clear both token and saved profile.
    function clearToken() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
    }

    // ==========================================
    // JWT DECODING
    // ==========================================

    function decodeToken(token) {
        if (!token) {
            return null;
        }

        try {
            const parts = token.split(".");

            if (parts.length !== 3) {
                return null;
            }

            const base64 = parts[1]
                .replace(/-/g, "+")
                .replace(/_/g, "/");

            const json = decodeURIComponent(
                atob(base64)
                    .split("")
                    .map(function (character) {
                        return "%" +
                            (
                                "00" +
                                character.charCodeAt(0)
                                    .toString(16)
                            ).slice(-2);
                    })
                    .join("")
            );

            return JSON.parse(json);
        } catch (error) {
            return null;
        }
    }

    // ==========================================
    // USER ID FROM JWT
    // ==========================================

    // NEW: Reusable helper for reading
    // the user identifier from JWT claims.
    function getUserIdFromClaims(claims) {
        if (!claims) {
            return null;
        }

        const rawId =
            claims[
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            ] ||
            claims.nameid ||
            claims.sub ||
            claims.userId;

        const userId = Number(rawId);

        if (
            rawId === undefined ||
            rawId === null ||
            rawId === "" ||
            !Number.isInteger(userId) ||
            userId <= 0
        ) {
            return null;
        }

        return userId;
    }

    // ==========================================
    // GET VALID TOKEN
    // ==========================================

    function getToken() {
        const token = localStorage.getItem(TOKEN_KEY);

        if (!token) {
            return null;
        }

        const claims = decodeToken(token);

        if (
            !claims ||
            !claims.exp ||
            claims.exp <= Math.floor(Date.now() / 1000)
        ) {
            clearToken();
            return null;
        }

        return token;
    }

    // ==========================================
    // CURRENT AUTHENTICATED USER
    // ==========================================

    // CHANGED:
    // Identity and role come from JWT claims.
    // Names come from the saved backend response.
    function getCurrentUser() {
        const token = getToken();

        if (!token) {
            return null;
        }

        const claims = decodeToken(token);

        if (!claims) {
            return null;
        }

        const userId = getUserIdFromClaims(claims);

        if (userId === null) {
            return null;
        }

        // NEW: Retrieve saved profile details.
        let savedUser = {};

        try {
            savedUser = JSON.parse(
                localStorage.getItem(USER_KEY) || "{}"
            ) || {};
        } catch (error) {
            savedUser = {};
        }

        // NEW: Do not use profile information
        // belonging to a different user.
        const matchingProfile =
            Number(savedUser.userId) === userId
                ? savedUser
                : {};

        const email =
            claims[
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
            ] ||
            claims.email ||
            "";

        const role =
            claims[
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ] ||
            claims.role ||
            "";

        return {
            userId: userId,

            // NEW: Real learner/admin names.
            firstName: matchingProfile.firstName || "",
            lastName: matchingProfile.lastName || "",

            email: email,
            role: role
        };
    }

    // ==========================================
    // AUTHENTICATION
    // ==========================================

    function requireAuth() {
        if (!getCurrentUser()) {
            window.location.replace(LOGIN_PAGE);
            return false;
        }

        return true;
    }

    function requireLearner() {
        if (!requireAuth()) {
            return false;
        }

        const user = getCurrentUser();

        if (
            String(user.role).trim().toLowerCase() !==
            "learner"
        ) {
            window.location.replace(ADMIN_DASHBOARD);
            return false;
        }

        return true;
    }

    function requireAdmin() {
        if (!requireAuth()) {
            return false;
        }

        const user = getCurrentUser();

        if (
            String(user.role).trim().toLowerCase() !==
            "admin"
        ) {
            window.location.replace(LEARNER_DASHBOARD);
            return false;
        }

        return true;
    }

    // ==========================================
    // LOGOUT
    // ==========================================

    // CHANGED:
    // Remove token and profile information.
    function logout() {
        clearToken();
        window.location.replace(HOME_PAGE);
    }

    // ==========================================
    // REQUEST HEADERS
    // ==========================================

    function authHeaders(options) {
        options = options || {};

        const headers = {};

        if (options.auth !== false) {
            const token = getToken();

            if (token) {
                headers.Authorization =
                    "Bearer " + token;
            }
        }

        return headers;
    }

    // ==========================================
    // ERROR HANDLING
    // ==========================================

    function readableError(data, status) {
        if (data && data.errors) {
            const messages = [];

            Object.keys(data.errors).forEach(
                function (key) {
                    [].concat(data.errors[key]).forEach(
                        function (message) {
                            messages.push(message);
                        }
                    );
                }
            );

            if (messages.length) {
                return messages.join(" ");
            }
        }

        return (
            (data && (data.message || data.title)) ||
            (typeof data === "string" && data) ||
            "Request failed (" + status + ")."
        );
    }

    function networkError() {
        return new Error(
            "Could not reach the EduQuest server. " +
            "Make sure the backend API is running at " +
            BASE_URL + "."
        );
    }

    // ==========================================
    // API REQUESTS
    // ==========================================

    async function request(path, options) {
        options = options || {};

        const headers = authHeaders(options);

        const isForm =
            typeof FormData !== "undefined" &&
            options.body instanceof FormData;

        let body;

        if (isForm) {
            body = options.body;
        } else {
            headers["Content-Type"] =
                "application/json";

            body = options.body !== undefined
                ? JSON.stringify(options.body)
                : undefined;
        }

        let response;

        try {
            response = await fetch(BASE_URL + path, {
                method: options.method || "GET",
                headers: headers,
                body: body
            });
        } catch (error) {
            throw networkError();
        }

        // ======================================
        // UNAUTHORIZED
        // ======================================

        if (response.status === 401) {
            if (options.auth === false) {
                throw new Error(
                    "Incorrect email or password. Please try again."
                );
            }

            clearToken();

            throw new Error(
                "Your session has expired. Please sign in again."
            );
        }

        // ======================================
        // FORBIDDEN
        // ======================================

        if (response.status === 403) {
            throw new Error(
                "You do not have permission to perform this action."
            );
        }

        // ======================================
        // NO CONTENT
        // ======================================

        if (response.status === 204) {
            return null;
        }

        // ======================================
        // READ RESPONSE
        // ======================================

        const responseText = await response.text();

        let data = null;

        if (responseText) {
            try {
                data = JSON.parse(responseText);
            } catch (error) {
                data = responseText;
            }
        }

        if (!response.ok) {
            throw new Error(
                readableError(data, response.status)
            );
        }

        return data;
    }

    // ==========================================
    // PROTECTED FILE DOWNLOADS
    // ==========================================

    async function getBlob(path, options) {
        options = options || {};

        let response;

        try {
            response = await fetch(BASE_URL + path, {
                headers: authHeaders(options)
            });
        } catch (error) {
            throw networkError();
        }

        if (response.status === 401) {
            clearToken();

            throw new Error(
                "Your session has expired. Please sign in again."
            );
        }

        if (response.status === 403) {
            throw new Error(
                "You do not have permission to download this file."
            );
        }

        if (!response.ok) {
            const responseText = await response.text();

            let data = responseText;

            try {
                data = JSON.parse(responseText);
            } catch (error) {
                // Keep original response text.
            }

            throw new Error(
                readableError(data, response.status)
            );
        }

        return response.blob();
    }

    // ==========================================
    // PUBLIC API
    // ==========================================

    return {
        BASE_URL: BASE_URL,

        get: function (path, options) {
            return request(
                path,
                Object.assign(
                    { method: "GET" },
                    options
                )
            );
        },

        post: function (path, body, options) {
            return request(
                path,
                Object.assign(
                    {
                        method: "POST",
                        body: body
                    },
                    options
                )
            );
        },

        put: function (path, body, options) {
            return request(
                path,
                Object.assign(
                    {
                        method: "PUT",
                        body: body
                    },
                    options
                )
            );
        },

        del: function (path, options) {
            return request(
                path,
                Object.assign(
                    { method: "DELETE" },
                    options
                )
            );
        },

        getBlob: getBlob,

        // TOKEN MANAGEMENT
        saveToken: saveToken,
        getToken: getToken,
        clearToken: clearToken,

        // CURRENT USER
        getCurrentUser: getCurrentUser,

        // AUTHENTICATION
        requireAuth: requireAuth,
        requireAdmin: requireAdmin,
        requireLearner: requireLearner,

        // LOGOUT
        logout: logout
    };

})();
