/* =============================================================
   EduQuest - dashboard.js  (used by pages/admin/dashboard.html)
   Admin home page. Reads from the backend:
     GET /api/users          -> user totals, registered this year/month
     GET /api/quizzes        -> total quizzes
     GET /api/studymaterials -> total resources
     GET /api/competitions   -> active / upcoming competition counts
   Needs api.js (EduQuestAPI) loaded first.
============================================================= */
(function () {
    "use strict";

    if (!window.EduQuestAPI) {
        console.error("dashboard.js: api.js must be loaded before this script.");
        return;
    }

    /* The shared login page lives in the learner frontend. */
    var LOGIN_URL = "../../EduQuest-with-js/EduQuest/login.html";

    /* Admin-only page: send anyone else to sign in. */
    var me = EduQuestAPI.getCurrentUser();
    if (!me || me.role !== "Admin") {
        window.location.href = LOGIN_URL;
        return;
    }

    function setText(id, value) {
        var el = document.getElementById(id);
        if (el) { el.textContent = value; }
    }

    /* The API sends UTC timestamps; EF can drop the trailing "Z", so add it back. */
    function parseDate(value) {
        if (!value) { return null; }
        var text = String(value);
        if (!/(Z|[+-]\d\d:?\d\d)$/.test(text)) { text += "Z"; }
        var date = new Date(text);
        return isNaN(date.getTime()) ? null : date;
    }

    /* Reads a field whether the API sends camelCase or PascalCase. */
    function field(obj, camel, pascal) {
        return obj[camel] !== undefined ? obj[camel] : obj[pascal];
    }

    function showError(message) {
        setText("competitionMessage", "Unable to load dashboard data");
        setText("competitionDescription", message);
    }

    async function loadDashboard() {
        try {
            var results = await Promise.all([
                EduQuestAPI.get("/users"),
                EduQuestAPI.get("/quizzes"),
                EduQuestAPI.get("/studymaterials"),
                EduQuestAPI.get("/competitions")
            ]);
            var users = results[0];
            var quizzes = results[1];
            var resources = results[2];
            var competitions = results[3];

            var now = new Date();

            /* ---- Users ---- */
            var thisYear = 0;
            var thisMonth = 0;
            users.forEach(function (u) {
                var created = parseDate(field(u, "dateCreated", "DateCreated"));
                if (!created) { return; }
                if (created.getFullYear() === now.getFullYear()) {
                    thisYear += 1;
                    if (created.getMonth() === now.getMonth()) { thisMonth += 1; }
                }
            });
            setText("totalUsers", users.length);
            setText("usersThisYear", thisYear);
            setText("usersThisMonth", thisMonth);

            /* ---- Quizzes and resources ---- */
            setText("totalQuizzes", quizzes.length);
            setText("totalResources", resources.length);

            /* ---- Competitions ---- */
            var active = 0;
            var upcoming = 0;
            competitions.forEach(function (c) {
                var start = parseDate(field(c, "startDate", "StartDate"));
                var end = parseDate(field(c, "endDate", "EndDate"));
                if (!start) { return; }
                if (start > now) {
                    upcoming += 1;
                } else if (!end || end >= now) {
                    active += 1;
                }
            });
            setText("activeCompetitions", active);
            setText("upcomingCompetitions", upcoming);

            if (active > 0) {
                setText("competitionMessage", active + " Active Competition" + (active === 1 ? "" : "s"));
                setText("competitionDescription", "There are competitions currently running.");
            } else {
                setText("competitionMessage", "No Active Competitions");
                setText("competitionDescription", upcoming > 0
                    ? "Upcoming competitions are available to look forward to."
                    : "There are currently no competitions running.");
            }

            /* Not available from the API yet. */
            setText("completedQuizzes", "—");
            setText("textbooksAccessed", "—");
            setText("pastPapersCompleted", "—");
            setText("competitionParticipants", "—");

        } catch (err) {
            /* api.js clears the token on a 401: if it is gone, sign in again. */
            if (!EduQuestAPI.getToken()) {
                window.location.href = LOGIN_URL;
                return;
            }
            console.error("Could not load dashboard:", err);
            showError(err.message);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", loadDashboard);
    } else {
        loadDashboard();
    }
})();