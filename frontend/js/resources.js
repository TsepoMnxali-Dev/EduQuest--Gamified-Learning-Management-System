/*
========================================================
EduQuest - Admin Resources Management
Admin can:
1. View all study resources.
2. Upload documents.
3. Add study material links.
4. Search and filter resources.
5. Delete resources.
Learners cannot use admin API operations.
CHANGED = Updated existing functionality.
NEW = Added functionality.
========================================================
*/
(function () {
    "use strict";
    const LOGIN_URL = "../../EduQuest-with-js/EduQuest/login.html";
    // Maximum file size: 10 MB.
    const MAX_FILE_SIZE = 10 * 1024 * 1024;
    // Must match StudyMaterialsController.cs.
    const ALLOWED_EXTENSIONS = [
        ".pdf", ".doc", ".docx",
        ".ppt", ".pptx", ".txt"
    ];
    // Helper for finding HTML elements.
    function byId(id) {
        return document.getElementById(id);
    }
    // ==================================================
    // SECURITY: ADMIN ACCESS
    // ==================================================
    const user = EduQuestAPI.getCurrentUser();
    if (
        !EduQuestAPI.getToken() ||
        !user ||
        user.role !== "Admin"
    ) {
        window.location.href = LOGIN_URL;
        return;
    }
    // IMPORTANT:
    // This frontend check is for navigation only.
    // The backend must enforce admin authorization.
    // ==================================================
    // HTML ELEMENTS
    // ==================================================
    const emptyMessage = byId("emptyResourcesMessage");
    const resourcesMessage = byId("resourcesMessage");
    const searchInput = byId("resourceSearch");
    const gradeFilter = byId("gradeFilter");
    const typeFilter = byId("typeFilter");
    const modal = byId("addResourceModal");
    const form = byId("addResourceForm");
    const formError = byId("resourceFormError");
    const gradeSelect = byId("resourceGrade");
    const subjectSelect = byId("resourceSubject");
    const topicSelect = byId("resourceTopic");
    const fileInput = byId("resourceFile");
    const urlInput = byId("resourceUrl");
    // NEW: Toggle entire input sections.
    const fileSection = byId("resourceFileSection");
    const urlSection = byId("resourceUrlSection");
    const saveButton = byId("saveResourceBtn");
    let materials = [];
    let grades = [];
    let topics = [];
    // ==================================================
    // HELPER METHODS
    // ==================================================
    function createElement(tag, className, value) {
        const element = document.createElement(tag);
        if (className) {
            element.className = className;
        }
        if (value !== undefined) {
            element.textContent = value;
        }
        return element;
    }
    function createCell(value) {
        return createElement(
            "td",
            "",
            value === null || value === undefined ||
            value === "" ? "-" : String(value)
        );
    }
    // NEW: Show status messages to the admin.
    function showMessage(message) {
        resourcesMessage.textContent = message;
        resourcesMessage.style.display =
            message ? "block" : "none";
    }
    function showFormError(message) {
        formError.textContent = message;
        formError.style.display =
            message ? "block" : "none";
    }
    function isSafeUrl(url) {
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
    // ==================================================
    // RESOURCE STATISTICS
    // ==================================================
    function updateStats() {
        byId("statTotal").textContent = materials.length;
        const textbooks = materials.filter(function (m) {
            return m.resourceType === "Textbook";
        });
        byId("statTextbooks").textContent =
            textbooks.length;
        const subjects = new Set();
        materials.forEach(function (m) {
            subjects.add(m.subjectID);
        });
        byId("statSubjects").textContent =
            subjects.size;
    }
    // ==================================================
    // VIEW OR DOWNLOAD RESOURCE
    // ==================================================
    function openMaterial(material) {
        // Documents are retrieved using the JWT token.
        if (material.fileName) {
            const newTab = window.open("", "_blank");
            EduQuestAPI.getBlob(
                "/studymaterials/" +
                material.studyMaterialID +
                "/file"
            )
                .then(function (blob) {
                    const fileUrl = URL.createObjectURL(blob);
                    if (newTab) {
                        newTab.location = fileUrl;
                    } else {
                        // Fallback if popups are blocked.
                        const link = document.createElement("a");
                        link.href = fileUrl;
                        link.download = material.fileName;
                        link.click();
                    }
                    setTimeout(function () {
                        URL.revokeObjectURL(fileUrl);
                    }, 60000);
                })
                .catch(function (error) {
                    if (newTab) {
                        newTab.close();
                    }
                    showMessage(error.message);
                });
            return;
        }
        // External study links.
        if (isSafeUrl(material.fileURL)) {
            window.open(
                material.fileURL,
                "_blank",
                "noopener,noreferrer"
            );
        } else {
            showMessage("This resource has no valid link.");
        }
    }
    // ==================================================
    // DELETE RESOURCE - ADMIN ONLY
    // ==================================================
    function deleteMaterial(material) {
        const confirmed = confirm(
            'Delete "' + material.title + '"? ' +
            "Learners will no longer see it."
        );
        if (!confirmed) {
            return;
        }
        EduQuestAPI.del(
            "/studymaterials/" + material.studyMaterialID
        )
            .then(function () {
                return loadMaterials();
            })
            .then(function () {
                showMessage("Resource deleted successfully.");
            })
            .catch(function (error) {
                showMessage(error.message);
            });
    }
    // ==================================================
    // GROUPED RESOURCE CARDS: Grade > Subject > Topic
    // ==================================================
    function youtubeThumbnail(url) {
        if (!isSafeUrl(url)) return null;
        try {
            const parsed = new URL(url);
            const host = parsed.hostname.toLowerCase();
            let id = null;
            if (host === "youtu.be" || host === "www.youtu.be") {
                id = parsed.pathname.split("/")[1];
            } else if (["youtube.com", "www.youtube.com", "m.youtube.com"].includes(host)) {
                if (parsed.pathname === "/watch") id = parsed.searchParams.get("v");
                else if (/^\/(shorts|embed)\//.test(parsed.pathname)) id = parsed.pathname.split("/")[2];
            }
            return id && /^[a-zA-Z0-9_-]{11}$/.test(id)
                ? "https://img.youtube.com/vi/" + id + "/hqdefault.jpg"
                : null;
        } catch { return null; }
    }

    function resourceCard(material) {
        const card = createElement("article", "resource-card");
        const preview = createElement("div", "resource-preview");
        const thumbnail = !material.fileName ? youtubeThumbnail(material.fileURL) : null;
        if (thumbnail) {
            const img = createElement("img", "resource-thumbnail");
            img.src = thumbnail;
            img.alt = "Video thumbnail";
            img.loading = "lazy";
            img.referrerPolicy = "no-referrer";
            img.addEventListener("error", function () {
                img.replaceWith(createElement("span", "resource-preview-icon", "▶"));
            });
            preview.appendChild(img);
        } else {
            const extension = (material.fileName || "").split(".").pop().toLowerCase();
            const symbol = extension === "pdf" ? "PDF" :
                ["doc", "docx"].includes(extension) ? "DOC" :
                ["ppt", "pptx"].includes(extension) ? "PPT" :
                material.fileName ? "FILE" : "LINK";
            preview.appendChild(createElement("span", "resource-preview-icon", symbol));
        }
        card.appendChild(preview);
        const body = createElement("div", "resource-card-body");
        body.appendChild(createElement("span", "resource-type-label", material.resourceType || "Resource"));
        body.appendChild(createElement("h4", "resource-card-title", material.title || "Untitled"));
        if (material.description) body.appendChild(createElement("p", "resource-card-description", material.description));
        const actions = createElement("div", "resource-card-actions");
        const view = createElement("button", "resource-btn resource-btn-outline", "View");
        view.type = "button";
        view.addEventListener("click", function () { openMaterial(material); });
        const del = createElement("button", "resource-btn resource-btn-danger", "Delete");
        del.type = "button";
        del.addEventListener("click", function () { deleteMaterial(material); });
        actions.append(view, del);
        body.appendChild(actions);
        card.appendChild(body);
        return card;
    }

    function groupBy(items, keyFn) {
        const result = new Map();
        items.forEach(function (item) {
            const key = keyFn(item);
            if (!result.has(key)) result.set(key, []);
            result.get(key).push(item);
        });
        return result;
    }

    function renderResources() {
        const container = byId("resourcesGrouped");
        container.replaceChildren();
        updateStats();
        if (materials.length === 0) {
            emptyMessage.style.display = "block";
            return;
        }
        emptyMessage.style.display = "none";
        const query = searchInput.value.trim().toLowerCase();
        const filtered = materials.filter(function (m) {
            const searchText = [m.title, m.subjectName, m.topicName, m.description]
                .filter(Boolean).join(" ").toLowerCase();
            return (!query || searchText.includes(query)) &&
                (gradeFilter.value === "all" || m.gradeLevel === gradeFilter.value) &&
                (typeFilter.value === "all" || m.resourceType === typeFilter.value);
        });
        if (!filtered.length) {
            container.appendChild(createElement("p", "resource-no-results", "No resources match your search or filters."));
            return;
        }
        const gradesGrouped = groupBy(filtered, m => m.gradeLevel || "Other grades");
        Array.from(gradesGrouped.keys()).sort().forEach(function (gradeName) {
            const gradeSection = createElement("section", "resource-grade-group");
            gradeSection.appendChild(createElement("h3", "resource-grade-title", gradeName));
            const subjectGroups = groupBy(gradesGrouped.get(gradeName), m => m.subjectName || "Other subjects");
            Array.from(subjectGroups.keys()).sort().forEach(function (subjectName) {
                const subjectDetails = createElement("details", "resource-subject-group");
                subjectDetails.open = !!query || gradeFilter.value !== "all";
                const summary = createElement("summary", "resource-subject-heading");
                summary.appendChild(createElement("span", "", subjectName));
                summary.appendChild(createElement("span", "resource-count", subjectGroups.get(subjectName).length + " resources"));
                subjectDetails.appendChild(summary);
                const topicGroups = groupBy(subjectGroups.get(subjectName), m => m.topicName || "General resources");
                Array.from(topicGroups.keys()).sort().forEach(function (topicName) {
                    const topicSection = createElement("section", "resource-topic-group");
                    topicSection.appendChild(createElement("h4", "resource-topic-title", topicName));
                    const grid = createElement("div", "resource-card-grid");
                    topicGroups.get(topicName).forEach(m => grid.appendChild(resourceCard(m)));
                    topicSection.appendChild(grid);
                    subjectDetails.appendChild(topicSection);
                });
                gradeSection.appendChild(subjectDetails);
            });
            container.appendChild(gradeSection);
        });
    }

    // ==================================================
    // LOAD RESOURCES FROM DATABASE
    // ==================================================
    function loadMaterials() {
        showMessage("Loading resources...");
        return EduQuestAPI.get("/studymaterials")
            .then(function (data) {
                materials = Array.isArray(data) ? data : [];
                showMessage("");
                renderResources();
            })
            .catch(function (error) {
                // NEW: Do not display an empty database
                // message when the API has actually failed.
                materials = [];
                byId("resourcesGrouped").replaceChildren();
                emptyMessage.style.display = "none";
                updateStats();
                showMessage(
                    "Unable to load resources: " + error.message
                );
                // Allow callers to detect failure.
                throw error;
            });
    }
    // ==================================================
    // FILL DROPDOWN OPTIONS
    // ==================================================
    function fillSelect(
        select, items, valueKey, textKey, placeholder
    ) {
        select.innerHTML = "";
        const first = createElement(
            "option", "", placeholder
        );
        first.value = "";
        select.appendChild(first);
        items.forEach(function (item) {
            const option = createElement(
                "option", "", item[textKey]
            );
            option.value = item[valueKey];
            select.appendChild(option);
        });
    }
    // ==================================================
    // OPEN AND CLOSE ADD RESOURCE FORM
    // ==================================================
    function openModal() {
        form.reset();
        showFormError("");
        subjectSelect.disabled = true;
        topicSelect.disabled = true;
        fillSelect(
            subjectSelect, [], "", "",
            "Select grade first"
        );
        fillSelect(
            topicSelect, [], "", "",
            "Topic (optional)"
        );
        updateSourceFields();
        modal.style.display = "flex";
    }
    function closeModal() {
        modal.style.display = "none";
    }
    function selectedSource() {
        const selected = form.querySelector(
            'input[name="resourceSource"]:checked'
        );
        return selected ? selected.value : "file";
    }
    // ==================================================
    // CHANGED: FILE OR URL INPUT SECTIONS
    // ==================================================
    function updateSourceFields() {
        const isFile = selectedSource() === "file";
        fileSection.style.display =
            isFile ? "block" : "none";
        urlSection.style.display =
            isFile ? "none" : "block";
        fileInput.required = isFile;
        urlInput.required = !isFile;
        // Clear the inactive source so the admin
        // cannot accidentally submit both.
        if (isFile) {
            urlInput.value = "";
        } else {
            fileInput.value = "";
        }
    }
    // ==================================================
    // LOAD SUBJECTS WHEN GRADE CHANGES
    // ==================================================
    gradeSelect.addEventListener("change", function () {
        subjectSelect.disabled = true;
        topicSelect.disabled = true;
        fillSelect(
            subjectSelect, [], "", "",
            "Select grade first"
        );
        fillSelect(
            topicSelect, [], "", "",
            "Topic (optional)"
        );
        if (!gradeSelect.value) {
            return;
        }
        EduQuestAPI.get(
            "/grades/" + gradeSelect.value + "/subjects"
        )
            .then(function (subjects) {
                if (!subjects.length) {
                    fillSelect(
                        subjectSelect, [], "", "",
                        "No subjects for this grade"
                    );
                    return;
                }
                fillSelect(
                    subjectSelect,
                    subjects,
                    "gradeSubjectID",
                    "subjectName",
                    "Select subject"
                );
                // Keep SubjectID for topic filtering.
                subjects.forEach(function (subject, index) {
                    subjectSelect.options[index + 1]
                        .dataset.subjectId = subject.subjectID;
                });
                subjectSelect.disabled = false;
            })
            .catch(function (error) {
                showFormError(error.message);
            });
    });
    // ==================================================
    // LOAD TOPICS WHEN SUBJECT CHANGES
    // ==================================================
    subjectSelect.addEventListener("change", function () {
        const option =
            subjectSelect.options[subjectSelect.selectedIndex];
        const subjectId =
            option ? option.dataset.subjectId : null;
        const gradeName =
            gradeSelect.options[gradeSelect.selectedIndex]
                .textContent;
        const matchingTopics = topics.filter(function (t) {
            return (
                String(t.subjectID) === String(subjectId) &&
                t.gradeLevel === gradeName
            );
        });
        fillSelect(
            topicSelect,
            matchingTopics,
            "topicID",
            "topicName",
            "Topic (optional)"
        );
        topicSelect.disabled = matchingTopics.length === 0;
    });
    // ==================================================
    // FILE VALIDATION
    // ==================================================
    function validateFile(file) {
        if (!file) {
            return "Please select a document.";
        }
        if (file.size === 0) {
            return "The selected document is empty.";
        }
        if (file.size > MAX_FILE_SIZE) {
            return "File must not exceed 10 MB.";
        }
        const extension =
            "." + file.name.split(".").pop().toLowerCase();
        if (!ALLOWED_EXTENSIONS.includes(extension)) {
            return "Unsupported document format.";
        }
        return null;
    }
    // ==================================================
    // SAVE RESOURCE TO DATABASE
    // ==================================================
    form.addEventListener("submit", function (event) {
        event.preventDefault();
        showFormError("");
        const source = selectedSource();
        const file = fileInput.files[0];
        const url = urlInput.value.trim();
        const title = byId("resourceTitle").value.trim();
        const description =
            byId("resourceDescription").value.trim();
        const resourceType = byId("resourceType").value;
        // Validate required fields.
        if (!title || !resourceType ||
            !gradeSelect.value || !subjectSelect.value) {
            showFormError(
                "Please complete all required fields."
            );
            return;
        }
        if (source === "file") {
            const error = validateFile(file);
            if (error) {
                showFormError(error);
                return;
            }
        } else {
            if (!isSafeUrl(url)) {
                showFormError(
                    "Please enter a valid HTTP or HTTPS URL."
                );
                return;
            }
        }
        // FormData is required for file uploads.
        const data = new FormData();
        data.append(
            "GradeSubjectID",
            subjectSelect.value
        );
        if (topicSelect.value) {
            data.append("TopicID", topicSelect.value);
        }
        data.append("Title", title);
        data.append("Description", description);
        data.append("ResourceType", resourceType);
        if (source === "file") {
            data.append("File", file);
        } else {
            data.append("FileURL", url);
        }
        saveButton.disabled = true;
        saveButton.textContent = "Saving...";
        // Admin-only backend endpoint.
        EduQuestAPI.post("/studymaterials", data)
            .then(function () {
                closeModal();
                return loadMaterials();
            })
            .then(function () {
                showMessage(
                    "Resource added successfully."
                );
            })
            .catch(function (error) {
                // Keep the form open for submission
                // errors so the admin can correct them.
                if (modal.style.display !== "none") {
                    showFormError(error.message);
                } else {
                    showMessage(error.message);
                }
            })
            .finally(function () {
                saveButton.disabled = false;
                saveButton.textContent = "Save Resource";
            });
    });
    // ==================================================
    // BUTTON EVENTS
    // ==================================================
    byId("openAddResource")
        .addEventListener("click", openModal);
    byId("closeAddResource")
        .addEventListener("click", closeModal);
    byId("resourceSearchBtn")
        .addEventListener("click", renderResources);
    searchInput.addEventListener(
        "input", renderResources
    );
    gradeFilter.addEventListener(
        "change", renderResources
    );
    typeFilter.addEventListener(
        "change", renderResources
    );
    form.querySelectorAll(
        'input[name="resourceSource"]'
    ).forEach(function (radio) {
        radio.addEventListener(
            "change", updateSourceFields
        );
    });
    // ==================================================
    // INITIAL PAGE LOAD
    // ==================================================
    Promise.all([
        EduQuestAPI.get("/grades", { auth: false }),
        EduQuestAPI.get("/topics")
    ])
        .then(function (results) {
            grades = results[0];
            topics = results[1];
            fillSelect(
                gradeSelect,
                grades,
                "gradeID",
                "gradeName",
                "Select grade"
            );
            grades.forEach(function (grade) {
                const option = createElement(
                    "option", "", grade.gradeName
                );
                option.value = grade.gradeName;
                gradeFilter.appendChild(option);
            });
        })
        .catch(function (error) {
            showMessage(
                "Unable to load grade or topic options: " +
                error.message
            );
        });
    // Load existing study materials.
    loadMaterials().catch(function () {
        // Error is already displayed by loadMaterials.
    });
})();
