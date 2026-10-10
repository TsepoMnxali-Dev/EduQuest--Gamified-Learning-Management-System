/* =============================================================
   EduQuest - quizzes.js  (used by Quizzes.html)

   Shows the signed-in learner's real quizzes:

     GET /api/learners/me          name for the top-right badge
     GET /api/learners/me/quizzes  published quizzes for the learner's grade
                                   and chosen subjects, each with their
                                   latest submitted result

   The summary numbers (completed, still to do, average score) and each
   subject's "x of y quizzes completed" line are counted from that list.

   Needs js/api.js (EduQuestAPI) loaded first.
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    function byId(id) { return document.getElementById(id); }

    function esc(value) {
        return String(value === null || value === undefined ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function showMessage(html) {
        var box = byId("quizzesMessage");
        box.innerHTML = html;
        box.style.display = "block";
    }

    function timeAgo(iso) {
        var days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
        if (isNaN(days) || days < 1) { return "today"; }
        if (days < 7) { return days + (days === 1 ? " day ago" : " days ago"); }
        var weeks = Math.floor(days / 7);
        if (weeks < 5) { return weeks + (weeks === 1 ? " week ago" : " weeks ago"); }
        var months = Math.floor(days / 30);
        return months + (months === 1 ? " month ago" : " months ago");
    }

    function quizRow(quiz) {
        var meta = quiz.questionCount + " questions &middot; " + esc(quiz.difficulty);

        if (quiz.completed) {
            return '<div class="quiz-item" data-quiz-id="' + quiz.quizID + '">' +
                '<div class="quiz-status-icon done">✓</div>' +
                '<div class="quiz-info"><h4>' + esc(quiz.quizTitle) + "</h4>" +
                    "<p>" + meta + " &middot; Completed " + timeAgo(quiz.latestDateTaken) + "</p></div>" +
                '<span class="quiz-badge completed">' + quiz.latestPercentage + "%</span>" +
                '<a href="quiz.html?id=' + quiz.quizID + '&amp;review=1" class="btn btn-outline">Review</a>' +
            "</div>";
        }

        return '<div class="quiz-item" data-quiz-id="' + quiz.quizID + '">' +
            '<div class="quiz-status-icon pending">•</div>' +
            '<div class="quiz-info"><h4>' + esc(quiz.quizTitle) + "</h4>" +
                "<p>" + meta + " &middot; Not started</p></div>" +
            '<span class="quiz-badge pending">To Do</span>' +
            '<a href="quiz.html?id=' + quiz.quizID + '" class="btn btn-primary">Start Quiz</a>' +
        "</div>";
    }

    function subjectBlock(name, quizzes) {
        var done = quizzes.filter(function (q) { return q.completed; }).length;

        return '<section class="subject-block">' +
            '<div class="subject-block-head">' +
                '<div class="subject-icon">' + esc(name.charAt(0).toUpperCase()) + "</div>" +
                "<div><h2>" + esc(name) + "</h2>" +
                    "<p>" + done + " of " + quizzes.length + " quizzes completed</p></div>" +
            "</div>" +
            '<div class="quiz-list">' + quizzes.map(quizRow).join("") + "</div>" +
        "</section>";
    }

    function render(quizzes) {
        var grid = byId("quizzesGrid");

        var done = quizzes.filter(function (q) { return q.completed; });
        var scoreSum = done.reduce(function (sum, q) { return sum + q.latestPercentage; }, 0);

        byId("sumDone").textContent = done.length;
        byId("sumTodo").textContent = quizzes.length - done.length;
        byId("sumAvg").textContent = done.length ? Math.round(scoreSum / done.length) + "%" : "–";

        if (quizzes.length === 0) {
            grid.innerHTML = "";
            showMessage("There are no quizzes for your grade and subjects yet. " +
                'You can choose your subjects under <a href="profile.html">Profile &rarr; Manage Subjects</a>.');
            return;
        }

        var bySubject = {};
        var order = [];
        quizzes.forEach(function (q) {
            if (!bySubject[q.subjectName]) {
                bySubject[q.subjectName] = [];
                order.push(q.subjectName);
            }
            bySubject[q.subjectName].push(q);
        });

        grid.innerHTML = order.map(function (name) {
            return subjectBlock(name, bySubject[name]);
        }).join("");
    }

    async function load() {
        var results;
        try {
            results = await Promise.all([
                EduQuestAPI.get("/learners/me"),
                EduQuestAPI.get("/learners/me/quizzes")
            ]);
        } catch (err) {
            if (/session has expired/i.test(err.message)) {
                window.location.href = "login.html";
                return;
            }
            byId("quizzesGrid").innerHTML = "";
            ["sumDone", "sumTodo", "sumAvg"].forEach(function (id) { byId(id).textContent = "-"; });
            showMessage(esc(err.message));
            return;
        }

        var me = results[0];
        byId("quizLearnerName").textContent = me.firstName;
        byId("quizAvatar").textContent = (me.firstName || "?").charAt(0).toUpperCase();

        render(results[1]);
    }

    load();
})();
