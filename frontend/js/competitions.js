
// =======================================
// MOCK COMPETITION DATA
// =======================================

let competitions = [

    {
        id: 1,
        name: "Accounting Masters",
        subject: "Accounting",
        grade: "Grade 11",
        term: "Term 3 2026",
        mode: "In-system",
        venue: "Runs live in EduQuest",
        startDate: "2026-08-10",
        endDate: "2026-08-17",
        status: "Completed"
    },

    {
        id: 2,
        name: "Accounting Past-Paper Sprint",
        subject: "Accounting",
        grade: "Grade 10",
        term: "Term 3 2026",
        mode: "In-system",
        venue: "Runs live in EduQuest",
        startDate: "2026-08-25",
        endDate: "2026-09-01",
        status: "Completed"
    },

    {
        id: 3,
        name: "Interschool Physics Expo",
        subject: "Physical Sciences",
        grade: "Grade 12",
        term: "Term 3 2026",
        mode: "Venue",
        venue: "Nelson Mandela Bay Science Centre",
        startDate: "2026-09-05",
        endDate: "2026-09-06",
        status: "Completed"
    },

    {
        id: 4,
        name: "Physical Sciences Olympiad Heats",
        subject: "Physical Sciences",
        grade: "Grade 11",
        term: "Term 3 2026",
        mode: "Venue",
        venue: "Grey High School Hall",
        startDate: "2026-09-15",
        endDate: "2026-09-15",
        status: "Awaiting results"
    },

    {
        id: 5,
        name: "Mathematics Challenge",
        subject: "Mathematics",
        grade: "Grade 10",
        term: "Term 3 2026",
        mode: "In-system",
        venue: "Runs live in EduQuest",
        startDate: "2026-09-20",
        endDate: "2026-09-27",
        status: "Active"
    },

    {
        id: 6,
        name: "Physical Sciences Sprint",
        subject: "Physical Sciences",
        grade: "Grade 10",
        term: "Term 4 2026",
        mode: "In-system",
        venue: "Runs live in EduQuest",
        startDate: "2026-10-01",
        endDate: "2026-10-07",
        status: "Upcoming"
    }
];


// =======================================
// SELECT HTML ELEMENTS
// =======================================

const competitionGrid =
    document.getElementById("competitionGrid");

const seasonFilter =
    document.getElementById("seasonFilter");

const modeFilter =
    document.getElementById("modeFilter");

const statusButtons =
    document.querySelectorAll(".status-filter");

const noCompetitions =
    document.getElementById("noCompetitions");

const addCompetitionBtn =
    document.getElementById("addCompetitionBtn");


// Currently selected status
let selectedStatus = "all";


// =======================================
// FORMAT DATE
// =======================================

function formatDate(date) {

    const options = {
        day: "2-digit",
        month: "short",
        year: "numeric"
    };

    return new Date(date + "T00:00:00")
        .toLocaleDateString("en-GB", options);
}


// =======================================
// DISPLAY COMPETITION CARDS
// =======================================

function displayCompetitions(list) {

    competitionGrid.innerHTML = "";

    // Show message if no results
    if (list.length === 0) {

        noCompetitions.style.display = "block";
        return;

    } else {

        noCompetitions.style.display = "none";
    }

    list.forEach(competition => {

        const modeClass =
            competition.mode === "Venue"
                ? "mode-venue"
                : "mode-system";

        let statusClass = "";

        switch (competition.status) {

            case "Upcoming":
                statusClass = "status-upcoming";
                break;

            case "Active":
                statusClass = "status-active";
                break;

            case "Awaiting results":
                statusClass = "status-awaiting";
                break;

            case "Completed":
                statusClass = "status-completed";
                break;
        }

        const card = document.createElement("div");

        card.className = "competition-card";

        card.innerHTML = `

            <div class="competition-card-top">

                <span class="competition-mode ${modeClass}">
                    ${competition.mode === "Venue"
                        ? "📍 Venue"
                        : "💻 In-system"}
                </span>

                <span class="competition-status ${statusClass}">
                    ${competition.status}
                </span>

            </div>

            <h3>${competition.name}</h3>

            <p class="competition-subject">
                ${competition.subject} · ${competition.grade}
            </p>

            <span class="competition-term">
                ${competition.term}
            </span>

            <div class="competition-details">

                <div class="competition-detail">
                    <span class="detail-icon">
                        ${competition.mode === "Venue" ? "📍" : "💻"}
                    </span>

                    <span>${competition.venue}</span>
                </div>

                <div class="competition-detail">
                    <span class="detail-icon">🗓️</span>

                    <span>
                        ${formatDate(competition.startDate)}
                        ${competition.startDate !== competition.endDate
                            ? " – " + formatDate(competition.endDate)
                            : " – " + formatDate(competition.endDate)}
                    </span>
                </div>

            </div>

            <div class="competition-actions">

                <button class="view-btn"
                        data-action="view"
                        data-id="${competition.id}">
                    View
                </button>

                <button class="edit-btn"
                        data-action="edit"
                        data-id="${competition.id}">
                    Edit
                </button>

                <button class="delete-btn"
                        data-action="delete"
                        data-id="${competition.id}">
                    Delete
                </button>

            </div>
        `;

        competitionGrid.appendChild(card);
    });
}


// =======================================
// FILTER COMPETITIONS
// =======================================

function filterCompetitions() {

    const selectedSeason = seasonFilter.value;
    const selectedMode = modeFilter.value;

    const filteredCompetitions = competitions.filter(competition => {

        // Season filter
        const matchesSeason =
            selectedSeason === "all" ||
            competition.term === selectedSeason;

        // Mode filter
        const matchesMode =
            selectedMode === "all" ||
            competition.mode === selectedMode;

        // Status filter
        const matchesStatus =
            selectedStatus === "all" ||
            competition.status === selectedStatus;

        // All selected filters must match
        return matchesSeason && matchesMode && matchesStatus;

    });

    displayCompetitions(filteredCompetitions);
}


// =======================================
// SEASON AND MODE FILTER EVENTS
// =======================================

seasonFilter.addEventListener("change", filterCompetitions);

modeFilter.addEventListener("change", filterCompetitions);


// =======================================
// STATUS BUTTON FILTER EVENTS
// =======================================

statusButtons.forEach(button => {

    button.addEventListener("click", function () {

        // Remove active styling from all buttons
        statusButtons.forEach(btn => {
            btn.classList.remove("active");
        });

        // Highlight the selected button
        this.classList.add("active");

        // Update selected status
        selectedStatus = this.dataset.status;

        // Apply all filters
        filterCompetitions();

    });

});


// =======================================
// VIEW, EDIT AND DELETE
// =======================================

competitionGrid.addEventListener("click", function (event) {

    const button = event.target.closest("button");

    if (!button) return;

    const action = button.dataset.action;
    const id = Number(button.dataset.id);

    const competition = competitions.find(c => c.id === id);

    if (!competition) return;


    // VIEW
    if (action === "view") {

        alert(
            `Competition: ${competition.name}\n` +
            `Subject: ${competition.subject}\n` +
            `Grade: ${competition.grade}\n` +
            `Mode: ${competition.mode}\n` +
            `Status: ${competition.status}\n` +
            `Venue: ${competition.venue}\n` +
            `Dates: ${formatDate(competition.startDate)} - ${formatDate(competition.endDate)}`
        );

    }


    // EDIT
    if (action === "edit") {

        const newName = prompt(
            "Edit competition name:",
            competition.name
        );

        if (newName && newName.trim() !== "") {

            competition.name = newName.trim();

            filterCompetitions();
        }

    }


    // DELETE
    if (action === "delete") {

        const confirmed = confirm(
            `Are you sure you want to delete ${competition.name}?`
        );

        if (confirmed) {

            competitions = competitions.filter(
                c => c.id !== id
            );

            filterCompetitions();
        }

    }

});


// =======================================
// ADD COMPETITION
// =======================================

addCompetitionBtn.addEventListener("click", function () {

    const name = prompt("Enter competition name:");

    if (!name || name.trim() === "") return;

    const subject = prompt("Enter subject:");

    if (!subject || subject.trim() === "") return;

    const grade = prompt("Enter grade (e.g. Grade 10):");

    if (!grade || grade.trim() === "") return;

    const mode = prompt(
        "Enter competition mode: In-system or Venue"
    );

    if (mode !== "In-system" && mode !== "Venue") {

        alert("Please enter In-system or Venue.");
        return;
    }

    const startDate = prompt("Enter start date (YYYY-MM-DD):");

    if (!startDate || isNaN(Date.parse(startDate))) {

        alert("Please enter a valid start date.");
        return;
    }

    const endDate = prompt("Enter end date (YYYY-MM-DD):");

    if (!endDate || isNaN(Date.parse(endDate))) {

        alert("Please enter a valid end date.");
        return;
    }

    if (endDate < startDate) {

        alert("End date cannot be before start date.");
        return;
    }

    const term = prompt(
        "Enter term (e.g. Term 3 2026):"
    );

    if (!term || term.trim() === "") return;

    const venue = mode === "In-system"
        ? "Runs live in EduQuest"
        : prompt("Enter venue name:");

    if (!venue || venue.trim() === "") return;

    // Determine status from dates
    const today = new Date().toISOString().split("T")[0];

    let status;

    if (endDate < today) {
        status = "Awaiting results";
    } else if (startDate > today) {
        status = "Upcoming";
    } else {
        status = "Active";
    }

    // Create new competition
    const newCompetition = {

        id: Date.now(),
        name: name.trim(),
        subject: subject.trim(),
        grade: grade.trim(),
        term: term.trim(),
        mode: mode,
        venue: venue.trim(),
        startDate: startDate,
        endDate: endDate,
        status: status
    };

    competitions.push(newCompetition);

    // Refresh the cards using selected filters
    filterCompetitions();

});


// =======================================
// INITIAL DISPLAY
// =======================================

displayCompetitions(competitions);