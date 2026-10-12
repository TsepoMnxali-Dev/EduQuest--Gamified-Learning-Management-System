/* =============================================================
   EduQuest - api.js  (shared by every page)

   Small helper for talking to the EduQuest backend API and for
   keeping track of the signed-in user's session (JWT token) in
   the browser. Load this script BEFORE a page's own script, e.g.:

       <script src="js/api.js" defer></script>
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
            return null;
        }
    }

    /* Call this at the top of any page that requires sign-in.
       Sends visitors without a session back to login.html. */
    function requireAuth() {
        if (!getToken()) {
            window.location.href = "login.html";
        }
    }

    function logout() {
        clearToken();
        window.location.href = "login.html";
    }

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
            );
            throw new Error(
                "Could not reach the EduQuest server at " + BASE_URL +
                ". Make sure the API is running (see the browser console for details)."
            );
        } finally {
            clearTimeout(timeoutId);
        }

        /* Only an expired/invalid SESSION if we actually sent a token.
           A 401 from login (no token sent) means wrong credentials. */
        if (response.status === 401 && sentToken) {
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
            throw new Error(extractErrorMessage(data, response.status, sentToken));
        }

        return data;
    }

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

    return {
        BASE_URL: BASE_URL,
        provinces: provinces,
        grades: grades,
        subjects: subjects,
        schools: schools,
        learners: learners,
        auth: auth,
        escapeHtml: escapeHtml,
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