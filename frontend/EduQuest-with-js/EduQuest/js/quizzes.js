/* ============================================================
   EduQuest learner quiz list
   - Published quizzes + database-backed completed attempts
   - TWO submitted attempts allowed for each quiz
   - Best score counts toward learner average
   - One completed quiz counts once, even after a retake
   - No localStorage-based marks or sample quiz data
   API: GET /api/quizzes, GET /api/learner-quiz-progress
============================================================ */
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const api = window.EduQuestAPI;
        const grid = document.getElementById("quizzesGrid");
        const completedEl = document.getElementById("completedCount");
        const pendingEl = document.getElementById("pendingCount");
        const averageEl = document.getElementById("averageScore");

        if (!api || !grid || !completedEl || !pendingEl || !averageEl) {
            console.error("EduQuest quiz page is missing a required element or API helper.");
            return;
        }
        if (!api.requireLearner()) return;

        const user = api.getCurrentUser() || {};
        const name = [user.firstName, user.lastName].filter(Boolean).join(" ");
        const nameEl = document.getElementById("learnerName");
        const avatarEl = document.getElementById("learnerAvatar");
        if (nameEl) nameEl.textContent = name || "Learner";
        if (avatarEl) avatarEl.textContent = String(user.firstName || user.email || "L").charAt(0).toUpperCase();

        let loadVersion = 0;
        const MAX_ATTEMPTS = 2;

        function element(tag, className, text) {
            const node = document.createElement(tag);
            if (className) node.className = className;
            if (text !== undefined) node.textContent = String(text);
            return node;
        }

        function message(text) {
            grid.replaceChildren(element("p", "quiz-note", text));
        }

        function setStatistics(completed, pending, average) {
            completedEl.textContent = completed;
            pendingEl.textContent = pending;
            averageEl.textContent = average;
        }

        function addAction(container, label, url, primary) {
            const link = element("a", "btn " + (primary ? "btn-primary" : "btn-outline"), label);
            link.href = url;
            container.appendChild(link);
        }

        function makeQuizItem(quiz, progress, topic) {
            const id = Number(quiz.quizID);
            const completedAttempts = progress
                ? Math.max(0, Number(progress.completedAttempts) || 0)
                : 0;
            const used = Math.min(MAX_ATTEMPTS, completedAttempts);
            const completed = completedAttempts > 0;
            const remaining = Math.max(0, MAX_ATTEMPTS - completedAttempts);
            const bestScore = completed ? Number(progress.percentage) : null;

            const item = element("div", "quiz-item");
            item.dataset.quizId = String(id);
            item.appendChild(element(
                "div", "quiz-status-icon " + (completed ? "done" : "pending"),
                completed ? "✓" : "•"
            ));

            const info = element("div", "quiz-info");
            info.appendChild(element("h4", "", quiz.quizTitle));
            const description = [topic?.topicName || quiz.difficulty || "Quiz"];
            if (topic?.gradeLevel) description.push(topic.gradeLevel);
            description.push(used + " of " + MAX_ATTEMPTS + " attempts used");
            info.appendChild(element("p", "", description.join(" · ")));
            item.appendChild(info);

            item.appendChild(element(
                "span", "quiz-badge " + (completed ? "completed" : "pending"),
                completed && Number.isFinite(bestScore) ? Math.round(bestScore) + "% best" : "To Do"
            ));

            const actions = element("div", "quiz-actions");
            const quizUrl = "quiz.html?id=" + encodeURIComponent(id);

            if (completed) {
                const attempts = Array.isArray(progress.attempts)
                    ? progress.attempts
                    : [];
                // Display a review link for EACH completed submission.
                attempts.forEach((attempt, index) => {
                    const attemptId = Number(attempt.attemptID);
                    if (Number.isInteger(attemptId) && attemptId > 0) {
                        addAction(
                            actions,
                            "Review Attempt " + (index + 1),
                            quizUrl + "&review=1&attempt=" + attemptId,
                            false
                        );
                    }
                });
                // Compatibility with old progress data if needed.
                if (!attempts.length) {
                    const reviewId = Number(progress.bestAttemptID || progress.quizAttemptID);
                    if (Number.isInteger(reviewId) && reviewId > 0) {
                        addAction(actions, "Review Results", quizUrl + "&review=1&attempt=" + reviewId, false);
                    }
                }
            }
            if (remaining > 0) {
                addAction(actions, completed ? "Try Again" : "Start Quiz", quizUrl, true);
            }
            if (completed && remaining === 0 && !actions.children.length) {
                // Never display an inert review button if the backend has no review ID.
                actions.appendChild(element("span", "quiz-badge completed", "Results unavailable"));
            }
            item.appendChild(actions);
            return item;
        }

        function makeGroup(subject, entries) {
            const section = element("section", "subject-block");
            const header = element("div", "subject-block-head");
            header.appendChild(element("div", "subject-icon", subject.charAt(0).toUpperCase()));
            const heading = element("div", "");
            heading.appendChild(element("h2", "", subject));
            heading.appendChild(element(
                "p", "", entries.filter(e => e.progress).length +
                    " of " + entries.length + " quizzes completed"
            ));
            header.appendChild(heading);
            section.appendChild(header);
            const list = element("div", "quiz-list");
            entries.forEach(e => list.appendChild(makeQuizItem(e.quiz, e.progress, e.topic)));
            section.appendChild(list);
            return section;
        }

        async function loadQuizzes() {
            const version = ++loadVersion;
            message("Loading quizzes and your completed attempts…");
            setStatistics("–", "–", "–");

            try {
                const [quizzes, history] = await Promise.all([
                    api.get("/quizzes"),
                    api.get("/learner-quiz-progress")
                ]);

                if (!Array.isArray(quizzes) || !Array.isArray(history)) {
                    throw new Error("Unexpected response from the quizzes or progress API.");
                }

                let topics = [];
                try {
                    const data = await api.get("/topics");
                    if (Array.isArray(data)) topics = data;
                } catch (error) {
                    console.warn("Topics are unavailable; quizzes can still load.", error);
                }
                if (version !== loadVersion) return;

                const topicMap = new Map(topics.map(t => [Number(t.topicID), t]));
                const historyMap = new Map(history
                    .filter(h => Number.isInteger(Number(h.quizID)) && Number(h.completedAttempts) > 0)
                    .map(h => [Number(h.quizID), h]));
                const published = quizzes.filter(q =>
                    q?.isPublished === true && Number.isInteger(Number(q.quizID)) &&
                    Number(q.quizID) > 0 && typeof q.quizTitle === "string" && q.quizTitle.trim()
                );

                const groups = new Map();
                let completed = 0;
                let scoresTotal = 0;

                published.forEach(quiz => {
                    const progress = historyMap.get(Number(quiz.quizID)) || null;
                    if (progress) {
                        completed++;
                        scoresTotal += Number(progress.percentage) || 0;
                    }
                    const topic = topicMap.get(Number(quiz.topicID)) || null;
                    const subject = topic?.subjectName || "Other Quizzes";
                    if (!groups.has(subject)) groups.set(subject, []);
                    groups.get(subject).push({ quiz, progress, topic });
                });

                setStatistics(
                    completed,
                    published.length - completed,
                    completed ? Math.round(scoresTotal / completed) + "%" : "–"
                );

                grid.replaceChildren();
                if (!published.length) {
                    message("No published quizzes are available yet.");
                    return;
                }
                [...groups.keys()].sort((a, b) => a.localeCompare(b))
                    .forEach(subject => grid.appendChild(makeGroup(subject, groups.get(subject))));
            } catch (error) {
                if (version !== loadVersion) return;
                console.error("Unable to retrieve learner quiz progress:", error);
                message("Unable to load quizzes and your marks. " + (error.message || "Please try again.") + " Check GET /api/learner-quiz-progress in Swagger.");
                setStatistics("–", "–", "–");
                const retry = element("button", "btn btn-primary", "Try Again");
                retry.type = "button";
                retry.addEventListener("click", loadQuizzes);
                grid.appendChild(retry);
            }
        }

        window.addEventListener("pageshow", event => {
            if (event.persisted) loadQuizzes();
        });
        document.addEventListener("visibilitychange", () => {
            if (!document.hidden) loadQuizzes();
        });
        loadQuizzes();
    });
})();
