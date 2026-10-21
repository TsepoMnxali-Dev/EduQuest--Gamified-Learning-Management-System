
/*
========================================================
EduQuest - Learner Resources
========================================================

Learners can:
1. View resources uploaded by administrators.
2. Search resources by title, subject or topic.
3. Filter resources by grade, subject and type.
4. Browse resources grouped by grade, subject and topic.
5. Open external educational links.
6. View and download uploaded documents.

Learners cannot:
1. Add resources.
2. Edit resources.
3. Delete resources.

Resources are loaded from GET /studymaterials.
Learner information is loaded from GET /learner/me.
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
    // HTML ELEMENTS
    // ==============================================

    const byId = id => document.getElementById(id);

    const grid = byId("resourcesGrid");
    const status = byId("resourcesStatus");
    const empty = byId("emptyResources");
    const filters = byId("resourceFilters");

    const gradeFilter = byId("resourceGradeFilter");
    const typeFilter = byId("resourceTypeFilter");
    const search = byId("resourceSearch");

    const totalElement = byId("resourceTotal");
    const subjectsElement = byId("resourceSubjects");
    const documentsElement = byId("resourceDocuments");

    let materials = [];
    let selectedSubject = "all";

    // ==============================================
    // HELPER FUNCTIONS
    // ==============================================

    function el(tag, className, content) {
        const node = document.createElement(tag);

        if (className) {
            node.className = className;
        }

        if (content !== undefined) {
            node.textContent = content;
        }

        return node;
    }

    function showStatus(message) {
        if (!status) {
            return;
        }

        status.textContent = message;
        status.style.display = message ? "block" : "none";
    }

    function safeUrl(url) {
        try {
            const parsed = new URL(url);

            return (
                parsed.protocol === "http:" ||
                parsed.protocol === "https:"
            );
        } catch {
            return false;
        }
    }

    function groupBy(items, key) {
        const groups = new Map();

        items.forEach(item => {
            const label = key(item);

            if (!groups.has(label)) {
                groups.set(label, []);
            }

            groups.get(label).push(item);
        });

        return groups;
    }

    // ==============================================
    // LOAD LEARNER NAME AND GRADE
    // ==============================================

    async function loadIdentity() {
        try {
            const me = await EduQuestAPI.get("/learner/me");

            const firstName =
                String(me.firstName || "").trim() || "Learner";

            const gradeName = me.gradeName
                ? " · " + me.gradeName
                : "";

            const avatar = document.querySelector(
                ".profile-mini .profile-avatar"
            );

            const label = document.querySelector(
                ".profile-mini span"
            );

            if (avatar) {
                avatar.textContent =
                    firstName.charAt(0).toUpperCase();
            }

            if (label) {
                label.textContent = firstName + gradeName;
            }

        } catch (error) {
            console.error(
                "Learner identity failed to load:",
                error
            );
        }
    }

    // ==============================================
    // YOUTUBE THUMBNAILS
    // ==============================================

    function youtubeThumbnail(url) {
        if (!safeUrl(url)) {
            return null;
        }

        try {
            const parsed = new URL(url);
            const host = parsed.hostname.toLowerCase();

            let videoId = null;

            if (
                host === "youtu.be" ||
                host === "www.youtu.be"
            ) {
                videoId = parsed.pathname.split("/")[1];
            }

            else if (
                [
                    "youtube.com",
                    "www.youtube.com",
                    "m.youtube.com",
                    "youtube-nocookie.com",
                    "www.youtube-nocookie.com"
                ].includes(host)
            ) {
                if (parsed.pathname === "/watch") {
                    videoId = parsed.searchParams.get("v");
                }

                else if (
                    /^\/(shorts|embed)\//.test(parsed.pathname)
                ) {
                    videoId = parsed.pathname.split("/")[2];
                }
            }

            if (/^[\w-]{11}$/.test(videoId || "")) {
                return (
                    "https://img.youtube.com/vi/" +
                    videoId +
                    "/hqdefault.jpg"
                );
            }

            return null;

        } catch {
            return null;
        }
    }

    // ==============================================
    // FETCH DOCUMENT WITH AUTHENTICATION
    // ==============================================

    async function fetchFile(material) {
        const token = EduQuestAPI.getToken();

        if (!token) {
            throw new Error(
                "Your session has expired. Please sign in again."
            );
        }

        const endpoint =
            EduQuestAPI.BASE_URL +
            "/studymaterials/" +
            encodeURIComponent(material.studyMaterialID) +
            "/file";

        const response = await fetch(endpoint, {
            headers: {
                Authorization: "Bearer " + token
            }
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error(
                    "Your session has expired. Please sign in again."
                );
            }

            if (response.status === 403) {
                throw new Error(
                    "You do not have permission to access this document."
                );
            }

            if (response.status === 404) {
                throw new Error(
                    "This document could not be found."
                );
            }

            throw new Error(
                "Unable to retrieve this document."
            );
        }

        return await response.blob();
    }

    // ==============================================
    // VIEW OR DOWNLOAD DOCUMENT
    // ==============================================

    async function openFile(material, download) {
        const tab = download
            ? null
            : window.open("", "_blank");

        if (!download && !tab) {
            showStatus(
                "Please allow popups to view this document."
            );
            return;
        }

        try {
            showStatus(
                download
                    ? "Preparing download..."
                    : "Opening document..."
            );

            const blob = await fetchFile(material);
            const fileUrl = URL.createObjectURL(blob);

            if (download) {
                const link = document.createElement("a");

                link.href = fileUrl;
                link.download =
                    material.fileName || "study-material";

                document.body.appendChild(link);
                link.click();
                link.remove();

            } else {
                tab.opener = null;
                tab.location.href = fileUrl;
            }

            setTimeout(() => {
                URL.revokeObjectURL(fileUrl);
            }, 60000);

            showStatus("");

        } catch (error) {
            if (tab) {
                tab.close();
            }

            showStatus(
                error.message ||
                "Unable to retrieve this document."
            );
        }
    }

    // ==============================================
    // VIEW RESOURCE
    // ==============================================

    function viewMaterial(material) {
        if (material.fileName) {
            openFile(material, false);
            return;
        }

        if (!safeUrl(material.fileURL)) {
            showStatus(
                "This resource does not have a valid link."
            );
            return;
        }

        window.open(
            material.fileURL,
            "_blank",
            "noopener,noreferrer"
        );
    }

    // ==============================================
    // CREATE RESOURCE CARD
    // ==============================================

    function resourceCard(material) {
        const card = el("article", "lr-card");
        const preview = el("div", "lr-preview");

        const thumbnail = !material.fileName
            ? youtubeThumbnail(material.fileURL)
            : null;

        if (thumbnail) {
            const img = el("img", "lr-thumbnail");

            img.src = thumbnail;
            img.alt = "Video thumbnail";
            img.loading = "lazy";
            img.referrerPolicy = "no-referrer";

            img.addEventListener("error", () => {
                img.replaceWith(
                    el("span", "lr-preview-icon", "▶")
                );
            });

            preview.appendChild(img);

        } else {
            const extension = (
                material.fileName || ""
            ).split(".").pop().toLowerCase();

            let symbol = "LINK";

            if (extension === "pdf") {
                symbol = "PDF";
            }

            else if (
                extension === "doc" ||
                extension === "docx"
            ) {
                symbol = "DOC";
            }

            else if (
                extension === "ppt" ||
                extension === "pptx"
            ) {
                symbol = "PPT";
            }

            else if (material.fileName) {
                symbol = "FILE";
            }

            preview.appendChild(
                el("span", "lr-preview-icon", symbol)
            );
        }

        card.appendChild(preview);

        const body = el("div", "lr-card-body");

        body.appendChild(
            el(
                "span",
                "lr-type",
                material.resourceType || "Resource"
            )
        );

        body.appendChild(
            el(
                "h5",
                "lr-card-title",
                material.title || "Untitled resource"
            )
        );

        if (material.description) {
            body.appendChild(
                el(
                    "p",
                    "lr-description",
                    material.description
                )
            );
        }

        // ==========================================
        // RESOURCE ACTIONS
        // ==========================================

        const actions = el("div", "lr-actions");

        const viewButton = el(
            "button",
            "lr-btn lr-btn-primary",
            "View"
        );

        viewButton.type = "button";

        viewButton.addEventListener("click", () => {
            viewMaterial(material);
        });

        actions.appendChild(viewButton);

        // Only uploaded documents can be downloaded.

        if (material.fileName) {
            const downloadButton = el(
                "button",
                "lr-btn lr-btn-secondary",
                "Download"
            );

            downloadButton.type = "button";

            downloadButton.addEventListener("click", () => {
                openFile(material, true);
            });

            actions.appendChild(downloadButton);
        }

        body.appendChild(actions);
        card.appendChild(body);

        return card;
    }

    // ==============================================
    // UPDATE RESOURCE STATISTICS
    // ==============================================

    function updateStats() {
        if (totalElement) {
            totalElement.textContent = materials.length;
        }

        if (subjectsElement) {
            const subjects = new Set(
                materials.map(material =>
                    `${material.gradeLevel || ""}|${material.subjectID || material.subjectName || ""}`
                )
            );

            subjectsElement.textContent = subjects.size;
        }

        if (documentsElement) {
            documentsElement.textContent =
                materials.filter(material =>
                    Boolean(material.fileName)
                ).length;
        }
    }

    // ==============================================
    // CREATE SUBJECT FILTERS
    // ==============================================

    function renderSubjectFilters() {
        filters.replaceChildren();

        const names = [
            ...new Set(
                materials
                    .filter(material =>
                        gradeFilter.value === "all" ||
                        material.gradeLevel === gradeFilter.value
                    )
                    .map(material => material.subjectName)
                    .filter(Boolean)
            )
        ].sort();

        if (
            selectedSubject !== "all" &&
            !names.includes(selectedSubject)
        ) {
            selectedSubject = "all";
        }

        ["all", ...names].forEach(name => {
            const button = el(
                "button",
                "lr-subject-chip" +
                (selectedSubject === name ? " active" : ""),
                name === "all" ? "All subjects" : name
            );

            button.type = "button";

            button.setAttribute(
                "aria-pressed",
                String(selectedSubject === name)
            );

            button.addEventListener("click", () => {
                selectedSubject = name;

                renderSubjectFilters();
                renderResources();
            });

            filters.appendChild(button);
        });
    }

    // ==============================================
    // DISPLAY RESOURCES
    // ==============================================

    function renderResources() {
        grid.replaceChildren();

        updateStats();

        if (!materials.length) {
            empty.style.display = "block";
            showStatus("");
            return;
        }

        empty.style.display = "none";

        const query = search.value.trim().toLowerCase();

        const selected = materials.filter(material => {
            const searchableText = [
                material.title,
                material.subjectName,
                material.topicName,
                material.description
            ]
                .filter(Boolean)
                .join(" ")
                .toLowerCase();

            const matchesSearch =
                !query || searchableText.includes(query);

            const matchesGrade =
                gradeFilter.value === "all" ||
                material.gradeLevel === gradeFilter.value;

            const matchesSubject =
                selectedSubject === "all" ||
                material.subjectName === selectedSubject;

            const matchesType =
                typeFilter.value === "all" ||
                material.resourceType === typeFilter.value;

            return (
                matchesSearch &&
                matchesGrade &&
                matchesSubject &&
                matchesType
            );
        });

        if (!selected.length) {
            showStatus("No resources match your filters.");
            return;
        }

        showStatus("");

        // ==========================================
        // GROUP BY GRADE
        // ==========================================

        const gradeGroups = groupBy(
            selected,
            material => material.gradeLevel || "Other grades"
        );

        const sortedGrades = [
            ...gradeGroups.keys()
        ].sort((a, b) =>
            a.localeCompare(b, undefined, {
                numeric: true
            })
        );

        sortedGrades.forEach(grade => {
            const section = el("section", "lr-grade");

            section.appendChild(
                el("h3", "lr-grade-heading", grade)
            );

            // ======================================
            // GROUP BY SUBJECT
            // ======================================

            const subjectGroups = groupBy(
                gradeGroups.get(grade),
                material =>
                    material.subjectName || "Other subjects"
            );

            const sortedSubjects = [
                ...subjectGroups.keys()
            ].sort();

            sortedSubjects.forEach(subject => {
                const details = el(
                    "details",
                    "lr-subject"
                );

                details.open =
                    Boolean(query) ||
                    selectedSubject !== "all" ||
                    gradeFilter.value !== "all";

                const summary = el(
                    "summary",
                    "lr-subject-summary"
                );

                summary.appendChild(
                    el("span", "", subject)
                );

                const count =
                    subjectGroups.get(subject).length;

                summary.appendChild(
                    el(
                        "span",
                        "lr-count",
                        `${count} resources`
                    )
                );

                details.appendChild(summary);

                // ==================================
                // GROUP BY TOPIC
                // ==================================

                const topics = groupBy(
                    subjectGroups.get(subject),
                    material =>
                        material.topicName || "General resources"
                );

                const sortedTopics = [
                    ...topics.keys()
                ].sort();

                sortedTopics.forEach(topic => {
                    const topicSection = el(
                        "section",
                        "lr-topic"
                    );

                    topicSection.appendChild(
                        el(
                            "h4",
                            "lr-topic-heading",
                            topic
                        )
                    );

                    const cards = el(
                        "div",
                        "lr-cards"
                    );

                    topics.get(topic).forEach(material => {
                        cards.appendChild(
                            resourceCard(material)
                        );
                    });

                    topicSection.appendChild(cards);
                    details.appendChild(topicSection);
                });

                section.appendChild(details);
            });

            grid.appendChild(section);
        });
    }

    // ==============================================
    // LOAD GRADE OPTIONS
    // ==============================================

    function renderGradeFilters() {
        gradeFilter.replaceChildren();

        const allOption = el(
            "option",
            "",
            "All grades"
        );

        allOption.value = "all";
        gradeFilter.appendChild(allOption);

        const grades = [
            ...new Set(
                materials
                    .map(material => material.gradeLevel)
                    .filter(Boolean)
            )
        ].sort((a, b) =>
            a.localeCompare(b, undefined, {
                numeric: true
            })
        );

        grades.forEach(grade => {
            const option = el("option", "", grade);

            option.value = grade;
            gradeFilter.appendChild(option);
        });
    }

    // ==============================================
    // LOAD RESOURCES FROM DATABASE
    // ==============================================

    async function loadResources() {
        showStatus("Loading study materials...");

        empty.style.display = "none";

        try {
            const data = await EduQuestAPI.get(
                "/studymaterials"
            );

            materials = Array.isArray(data)
                ? data
                : [];

            renderGradeFilters();
            renderSubjectFilters();
            renderResources();

        } catch (error) {
            materials = [];

            grid.replaceChildren();

            empty.style.display = "none";

            updateStats();

            showStatus(
                "Unable to load study materials: " +
                (error.message || "Unknown error")
            );
        }
    }

    // ==============================================
    // FILTER EVENTS
    // ==============================================

    gradeFilter.addEventListener("change", () => {
        renderSubjectFilters();
        renderResources();
    });

    typeFilter.addEventListener(
        "change",
        renderResources
    );

    search.addEventListener(
        "input",
        renderResources
    );

    // ==============================================
    // INITIAL PAGE LOAD
    // ==============================================

    loadIdentity();
    loadResources();

})();
