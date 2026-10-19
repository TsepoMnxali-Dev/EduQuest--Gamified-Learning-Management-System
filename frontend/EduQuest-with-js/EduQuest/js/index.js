/* =============================================================
   EduQuest - index.js  (used by index.html)

   Loads the landing-page numbers from the backend
   (GET /api/landing-stats): students enrolled, schools reached
   and quizzes completed.
   If the server cannot be reached, or does not send a number,
   that number shows a dash (and the Console explains why).
============================================================= */
(function () {
    "use strict";

    function byId(id) {
        return document.getElementById(id);
    }

    /* A real number is shown formatted; anything else becomes a dash. */
    function show(id, value, fieldName) {
        var node = byId(id);

        if (typeof value === "number" && isFinite(value)) {
            node.textContent = value.toLocaleString();
        } else {
            node.textContent = "\u2014";
            console.error(
                "Landing-page stats: the server did not send \"" + fieldName + "\"."
            );
        }
    }

    function showUnavailable() {
        ["statStudents", "statSchools", "statQuizzes"].forEach(function (id) {
            byId(id).textContent = "\u2014";
        });
    }

    EduQuestAPI.get("/landing-stats", { auth: false })
        .then(function (stats) {
            /* TEMPORARY: shows exactly what answered. */
            console.log("Landing stats from " + EduQuestAPI.BASE_URL + "/landing-stats", stats);

            show("statStudents", stats.studentsEnrolled, "studentsEnrolled");
            show("statSchools", stats.schoolsReached, "schoolsReached");
            show("statQuizzes", stats.quizzesCompleted, "quizzesCompleted");
        })
        .catch(function (err) {
            console.error("Landing-page stats failed to load:", err);
            showUnavailable();
        });
})();