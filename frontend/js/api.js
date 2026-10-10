
/* =============================================================
   EduQuest - Shared API Helper

   Used by the Admin frontend.

   CHANGED:
   1. Correctly handles incorrect login credentials.
   2. Checks whether JWT tokens have expired.
   3. Supports Admin and Learner role information.
   4. Improves login and logout redirects.
   5. Preserves existing API methods.
   6. Keeps FormData uploads and protected downloads.
============================================================= */

window.EduQuestAPI = (function () {
    "use strict";

    // ==========================================
    // API CONFIGURATION
    // ==========================================

    const BASE_URL = "https://localhost:7021/api";

    // Keep the existing token key.
    const TOKEN_KEY = "eduquest_token";

    // NEW: Admin login page.
    // Paths are resolved from the frontend root.
    const ADMIN_LOGIN = "/pages/admin/login.html";

    // ==========================================
    // TOKEN MANAGEMENT
    // ==========================================

    function saveToken(token) {
        localStorage.setItem(TOKEN_KEY, token);
    }

    function clearToken() {
        localStorage.removeItem(TOKEN_KEY);
    }

    // NEW: Decode the JWT payload.
    function decodeToken(token) {

        if (!token) {
            return null;
        }

        try {
            const payload = token.split(".")[1];

            const base64 = payload
                .replace(/-/g, "+")
                .replace(/_/g, "/");

            const json = decodeURIComponent(
                atob(base64)
                    .split("")
                    .map(function (character) {
                        return "%" +
                            ("00" + character
                                .charCodeAt(0)
                                .toString(16)).slice(-2);
                    })
                    .join("")
            );

            return JSON.parse(json);

        } catch (error) {
            return null;
        }
    }

    // NEW: Check whether a token has expired.
    function isTokenExpired(token) {

        const claims = decodeToken(token);

        if (!claims || !claims.exp) {
            return true;
        }

        const currentTime = Math.floor(
            Date.now() / 1000
        );

        return claims.exp <= currentTime;
    }

    // CHANGED: Do not return expired tokens.
    function getToken() {

        const token = localStorage.getItem(TOKEN_KEY);

        if (!token) {
            return null;
        }

        if (isTokenExpired(token)) {
            clearToken();
            return null;
        }

        return token;
    }

    // ==========================================
    // CURRENT LOGGED-IN USER
    // ==========================================

    function getCurrentUser() {

        const token = getToken();

        if (!token) {
            return null;
        }

        const claims = decodeToken(token);

        if (!claims) {
            return null;
        }

        // CHANGED:
        // Read the real role from the JWT.
        // Never automatically assume Learner.

        const role =
            claims[
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ] ||
            claims.role ||
            claims.roles;

        return {
            userId: Number(
                claims[
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                ] ||
                claims.nameid ||
                claims.sub
            ),

            email:
                claims[
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
                ] ||
                claims.email,

            role: role
        };
    }

    // ==========================================
    // AUTHENTICATION
    // ==========================================

    // CHANGED:
    // Require a valid token and user role.
    function requireAuth() {

        const token = getToken();
        const user = getCurrentUser();

        if (!token || !user || !user.role) {

            window.location.href = ADMIN_LOGIN;

            return false;
        }

        return true;
    }

    // NEW:
    // Check whether the current user is an Admin.
    function requireAdmin() {

        if (!requireAuth()) {
            return false;
        }

        const user = getCurrentUser();

        if (
            String(user.role).trim().toLowerCase()
                !== "admin"
        ) {

            window.location.href = ADMIN_LOGIN;

            return false;
        }

        return true;
    }

    // CHANGED: Logout goes to Admin login.
    function logout() {

        clearToken();

        window.location.href = ADMIN_LOGIN;
    }

    // ==========================================
    // AUTHORIZATION HEADERS
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
    // READABLE ERROR MESSAGES
    // ==========================================

    function readableError(data, status) {

        // ASP.NET Core validation errors.
        if (data && data.errors) {

            const messages = [];

            Object.keys(data.errors).forEach(
                function (key) {

                    const errors = data.errors[key];

                    [].concat(errors).forEach(
                        function (message) {

                            messages.push(message);
                        }
                    );
                }
            );

            if (messages.length > 0) {
                return messages.join(" ");
            }
        }

        // CHANGED:
        // Prefer backend notification messages.
        if (data && data.message) {
            return data.message;
        }

        if (data && data.title) {
            return data.title;
        }

        if (typeof data === "string" && data) {
            return data;
        }

        return "Request failed (" + status + ").";
    }

    function networkError() {

        return new Error(
            "Could not reach the EduQuest server. " +
            "Make sure the backend API is running at " +
            BASE_URL + "."
        );
    }

    // ==========================================
    // CHANGED: RESPONSE HANDLING
    // ==========================================

    async function handleResponse(response, options) {

        options = options || {};

        // NEW:
        // A 401 during login means incorrect
        // credentials, not an expired session.
        if (response.status === 401) {

            if (options.auth === false) {

                throw new Error(
                    "Incorrect email or password. " +
                    "Please try again."
                );
            }

            // Protected request:
            // Existing token is invalid or expired.
            clearToken();

            throw new Error(
                "Your session has expired. " +
                "Please sign in again."
            );
        }

        if (response.status === 403) {

            throw new Error(
                "You do not have permission " +
                "to perform this action."
            );
        }

        if (response.status === 204) {
            return null;
        }

        const text = await response.text();

        let data = null;

        if (text) {

            try {
                data = JSON.parse(text);
            } catch (error) {
                data = text;
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
    // GENERAL API REQUEST
    // ==========================================

    async function request(path, options) {

        options = options || {};

        const headers = authHeaders(options);

        let body;

        // Preserve multipart file uploads.
        const isFormData =
            typeof FormData !== "undefined" &&
            options.body instanceof FormData;

        if (isFormData) {

            body = options.body;

        } else {

            headers["Content-Type"] =
                "application/json";

            if (options.body !== undefined) {

                body = JSON.stringify(
                    options.body
                );
            }
        }

        let response;

        try {

            response = await fetch(
                BASE_URL + path,
                {
                    method: options.method || "GET",
                    headers: headers,
                    body: body
                }
            );

        } catch (error) {

            throw networkError();
        }

        // CHANGED:
        // Pass request options so login errors
        // are handled differently.
        return await handleResponse(
            response,
            options
        );
    }

    // ==========================================
    // PROTECTED FILE DOWNLOADS
    // ==========================================

    async function getBlob(path, options) {

        options = options || {};

        let response;

        try {

            response = await fetch(
                BASE_URL + path,
                {
                    method: "GET",
                    headers: authHeaders(options)
                }
            );

        } catch (error) {

            throw networkError();
        }

        if (response.status === 401) {

            clearToken();

            throw new Error(
                "Your session has expired. " +
                "Please sign in again."
            );
        }

        // NEW: Handle forbidden downloads.
        if (response.status === 403) {

            throw new Error(
                "You do not have permission " +
                "to access this document."
            );
        }

        if (!response.ok) {

            const text = await response.text();

            throw new Error(
                text ||
                "Download failed (" +
                response.status + ")."
            );
        }

        return await response.blob();
    }

    // ==========================================
    // PUBLIC API METHODS
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

        saveToken: saveToken,
        getToken: getToken,
        clearToken: clearToken,

        getCurrentUser: getCurrentUser,

        requireAuth: requireAuth,

        // NEW: Admin-specific access check.
        requireAdmin: requireAdmin,

        logout: logout
    };

})();
