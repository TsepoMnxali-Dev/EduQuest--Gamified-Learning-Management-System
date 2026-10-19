
/* =============================================================
   EduQuest - Shared Admin API Helper

   CHANGED:
   - Logout returns to the public EduQuest homepage.
   - Unauthenticated users go to the unified login.
   - Supports Admin and Learner role checks.
   - Checks JWT expiry.
   - Preserves JSON requests, FormData and file downloads.
   - Handles incorrect login credentials separately.
============================================================= */

window.EduQuestAPI = (function () {
    "use strict";

    const BASE_URL = (() => {
    // Optional override for servers where frontend and API
    // are hosted at different addresses.
    const configuredUrl = window.EDUQUEST_API_BASE_URL;

    if (
        typeof configuredUrl === "string" &&
        configuredUrl.trim()
    ) {
        const url = new URL(configuredUrl.trim(), window.location.href);

        if (!["http:", "https:"].includes(url.protocol)) {
            throw new Error("Invalid EduQuest API URL.");
        }

        return url.href.replace(/\/+$/, "");
    }

    // Preserve your existing local Visual Studio setup.
    const isLocal =
        window.location.hostname === "localhost" ||
        window.location.hostname === "127.0.0.1";

    if (isLocal) {
        return "https://localhost:7021/api";
    }

    // Default production setup:
    // frontend and backend hosted under the same domain.
    return window.location.origin + "/api";
})();
    const TOKEN_KEY = "eduquest_token";

    // NEW: Resolve paths relative to this script,
    // not the currently opened HTML page.
    const scriptUrl = document.currentScript.src;

    const HOME_PAGE = new URL(
        "../EduQuest-with-js/EduQuest/index.html",
        scriptUrl
    ).href;

    const LOGIN_PAGE = new URL(
        "../EduQuest-with-js/EduQuest/login.html",
        scriptUrl
    ).href;

    const ADMIN_DASHBOARD = new URL(
        "../pages/admin/dashboard.html",
        scriptUrl
    ).href;

    const LEARNER_DASHBOARD = new URL(
        "../EduQuest-with-js/EduQuest/dashboard.html",
        scriptUrl
    ).href;

    // ==========================================
    // TOKEN MANAGEMENT
    // ==========================================

    function saveToken(token) {
        localStorage.setItem(TOKEN_KEY, token);
    }

    function clearToken() {
        localStorage.removeItem(TOKEN_KEY);
    }

    function decodeToken(token) {
        if (!token) return null;

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
                            ("00" +
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

    function getToken() {
        const token = localStorage.getItem(TOKEN_KEY);

        if (!token) return null;

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
    // CURRENT USER
    // ==========================================

    function getCurrentUser() {
        const token = getToken();

        if (!token) return null;

        const claims = decodeToken(token);

        if (!claims) return null;

        const rawId =
            claims[
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            ] ||
            claims.nameid ||
            claims.sub ||
            claims.userId;

        const userId = Number(rawId);

        if (!Number.isFinite(userId)) {
            return null;
        }

        return {
            userId: userId,

            email:
                claims[
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
                ] ||
                claims.email ||
                "",

            role:
                claims[
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                ] ||
                claims.role ||
                ""
        };
    }

    // ==========================================
    // AUTHENTICATION
    // ==========================================

    function requireAuth() {
        const user = getCurrentUser();

        if (!user) {
            window.location.replace(LOGIN_PAGE);
            return false;
        }

        return true;
    }

    function requireAdmin() {
        if (!requireAuth()) return false;

        const user = getCurrentUser();

        if (
            String(user.role).trim().toLowerCase() !== "admin"
        ) {
            window.location.replace(LEARNER_DASHBOARD);
            return false;
        }

        return true;
    }

    function requireLearner() {
        if (!requireAuth()) return false;

        const user = getCurrentUser();

        if (
            String(user.role).trim().toLowerCase() !== "learner"
        ) {
            window.location.replace(ADMIN_DASHBOARD);
            return false;
        }

        return true;
    }

    // CHANGED: Logout returns to the homepage.
    function logout() {
        clearToken();
        window.location.replace(HOME_PAGE);
    }

    // ==========================================
    // API REQUEST HEADERS
    // ==========================================

    function authHeaders(options) {
        const headers = {};

        if (options.auth !== false) {
            const token = getToken();

            if (token) {
                headers.Authorization = "Bearer " + token;
            }
        }

        return headers;
    }

    function readableError(data, status) {
        if (data && data.errors) {
            const messages = [];

            Object.keys(data.errors).forEach(function (key) {
                [].concat(data.errors[key]).forEach(function (m) {
                    messages.push(m);
                });
            });

            if (messages.length) {
                return messages.join(" ");
            }
        }

        return (
            (data && (data.title || data.message)) ||
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
    // JSON / FILE UPLOAD REQUESTS
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
            headers["Content-Type"] = "application/json";

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

        // CHANGED: A failed login is not an expired session.
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

        if (response.status === 403) {
            throw new Error(
                "You do not have permission to perform this action."
            );
        }

        if (response.status === 204) {
            return null;
        }

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
            const text = await response.text();

            throw new Error(
                text ||
                "Download failed (" + response.status + ")."
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
                Object.assign({ method: "GET" }, options)
            );
        },

        post: function (path, body, options) {
            return request(
                path,
                Object.assign(
                    { method: "POST", body: body },
                    options
                )
            );
        },

        put: function (path, body, options) {
            return request(
                path,
                Object.assign(
                    { method: "PUT", body: body },
                    options
                )
            );
        },

        del: function (path, options) {
            return request(
                path,
                Object.assign({ method: "DELETE" }, options)
            );
        },

        getBlob: getBlob,

        saveToken: saveToken,
        getToken: getToken,
        clearToken: clearToken,
        getCurrentUser: getCurrentUser,

        requireAuth: requireAuth,
        requireAdmin: requireAdmin,
        requireLearner: requireLearner,

        logout: logout
    };
})();
