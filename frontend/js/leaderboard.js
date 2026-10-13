
/*
========================================================
EduQuest - Admin Leaderboard

CHANGED:
- No dummy learners or scores.
- Rankings are retrieved from the backend.
- Only learners with points above zero appear.
- Supports subject filtering.
- Supports province filtering.
- Displays the top three learners.
- Displays the top learner per province.
- Shows an empty state when nobody has points.

Backend endpoints:

GET /api/subjects
GET /api/provinces
GET /api/leaderboards/rankings

========================================================
*/

(function () {

    "use strict";

    // ==============================================
    // AUTHENTICATION
    // ==============================================

    const LOGIN_URL =
        "../../EduQuest-with-js/EduQuest/login.html";

    const user = EduQuestAPI.getCurrentUser();

    if (
        !EduQuestAPI.getToken() ||
        !user ||
        user.role !== "Admin"
    ) {
        window.location.href = LOGIN_URL;
        return;
    }

    // ==============================================
    // HTML ELEMENTS
    // ==============================================

    const topLearnersContainer =
        document.getElementById("topLearners");

    const regionalLeadersContainer =
        document.getElementById("regionalLeaders");

    const regionFilter =
        document.getElementById("regionFilter");

    const rankingTitle =
        document.getElementById("rankingTitle");

    const rankingDescription =
        document.getElementById("rankingDescription");

    const subjectTabsContainer =
        document.getElementById("subjectTabs");

    const leaderboardStatus =
        document.getElementById("leaderboardStatus");

    // ==============================================
    // DATA
    // ==============================================

    let provinces = [];

    let entries = [];

    let currentSubjectId = null;

    let currentSubjectName = "Overall";

    // NEW:
    // Prevent older API responses from replacing
    // newer selections when tabs are clicked quickly.
    let requestNumber = 0;

    // ==============================================
    // CREATE HTML ELEMENT
    // ==============================================

    function el(tag, className, text) {

        const node = document.createElement(tag);

        if (className) {
            node.className = className;
        }

        if (text !== undefined) {
            node.textContent = text;
        }

        return node;
    }

    // ==============================================
    // LEARNER INITIALS
    // ==============================================

    function initials(name) {

        if (!name) {
            return "L";
        }

        return name
            .split(" ")
            .filter(Boolean)
            .map(function (part) {
                return part.charAt(0);
            })
            .join("")
            .substring(0, 2)
            .toUpperCase();
    }

    // ==============================================
    // STATUS MESSAGE
    // ==============================================

    function showStatus(message) {

        leaderboardStatus.textContent = message;

        leaderboardStatus.style.display =
            message ? "block" : "none";
    }

    // ==============================================
    // EMPTY STATE
    // ==============================================

    function emptyNote(container, message) {

        container.innerHTML = "";

        const note = el(
            "p",
            "leaderboard-empty",
            message
        );

        container.appendChild(note);
    }

    // ==============================================
    // NEW: QUALIFYING LEARNERS
    // ==============================================

    function getQualifiedEntries() {

        // Backend already excludes zero-point learners.
        // This is an additional frontend safeguard.

        return entries.filter(function (learner) {

            return Number(learner.points) > 0;
        });
    }

    // ==============================================
    // NATIONAL / PROVINCIAL TOP THREE
    // ==============================================

    function displayTopLearners() {

        const provinceId = regionFilter.value;

        const qualifiedEntries = getQualifiedEntries();

        // Apply the selected province filter.
        const rows = qualifiedEntries.filter(
            function (learner) {

                return (
                    !provinceId ||
                    String(learner.provinceID) ===
                    provinceId
                );
            }
        );

        topLearnersContainer.innerHTML = "";

        // ==========================================
        // NEW: EMPTY STATE
        // ==========================================

        if (rows.length === 0) {

            emptyNote(
                topLearnersContainer,
                "No learners have earned points " +
                "for this selection yet."
            );

            return;
        }

        // Backend already sorts learners by points.
        // Take the first three qualifying learners.

        rows.slice(0, 3).forEach(
            function (learner, index) {

                // NEW:
                // Ranking within the selected region.
                const rank = index + 1;

                let rankClass = "";

                if (rank === 1) {
                    rankClass = "rank-one";
                }
                else if (rank === 2) {
                    rankClass = "rank-two";
                }
                else {
                    rankClass = "rank-three";
                }

                const card = el(
                    "div",
                    "top-learner-card" +
                    (rank === 1 ? " first-place" : "")
                );

                // Rank number.
                const rankCircle = el(
                    "div",
                    "rank-circle " + rankClass,
                    String(rank)
                );

                // Learner name.
                const learnerName = el(
                    "h3",
                    "learner-name",
                    learner.displayName
                );

                // School and province.
                const school = el(
                    "p",
                    "learner-school",
                    (learner.schoolName || "School unavailable") +
                    " \u00B7 " +
                    (learner.provinceName || "Province unavailable")
                );

                // Points and quizzes completed.
                const score = el(
                    "p",
                    "learner-score",
                    learner.points +
                    " pts \u00B7 " +
                    learner.quizzesTaken +
                    " quizzes"
                );

                card.appendChild(rankCircle);
                card.appendChild(learnerName);
                card.appendChild(school);
                card.appendChild(score);

                topLearnersContainer.appendChild(card);
            }
        );
    }

    // ==============================================
    // TOP LEARNER PER PROVINCE
    // ==============================================

    function displayRegionalLeaders() {

        regionalLeadersContainer.innerHTML = "";

        const qualifiedEntries = getQualifiedEntries();

        let any = false;

        provinces.forEach(function (province) {

            // Backend results are already ordered.
            // Find the first learner in this province.

            const topLearner = qualifiedEntries.find(
                function (learner) {

                    return String(learner.provinceID) ===
                        String(province.provinceID);
                }
            );

            // No learner with points in this province.
            if (!topLearner) {
                return;
            }

            any = true;

            const card = el(
                "div",
                "regional-card"
            );

            // Province name.
            const regionName = el(
                "p",
                "region-name",
                province.provinceName
            );

            card.appendChild(regionName);

            const learner = el(
                "div",
                "regional-learner"
            );

            // Avatar.
            const avatar = el(
                "div",
                "learner-initials",
                initials(topLearner.displayName)
            );

            learner.appendChild(avatar);

            // Learner details.
            const info = el(
                "div",
                "regional-learner-info"
            );

            const name = el(
                "h3",
                "regional-learner-name",
                topLearner.displayName
            );

            const school = el(
                "p",
                "regional-school",
                topLearner.schoolName ||
                "School unavailable"
            );

            info.appendChild(name);
            info.appendChild(school);

            learner.appendChild(info);

            card.appendChild(learner);

            // Total points.
            const score = el(
                "p",
                "regional-score",
                topLearner.points + " pts"
            );

            card.appendChild(score);

            regionalLeadersContainer.appendChild(card);
        });

        // ==========================================
        // NEW: NO REGIONAL RANKINGS
        // ==========================================

        if (!any) {

            emptyNote(
                regionalLeadersContainer,
                "No learners have earned points " +
                "in any province yet."
            );
        }
    }

    // ==============================================
    // UPDATE PAGE HEADING
    // ==============================================

    function updateHeading() {

        const selected =
            regionFilter.options[
                regionFilter.selectedIndex
            ];

        if (!regionFilter.value) {

            rankingTitle.textContent = "nationally";

            rankingDescription.textContent =
                currentSubjectName +
                " ranking, all provinces";
        }
        else {

            rankingTitle.textContent =
                "in " + selected.textContent;

            rankingDescription.textContent =
                currentSubjectName +
                " ranking in " +
                selected.textContent;
        }
    }

    // ==============================================
    // RENDER LEADERBOARD
    // ==============================================

    function render() {

        displayTopLearners();

        displayRegionalLeaders();

        updateHeading();
    }

    // ==============================================
    // LOAD REAL BACKEND RANKINGS
    // ==============================================

    async function loadRankings() {

        // Track the latest request.
        const thisRequest = ++requestNumber;

        // NEW:
        // Clear previous rankings before fetching.
        entries = [];

        topLearnersContainer.innerHTML = "";

        regionalLeadersContainer.innerHTML = "";

        showStatus("Loading leaderboard...");

        // Backend query.
        let path =
            "/leaderboards/rankings?period=all";

        // Optional subject filter.
        if (currentSubjectId !== null) {

            path +=
                "&subjectId=" +
                encodeURIComponent(currentSubjectId);
        }

        try {

            // GET actual quiz-based rankings.
            const data = await EduQuestAPI.get(path);

            // Ignore outdated responses.
            if (thisRequest !== requestNumber) {
                return;
            }

            if (!Array.isArray(data)) {

                throw new Error(
                    "Invalid leaderboard response."
                );
            }

            // ======================================
            // NEW: NO DUMMY DATA
            // ======================================

            // Only backend results are used.
            entries = data;

            render();

            showStatus("");

        } catch (error) {

            if (thisRequest !== requestNumber) {
                return;
            }

            entries = [];

            topLearnersContainer.innerHTML = "";

            regionalLeadersContainer.innerHTML = "";

            // API failure is different from
            // an empty leaderboard.
            showStatus(
                "Unable to load leaderboard: " +
                error.message
            );
        }
    }

    // ==============================================
    // BUILD SUBJECT TABS
    // ==============================================

    function buildSubjectTabs(subjects) {

        subjectTabsContainer.innerHTML = "";

        function addTab(label, subjectId, active) {

            const button = el(
                "button",
                "subject-tab" +
                (active ? " active" : ""),
                label
            );

            button.type = "button";

            button.addEventListener(
                "click",
                function () {

                    // Remove active state.
                    subjectTabsContainer
                        .querySelectorAll(".subject-tab")
                        .forEach(function (tab) {

                            tab.classList.remove("active");
                        });

                    // Highlight selected subject.
                    button.classList.add("active");

                    currentSubjectId = subjectId;

                    currentSubjectName = label;

                    // Reload actual rankings.
                    loadRankings();
                }
            );

            subjectTabsContainer.appendChild(button);
        }

        // Overall leaderboard.
        addTab("Overall", null, true);

        // Actual subjects from the database.
        subjects.forEach(function (subject) {

            addTab(
                subject.subjectName,
                subject.subjectID,
                false
            );
        });
    }

    // ==============================================
    // BUILD PROVINCE FILTER
    // ==============================================

    function buildRegionFilter() {

        regionFilter.innerHTML = "";

        // Default option.
        const all = el(
            "option",
            "",
            "All provinces"
        );

        all.value = "";

        regionFilter.appendChild(all);

        // Actual provinces from the database.
        provinces.forEach(function (province) {

            const option = el(
                "option",
                "",
                province.provinceName
            );

            option.value = province.provinceID;

            regionFilter.appendChild(option);
        });
    }

    // ==============================================
    // REGION FILTER EVENT
    // ==============================================

    regionFilter.addEventListener(
        "change",
        function () {

            // Filter existing backend rankings.
            render();
        }
    );

    // ==============================================
    // INITIAL LOAD
    // ==============================================

    async function initializeLeaderboard() {

        showStatus("Loading leaderboard...");

        try {

            // Load subjects and provinces.
            const results = await Promise.all([
                EduQuestAPI.get("/subjects"),
                EduQuestAPI.get("/provinces")
            ]);

            if (
                !Array.isArray(results[0]) ||
                !Array.isArray(results[1])
            ) {
                throw new Error(
                    "Invalid subjects or provinces response."
                );
            }

            // Subject tabs.
            buildSubjectTabs(results[0]);

            // Province options.
            provinces = results[1];

            buildRegionFilter();

            // Load actual rankings.
            await loadRankings();

        } catch (error) {

            showStatus(
                "Unable to initialize leaderboard: " +
                error.message
            );
        }
    }

    initializeLeaderboard();

})();
