
/* =============================================================
   EduQuest - Learner API Helper

<<<<<<< HEAD
   CHANGED:
   1. Handles incorrect login credentials correctly.
   2. Checks JWT expiration.
   3. Reads the actual user role from the JWT.
   4. Redirects learners to the learner login page.
   5. Preserves existing API methods.
   6. Supports file uploads and protected downloads.
=======
   Small helper for talking to the EduQuest backend API and for
   keeping track of the signed-in user's session (JWT token) in
   the browser. Load this script BEFORE a page's own script, e.g.:

       <script src="js/api.js" defer></script>
       <script src="js/login.js" defer></script>

   Everything is exposed on window.EduQuestAPI.
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
============================================================= */

window.EduQuestAPI = (function () {
    "use strict";

    // ==========================================
    // API CONFIGURATION
    // ==========================================

    const BASE_URL = "https://localhost:7021/api";

    const TOKEN_KEY = "eduquest_token";

    // NEW: Learner login page.
    // Resolved relative to this frontend's HTML pages.
    const LEARNER_LOGIN = "login.html";

    // ==========================================
    // TOKEN MANAGEMENT
    // ==========================================

    function saveToken(token) {
        localStorage.setItem(TOKEN_KEY, token);
    }

    function clearToken() {
        localStorage.removeItem(TOKEN_KEY);
    }

    // NEW: Decode JWT payload.
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
                            ("00" +
                                character.charCodeAt(0)
                                    .toString(16)
                            ).slice(-2);
                    })
                    .join("")
            );

<<<<<<< HEAD
            return JSON.parse(json);

        } catch (error) {

=======
            var rawId =
                claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                claims.nameid ||
                claims.sub ||
                claims.uid ||
                claims.userId;
            var userId = Number(rawId);

            /* If the id claim is missing or not numeric, treat the
               session as unreadable instead of returning NaN. */
            if (!rawId || Number.isNaN(userId)) {
                console.warn("EduQuestAPI: could not find a numeric user id in the token. Claims:", claims);
                return null;
            }

            return {
                userId: userId,
                email:
                    claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
                    claims.email,
                role:
                    claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
                    claims.role
            };
        } catch (err) {
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
            return null;
        }
    }

<<<<<<< HEAD
    // NEW: Check JWT expiration.
    function isTokenExpired(token) {

        const claims = decodeToken(token);

        if (!claims || !claims.exp) {
            return true;
=======
    /* Call this at the top of any page that requires sign-in.
       Sends visitors without a session back to login.html. */
    function requireAuth() {
        if (!getToken()) {
            window.location.href = "login.html";
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
        }

        return claims.exp <= Math.floor(
            Date.now() / 1000
        );
    }

    // CHANGED: Reject expired tokens.
    function getToken() {

        const token =
            localStorage.getItem(TOKEN_KEY);

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
        // Read the actual role from JWT claims.

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
    // Require a valid token and role.
    function requireAuth() {

        const token = getToken();
        const user = getCurrentUser();

        if (!token || !user || !user.role) {

            window.location.href = LEARNER_LOGIN;

            return false;
        }

        return true;
    }

    // NEW:
    // Only allow Learner accounts on learner pages.
    function requireLearner() {

        if (!requireAuth()) {
            return false;
        }

        const user = getCurrentUser();

        if (
            String(user.role).trim().toLowerCase()
                !== "learner"
        ) {

            // An Admin should use the Admin portal.
            window.location.href = LEARNER_LOGIN;

            return false;
        }

        return true;
    }

    // CHANGED: Learner logout.
    function logout() {

        clearToken();

        window.location.href = LEARNER_LOGIN;
    }

<<<<<<< HEAD
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

            if (messages.length > 0) {
                return messages.join(" ");
            }
        }

        // CHANGED:
        // Prefer the message from AuthController.
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
            "Make sure the backend is running at " +
            BASE_URL + "."
        );
    }

    // ==========================================
    // CHANGED: RESPONSE HANDLING
    // ==========================================

    async function handleResponse(response, options) {

        options = options || {};

        if (response.status === 401) {

            // NEW:
            // Invalid credentials during login.
            if (options.auth === false) {

                throw new Error(
                    "Incorrect email or password. " +
                    "Please try again."
                );
            }

            // Expired or invalid session.
            clearToken();

            throw new Error(
                "Your session has expired. " +
                "Please sign in again."
=======
    /* Pulls the most useful message out of an error response.
       ASP.NET validation failures put the real messages in data.errors. */
    function extractErrorMessage(data, status, sentToken) {
        if (data && typeof data === "object") {
            if (data.errors && typeof data.errors === "object") {
                var all = Object.keys(data.errors).reduce(function (acc, key) {
                    return acc.concat(data.errors[key]);
                }, []);
                if (all.length > 0) return all[0];
            }
            if (data.message) return data.message;
            if (data.title) return data.title;
        }
        if (typeof data === "string" && data) return data;

        if (status === 401 && !sentToken) return "Incorrect email or password.";
        return "Request failed (" + status + ").";
    }

    /* Core request helper.
       - Adds the JSON content-type header ONLY when there is a body
         (avoids needless CORS preflights on GET requests).
       - Adds "Authorization: Bearer <token>" automatically when signed in
         (pass { auth: false } to skip this, e.g. for login/register).
       - Throws a plain Error with a readable message on any failure,
         so callers can just try/catch and show err.message. */
    async function request(path, options) {
        options = options || {};
        var headers = {};
        var hasBody = options.body !== undefined && options.body !== null;
        var sentToken = false;

        if (hasBody) headers["Content-Type"] = "application/json";

        if (options.auth !== false) {
            var token = getToken();
            if (token) {
                headers.Authorization = "Bearer " + token;
                sentToken = true;
            }
        }

        /* Browsers block API calls from pages opened by double-clicking
           the HTML file (file://). It must be served over http(s). */
        if (window.location.protocol === "file:") {
            throw new Error(
                "This page was opened as a file. Open it through a local web server " +
                "(e.g. VS Code Live Server) instead of double-clicking it."
            );
        }

        /* Give up after 15 seconds so the page never hangs forever. */
        var controller = new AbortController();
        var timeoutId = setTimeout(function () { controller.abort(); }, 15000);

        var response;
        try {
            response = await fetch(BASE_URL + path, {
                method: options.method || "GET",
                headers: headers,
                body: hasBody ? JSON.stringify(options.body) : undefined,
                signal: controller.signal
            });
        } catch (networkErr) {
            if (networkErr && networkErr.name === "AbortError") {
                throw new Error("The server took too long to respond. Please try again.");
            }
            console.error(
                "EduQuestAPI: request to " + BASE_URL + path + " failed.\n" +
                "Page origin: " + window.location.origin + "\n" +
                "Check: (1) the API is running, (2) the https dev certificate is trusted " +
                "(open " + BASE_URL + " in the browser once), (3) this origin is in the " +
                "backend CORS list.",
                networkErr
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
            );
            throw new Error(
                "Could not reach the EduQuest server at " + BASE_URL +
                ". Make sure the API is running (see the browser console for details)."
            );
        } finally {
            clearTimeout(timeoutId);
        }

<<<<<<< HEAD
        if (response.status === 403) {

            throw new Error(
                "You do not have permission " +
                "to perform this action."
            );
=======
        /* Only an expired/invalid SESSION if we actually sent a token.
           A 401 from login (no token sent) means wrong credentials. */
        if (response.status === 401 && sentToken) {
            clearToken();
            throw new Error("Your session has expired. Please sign in again.");
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
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
<<<<<<< HEAD

            throw new Error(
                readableError(data, response.status)
            );
=======
            throw new Error(extractErrorMessage(data, response.status, sentToken));
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
        }

        return data;
    }

<<<<<<< HEAD
    // ==========================================
    // GENERAL API REQUEST
    // ==========================================

    async function request(path, options) {

        options = options || {};

        const headers = authHeaders(options);

        let body;

        // Preserve FormData uploads.
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
        // Pass request options to distinguish
        // login errors from expired sessions.
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
                "Unable to download document."
            );
        }

        return await response.blob();
    }

    // ==========================================
    // PUBLIC API METHODS
    // ==========================================

=======
    /* ---------------------------------------------------------
       Ready-made calls for every backend endpoint, so any page can
       load real data instead of hard-coded sample content, e.g.

           EduQuestAPI.grades.list().then(function (grades) { ... });
    --------------------------------------------------------- */
    var provinces = {
        list: function () { return request("/provinces", { auth: false }); },
        get: function (id) { return request("/provinces/" + id, { auth: false }); }
    };

    var grades = {
        list: function () { return request("/grades", { auth: false }); },
        get: function (id) { return request("/grades/" + id, { auth: false }); }
    };

    var subjects = {
        list: function () { return request("/subjects", { auth: false }); },
        get: function (id) { return request("/subjects/" + id, { auth: false }); }
    };

    var schools = {
        list: function (provinceId) {
            var query = provinceId ? "?provinceId=" + encodeURIComponent(provinceId) : "";
            return request("/schools" + query, { auth: false });
        },
        get: function (id) { return request("/schools/" + id, { auth: false }); }
    };

    var learners = {
        get: function (id) { return request("/learners/" + id); },
        update: function (id, body) {
            return request("/learners/" + id, { method: "PUT", body: body });
        }
    };

    var auth = {
        login: function (email, password) {
            return request("/auth/login", {
                method: "POST", body: { email: email, password: password }, auth: false
            }).then(function (result) {
                saveToken(result.token);
                return result;
            });
        },
        registerLearner: function (body) {
            return request("/auth/register-learner", {
                method: "POST", body: body, auth: false
            }).then(function (result) {
                saveToken(result.token);
                return result;
            });
        }
    };

    /* Use when putting server text into innerHTML. (textContent is safer
       and preferred; this is for template strings.) */
    function escapeHtml(value) {
        return String(value === null || value === undefined ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
    return {

        BASE_URL: BASE_URL,
<<<<<<< HEAD

=======
        provinces: provinces,
        grades: grades,
        subjects: subjects,
        schools: schools,
        learners: learners,
        auth: auth,
        escapeHtml: escapeHtml,
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
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

        // NEW: Learner-specific check.
        requireLearner: requireLearner,

        logout: logout
    };
<<<<<<< HEAD

})();
=======
})();
>>>>>>> bb7ab279774c11418209c8b834f3738e7f6d9b51
