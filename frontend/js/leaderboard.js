/* =========================
   MOCK LEADERBOARD DATA
========================= */

const learners = [

    {
        name: "Amanda Mthembu",
        school: "Grens High School",
        region: "Nelson Mandela Bay",
        Mathematics: 920,
        "Physical Sciences": 940,
        Accounting: 910
    },

    {
        name: "Sihle Radebe",
        school: "Alexander Road High",
        region: "Nelson Mandela Bay",
        Mathematics: 880,
        "Physical Sciences": 910,
        Accounting: 890
    },

    {
        name: "Emelicia Peterson",
        school: "Grey High School",
        region: "Buffalo City",
        Mathematics: 850,
        "Physical Sciences": 890,
        Accounting: 860
    },

    {
        name: "Lindiwe Ngcobo",
        school: "Graeme College",
        region: "Sarah Baartman",
        Mathematics: 810,
        "Physical Sciences": 840,
        Accounting: 830
    },

    {
        name: "Zola Mahlangu",
        school: "Union High School",
        region: "Chris Hani",
        Mathematics: 790,
        "Physical Sciences": 820,
        Accounting: 800
    },

    {
        name: "Kagiso Moutang",
        school: "Aliwal North High",
        region: "Joe Gqabi",
        Mathematics: 780,
        "Physical Sciences": 805,
        Accounting: 790
    }

];


/* =========================
   ELEMENTS
========================= */

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

const subjectTabs =
    document.querySelectorAll(".subject-tab");


/* =========================
   CURRENT SUBJECT
========================= */

let currentSubject = "Overall";


/* =========================
   GET SCORE
========================= */

function getScore(learner) {

    if (currentSubject === "Overall") {

        const scores = [
            learner.Mathematics,
            learner["Physical Sciences"],
            learner.Accounting
        ];

        return Math.round(
            scores.reduce((total, score) => total + score, 0)
            / scores.length
        );
    }

    return learner[currentSubject];
}


/* =========================
   INITIALS
========================= */

function getInitials(name) {

    const names = name.split(" ");

    return names
        .map(name => name[0])
        .join("")
        .substring(0, 2)
        .toUpperCase();
}


/* =========================
   DISPLAY TOP THREE
========================= */

function displayTopLearners() {

    const selectedRegion = regionFilter.value;

    let filteredLearners = [...learners];

    if (selectedRegion !== "All regions") {

        filteredLearners = filteredLearners.filter(
            learner => learner.region === selectedRegion
        );
    }


    filteredLearners.sort(
        (a, b) => getScore(b) - getScore(a)
    );


    const topThree = filteredLearners.slice(0, 3);


    topLearnersContainer.innerHTML = "";


    topThree.forEach((learner, index) => {

        const rank = index + 1;

        let rankClass = "";

        if (rank === 1) {
            rankClass = "rank-one";
        } else if (rank === 2) {
            rankClass = "rank-two";
        } else {
            rankClass = "rank-three";
        }


        const card = document.createElement("div");

        card.className =
            `top-learner-card ${rank === 1 ? "first-place" : ""}`;


        card.innerHTML = `

            <div class="rank-circle ${rankClass}">
                ${rank}
            </div>

            <h3 class="learner-name">
                ${learner.name}
            </h3>

            <p class="learner-school">
                ${learner.school} · ${learner.region}
            </p>

            <p class="learner-score">
                ${getScore(learner)} pts
            </p>

            <span class="qualifier-badge">
                National qualifier
            </span>

        `;


        topLearnersContainer.appendChild(card);

    });
}


/* =========================
   DISPLAY TOP LEARNER
   FOR EACH REGION
========================= */

function displayRegionalLeaders() {

    regionalLeadersContainer.innerHTML = "";


    const regions = [
        "Nelson Mandela Bay",
        "Buffalo City",
        "Sarah Baartman",
        "Chris Hani",
        "Joe Gqabi"
    ];


    regions.forEach(region => {

        const regionalLearners = learners
            .filter(learner => learner.region === region)
            .sort(
                (a, b) => getScore(b) - getScore(a)
            );


        const topLearner = regionalLearners[0];


        if (!topLearner) {
            return;
        }


        const card = document.createElement("div");

        card.className = "regional-card";


        card.innerHTML = `

            <p class="region-name">
                ${region}
            </p>

            <div class="regional-learner">

                <div class="learner-initials">
                    ${getInitials(topLearner.name)}
                </div>

                <div class="regional-learner-info">

                    <h3 class="regional-learner-name">
                        ${topLearner.name}
                    </h3>

                    <p class="regional-school">
                        ${topLearner.school}
                    </p>

                </div>

            </div>

            <p class="regional-score">
                ${getScore(topLearner)} pts
            </p>

        `;


        regionalLeadersContainer.appendChild(card);

    });
}


/* =========================
   UPDATE LEADERBOARD
========================= */

function updateLeaderboard() {

    displayTopLearners();

    displayRegionalLeaders();


    if (regionFilter.value === "All regions") {

        rankingTitle.textContent = "national";

        rankingDescription.textContent =
            "Overall ranking, all regions";

    } else {

        rankingTitle.textContent =
            regionFilter.value;

        rankingDescription.textContent =
            `Top learners in ${regionFilter.value}`;

    }

}


/* =========================
   SUBJECT BUTTONS
========================= */

subjectTabs.forEach(button => {

    button.addEventListener("click", function () {

        subjectTabs.forEach(tab => {
            tab.classList.remove("active");
        });

        this.classList.add("active");

        currentSubject =
            this.dataset.subject;

        updateLeaderboard();

    });

});


/* =========================
   REGION FILTER
========================= */

regionFilter.addEventListener(
    "change",
    updateLeaderboard
);


/*
   LOAD PAGE */

updateLeaderboard();

