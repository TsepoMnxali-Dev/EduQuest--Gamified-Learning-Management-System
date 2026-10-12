/* =============================================================
   EduQuest - dashboard.js  (used by dashboard.html)

   Loads everything on the learner dashboard in one request
   (GET /api/learner/dashboard): name, recent activity, enrolled
   classes, points, ranking and the weekly learning goal.
   Server text is written with textContent, never innerHTML.
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

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

    function timeAgo(iso) {
        var days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
        if (days < 1) { return "Today"; }
        if (days === 1) { return "Yesterday"; }
        if (days < 7) { return days + " days ago"; }
        var weeks = Math.floor(days / 7);
        return weeks + (weeks === 1 ? " week ago" : " weeks ago");
    }

    function showStatus(container, message, isError) {
        clear(container);
        container.appendChild(el("p", "dash-status" + (isError ? " error" : ""), message));
    }

    /* ---------------------------------------------------------
       Sections
    --------------------------------------------------------- */

    function renderWelcome(data) {
        byId("welcomeHeading").textContent = "Welcome back, " + data.firstName + "!";
        byId("profileAvatar").textContent = data.firstName.charAt(0).toUpperCase();
        byId("profileLabel").textContent = data.firstName + " \u00b7 " + data.gradeName;
    }

    function renderActivity(items) {
        var list = byId("activityList");
        clear(list);

        if (!items || items.length === 0) {
            showStatus(list, "Nothing here yet. Complete a quiz to get started.", false);
            return;
        }

        items.forEach(function (item) {
            var row = el("div", "activity-item");
            row.appendChild(el("div", "activity-dot"));
            row.appendChild(el("div", "activity-text", item.text));
            row.appendChild(el("div", "activity-time", timeAgo(item.date)));
            list.appendChild(row);
        });
    }

    function renderSubjects(subjects) {
        var grid = byId("subjectGrid");
        clear(grid);

        if (!subjects || subjects.length === 0) {
            showStatus(grid, "You are not enrolled in any subjects yet.", false);
            return;
        }

        subjects.forEach(function (subject) {
            var card = el("article", "subject-card");
            card.appendChild(el("div", "subject-icon", subject.subjectName.charAt(0).toUpperCase()));
            card.appendChild(el("h3", null, subject.subjectName));

            var progress = subject.quizzesTotal > 0
                ? subject.quizzesCompleted + " of " + subject.quizzesTotal +
                  (subject.quizzesTotal === 1 ? " quiz" : " quizzes") + " completed"
                : "No quizzes available yet.";
            card.appendChild(el("p", null, progress));
            card.appendChild(document.createElement("br"));

            var open = el("a", "btn btn-primary", "Open Class");
            open.href = "class.html?subject=" + subject.subjectID;
            card.appendChild(open);

            grid.appendChild(card);
        });
    }

    function renderScore(data) {
        byId("scoreNumber").textContent = data.totalPoints;
        byId("scoreMessage").textContent = data.totalPoints > 0
            ? "Keep going! You're doing great."
            : "Complete a quiz to start earning points.";
    }

    function buildRankRow(entry) {
        var row = el("div", "dashboard-list-item" + (entry.isYou ? " you" : ""));
        row.appendChild(el("span", "rank", "#" + entry.rank));
        row.appendChild(el("span", "list-name", entry.name));
        row.appendChild(el("span", "list-score", entry.points + " pts"));
        return row;
    }

    function renderRanking(data) {
        var list = byId("rankingList");
        var note = byId("rankingNote");
        clear(list);
        note.textContent = "";

        var top = data.topLearners || [];
        var inTop = top.some(function (entry) { return entry.isYou; });

        if (top.length === 0) {
            showStatus(list, "No one has completed a quiz yet. Be the first!", false);
            return;
        }

        top.forEach(function (entry) {
            list.appendChild(buildRankRow(entry));
        });

        if (!inTop) {
            if (data.rank !== null && data.rank !== undefined) {
                list.appendChild(el("div", "dashboard-list-item gap", "\u2026"));
                list.appendChild(buildRankRow({
                    rank: data.rank,
                    name: "You",
                    points: data.totalPoints,
                    isYou: true
                }));
            } else {
                note.textContent = "Complete a quiz to join the ranking.";
            }
        }

        if (data.totalLearners) {
            var existing = note.textContent;
            var total = "Out of " + data.totalLearners + (data.totalLearners === 1 ? " learner" : " learners");
            note.textContent = existing ? existing + " " + total : total;
        }
    }

    function renderGoal(goal) {
        var percent = goal.target > 0
            ? Math.min(100, Math.round(goal.completed * 100 / goal.target))
            : 0;

        byId("goalText").textContent = "Complete " + goal.target + " quizzes this week.";
        byId("goalFill").style.width = percent + "%";
        byId("goalCount").textContent = goal.completed + " of " + goal.target + " quizzes";
        byId("goalPercent").textContent = percent + "%";
    }

    /* ---------------------------------------------------------
       Start
    --------------------------------------------------------- */

    byId("logoutLink").addEventListener("click", function (event) {
        event.preventDefault();
        EduQuestAPI.logout();
    });

    EduQuestAPI.get("/learner/dashboard")
        .then(function (data) {
            renderWelcome(data);
            renderActivity(data.recentActivity);
            renderSubjects(data.subjects);
            renderScore(data);
            renderRanking(data);
            renderGoal(data.goal);
        })
        .catch(function (err) {
            console.error("Dashboard failed to load:", err);
            var message = err.message || "Could not load your dashboard.";
            showStatus(byId("activityList"), message, true);
            showStatus(byId("subjectGrid"), message, true);
            showStatus(byId("rankingList"), message, true);
            byId("scoreNumber").textContent = "\u2013";
            byId("goalCount").textContent = "\u2013";
        });
})();