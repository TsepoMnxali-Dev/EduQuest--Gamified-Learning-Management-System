/* =============================================================
   EduQuest - index.js  (used by index.html)

   Loads the landing-page numbers from the backend
   (GET /api/public-stats) instead of showing hard-coded figures.
   - If the server has no "improved" figure yet, it shows 0%.
   - If the server cannot be reached, the numbers show a dash.
============================================================= */
(function () {
    "use strict";

    function byId(id) {
        return document.getElementById(id);
    }

    function formatNumber(value) {
        return Number(value).toLocaleString();
    }

    function showUnavailable() {
        ["statStudents", "statSchools", "statImproved"].forEach(function (id) {
            byId(id).textContent = "\u2014";
        });
    }

    EduQuestAPI.get("/public-stats", { auth: false })
        .then(function (stats) {
            byId("statStudents").textContent = formatNumber(stats.studentsEnrolled);
            byId("statSchools").textContent = formatNumber(stats.schoolsReached);

            var improved = Number(stats.improvedPercent);
            if (Number.isNaN(improved)) improved = 0;
            byId("statImproved").textContent = improved + "%";
        })
        .catch(function (err) {
            console.error("Landing-page stats failed to load:", err);
            showUnavailable();
        });
})();