
const reportSummary = {
    totalLearners: 512,
    activeLearners: 398,
    quizzesCompleted: 1842,
    achievementsEarned: 623,
    competitionsHeld: 8
};


// ============================================
// 2. LEARNER PROGRESS DATA
// ============================================

const progressData = {
    averageQuizScore: 73,
    learnersPassed: 76,
    needImprovement: 24
};


// ============================================
// 3. LEARNERS BY REGION
// ============================================

const provinceData = [
    { label: "Nelson Mandela Bay", value: 120, color: "var(--report-c1)" },
    { label: "Buffallo City", value: 85, color: "var(--report-c2)" },
    { label: "Sarah Baartman", value: 150, color: "var(--report-c3)" },
    { label: "Chris Hani", value: 100, color: "var(--report-c4)" },
    { label: "Joe Gqabi", value: 45, color: "var(--report-c5)" }
];


// ============================================
// 4. LEARNERS BY GRADE
// ============================================

const gradeData = [
    { label: "Grade 10", value: 91 },
    { label: "Grade 11", value: 83 },
    { label: "Grade 12", value: 110 }
];


// ============================================
// 5. AVERAGE QUIZ PERFORMANCE BY GRADE
// ============================================

const perfData = [
    { label: "Grade 10", value: 75 },
    { label: "Grade 11", value: 71 },
    { label: "Grade 12", value: 79 }
];


// ============================================
// 6. QUIZ PERFORMANCE BY SUBJECT
// ============================================

const quizPerf = [
    {
        subject: "Mathematics",
        attempts: 245,
        avg: 74,
        pass: 81
    },
    {
        subject: "Physical Sciences",
        attempts: 210,
        avg: 70,
        pass: 79
    },
    {
        subject: "Accounting",
        attempts: 176,
        avg: 73,
        pass: 80
    }
];


// ============================================
// 7. MOST ACCESSED RESOURCES
// ============================================

const resources = [
    {
        name: "Accounting Study Guide",
        subject: "Accounting",
        grade: "Grade 12",
        views: 342
    },
    {
        name: "Mathematics Revision Notes",
        subject: "Mathematics",
        grade: "Grade 10",
        views: 289
    },
    {
        name: "Physical Sciences Practice Test",
        subject: "Physical Sciences",
        grade: "Grade 11",
        views: 210
    },
    {
        name: "Mathematics Study Guide",
        subject: "Mathematics",
        grade: "Grade 10",
        views: 241
    },
    {
        name: "Accounting Past Paper",
        subject: "Accounting",
        grade: "Grade 12",
        views: 198
    }
];


// ============================================
// 8. GET HTML ELEMENTS
// ============================================

const provinceFilter = document.getElementById("fProvince");
const gradeFilter = document.getElementById("fGrade");
const subjectFilter = document.getElementById("fSubject");
const dateFilter = document.getElementById("fDate");

const applyBtn = document.getElementById("applyBtn");
const resetBtn = document.getElementById("resetBtn");
const exportBtn = document.getElementById("exportBtn");

const filterStatus = document.getElementById("filterStatus");

const summaryContainer = document.getElementById("reportSummary");
const progressContainer = document.getElementById("progressStats");


// ============================================
// 9. RENDER SUMMARY CARDS
// Values come from JavaScript data
// ============================================

function renderSummary() {

    const activePercentage = reportSummary.totalLearners > 0
        ? Math.round(
            (reportSummary.activeLearners /
             reportSummary.totalLearners) * 100
          )
        : 0;

    const summaryCards = [
        {
            label: "Total learners",
            value: reportSummary.totalLearners.toLocaleString()
        },
        {
            label: "Active learners",
            value: reportSummary.activeLearners.toLocaleString(),
            percentage: activePercentage + "%"
        },
        {
            label: "Quizzes completed",
            value: reportSummary.quizzesCompleted.toLocaleString()
        },
        {
            label: "Achievements earned",
            value: reportSummary.achievementsEarned.toLocaleString()
        },
        {
            label: "Competitions held",
            value: reportSummary.competitionsHeld.toLocaleString()
        }
    ];

    summaryContainer.innerHTML = summaryCards.map(card => `
        <div class="summary-card">

            <div class="summary-label">
                ${card.label}
            </div>

            <div class="summary-value">
                ${card.value}

                ${card.percentage
                    ? `<span>${card.percentage}</span>`
                    : ""
                }
            </div>

        </div>
    `).join("");
}


// ============================================
// 10. RENDER LEARNER PROGRESS STATS
// ============================================

function renderProgressStats() {

    const stats = [
        {
            label: "Average quiz score",
            value: progressData.averageQuizScore + "%"
        },
        {
            label: "Quizzes completed",
            value: reportSummary.quizzesCompleted.toLocaleString()
        },
        {
            label: "Learners who passed",
            value: progressData.learnersPassed + "%"
        },
        {
            label: "Need improvement",
            value: progressData.needImprovement + "%"
        }
    ];

    progressContainer.innerHTML = stats.map(stat => `
        <div class="mini-stat">

            <div class="mini-label">
                ${stat.label}
            </div>

            <div class="mini-value">
                ${stat.value}
            </div>

        </div>
    `).join("");
}


// ============================================
// 11. RENDER BAR CHARTS
// ============================================

function renderBars(containerId, data, highlightLabel) {

    const container = document.getElementById(containerId);

    if (!data.length) {
        container.innerHTML = "<p>No data available.</p>";
        return;
    }

    const maxValue = Math.max(...data.map(item => item.value));

    container.innerHTML = data.map(item => {

        const percentage = maxValue > 0
            ? Math.round((item.value / maxValue) * 100)
            : 0;

        const highlighted = item.label === highlightLabel;

        const color = item.color || "var(--report-teal)";

        const displayValue = containerId === "perfChart"
            ? item.value + "%"
            : item.value.toLocaleString();

        return `
            <div class="bar-row ${highlighted ? "highlight" : ""}">

                <div class="bar-label">
                    ${item.label}
                </div>

                <div class="bar-track">

                    <div class="bar-fill"
                         style="width: ${percentage}%; background: ${color};">
                    </div>

                </div>

                <div class="bar-val">
                    ${displayValue}
                </div>

            </div>
        `;

    }).join("");
}


// ============================================
// 12. RENDER QUIZ PERFORMANCE TABLE
// ============================================

function renderQuizPerf(selectedSubject) {

    const tbody = document.getElementById("quizPerfBody");

    const filteredData = quizPerf.filter(quiz => {

        return selectedSubject === "all" ||
               quiz.subject === selectedSubject;

    });

    if (filteredData.length === 0) {

        tbody.innerHTML = `
            <tr>
                <td colspan="4" style="text-align:center;">
                    No data for this subject yet
                </td>
            </tr>
        `;

        return;
    }

    tbody.innerHTML = filteredData.map(quiz => `
        <tr>

            <td style="font-weight:600;">
                ${quiz.subject}
            </td>

            <td>${quiz.attempts.toLocaleString()}</td>

            <td>${quiz.avg}%</td>

            <td>
                <span class="pass-pill">
                    ${quiz.pass}%
                </span>
            </td>

        </tr>
    `).join("");
}


// ============================================
// 13. RENDER RESOURCE TABLE
// ============================================

function renderResources(selectedGrade) {

    const tbody = document.getElementById("resourceBody");

    const filteredResources = resources
        .filter(resource => {

            return selectedGrade === "all" ||
                   resource.grade === selectedGrade;

        })
        .sort((a, b) => b.views - a.views);

    if (filteredResources.length === 0) {

        tbody.innerHTML = `
            <tr>
                <td colspan="4" style="text-align:center;">
                    No resource views for this grade yet
                </td>
            </tr>
        `;

        return;
    }

    tbody.innerHTML = filteredResources.map(resource => `
        <tr>

            <td style="font-weight:600;">
                ${resource.name}
            </td>

            <td>${resource.subject}</td>

            <td>${resource.grade}</td>

            <td>${resource.views.toLocaleString()}</td>

        </tr>
    `).join("");
}


// ============================================
// 14. APPLY REPORT FILTERS
// ============================================

function renderAll() {

    const province = provinceFilter.value;
    const grade = gradeFilter.value;
    const subject = subjectFilter.value;
    const date = dateFilter.value;


    // Province chart
    renderBars(
        "provinceChart",
        provinceData,
        province !== "all" ? province : null
    );


    // Grade chart
    renderBars(
        "gradeChart",
        gradeData,
        grade !== "all" ? grade : null
    );


    // Quiz performance chart
    renderBars(
        "perfChart",
        perfData,
        grade !== "all" ? grade : null
    );


    // Quiz performance table
    renderQuizPerf(subject);


    // Resource usage table
    renderResources(grade);


    // Update filter description
    const parts = [];

    parts.push(
        date === "All time"
            ? "all-time data"
            : date.toLowerCase()
    );

    parts.push(
        province === "all"
            ? "all provinces"
            : province
    );

    if (grade !== "all") {
        parts.push(grade);
    }

    if (subject !== "all") {
        parts.push(subject);
    }

    filterStatus.textContent = "Showing " + parts.join(", ");
}


// ============================================
// 15. APPLY FILTERS BUTTON
// ============================================

applyBtn.addEventListener("click", function () {

    renderAll();

});


// ============================================
// 16. RESET FILTERS BUTTON
// ============================================

resetBtn.addEventListener("click", function () {

    dateFilter.value = "All time";
    provinceFilter.value = "all";
    gradeFilter.value = "all";
    subjectFilter.value = "all";

    renderAll();

});


// ============================================
// 17. EXPORT FILTERED TABLES TO CSV
// ============================================

exportBtn.addEventListener("click", function () {

    const selectedSubject = subjectFilter.value;
    const selectedGrade = gradeFilter.value;

    const filteredQuizzes = quizPerf.filter(quiz =>
        selectedSubject === "all" ||
        quiz.subject === selectedSubject
    );

    const filteredResources = resources
        .filter(resource =>
            selectedGrade === "all" ||
            resource.grade === selectedGrade
        )
        .sort((a, b) => b.views - a.views);


    const csvRows = [
        ["EDUQUEST REPORT"],
        ["Date range", dateFilter.value],
        ["Province", provinceFilter.value],
        ["Grade", selectedGrade],
        ["Subject", selectedSubject],
        [],
        ["QUIZ PERFORMANCE"],
        ["Subject", "Attempts", "Average Score", "Pass Rate"]
    ];


    filteredQuizzes.forEach(quiz => {

        csvRows.push([
            quiz.subject,
            quiz.attempts,
            quiz.avg + "%",
            quiz.pass + "%"
        ]);

    });


    csvRows.push(
        [],
        ["MOST ACCESSED RESOURCES"],
        ["Resource", "Subject", "Grade", "Views"]
    );


    filteredResources.forEach(resource => {

        csvRows.push([
            resource.name,
            resource.subject,
            resource.grade,
            resource.views
        ]);

    });


    // Convert data to CSV
    const csvContent = csvRows.map(row => {

        return row.map(value => {

            const safeValue = String(value ?? "")
                .replace(/"/g, '""');

            return `"${safeValue}"`;

        }).join(",");

    }).join("\n");


    // Create downloadable file
    const blob = new Blob(
        [csvContent],
        { type: "text/csv;charset=utf-8;" }
    );

    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");

    link.href = url;
    link.download = "EduQuest_Report.csv";

    document.body.appendChild(link);

    link.click();

    document.body.removeChild(link);

    URL.revokeObjectURL(url);

});


// ============================================
// 18. INITIAL PAGE LOAD
// ============================================

// Generate summary cards from JavaScript
renderSummary();

// Generate learner progress stats
renderProgressStats();

// Generate charts and tables
renderAll();