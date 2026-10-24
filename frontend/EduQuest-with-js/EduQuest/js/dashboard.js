
/* =============================================================
   EduQuest - Learner Dashboard

   Uses existing backend endpoints:
   GET /api/learner/me
   GET /api/learner/quizzes
   GET /api/learner-quiz-progress
   ============================================================= */

(function () {
    "use strict";

    const API = window.EduQuestAPI;

    if (!API) {
        console.error("EduQuestAPI is missing.");
        return;
    }

    if (typeof API.requireLearner === "function" &&
        !API.requireLearner()) {
        return;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    function byId(id) {
        return document.getElementById(id);
    }

    function el(tag, className, text) {
        const node = document.createElement(tag);

        if (className) {
            node.className = className;
        }

        if (text !== undefined && text !== null) {
            node.textContent = String(text);
        }

        return node;
    }

    function clear(node) {
        if (!node) return;
        node.replaceChildren();
    }

    function showStatus(container, message, isError = false) {
        if (!container) return;

        clear(container);

        container.appendChild(
            el(
                "p",
                "dashboard-empty" + (isError ? " error" : ""),
                message
            )
        );
    }

    function number(value) {
        const parsed = Number(value);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function formatDate(value) {
        if (!value) return "Date unavailable";

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return "Date unavailable";
        }

        return date.toLocaleDateString("en-ZA", {
            day: "numeric",
            month: "short",
            year: "numeric"
        });
    }

    function timeAgo(value) {
        if (!value) return "Date unavailable";

        const timestamp = new Date(value).getTime();

        if (!Number.isFinite(timestamp)) {
            return "Date unavailable";
        }

        const days = Math.max(
            0,
            Math.floor((Date.now() - timestamp) / 86400000)
        );

        if (days === 0) return "Today";
        if (days === 1) return "Yesterday";
        if (days < 7) return days + " days ago";

        return formatDate(value);
    }

    function createLink(label, href) {
        const link = el(
            "a",
            "btn btn-primary dashboard-link",
            label
        );

        link.href = href;
        return link;
    }

    // =========================================================
    // LEARNER PROFILE
    // =========================================================

    async function loadLearnerIdentity() {
        try {
            const me = await API.get("/learner/me");

            const firstName =
                String(me.firstName || "").trim();

            const lastName =
                String(me.lastName || "").trim();

            const fullName =
                [firstName, lastName].filter(Boolean).join(" ") ||
                "Learner";

            const gradeName =
                me.gradeName ||
                (me.grade && me.grade.gradeName) ||
                "";

            const welcome = byId("welcomeHeading");
            const avatar = byId("profileAvatar");
            const label = byId("profileLabel");

            if (welcome) {
                welcome.textContent =
                    "Welcome back, " + fullName + "!";
            }

            if (avatar) {
                avatar.textContent =
                    (firstName || "L").charAt(0).toUpperCase();
            }

            if (label) {
                label.textContent =
                    (firstName || "Learner") +
                    (gradeName ? " · " + gradeName : "");
            }

        } catch (error) {
            console.error(
                "Failed to load learner identity:",
                error
            );
        }
    }

    // =========================================================
    // QUIZ TITLE LOOKUP
    // =========================================================

    function buildQuizLookup(subjects) {
        const lookup = new Map();

        if (!Array.isArray(subjects)) {
            return lookup;
        }

        subjects.forEach(function (subject) {
            const quizzes = Array.isArray(subject.quizzes)
                ? subject.quizzes
                : [];

            quizzes.forEach(function (quiz) {
                lookup.set(String(quiz.quizID), {
                    title: quiz.title || "Quiz #" + quiz.quizID,
                    subjectName: subject.subjectName || "Subject",
                    subjectID: subject.subjectID
                });
            });
        });

        return lookup;
    }

    // =========================================================
    // RECENT ACTIVITY
    // =========================================================

    function renderActivity(progress, quizLookup) {
        const container = byId("recentActivity");

        if (!container) return;

        clear(container);

        if (!Array.isArray(progress) || progress.length === 0) {
            showStatus(
                container,
                "No completed quiz attempts yet. Complete a quiz to see your activity."
            );
            return;
        }

        const attempts = [];

        progress.forEach(function (quiz) {
            const quizInfo =
                quizLookup.get(String(quiz.quizID));

            const history = Array.isArray(quiz.attempts)
                ? quiz.attempts
                : [];

            history.forEach(function (attempt) {
                attempts.push({
                    quizID: quiz.quizID,
                    title: quizInfo
                        ? quizInfo.title
                        : "Quiz #" + quiz.quizID,
                    subjectName: quizInfo
                        ? quizInfo.subjectName
                        : "",
                    score: number(attempt.score),
                    totalQuestions: number(attempt.totalQuestions),
                    percentage: number(attempt.percentage),
                    dateTaken: attempt.dateTaken,
                    attemptID: attempt.attemptID
                });
            });
        });

        attempts.sort(function (a, b) {
            const dateDifference =
                new Date(b.dateTaken || 0).getTime() -
                new Date(a.dateTaken || 0).getTime();

            return dateDifference ||
                number(b.attemptID) - number(a.attemptID);
        });

        if (attempts.length === 0) {
            showStatus(
                container,
                "No submitted quiz activity found."
            );
            return;
        }

        attempts.slice(0, 5).forEach(function (attempt) {
            const row = el("div", "activity-item");

            row.style.padding = "12px 0";
            row.style.borderBottom = "1px solid #e2e8f0";

            const dot = el("div", "activity-dot");

            const text = el("div", "activity-text");

            const title = el(
                "div",
                null,
                attempt.title
            );

            title.style.fontWeight = "600";

            let result;

            if (attempt.totalQuestions > 0) {
                result =
                    attempt.score +
                    "/" +
                    attempt.totalQuestions +
                    " (" +
                    attempt.percentage.toFixed(1) +
                    "%)";
            } else {
                result = "Score: " + attempt.score;
            }

            const detail = el(
                "div",
                null,
                [
                    attempt.subjectName,
                    result
                ].filter(Boolean).join(" · ")
            );

            detail.style.fontSize = "13px";
            detail.style.color = "#64748b";

            text.appendChild(title);
            text.appendChild(detail);

            const date = el(
                "div",
                "activity-time",
                timeAgo(attempt.dateTaken)
            );

            row.appendChild(dot);
            row.appendChild(text);
            row.appendChild(date);

            container.appendChild(row);
        });
    }

    // =========================================================
    // MY CLASSES
    // =========================================================

    function renderSubjects(subjects) {
        const container = byId("learnerClasses");

        if (!container) return;

        clear(container);

        if (!Array.isArray(subjects) || subjects.length === 0) {
            showStatus(
                container,
                "No eligible quizzes are available for your enrolled subjects yet."
            );

            container.appendChild(
                createLink("View Classes", "classes.html")
            );

            return;
        }

        subjects.forEach(function (subject) {
            const quizzes = Array.isArray(subject.quizzes)
                ? subject.quizzes
                : [];

            const completed = quizzes.filter(
                quiz => quiz.completed === true
            ).length;

            const total = quizzes.length;

            const card = el("article", "subject-card");

            const subjectName =
                subject.subjectName || "Subject";

            card.appendChild(
                el(
                    "div",
                    "subject-icon",
                    subjectName.charAt(0).toUpperCase()
                )
            );

            card.appendChild(
                el("h3", null, subjectName)
            );

            const progressText = total > 0
                ? completed + " of " + total +
                  " quizzes completed"
                : "No available quizzes yet.";

            card.appendChild(
                el("p", null, progressText)
            );

            const link = createLink(
                "Open Class",
                subject.subjectID != null
                    ? "class.html?subject=" +
                      encodeURIComponent(subject.subjectID)
                    : "classes.html"
            );

            card.appendChild(link);
            container.appendChild(card);
        });
    }

    // =========================================================
    // TOTAL SCORE
    // =========================================================

    function renderScore(progress) {
        const scoreNumber = byId("learnerTotalScore");
        const scoreMessage = byId("scoreMessage");

        if (!scoreNumber) return;

        if (!Array.isArray(progress)) {
            scoreNumber.textContent = "—";

            if (scoreMessage) {
                scoreMessage.textContent =
                    "Unable to retrieve your quiz scores.";
            }

            return;
        }

        // The progress API provides the best submitted
        // attempt for each distinct quiz.
        // Do not count both attempts of the same quiz.

        const total = progress.reduce(function (sum, quiz) {
            return sum + number(quiz.score);
        }, 0);

        scoreNumber.textContent = total.toLocaleString();

        if (scoreMessage) {
            scoreMessage.textContent =
                progress.length > 0
                    ? "Combined marks from your best attempt per quiz. These are quiz marks, not leaderboard points."
                    : "Complete a quiz to start recording scores.";
        }
    }

    // =========================================================
    // RANKING
    // =========================================================

    function renderRanking() {
        const container = byId("learnerRanking");

        if (!container) return;

        clear(container);

        showStatus(
            container,
            "Your actual ranking is available on the leaderboard."
        );

        container.appendChild(
            createLink(
                "View Leaderboard",
                "leaderboard.html"
            )
        );
    }

    // =========================================================
    // LEARNING PROGRESS
    // =========================================================

    function renderLearningProgress(data, progress) {
        const container = byId("learningProgress");

        if (!container) return;

        if (!data || !Array.isArray(data.subjects)) {
            container.textContent =
                "Unable to load learning progress.";
            return;
        }

        const allQuizzes = data.subjects.flatMap(
            function (subject) {
                return Array.isArray(subject.quizzes)
                    ? subject.quizzes
                    : [];
            }
        );

        const completed = allQuizzes.filter(
            quiz => quiz.completed === true
        ).length;

        const total = allQuizzes.length;

        const remaining = Math.max(0, total - completed);

        const completionPercent = total > 0
            ? Math.round((completed / total) * 100)
            : 0;

        const average = data.summary
            ? data.summary.averagePercent
            : null;

        clear(container);

        const lines = [
            "Completed quizzes: " + completed + " of " + total,
            "Quizzes remaining: " + remaining,
            "Overall completion: " + completionPercent + "%"
        ];

        if (
            average !== null &&
            average !== undefined &&
            Number.isFinite(Number(average))
        ) {
            lines.push(
                "Average latest score: " +
                Number(average).toFixed(1) + "%"
            );
        }

        if (
            Array.isArray(progress) &&
            progress.length > completed
        ) {
            lines.push(
                "Submitted quiz history: " +
                progress.length +
                " distinct quizzes (including quizzes no longer listed as available)."
            );
        }

        lines.forEach(function (line) {
            const item = el("span", null, line);

            item.style.display = "block";
            item.style.marginBottom = "8px";

            container.appendChild(item);
        });
    }

    // =========================================================
    // DASHBOARD DATA LOADING
    // =========================================================

    async function loadDashboard() {
        const activityContainer = byId("recentActivity");
        const classesContainer = byId("learnerClasses");
        const progressContainer = byId("learningProgress");

        showStatus(
            activityContainer,
            "Loading recent activity..."
        );

        showStatus(
            classesContainer,
            "Loading your classes..."
        );

        if (progressContainer) {
            progressContainer.textContent =
                "Loading learning progress...";
        }

        renderRanking();

        // Keep requests independent:
        // one failed request should not prevent
        // the other dashboard sections from loading.

        const results = await Promise.allSettled([
            API.get("/learner/quizzes"),
            API.get("/learner-quiz-progress")
        ]);

        const quizResult = results[0];
        const progressResult = results[1];

        const quizData =
            quizResult.status === "fulfilled"
                ? quizResult.value
                : null;

        const progress =
            progressResult.status === "fulfilled"
                ? progressResult.value
                : null;

        if (quizResult.status === "rejected") {
            console.error(
                "Failed to load learner quizzes:",
                quizResult.reason
            );
        }

        if (progressResult.status === "rejected") {
            console.error(
                "Failed to load quiz progress:",
                progressResult.reason
            );
        }

        if (quizData && Array.isArray(quizData.subjects)) {
            renderSubjects(quizData.subjects);
        } else {
            showStatus(
                classesContainer,
                "Could not load classes.",
                true
            );
        }

        if (Array.isArray(progress)) {
            const lookup = buildQuizLookup(
                quizData && Array.isArray(quizData.subjects)
                    ? quizData.subjects
                    : []
            );

            renderActivity(progress, lookup);
            renderScore(progress);
        } else {
            showStatus(
                activityContainer,
                "Could not load recent quiz activity.",
                true
            );

            renderScore(null);
        }

        renderLearningProgress(quizData, progress);

        console.log("Learner dashboard data:", {
            quizData: quizData,
            quizProgress: progress
        });
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    function setupLogout() {
        const logoutLink = byId("logoutLink");

        if (!logoutLink) return;

        logoutLink.addEventListener(
            "click",
            function (event) {
                event.preventDefault();
                API.logout();
            }
        );
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    function initialize() {
        setupLogout();

        loadLearnerIdentity();
        loadDashboard();
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initialize,
            { once: true }
        );
    } else {
        initialize();
    }

})();
