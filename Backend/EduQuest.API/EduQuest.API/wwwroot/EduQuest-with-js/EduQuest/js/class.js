/* =============================================================
   EduQuest - class.js  (used by class.html?subject=ID)

   Loads one subject for the signed-in learner
   (GET /api/learner/classes/{subjectId}): overall progress, every
   published quiz for the subject, the topics for their grade with
   quiz status, and each topic's study materials.

   - Quizzes with too few questions show as "Coming soon".
   - External links open in a new tab (http/https only).
   - Uploaded files are fetched with the learner's token and
     saved to their device.
   Server text is written with textContent, never innerHTML.
============================================================= */
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    var COLOR_CLASSES = ["math", "science", "life", "geography"];

    /* A quiz needs this many approved questions before it can be started. */
    var QUESTIONS_NEEDED = 10;

    var subjectId = Number(new URLSearchParams(window.location.search).get("subject"));

    function byId(id) {
        return document.getElementById(id);
    }

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null) node.textContent = text;
        return node;
    }

    function clear(node) {
        while (node.firstChild) node.removeChild(node.firstChild);
    }

    function showStatus(container, message, isError) {
        clear(container);
        container.appendChild(el("p", "class-status" + (isError ? " error" : ""), message));
    }

    function showNotice(message) {
        var box = byId("classNotice");
        box.textContent = message;
        box.style.display = "block";
    }

    /* Only ordinary web links are allowed as hrefs. */
    function isWebLink(url) {
        return typeof url === "string" && /^https?:\/\//i.test(url);
    }

    var EXTENSIONS = {
        "application/pdf": ".pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document": ".docx",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation": ".pptx",
        "text/plain": ".txt",
        "image/png": ".png",
        "image/jpeg": ".jpg"
    };

    function fileNameFor(title, mimeType) {
        var name = String(title || "study-material").replace(/[\\\/:*?"<>|]+/g, "").trim() || "study-material";
        return name + (EXTENSIONS[mimeType] || "");
    }

    /* Uploaded files need the Authorization header, so a plain link
       cannot be used: fetch the file, then save it from memory. */
    function downloadMaterial(material, button) {
        var originalText = button.textContent;
        button.disabled = true;
        button.textContent = "Downloading...";

        fetch(EduQuestAPI.BASE_URL + "/learner/materials/" + material.studyMaterialID + "/file", {
            headers: { Authorization: "Bearer " + EduQuestAPI.getToken() }
        })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Could not download this file (" + response.status + ").");
                }
                return response.blob();
            })
            .then(function (blob) {
                var url = URL.createObjectURL(blob);
                var link = document.createElement("a");
                link.href = url;
                link.download = fileNameFor(material.title, blob.type);
                document.body.appendChild(link);
                link.click();
                link.remove();
                setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
            })
            .catch(function (err) {
                showNotice(err.message || "Could not download this file.");
            })
            .then(function () {
                button.disabled = false;
                button.textContent = originalText;
            });
    }

    function buildMaterialChip(material) {
        var label = material.resourceType || "Resource";

        if (material.hasFile) {
            var button = el("button", "chip-btn", "\u2B07\uFE0F " + label);
            button.type = "button";
            button.title = material.title;
            button.addEventListener("click", function () {
                downloadMaterial(material, button);
            });
            return button;
        }

        if (isWebLink(material.fileURL)) {
            var link = el("a", "chip-btn", "\uD83D\uDCC4 " + label);
            link.href = material.fileURL;
            link.target = "_blank";
            link.rel = "noopener noreferrer";
            link.title = material.title;
            return link;
        }

        return null;
    }

    /* ---------------------------------------------------------
       Quizzes
    --------------------------------------------------------- */

    /* The buttons for one quiz: Review, then Start / Retake or "Coming soon". */
    function quizActions(quiz) {
        var items = [];

        if (quiz.isCompleted && quiz.latestAttemptID) {
            var review = el("a", "chip-btn", "\uD83D\uDCC4 Review (" + quiz.latestPercentage + "%)");
            review.href = "quiz.html?attempt=" + quiz.latestAttemptID + "&review=1";
            items.push(review);
        }

        if (quiz.isReady) {
            var start = el(
                "a", "chip-btn primary",
                quiz.isCompleted ? "\uD83D\uDD01 Retake" : "\uD83D\uDCDD Start Quiz"
            );
            start.href = "quiz.html?id=" + quiz.quizID;
            items.push(start);
        } else if (!quiz.isCompleted) {
            items.push(el(
                "span", "chip-btn disabled",
                "Coming soon \u00b7 " + quiz.questionCount + " of " + QUESTIONS_NEEDED + " questions"
            ));
        }

        return items;
    }

    function buildQuizRow(quiz) {
        var row = el("div", "subject-resource-row");

        var info = el("div", "subject-resource-info");
        info.appendChild(el("div", "subject-resource-icon", quiz.isCompleted ? "\u2713" : "\uD83D\uDCDD"));

        var text = el("div");
        text.appendChild(el("strong", null, quiz.quizTitle));
        text.appendChild(el("span", null, [
            quiz.topicName,
            quiz.difficulty,
            quiz.timeLimitMinutes + " min"
        ].join(" \u00b7 ")));
        info.appendChild(text);
        row.appendChild(info);

        var actions = el("div", "topic-actions");
        quizActions(quiz).forEach(function (item) { actions.appendChild(item); });
        row.appendChild(actions);

        return row;
    }

    function renderQuizzes(quizzes) {
        var list = byId("quizList");
        clear(list);

        if (!quizzes || quizzes.length === 0) {
            showStatus(
                list,
                "No quizzes have been published for this subject and grade yet. Check back soon.",
                false
            );
            return;
        }

        quizzes.forEach(function (quiz) {
            list.appendChild(buildQuizRow(quiz));
        });
    }

    /* ---------------------------------------------------------
       Topics
    --------------------------------------------------------- */

    function statusClass(status) {
        if (status === "Completed") return "completed";
        if (status === "In Progress") return "progress";
        return "locked";
    }

    function buildTopicCard(topic) {
        var card = el("article", "topic-card");

        var top = el("div", "topic-card-top");
        var heading = el("div");
        heading.appendChild(el("h3", null, topic.topicName));

        var detail;
        if (topic.quizzesTotal > 0) {
            detail = topic.quizzesCompleted + " of " + topic.quizzesTotal +
                (topic.quizzesTotal === 1 ? " quiz" : " quizzes") + " completed";
        } else if (topic.quizzes.length > 0) {
            detail = "A quiz for this topic is being prepared.";
        } else {
            detail = "No quiz for this topic yet.";
        }
        var detailLine = el("p", null, detail);
        detailLine.style.cssText = "color:var(--muted);font-size:12px;";
        heading.appendChild(detailLine);

        top.appendChild(heading);
        top.appendChild(el("span", "topic-status " + statusClass(topic.status), topic.status));
        card.appendChild(top);

        if (topic.quizzes.length > 0) {
            var actions = el("div", "topic-actions");
            topic.quizzes.forEach(function (quiz) {
                quizActions(quiz).forEach(function (item) {
                    /* Make the title clear when a topic has more than one quiz. */
                    item.title = quiz.quizTitle;
                    actions.appendChild(item);
                });
            });
            card.appendChild(actions);
        }

        var chips = (topic.materials || [])
            .map(buildMaterialChip)
            .filter(function (chip) { return chip !== null; });

        if (chips.length > 0) {
            var materials = el("div", "topic-materials");
            materials.appendChild(el("div", "material-label", "Study materials"));
            var row = el("div", "topic-actions");
            chips.forEach(function (chip) { row.appendChild(chip); });
            materials.appendChild(row);
            card.appendChild(materials);
        }

        return card;
    }

    function render(detail) {
        document.title = "EduQuest | " + detail.subjectName;

        var icon = byId("classIcon");
        icon.textContent = detail.subjectName.charAt(0).toUpperCase();
        icon.className = "subject-icon " + COLOR_CLASSES[detail.subjectID % COLOR_CLASSES.length];

        byId("className").textContent = detail.subjectName;
        byId("classSubtitle").textContent = detail.gradeName +
            (detail.averageScorePercent !== null && detail.averageScorePercent !== undefined
                ? " \u00b7 Average score " + detail.averageScorePercent + "%"
                : "");

        byId("overallPercent").textContent = detail.progressPercent + "%";
        byId("overallFill").style.width = detail.progressPercent + "%";

        renderQuizzes(detail.quizzes);

        var topicsList = byId("topicsList");
        if (!detail.topics || detail.topics.length === 0) {
            showStatus(topicsList, "No topics have been added for this subject and grade yet.", false);
            return;
        }

        clear(topicsList);
        detail.topics.forEach(function (topic) {
            topicsList.appendChild(buildTopicCard(topic));
        });
    }

    if (!subjectId) {
        byId("className").textContent = "Class not found";
        showStatus(byId("quizList"), "No subject was selected.", true);
        showStatus(byId("topicsList"), "No subject was selected.", true);
        return;
    }

    EduQuestAPI.get("/learner/classes/" + subjectId)
        .then(render)
        .catch(function (err) {
            console.error("Class failed to load:", err);
            byId("className").textContent = "Class not available";
            var message = err.message || "Could not load this class.";
            showStatus(byId("quizList"), message, true);
            showStatus(byId("topicsList"), message, true);
        });
})();