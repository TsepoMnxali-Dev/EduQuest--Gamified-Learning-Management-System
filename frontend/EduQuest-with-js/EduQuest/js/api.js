/* =============================================================
   EduQuest - api.js  (shared by every page)

   Small helper for talking to the EduQuest backend API and for
   keeping track of the signed-in user's session (JWT token) in
   the browser. Load this script BEFORE a page's own script, e.g.:

       <script src="js/api.js"></script>
       <script src="js/login.js" defer></script>

   Everything is exposed on window.EduQuestAPI.
============================================================= */
window.EduQuestAPI = (function () {
    "use strict";

    /* Change this if your backend runs on a different host/port.
       This matches the "https" launch profile in launchSettings.json. */
    var BASE_URL = "https://localhost:7021/api";

    var TOKEN_KEY = "eduquest_token";

    function saveToken(token) {
        localStorage.setItem(TOKEN_KEY, token);
    }

    function getToken() {
        return localStorage.getItem(TOKEN_KEY);
    }

    function clearToken() {
        localStorage.removeItem(TOKEN_KEY);
    }

    /* Decodes the JWT payload on the client so pages can show the
       signed-in user's id/email/role without another server call.
       This does NOT verify the token - the server still does that
       on every request - it just reads the claims already inside it. */
    function getCurrentUser() {
        var token = getToken();
        if (!token) return null;

        try {
            var payload = token.split(".")[1];
            var json = decodeURIComponent(
                atob(payload.replace(/-/g, "+").replace(/_/g, "/"))
                    .split("")
                    .map(function (c) {
                        return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
                    })
                    .join("")
            );
            var claims = JSON.parse(json);

            return {
                userId: Number(
                    claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                    claims.nameid ||
                    claims.sub
                ),
                email:
                    claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
                    claims.email,
                role:
                    claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
                    claims.role
            };
        } catch (err) {
            return null;
        }
    }

    /* Call this at the top of any page that requires sign-in.
       Sends visitors without a valid-looking session back to login.html. */
    function requireAuth() {
        if (!getToken()) {
            window.location.href = "login.html";
        }
    }

    function logout() {
        clearToken();
        window.location.href = "login.html";
    }

    /* Core request helper.
       - Adds the JSON content-type header.
       - Adds "Authorization: Bearer <token>" automatically when signed in
         (pass { auth: false } to skip this, e.g. for login/register).
       - Throws a plain Error with a readable message on any failure,
         so callers can just try/catch and show err.message. */
    async function request(path, options) {
        options = options || {};
        var headers = { "Content-Type": "application/json" };

        if (options.auth !== false) {
            var token = getToken();
            if (token) headers.Authorization = "Bearer " + token;
        }

        var response;
        try {
            response = await fetch(BASE_URL + path, {
                method: options.method || "GET",
                headers: headers,
                body: options.body !== undefined ? JSON.stringify(options.body) : undefined
            });
        } catch (networkErr) {
            throw new Error(
                "Could not reach the EduQuest server. Make sure the backend API " +
                "is running at " + BASE_URL + "."
            );
        }

        if (response.status === 401) {
            clearToken();
            throw new Error("Your session has expired. Please sign in again.");
        }

        if (response.status === 204) {
            return null;
        }

        var data = null;
        var text = await response.text();
        if (text) {
            try {
                data = JSON.parse(text);
            } catch (parseErr) {
                data = text;
            }
        }

        if (!response.ok) {
            var message =
                (data && (data.title || data.message)) ||
                (typeof data === "string" && data) ||
                ("Request failed (" + response.status + ").");
            throw new Error(message);
        }

        return data;
    }

    return {
        BASE_URL: BASE_URL,
        get: function (path, options) {
            return request(path, Object.assign({ method: "GET" }, options));
        },
        post: function (path, body, options) {
            return request(path, Object.assign({ method: "POST", body: body }, options));
        },
        put: function (path, body, options) {
            return request(path, Object.assign({ method: "PUT", body: body }, options));
        },
        del: function (path, options) {
            return request(path, Object.assign({ method: "DELETE" }, options));
        },
        saveToken: saveToken,
        getToken: getToken,
        clearToken: clearToken,
        getCurrentUser: getCurrentUser,
        requireAuth: requireAuth,
        logout: logout
    };
})();
