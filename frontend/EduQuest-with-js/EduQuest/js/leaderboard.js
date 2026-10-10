
/*
========================================================
EduQuest - Learner Leaderboard

This file handles:
1. Loading rankings from the backend.
2. Displaying the top three learners.
3. Displaying full rankings.
4. Switching between weekly, monthly and
   all-time rankings.
5. Displaying an empty leaderboard when
   nobody has earned points.

IMPORTANT:
No dummy data is used.

The backend calculates points and rankings.
========================================================
*/

(function () {

    "use strict";

    // ==============================================
    // AUTHENTICATION
    // ==============================================

    // Learners must be logged in to view rankings.

    if (!EduQuestAPI.requireAuth()) {
        return;
    }

    // ==============================================
    // HTML ELEMENTS
    // ==============================================

    const podium =
        document.getElementById("leaderboardPodium");

    const leaderboardList =
        document.getElementById("leaderboardList");

    const leaderboardStatus =
        document.getElementById("leaderboardStatus");

    const emptyLeaderboard =
        document.getElementById("emptyLeaderboard");

    const rankingsSection =
        document.getElementById(
            "leaderboardRankingsSection"
        );

    const tabs = document.querySelectorAll(
        ".leaderboard-tab[data-period]"
    );

    // Default leaderboard period.
    let selectedPeriod = "all";

    // ==============================================
    // HELPER FUNCTIONS
    // ==============================================

    function createElement(tag, className, text) {

        const element = document.createElement(tag);

        if (className) {
            element.className = className;
        }

        if (text !== undefined) {
            element.textContent = text;
        }

        return element;
    }

    // ==============================================
    // STATUS MESSAGES
    // ==============================================

    function showStatus(message) {

        leaderboardStatus.textContent = message;

        leaderboardStatus.style.display =
            message ? "block" : "none";
    }

    // ==============================================
    // GET LEARNER INITIAL
    // ==============================================

    function getInitial(name) {

        if (!name || name.length === 0) {
            return "L";
        }

        return name.charAt(0).toUpperCase();
    }

    // ==============================================
    // NEW: CREATE PODIUM CARD
    // ==============================================

    function createPodiumCard(learner) {

        let positionClass = "";

        if (learner.rank === 1) {
            positionClass = "first";
        }
        else if (learner.rank === 2) {
            positionClass = "second";
        }
        else if (learner.rank === 3) {
            positionClass = "third";
        }

        const card = createElement(
            "div",
            "podium-card " + positionClass
        );

        // Display ranking position.
        const rank = createElement(
            "div",
            "podium-medal",
            "#" + learner.rank
        );

        // Learner avatar.
        const avatar = createElement(
            "div",
            "profile-avatar",
            getInitial(learner.displayName)
        );

        // Learner name.
        const name = createElement(
            "div",
            "podium-name",
            learner.displayName
        );

        // Points earned.
        const points = createElement(
            "div",
            "podium-score",
            learner.points + " pts"
        );

        card.appendChild(rank);
        card.appendChild(avatar);
        card.appendChild(name);
        card.appendChild(points);

        return card;
    }

    // ==============================================
    // DISPLAY TOP THREE LEARNERS
    // ==============================================

    function renderPodium(learners) {

        // Clear previous rankings.
        podium.innerHTML = "";

        // Only take the top three learners.
        const topThree = learners.slice(0, 3);

        // Podium visual order:
        // Second place, first place, third place.
        const order = [1, 0, 2];

        order.forEach(function (index) {

            if (topThree[index]) {

                const card = createPodiumCard(
                    topThree[index]
                );

                podium.appendChild(card);
            }
        });
    }

    // ==============================================
    // NEW: CREATE LEADERBOARD ROW
    // ==============================================

    function createRankingRow(learner) {

        const row = createElement(
            "div",
            "dashboard-list-item"
        );

        // Highlight currently logged-in learner.
        if (learner.isCurrentUser) {
            row.classList.add("me");
        }

        // Ranking position.
        const rank = createElement(
            "span",
            "rank",
            "#" + learner.rank
        );

        // Learner avatar.
        const avatar = createElement(
            "span",
            "list-avatar",
            getInitial(learner.displayName)
        );

        // Learner name.
        const name = createElement(
            "span",
            "list-name",
            learner.displayName
        );

        // Total points.
        const points = createElement(
            "span",
            "list-score",
            learner.points + " pts"
        );

        row.appendChild(rank);
        row.appendChild(avatar);
        row.appendChild(name);
        row.appendChild(points);

        return row;
    }

    // ==============================================
    // DISPLAY FULL LEADERBOARD
    // ==============================================

    function renderLeaderboard(learners) {

        // Clear previously displayed rankings.
        podium.innerHTML = "";
        leaderboardList.innerHTML = "";

        // ==========================================
        // NEW: NO POINTS = EMPTY LEADERBOARD
        // ==========================================

        // Defensive filtering:
        // Only display learners with positive points.
        // The backend should already enforce this.

        const qualifiedLearners = learners.filter(
            function (learner) {

                return Number(learner.points) > 0;
            }
        );

        if (qualifiedLearners.length === 0) {

            // Hide podium.
            podium.style.display = "none";

            // Hide full rankings.
            rankingsSection.style.display = "none";

            // Show empty leaderboard message.
            emptyLeaderboard.style.display = "block";

            showStatus("");

            return;
        }

        // ==========================================
        // DISPLAY RANKINGS WHEN POINTS EXIST
        // ==========================================

        emptyLeaderboard.style.display = "none";

        podium.style.display = "";

        rankingsSection.style.display = "";

        // Backend already sorts the rankings.
        renderPodium(qualifiedLearners);

        qualifiedLearners.forEach(function (learner) {

            const row = createRankingRow(learner);

            leaderboardList.appendChild(row);
        });

        showStatus("");
    }

    // ==============================================
    // LOAD LEADERBOARD FROM BACKEND
    // ==============================================

    async function loadLeaderboard() {

        // Show loading message.
        showStatus("Loading leaderboard...");

        // Hide old results while loading.
        podium.style.display = "none";

        rankingsSection.style.display = "none";

        emptyLeaderboard.style.display = "none";

        try {

            // Request actual leaderboard data
            // from the backend controller.

            const learners = await EduQuestAPI.get(
                "/leaderboards/rankings?period=" +
                selectedPeriod
            );

            // Check that the backend returned a list.
            if (!Array.isArray(learners)) {

                throw new Error(
                    "Invalid leaderboard response."
                );
            }

            // Display actual learner rankings.
            renderLeaderboard(learners);

        } catch (error) {

            // ======================================
            // NEW: HANDLE BACKEND ERRORS
            // ======================================

            // Clear previous data.
            podium.innerHTML = "";

            leaderboardList.innerHTML = "";

            // Do not show an empty leaderboard
            // message when the API has failed.
            emptyLeaderboard.style.display = "none";

            showStatus(
                "Unable to load leaderboard: " +
                error.message
            );
        }
    }

    // ==============================================
    // LEADERBOARD PERIOD BUTTONS
    // ==============================================

    tabs.forEach(function (tab) {

        tab.addEventListener("click", function () {

            // Get selected period.
            selectedPeriod = tab.dataset.period;

            // Remove active class from all buttons.
            tabs.forEach(function (button) {

                button.classList.remove("active");
            });

            // Highlight selected button.
            tab.classList.add("active");

            // Reload leaderboard from backend.
            loadLeaderboard();
        });
    });

    // ==============================================
    // INITIAL PAGE LOAD
    // ==============================================

    // Load actual leaderboard rankings
    // when the page opens.

    loadLeaderboard();

})();
