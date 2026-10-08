/* =============================================================
   EduQuest - classes.js  (used by classes.html)
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    /* Any element with data-href acts as a link (the subject "Continue" buttons). */
    document.querySelectorAll("[data-href]").forEach(function (el) {
        el.addEventListener("click", function () {
            window.location.href = el.getAttribute("data-href");
        });
    });

    var continueQuiz = document.getElementById("continue-quiz-btn");
    if (continueQuiz) {
        continueQuiz.addEventListener("click", function () {
            /* TODO (backend): open the quiz the learner last left unfinished. */
            alert("EduQuest: Continuing your Mathematics Algebra Quiz...");
        });
    }

    var viewLeaderboard = document.getElementById("view-leaderboard-btn");
    if (viewLeaderboard) {
        viewLeaderboard.addEventListener("click", function () {
            /* TODO: this could link straight to leaderboard.html. */
            alert("EduQuest: Opening the leaderboard...");
        });
    }
})();
