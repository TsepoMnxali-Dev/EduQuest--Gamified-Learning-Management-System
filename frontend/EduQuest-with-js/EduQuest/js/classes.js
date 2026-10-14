/* =============================================================
   EduQuest - classes.js  (used by classes.html)

   Loads the signed-in learner's classes from the backend
   (GET /api/learner/classes): enrolled subjects, quiz progress,
   average scores and the next quiz to take.
   Server text is written with textContent, never innerHTML.
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    /* Existing colour classes from the page design, cycled per subject. */
    var COLOR_CLASSES = ["math", "science", "life", "geography"];

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

    /* Top corner: first name and grade, e.g. "Ben · Grade 12",
       from GET /api/learner/me. */
    function showProfile() {
        EduQuestAPI.get("/learner/me")
            .then(function (me) {
                var firstName = String(me.firstName || "").trim() || "Learner";
                var gradeName = me.gradeName ? " \u00b7 " + me.gradeName : "";

                byId("profileAvatar").textContent = firstName.charAt(0).toUpperCase();
                byId("profileName").textContent = firstName + gradeName;
            })
            .catch(function (err) {
                // The page still works without it.
                console.error("Learner name failed to load:", err);
            });
    }

    function showGridStatus(message, isError) {
        var grid = byId("subjectsGrid");
        clear(grid);
        grid.appendChild(el("p", "classes-status" + (isError ? " error" : ""), message));
    }

    /* "Label" followed by a bold value, as in the original design. */
    function buildStat(label, value) {
        var box = el("div");
        box.appendChild(document.createTextNode(label));
        box.appendChild(el("strong", null, value));
        return box;
    }

    function buildSubjectCard(subject, index) {
        var card = el("article", "class-card");

        var top = el("div", "class-top");
        top.appendChild(el(
            "div",
            "subject-icon " + COLOR_CLASSES[index % COLOR_CLASSES.length],
            subject.subjectName.charAt(0).toUpperCase()
        ));
        card.appendChild(top);

        card.appendChild(el("h3", null, subject.subjectName));
        card.appendChild(el(
            "p", "description",
            subject.topicCount + (subject.topicCount === 1 ? " topic" : " topics")
        ));

        var stats = el("div", "stats");
        stats.appendChild(buildStat(
            "Quizzes",
            subject.quizzesCompleted + " of " + subject.quizzesTotal + " completed"
        ));
        stats.appendChild(buildStat("Resources", subject.resourceCount + " available"));
        card.appendChild(stats);

        var progress = el("div", "class-progress");
        var label = el("div", "progress-label");
        label.appendChild(el("span", null, "Your progress"));
        label.appendChild(el("strong", null, subject.progressPercent + "%"));
        progress.appendChild(label);

        var track = el("div", "progress-track");
        var fill = el("div", "progress-fill");
        fill.style.width = subject.progressPercent + "%";
        track.appendChild(fill);
        progress.appendChild(track);
        card.appendChild(progress);

        var open = el("a", "open-btn", "Continue \u2192");
        open.href = "class.html?subject=" + subject.subjectID;
        card.appendChild(open);

        return card;
    }

    function renderSubjects(subjects) {
        var grid = byId("subjectsGrid");
        clear(grid);
        subjects.forEach(function (subject, index) {
            grid.appendChild(buildSubjectCard(subject, index));
        });
    }

    function renderPerformance(subjects) {
        var rows = byId("performanceRows");
        clear(rows);

        subjects.forEach(function (subject) {
            var row = el("div", "performance-row");

            var name = el("div", "subject-name");
            name.appendChild(el("span", "small-dot"));
            name.appendChild(document.createTextNode(" " + subject.subjectName));
            row.appendChild(name);

            var hasScore = subject.averageScorePercent !== null && subject.averageScorePercent !== undefined;
            row.appendChild(el("div", "score", hasScore ? subject.averageScorePercent + "%" : "\u2013"));

            var track = el("div", "mini-track");
            var fill = el("div", "mini-fill");
            fill.style.width = (hasScore ? subject.averageScorePercent : 0) + "%";
            track.appendChild(fill);
            row.appendChild(track);

            rows.appendChild(row);
        });
    }

    function renderUpNext(data) {
        var completed = 0;
        var total = 0;
        data.subjects.forEach(function (s) {
            completed += s.quizzesCompleted;
            total += s.quizzesTotal;
        });
        var percent = total === 0 ? 0 : Math.round(completed * 100 / total);

        byId("overallPercent").textContent = percent + "%";
        byId("overallFill").style.width = percent + "%";

        var button = byId("upNextBtn");

        if (data.upNext) {
            byId("upNextTitle").textContent = data.upNext.subjectName + " \u2014 " + data.upNext.quizTitle;
            byId("upNextText").textContent = "Topic: " + data.upNext.topicName + ". Ready when you are!";
            button.textContent = "Start Quiz \u2192";
            button.href = "quiz.html?id=" + data.upNext.quizID;
        } else {
            byId("upNextTitle").textContent = "You're all caught up";
            byId("upNextText").textContent = total === 0
                ? "No quizzes are available yet. Check back soon."
                : "You've finished every available quiz. Review your results any time.";
            button.textContent = "View Quizzes \u2192";
            button.href = "Quizzes.html";
        }
    }

    showProfile();

    EduQuestAPI.get("/learner/classes")
        .then(function (data) {
            renderUpNext(data);

            if (!data.subjects || data.subjects.length === 0) {
                showGridStatus("You are not enrolled in any subjects yet.", false);
                return;
            }

            renderSubjects(data.subjects);
            renderPerformance(data.subjects);
        })
        .catch(function (err) {
            console.error("Classes failed to load:", err);
            byId("upNextTitle").textContent = "Could not load your classes";
            byId("upNextText").textContent = "";
            showGridStatus(err.message || "Could not load your classes.", true);
        });
})();