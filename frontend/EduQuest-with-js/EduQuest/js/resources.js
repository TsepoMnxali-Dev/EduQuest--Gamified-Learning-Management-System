
/*
========================================================
EduQuest - Learner Resources

Learners can:
1. View resources uploaded by the admin.
2. Filter resources by subject.
3. Open external learning links.
4. View or download uploaded documents.

Learners cannot:
1. Add resources.
2. Edit resources.
3. Delete resources.

Resources come from the database.
========================================================
*/

(function () {
    "use strict";

    // ==============================================
    // AUTHENTICATION
    // ==============================================

    EduQuestAPI.requireAuth();

    if (!EduQuestAPI.getToken()) {
        return;
    }

    // ==============================================
    // TEMPORARILY DISABLED: LEARNER ROLE REDIRECT
    // ==============================================

    // DEBUGGING:
    // This section may have been redirecting
    // authenticated learners back to dashboard.html.
    //
    // We are commenting it out temporarily to
    // check whether the Resources page opens.
    //
    // Backend authorization remains enabled.

    /*
    const user = EduQuestAPI.getCurrentUser();

    if (!user || user.role !== "Learner") {
        window.location.href = "dashboard.html";
        return;
    }
    */

    // ==============================================
    // HTML ELEMENTS
    // ==============================================

    const resourcesGrid =
        document.getElementById("resourcesGrid");

    const emptyResources =
        document.getElementById("emptyResources");

    const resourcesStatus =
        document.getElementById("resourcesStatus");

    const resourceFilters =
        document.getElementById("resourceFilters");

    let materials = [];
    let selectedSubject = "all";

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

    function showStatus(message) {
        resourcesStatus.textContent = message;
        resourcesStatus.style.display =
            message ? "block" : "none";
    }

    // Only permit HTTP/HTTPS resource links.
    function isSafeUrl(url) {

        try {
            const parsed = new URL(url);

            return parsed.protocol === "http:" ||
                   parsed.protocol === "https:";
        } catch {
            return false;
        }
    }

    // ==============================================
    // FETCH FILE USING AUTHENTICATION TOKEN
    // ==============================================

    async function fetchResourceFile(id) {

        const response = await fetch(
            EduQuestAPI.BASE_URL +
            "/studymaterials/" + id + "/file",
            {
                headers: {
                    Authorization:
                        "Bearer " + EduQuestAPI.getToken()
                }
            }
        );

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error(
                    "Your session has expired. Please sign in again."
                );
            }

            throw new Error(
                "Unable to retrieve this document."
            );
        }

        return await response.blob();
    }

    // ==============================================
    // DOWNLOAD DOCUMENT
    // ==============================================

    async function downloadMaterial(material) {

        showStatus("Preparing download...");

        try {

            const blob = await fetchResourceFile(
                material.studyMaterialID
            );

            const fileUrl = URL.createObjectURL(blob);

            const link = document.createElement("a");

            link.href = fileUrl;
            link.download =
                material.fileName || "study-material";

            document.body.appendChild(link);
            link.click();
            link.remove();

            setTimeout(function () {
                URL.revokeObjectURL(fileUrl);
            }, 60000);

            showStatus("");

        } catch (error) {
            showStatus(error.message);
        }
    }

    // ==============================================
    // VIEW DOCUMENT OR RESOURCE LINK
    // ==============================================

    async function viewMaterial(material) {

        if (material.fileURL) {

            if (!isSafeUrl(material.fileURL)) {
                showStatus("Invalid resource link.");
                return;
            }

            window.open(
                material.fileURL,
                "_blank",
                "noopener,noreferrer"
            );

            return;
        }

        const newTab = window.open("", "_blank");

        if (!newTab) {
            showStatus(
                "Please allow popups to view this document."
            );
            return;
        }

        showStatus("Opening document...");

        try {

            const blob = await fetchResourceFile(
                material.studyMaterialID
            );

            const fileUrl = URL.createObjectURL(blob);

            newTab.location.href = fileUrl;

            setTimeout(function () {
                URL.revokeObjectURL(fileUrl);
            }, 60000);

            showStatus("");

        } catch (error) {

            newTab.close();
            showStatus(error.message);
        }
    }

    // ==============================================
    // CREATE RESOURCE CARD
    // ==============================================

    function createResourceCard(material) {

        const card = createElement(
            "article", "resource-card"
        );

        const type = createElement(
            "span",
            "resource-type",
            material.resourceType || "Study Material"
        );

        card.appendChild(type);

        const title = createElement(
            "h3",
            "",
            material.title || "Untitled Resource"
        );

        card.appendChild(title);

        const description = createElement(
            "p",
            "",
            material.description ||
            "Study material provided by the administrator."
        );

        card.appendChild(description);

        const meta = createElement(
            "div", "resource-meta"
        );

        const subject = createElement(
            "span",
            "",
            material.subjectName || "General"
        );

        const grade = createElement(
            "span",
            "",
            material.gradeLevel || ""
        );

        meta.appendChild(subject);
        meta.appendChild(grade);

        card.appendChild(meta);

        if (material.topicName) {

            const topic = createElement(
                "p",
                "",
                "Topic: " + material.topicName
            );

            card.appendChild(topic);
        }

        // ==========================================
        // LEARNER ACTIONS
        // ==========================================

        const actions = createElement(
            "div", "resource-actions"
        );

        const viewButton = createElement(
            "button", "btn btn-primary", "View"
        );

        viewButton.type = "button";

        viewButton.addEventListener("click", function () {
            viewMaterial(material);
        });

        actions.appendChild(viewButton);

        // Download only for uploaded documents.
        if (material.fileName) {

            const downloadButton = createElement(
                "button",
                "btn btn-outline",
                "Download"
            );

            downloadButton.type = "button";

            downloadButton.addEventListener(
                "click",
                function () {
                    downloadMaterial(material);
                }
            );

            actions.appendChild(downloadButton);
        }

        card.appendChild(actions);

        return card;
    }

    // ==============================================
    // DISPLAY RESOURCE CARDS
    // ==============================================

    function renderResources() {

        resourcesGrid.innerHTML = "";

        if (materials.length === 0) {

            emptyResources.style.display = "block";
            return;
        }

        emptyResources.style.display = "none";

        const filtered = materials.filter(function (m) {

            return (
                selectedSubject === "all" ||
                m.subjectName === selectedSubject
            );
        });

        if (filtered.length === 0) {

            showStatus(
                "No study materials available for this subject yet."
            );

            return;
        }

        showStatus("");

        filtered.forEach(function (material) {

            const card = createResourceCard(material);

            resourcesGrid.appendChild(card);
        });
    }

    // ==============================================
    // CREATE SUBJECT FILTERS
    // ==============================================

    function renderSubjectFilters() {

        resourceFilters.innerHTML = "";

        const subjects = new Set();

        materials.forEach(function (material) {

            if (material.subjectName) {
                subjects.add(material.subjectName);
            }
        });

        const allButton = createElement(
            "button",
            "leaderboard-tab",
            "All"
        );

        allButton.type = "button";

        if (selectedSubject === "all") {
            allButton.classList.add("active");
        }

        allButton.addEventListener("click", function () {

            selectedSubject = "all";

            renderSubjectFilters();
            renderResources();
        });

        resourceFilters.appendChild(allButton);

        Array.from(subjects)
            .sort()
            .forEach(function (subjectName) {

                const button = createElement(
                    "button",
                    "leaderboard-tab",
                    subjectName
                );

                button.type = "button";

                if (selectedSubject === subjectName) {
                    button.classList.add("active");
                }

                button.addEventListener(
                    "click",
                    function () {

                        selectedSubject = subjectName;

                        renderSubjectFilters();
                        renderResources();
                    }
                );

                resourceFilters.appendChild(button);
            });
    }

    // ==============================================
    // LOAD RESOURCES FROM DATABASE
    // ==============================================

    async function loadResources() {

        showStatus("Loading study materials...");

        emptyResources.style.display = "none";

        try {

            const data = await EduQuestAPI.get(
                "/studymaterials"
            );

            materials = Array.isArray(data) ? data : [];

            renderSubjectFilters();
            renderResources();

        } catch (error) {

            resourcesGrid.innerHTML = "";

            emptyResources.style.display = "none";

            showStatus(
                "Unable to load study materials: " +
                error.message
            );
        }
    }

    // ==============================================
    // INITIAL PAGE LOAD
    // ==============================================

    loadResources();

})();
