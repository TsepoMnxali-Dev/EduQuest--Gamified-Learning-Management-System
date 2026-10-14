/* =============================================================
   EduQuest - quizzes.js  (used by Quizzes.html)

   Loads the learner's quizzes from the backend
   (GET /api/learner/quizzes): the quizzes the admin has published
   for the learner's grade and subjects, with the learner's latest
   result for each. Also fills the summary numbers and the
   top-corner name (GET /api/learner/me).
   Server text is written with textContent, never innerHTML.
============================================================= */
(function () {
    "use strict";

    if (!EduQuestAPI.requireLearner()) {
        return;
    }

    function byId(id) {
        return document.getElementById(id);
    }

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null) node.textContent = text;
        return node;
    }

    function clear(node) {
        while (node.firstChild) node.removeChild(node.firstChild);
    }

    /* Lower-cases every key so the page works whether the API sends
       camelCase or PascalCase. */
    function norm(value) {
        if (Array.isArray(value)) return value.map(norm);
        if (value && typeof value === "object") {
            var out = {};
            Object.keys(value).forEach(function (key) {
                out[key.toLowerCase()] = norm(value[key]);
            });
            return out;
        }
        return value;
    }

    function timeAgo(iso) {
        var days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
        if (days < 1) { return "today"; }
        if (days < 7) { return days + (days === 1 ? " day ago" : " days ago"); }
        var weeks = Math.floor(days / 7);
        return weeks + (weeks === 1 ? " week ago" : " weeks ago");
    }

    /* ---------------------------------------------------------
       Top corner: "Ben · Grade 12"
    --------------------------------------------------------- */

    function loadCorner() {
        EduQuestAPI.get("/learner/me")
            .then(function (raw) {
                var me = norm(raw);
                var firstName = String(me.firstname || "").trim() || "Learner";
                var gradeName = me.gradename ? " \u00b7 " + me.gradename : "";

                byId("profileAvatar").textContent = firstName.charAt(0).toUpperCase();
                byId("profileName").textContent = firstName + gradeName;
            })
            .catch(function (err) {
                console.error("Learner name failed to load:", err);
            });
    }

    /* ---------------------------------------------------------
       Quiz list
    --------------------------------------------------------- */

    function buildQuizItem(quiz) {
        var item = el("div", "quiz-item");
        var done = !!quiz.completed;

        item.appendChild(el(
            "div",
            "quiz-status-icon " + (done ? "done" : "pending"),
            done ? "\u2713" : "\u2022"
        ));

        var info = el("div", "quiz-info");
        info.appendChild(el("h4", null, quiz.title));
        info.appendChild(el(
            "p", null,
            quiz.questioncount + " questions \u00b7 " +
            (done ? "Completed " + timeAgo(quiz.latestdate) : "Not started")
        ));
        item.appendChild(info);

        var hasScore = done && quiz.latestscorepercent !== null && quiz.latestscorepercent !== undefined;
        item.appendChild(el(
            "span",
            "quiz-badge " + (done ? "completed" : "pending"),
            done ? (hasScore ? quiz.latestscorepercent + "%" : "Done") : "To Do"
        ));

        var button;
        if (done && quiz.latestattemptid) {
            button = el("a", "btn btn-outline", "Review");
            button.href = "quiz.html?id=" + quiz.quizid + "&review=1";
        } else {
            button = el("a", "btn btn-primary", "Start Quiz");
            button.href = "quiz.html?id=" + quiz.quizid;
        }
        item.appendChild(button);

        return item;
    }

    function buildSubjectBlock(subject) {
        var block = el("section", "subject-block");

        var completed = subject.quizzes.filter(function (q) { return q.completed; }).length;

        var head = el("div", "subject-block-head");
        head.appendChild(el("div", "subject-icon", subject.subjectname.charAt(0).toUpperCase()));

        var titles = el("div");
        titles.appendChild(el("h2", null, subject.subjectname));
        titles.appendChild(el(
            "p", null,
            completed + " of " + subject.quizzes.length + " quizzes completed"
        ));
        head.appendChild(titles);
        block.appendChild(head);

        var list = el("div", "quiz-list");
        subject.quizzes.forEach(function (quiz) {
            list.appendChild(buildQuizItem(quiz));
        });
        block.appendChild(list);

        return block;
    }

    function showStatus(message) {
        var grid = byId("quizzesGrid");
        clear(grid);
        grid.appendChild(el("p", "quiz-note", message));
    }

    function renderSummary(summary) {
        byId("statCompleted").textContent = summary.completed || 0;
        byId("statTodo").textContent = summary.todo || 0;
        byId("statAverage").textContent =
            summary.averagepercent === null || summary.averagepercent === undefined
                ? "\u2013"
                : summary.averagepercent + "%";
    }

    function render(data) {
        renderSummary(data.summary || {});

        var grid = byId("quizzesGrid");
        clear(grid);

        if (!data.subjects || data.subjects.length === 0) {
            showStatus("No quiz available yet. Check back soon.");
            return;
        }

        data.subjects.forEach(function (subject) {
            grid.appendChild(buildSubjectBlock(subject));
        });
    }

    loadCorner();

    showStatus("Loading your quizzes\u2026");

    EduQuestAPI.get("/learner/quizzes")
        .then(function (raw) {
            render(norm(raw));
        })
        .catch(function (err) {
            console.error("Quizzes failed to load:", err);
            showStatus(err.message || "Could not load your quizzes.");
        });
})();