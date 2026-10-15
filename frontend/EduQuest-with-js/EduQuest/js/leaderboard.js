
/*
========================================================
EduQuest - LEARNER LEADERBOARD

- Uses real backend rankings
- Displays top 3 learners
- Displays remaining learners
- Supports weekly, monthly and all-time rankings
- Shows current learner
- Handles empty rankings and API errors

API:
GET /api/leaderboards/rankings?period=all
========================================================
*/

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {

        // ==========================================
        // AUTHENTICATION
        // ==========================================

        if (!window.EduQuestAPI) {
            console.error("EduQuestAPI is not loaded.");
            return;
        }

        if (!EduQuestAPI.requireLearner()) {
            return;
        }

        // ==========================================
        // HTML ELEMENTS
        // ==========================================

        const podium = document.getElementById(
            "leaderboardPodium"
        );

        const leaderboardList = document.getElementById(
            "leaderboardList"
        );

        const leaderboardStatus = document.getElementById(
            "leaderboardStatus"
        );

        const emptyLeaderboard = document.getElementById(
            "emptyLeaderboard"
        );

        const rankingsSection = document.getElementById(
            "leaderboardRankingsSection"
        );

        const periodButtons = document.querySelectorAll(
            "[data-period]"
        );

        if (
            !podium ||
            !leaderboardList ||
            !leaderboardStatus ||
            !emptyLeaderboard ||
            !rankingsSection
        ) {
            console.error(
                "Learner leaderboard HTML elements are missing."
            );
            return;
        }

        // ==========================================
        // STATE
        // ==========================================

        let currentPeriod = "all";
        let requestNumber = 0;

        // ==========================================
        // HELPERS
        // ==========================================

        function createElement(tag, className, text) {
            const element = document.createElement(tag);

            if (className) {
                element.className = className;
            }

            if (text !== undefined && text !== null) {
                element.textContent = String(text);
            }

            return element;
        }

        function getInitials(name) {
            if (!name) return "L";

            return name
                .trim()
                .split(/\s+/)
                .map(part => part.charAt(0))
                .join("")
                .substring(0, 2)
                .toUpperCase();
        }

        function getName(learner) {
            return learner.displayName ||
                   learner.learnerName ||
                   "Learner";
        }

        function getPoints(learner) {
            return Number(learner.points) || 0;
        }

        function getQuizzesTaken(learner) {
            return Number(learner.quizzesTaken) || 0;
        }

        function getRank(learner, index) {
            return Number(learner.rank) || index + 1;
        }

        function getAverage(learner) {
            const value = Number(learner.averagePercent);

            return Number.isFinite(value)
                ? value.toFixed(1) + "%"
                : null;
        }

        function isCurrentLearner(learner) {
            return learner.isCurrentUser === true;
        }

        // ==========================================
        // STATUS AND EMPTY STATE
        // ==========================================

        function showStatus(message) {
            leaderboardStatus.textContent = message;
            leaderboardStatus.style.display =
                message ? "block" : "none";
        }

        function showEmpty() {
            podium.innerHTML = "";
            leaderboardList.innerHTML = "";

            podium.style.display = "none";
            rankingsSection.style.display = "none";

            emptyLeaderboard.style.display = "block";
        }

        function showRankings() {
            emptyLeaderboard.style.display = "none";
            podium.style.display = "";
            rankingsSection.style.display = "";
        }

        function clearRankings() {
            podium.innerHTML = "";
            leaderboardList.innerHTML = "";
        }

        // ==========================================
        // ACHIEVEMENT BADGES
        // ==========================================

        function getAchievement(rank) {
            if (rank === 1) {
                return {
                    label: "Champion",
                    icon: "🏆"
                };
            }

            if (rank === 2) {
                return {
                    label: "Silver Achiever",
                    icon: "🥈"
                };
            }

            if (rank === 3) {
                return {
                    label: "Bronze Achiever",
                    icon: "🥉"
                };
            }

            return null;
        }

        function createAchievementBadge(rank) {
            const achievement = getAchievement(rank);

            if (!achievement) return null;

            const badge = createElement(
                "span",
                "learner-achievement-badge"
            );

            badge.textContent =
                achievement.icon + " " + achievement.label;

            badge.style.display = "inline-block";
            badge.style.padding = "5px 10px";
            badge.style.marginTop = "8px";
            badge.style.borderRadius = "20px";
            badge.style.background = "#e8f4f3";
            badge.style.color = "#146c69";
            badge.style.fontSize = "12px";
            badge.style.fontWeight = "600";

            return badge;
        }

        // ==========================================
        // TOP THREE PODIUM
        // ==========================================

        function renderPodium(learners) {
            podium.innerHTML = "";

            const topThree = learners.slice(0, 3);

            topThree.forEach(function (learner, index) {
                const rank = getRank(learner, index);
                const name = getName(learner);

                const card = createElement(
                    "div",
                    "leaderboard-podium-card"
                );

                card.style.background = "#ffffff";
                card.style.border = "1px solid #e2e8f0";
                card.style.borderRadius = "14px";
                card.style.padding = "20px";
                card.style.textAlign = "center";
                card.style.flex = "1";
                card.style.minWidth = "180px";
                card.style.boxShadow =
                    "0 4px 12px rgba(0,0,0,0.05)";

                if (rank === 1) {
                    card.style.border = "2px solid #eab308";
                }

                if (isCurrentLearner(learner)) {
                    card.style.background = "#effcf9";
                }

                const rankIcon =
                    rank === 1 ? "🥇" :
                    rank === 2 ? "🥈" : "🥉";

                const medal = createElement(
                    "div",
                    "leaderboard-medal",
                    rankIcon
                );

                medal.style.fontSize = "36px";

                const avatar = createElement(
                    "div",
                    "leaderboard-avatar",
                    getInitials(name)
                );

                avatar.style.width = "55px";
                avatar.style.height = "55px";
                avatar.style.borderRadius = "50%";
                avatar.style.background = "#123b61";
                avatar.style.color = "#ffffff";
                avatar.style.display = "flex";
                avatar.style.alignItems = "center";
                avatar.style.justifyContent = "center";
                avatar.style.margin = "12px auto";
                avatar.style.fontWeight = "700";

                const learnerName = createElement(
                    "h3",
                    "leaderboard-learner-name",
                    name + (
                        isCurrentLearner(learner)
                            ? " (You)"
                            : ""
                    )
                );

                const school = createElement(
                    "p",
                    "leaderboard-school",
                    learner.schoolName || ""
                );

                const points = createElement(
                    "p",
                    "leaderboard-points",
                    getPoints(learner) + " points"
                );

                points.style.fontWeight = "700";
                points.style.color = "#123b61";

                const quizzes = createElement(
                    "p",
                    "leaderboard-quizzes",
                    getQuizzesTaken(learner) +
                    " quizzes completed"
                );

                card.appendChild(medal);
                card.appendChild(avatar);
                card.appendChild(learnerName);

                if (learner.schoolName) {
                    card.appendChild(school);
                }

                card.appendChild(points);
                card.appendChild(quizzes);

                const badge = createAchievementBadge(rank);

                if (badge) {
                    card.appendChild(badge);
                }

                podium.appendChild(card);
            });

            podium.style.display = "flex";
            podium.style.flexWrap = "wrap";
            podium.style.gap = "16px";
            podium.style.justifyContent = "center";
        }

        // ==========================================
        // RANKINGS LIST
        // ==========================================

        function renderLeaderboardList(learners) {
            leaderboardList.innerHTML = "";

            learners.forEach(function (learner, index) {
                const rank = getRank(learner, index);
                const name = getName(learner);

                const row = createElement(
                    "div",
                    "leaderboard-ranking-row"
                );

                row.style.display = "flex";
                row.style.alignItems = "center";
                row.style.justifyContent = "space-between";
                row.style.flexWrap = "wrap";
                row.style.gap = "12px";
                row.style.padding = "15px";
                row.style.marginBottom = "10px";
                row.style.borderRadius = "10px";
                row.style.border = "1px solid #e2e8f0";
                row.style.background = isCurrentLearner(learner)
                    ? "#effcf9"
                    : "#ffffff";

                const left = createElement(
                    "div",
                    "leaderboard-ranking-info"
                );

                left.style.display = "flex";
                left.style.alignItems = "center";
                left.style.gap = "14px";

                const rankNumber = createElement(
                    "strong",
                    "leaderboard-rank-number",
                    "#" + rank
                );

                rankNumber.style.minWidth = "35px";
                rankNumber.style.color = "#123b61";

                const details = createElement(
                    "div",
                    "leaderboard-ranking-details"
                );

                const nameElement = createElement(
                    "strong",
                    "leaderboard-ranking-name",
                    name + (
                        isCurrentLearner(learner)
                            ? " (You)"
                            : ""
                    )
                );

                const extra = createElement(
                    "p",
                    "leaderboard-ranking-extra",
                    getQuizzesTaken(learner) +
                    " quizzes completed"
                );

                extra.style.margin = "4px 0 0";
                extra.style.fontSize = "13px";
                extra.style.color = "#64748b";

                details.appendChild(nameElement);
                details.appendChild(extra);

                left.appendChild(rankNumber);
                left.appendChild(details);

                const right = createElement(
                    "div",
                    "leaderboard-ranking-score"
                );

                right.style.textAlign = "right";

                const points = createElement(
                    "strong",
                    "leaderboard-ranking-points",
                    getPoints(learner) + " pts"
                );

                right.appendChild(points);

                const average = getAverage(learner);

                if (average !== null) {
                    const averageElement = createElement(
                        "p",
                        "leaderboard-ranking-average",
                        "Average: " + average
                    );

                    averageElement.style.margin = "4px 0 0";
                    averageElement.style.fontSize = "12px";
                    averageElement.style.color = "#64748b";

                    right.appendChild(averageElement);
                }

                const badge = createAchievementBadge(rank);

                if (badge) {
                    details.appendChild(badge);
                }

                row.appendChild(left);
                row.appendChild(right);

                leaderboardList.appendChild(row);
            });
        }

        // ==========================================
        // RENDER ALL RANKINGS
        // ==========================================

        function renderRankings(data) {
            const learners = data
                .filter(function (learner) {
                    return getPoints(learner) > 0;
                })
                .sort(function (a, b) {
                    return getPoints(b) - getPoints(a);
                });

            if (learners.length === 0) {
                showEmpty();
                return;
            }

            showRankings();
            renderPodium(learners);
            renderLeaderboardList(learners);
        }

        // ==========================================
        // LOAD RANKINGS FROM BACKEND
        // ==========================================

        async function loadLeaderboard(period) {
            const thisRequest = ++requestNumber;

            currentPeriod = period;

            clearRankings();

            emptyLeaderboard.style.display = "none";
            podium.style.display = "none";
            rankingsSection.style.display = "none";

            showStatus("Loading leaderboard...");

            try {
                const endpoint =
                    "/leaderboards/rankings?period=" +
                    encodeURIComponent(period);

                const response = await EduQuestAPI.get(endpoint);

                if (thisRequest !== requestNumber) {
                    return;
                }

                if (!Array.isArray(response)) {
                    throw new Error(
                        "Invalid leaderboard response from server."
                    );
                }

                renderRankings(response);
                showStatus("");

            } catch (error) {
                if (thisRequest !== requestNumber) {
                    return;
                }

                console.error(
                    "Leaderboard loading error:",
                    error
                );

                clearRankings();

                emptyLeaderboard.style.display = "none";
                podium.style.display = "none";
                rankingsSection.style.display = "none";

                showStatus(
                    "Unable to load leaderboard: " +
                    (error.message || "Unknown error")
                );
            }
        }

        // ==========================================
        // PERIOD BUTTONS
        // ==========================================

        periodButtons.forEach(function (button) {
            button.addEventListener("click", function () {
                const period = button.dataset.period;

                if (!["week", "month", "all"].includes(period)) {
                    return;
                }

                periodButtons.forEach(function (item) {
                    item.classList.remove("active");
                    item.setAttribute(
                        "aria-pressed",
                        "false"
                    );
                });

                button.classList.add("active");
                button.setAttribute(
                    "aria-pressed",
                    "true"
                );

                loadLeaderboard(period);
            });
        });

        // ==========================================
        // INITIAL LOAD
        // ==========================================

        const selectedButton =
            document.querySelector(
                '[data-period].active'
            );

        const initialPeriod = selectedButton &&
            ["week", "month", "all"].includes(
                selectedButton.dataset.period
            )
                ? selectedButton.dataset.period
                : "all";

        loadLeaderboard(initialPeriod);

    });
})();
