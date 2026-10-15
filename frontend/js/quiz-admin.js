/* =============================================================
   EduQuest - quiz-admin.js  (used by pages/admin/quiz.html)

   Admin "Manage Quizzes" page, wired to the backend:

     GET    /api/quizzes                          list quizzes
     GET    /api/topics                           topic dropdown (subject + grade)
     POST   /api/quizzes                          create a quiz
     PUT    /api/quizzes/{id}                     edit / publish / unpublish
     DELETE /api/quizzes/{id}                     delete a quiz

     GET    /api/quizzes/{id}/questions           questions of a quiz
     POST   /api/quizzes/{id}/questions           add a question
     POST   /api/quizzes/{id}/questions/from-bank {count}
     POST   /api/quizzes/{id}/questions/generate  {count}   (Gemini)
     PUT    /api/questions/{id}                   edit a question
     DELETE /api/questions/{id}                   delete a question

     GET    /api/questions/{id}/options           options of a question
     POST   /api/questions/{id}/options           add an option
     PUT    /api/options/{id}                     edit an option
     DELETE /api/options/{id}                     delete an option

   Needs api.js and admin-guard.js loaded first.
============================================================= */
(function () {
    "use strict";

    if (!window.EduQuestAPI) {
        console.error("quiz-admin.js: api.js must be loaded before this script.");
        return;
    }

    var LOGIN_URL = "../../EduQuest-with-js/EduQuest/login.html";

    var me = EduQuestAPI.getCurrentUser();
    if (!me || me.role !== "Admin") {
        window.location.href = LOGIN_URL;
        return;
    }

    var MAX_OPTIONS = 6;
    var MIN_OPTIONS = 2;

    var quizzes = [];          /* [{ id, title, difficulty, minutes, published, topicId }] */
    var topics = [];           /* [{ id, subject, name, grade }] */
    var questionCounts = {};   /* quizId -> number (or null if it could not be read) */

    var editingQuiz = null;    /* quiz being edited, or null while creating */
    var activeQuiz = null;     /* quiz whose questions are open */
    var questions = [];        /* [{ id, text, explanation, ai, approved, options: [{ id, text, correct }] }] */
    var editingQuestion = null;
    var bulkMode = null;       /* "bank" | "ai" | null */
    var noticeTimer = null;

    /* ---------- helpers ---------- */

    function byId(id) { return document.getElementById(id); }

    function setText(id, value) {
        var el = byId(id);
        if (el) { el.textContent = value; }
    }

    /* Titles and question text are typed in by admins, so they are escaped
       before they go into innerHTML. */
    function esc(value) {
        return String(value === null || value === undefined ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    /* Question text, options and explanations are shown the way they'd be written in a maths paper
       (x^2 becomes x with a superscript 2, sqrt(x) gets a root sign, and so on). */
    function math(value) {
        return window.EduQuestMath ? EduQuestMath.toHtml(value) : esc(value);
    }

    /* Lower-cases every key so the page works whether the API sends
       camelCase ("quizID") or PascalCase ("QuizID"). */
    function lc(obj) {
        var out = {};
        Object.keys(obj || {}).forEach(function (key) { out[key.toLowerCase()] = obj[key]; });
        return out;
    }

    /* api.js clears the token on a 401. If it is gone, send the admin back to sign in. */
    function authLost() {
        if (!EduQuestAPI.getToken()) {
            window.location.href = LOGIN_URL;
            return true;
        }
        return false;
    }

    function show(el, visible) {
        if (el) { el.hidden = !visible; }
    }

    /* "00:30:00" or "1.00:00:00" (days) -> whole minutes, or null. */
    function parseMinutes(value) {
        var m = /^(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})/.exec(String(value || ""));
        if (!m) { return null; }
        var seconds = (Number(m[1] || 0) * 86400) + (Number(m[2]) * 3600) + (Number(m[3]) * 60) + Number(m[4]);
        return Math.round(seconds / 60);
    }

    /* minutes -> "HH:MM:SS", the format .NET reads into a TimeSpan. */
    function toTimeSpan(minutes) {
        var h = Math.floor(minutes / 60);
        var m = minutes % 60;
        return (h < 10 ? "0" : "") + h + ":" + (m < 10 ? "0" : "") + m + ":00";
    }

    function formatMinutes(minutes) {
        if (minutes === null || minutes === undefined) { return "—"; }
        if (minutes < 60) { return minutes + " min"; }
        var h = Math.floor(minutes / 60);
        var m = minutes % 60;
        return h + " h" + (m ? " " + m + " min" : "");
    }

    function normQuiz(raw) {
        var q = lc(raw);
        return {
            id: q.quizid,
            title: q.quiztitle || "",
            difficulty: q.difficulty || "",
            minutes: parseMinutes(q.timelimit),
            published: !!q.ispublished,
            topicId: q.topicid
        };
    }

    function normTopic(raw) {
        var t = lc(raw);
        return { id: t.topicid, subject: t.subjectname || "", name: t.topicname || "", grade: t.gradelevel || "" };
    }

    function topicById(id) {
        for (var i = 0; i < topics.length; i++) {
            if (topics[i].id === id) { return topics[i]; }
        }
        return null;
    }

    function topicLabel(topic) {
        return topic.subject + " – " + topic.name + (topic.grade ? " (" + topic.grade + ")" : "");
    }

    function findQuiz(id) {
        for (var i = 0; i < quizzes.length; i++) {
            if (quizzes[i].id === id) { return quizzes[i]; }
        }
        return null;
    }

    function plural(n, word) {
        return n + " " + word + (n === 1 ? "" : "s");
    }

    /* ---------- messages ---------- */

    function notify(el, message, kind, notes) {
        if (!el) { return; }
        var html = esc(message);
        if (notes && notes.length) {
            html += "<ul>" + notes.map(function (n) { return "<li>" + esc(n) + "</li>"; }).join("") + "</ul>";
        }
        el.className = "qz-notice" + (kind ? " " + kind : "");
        el.innerHTML = html;
        el.hidden = false;
    }

    function pageNotice(message, kind) {
        var el = byId("pageNotice");
        notify(el, message, kind);
        clearTimeout(noticeTimer);
        if (kind !== "error") {
            noticeTimer = setTimeout(function () { el.hidden = true; }, 6000);
        }
    }

    function questionsNotice(message, kind, notes) {
        notify(byId("questionsNotice"), message, kind, notes);
    }

    function formError(boxId, message) {
        var box = byId(boxId);
        if (!box) { alert(message); return; }
        if (message) {
            box.textContent = message;
            box.hidden = false;
        } else {
            box.textContent = "";
            box.hidden = true;
        }
    }

    /* ---------- table ---------- */

    function setTableMessage(message) {
        byId("quizTableBody").innerHTML = '<tr><td colspan="7">' + esc(message) + "</td></tr>";
    }

    function visibleQuizzes() {
        var term = byId("quizSearch").value.trim().toLowerCase();
        var difficulty = byId("difficultyFilter").value;
        var status = byId("statusFilter").value;

        return quizzes.filter(function (q) {
            var topic = topicById(q.topicId);
            var haystack = (q.title + " " + (topic ? topicLabel(topic) : "")).toLowerCase();
            if (term && haystack.indexOf(term) === -1) { return false; }
            if (difficulty !== "all" && q.difficulty.toLowerCase() !== difficulty) { return false; }
            if (status === "published" && !q.published) { return false; }
            if (status === "draft" && q.published) { return false; }
            return true;
        });
    }

    function questionCountCell(quiz) {
        var n = questionCounts[quiz.id];
        if (n === undefined) { return "…"; }
        if (n === null) { return "—"; }
        return String(n);
    }

    function rowHtml(quiz) {
        var topic = topicById(quiz.topicId);
        var status = quiz.published ? "published" : "draft";

        return (
            "<tr>" +
                '<td class="qz-title-cell"><strong>' + esc(quiz.title) + "</strong>" +
                    "<small>" + esc(topic ? topic.name : "Unknown topic") + "</small></td>" +
                "<td>" + esc(topic ? topic.subject + (topic.grade ? " · " + topic.grade : "") : "—") + "</td>" +
                '<td><span class="qz-badge ' + esc(quiz.difficulty.toLowerCase()) + '">' + esc(quiz.difficulty) + "</span></td>" +
                "<td>" + esc(formatMinutes(quiz.minutes)) + "</td>" +
                "<td>" + esc(questionCountCell(quiz)) + "</td>" +
                '<td><span class="qz-badge ' + status + '">' + (quiz.published ? "Published" : "Draft") + "</span></td>" +
                "<td>" +
                    '<div class="qz-row-actions">' +
                        '<button type="button" class="qz-link-btn" data-action="questions" data-id="' + quiz.id + '">Questions</button>' +
                        '<button type="button" class="qz-link-btn" data-action="edit" data-id="' + quiz.id + '">Edit</button>' +
                        '<button type="button" class="qz-link-btn" data-action="toggle" data-id="' + quiz.id + '">' +
                            (quiz.published ? "Unpublish" : "Publish") + "</button>" +
                        '<button type="button" class="qz-link-btn danger" data-action="delete" data-id="' + quiz.id + '">Delete</button>' +
                    "</div>" +
                "</td>" +
            "</tr>"
        );
    }

    function renderTable() {
        if (quizzes.length === 0) {
            setTableMessage("No quizzes yet. Click “New Quiz” to create the first one.");
            return;
        }
        var list = visibleQuizzes();
        if (list.length === 0) {
            setTableMessage("No quizzes match your filters.");
            return;
        }
        byId("quizTableBody").innerHTML = list.map(rowHtml).join("");
    }

    function renderOverview() {
        var published = quizzes.filter(function (q) { return q.published; }).length;
        var known = 0;
        var unknown = 0;
        quizzes.forEach(function (q) {
            var n = questionCounts[q.id];
            if (typeof n === "number") { known += n; } else { unknown += 1; }
        });

        setText("statTotal", quizzes.length);
        setText("statTotalSub", quizzes.length === 0 ? "No quizzes yet" : "Quizzes created");
        setText("statPublished", published);
        setText("statPublishedSub", plural(quizzes.length - published, "draft") + " not visible to learners");
        setText("statQuestions", known);
        setText("statQuestionsSub", unknown > 0 ? "Counting…" : "Across all quizzes");
    }

    function render() {
        renderTable();
        renderOverview();
    }

    /* ---------- loading ---------- */

    async function loadQuestionCounts() {
        var results = await Promise.allSettled(quizzes.map(function (q) {
            return EduQuestAPI.get("/quizzes/" + q.id + "/questions");
        }));
        results.forEach(function (r, i) {
            questionCounts[quizzes[i].id] = r.status === "fulfilled" && Array.isArray(r.value) ? r.value.length : null;
        });
        render();
    }

    async function loadAll() {
        setTableMessage("Loading quizzes...");

        var results = await Promise.allSettled([
            EduQuestAPI.get("/quizzes"),
            EduQuestAPI.get("/topics")
        ]);

        if (authLost()) { return; }

        if (results[0].status === "rejected") {
            setTableMessage("Unable to load quizzes: " + results[0].reason.message);
            return;
        }

        quizzes = results[0].value.map(normQuiz);

        if (results[1].status === "fulfilled") {
            topics = results[1].value.map(normTopic).sort(function (a, b) {
                return topicLabel(a).localeCompare(topicLabel(b));
            });
        } else {
            topics = [];
            pageNotice("Topics could not be loaded, so quizzes can't be created or edited right now: " +
                results[1].reason.message, "error");
        }

        questionCounts = {};
        render();
        loadQuestionCounts();
    }

    /* ---------- quiz modal ---------- */

    function fillTopicSelect(selectedId) {
        var select = byId("quizTopic");
        var html = '<option value="">Select a topic</option>';
        topics.forEach(function (t) {
            html += '<option value="' + t.id + '"' + (t.id === selectedId ? " selected" : "") + ">" +
                esc(topicLabel(t)) + "</option>";
        });
        select.innerHTML = html;
        setText("topicHint", topics.length === 0 ? "No topics found. Add topics before creating a quiz." : "");
    }

    function updatePublishHint() {
        var hint = byId("publishHint");
        var wantsPublish = byId("quizPublished").checked;
        var count = editingQuiz ? questionCounts[editingQuiz.id] : 0;
        hint.textContent = wantsPublish && count === 0
            ? "This quiz has no questions yet. Add questions before learners can use it."
            : "";
    }

    function openQuizModal(quiz) {
        editingQuiz = quiz || null;
        formError("quizFormError", "");

        setText("quizModalTitle", quiz ? "Edit Quiz" : "New Quiz");
        byId("quizTitle").value = quiz ? quiz.title : "";
        fillTopicSelect(quiz ? quiz.topicId : null);
        byId("quizDifficulty").value = quiz && quiz.difficulty ? quiz.difficulty : "Medium";
        byId("quizMinutes").value = quiz && quiz.minutes ? quiz.minutes : 30;
        byId("quizPublished").checked = quiz ? quiz.published : false;
        byId("quizAutoBank").checked = !quiz;
        updatePublishHint();
        updateAutomaticBankToggle();

        show(byId("quizModal"), true);
        byId("quizTitle").focus();
    }

    function closeQuizModal() {
        show(byId("quizModal"), false);
        editingQuiz = null;
        bankAvailabilityRequest++;
    }

    // Only applies to NEW quizzes. Editing never silently replaces questions.
    var bankAvailabilityRequest = 0;

    function updateAutomaticBankToggle() {
        var container = byId("autoBankSettings");
        if (container) { container.hidden = !!editingQuiz; }
        refreshBankAvailability();
    }

    async function refreshBankAvailability() {
        var info = byId("autoBankAvailability");
        if (!info) { return; }
        var request = ++bankAvailabilityRequest;
        var useBank = !editingQuiz && byId("quizAutoBank").checked;
        if (!useBank) {
            info.textContent = editingQuiz
                ? ""
                : "Manual mode creates an unpublished draft. Add and review its questions before publishing.";
            return;
        }
        var topicId = Number(byId("quizTopic").value);
        var difficulty = byId("quizDifficulty").value;
        if (!topicId) {
            info.textContent = "Select a topic to check for 10 matching questions.";
            return;
        }
        info.textContent = "Checking Question Bank availability...";
        try {
            var result = lc(await EduQuestAPI.get(
                "/quizzes/bank-availability?topicId=" + encodeURIComponent(topicId) +
                "&difficulty=" + encodeURIComponent(difficulty)
            ));
            if (request !== bankAvailabilityRequest) { return; }
            var count = Number(result.available);
            info.textContent = result.cancreate
                ? count + " matching questions available. EduQuest will select 10 automatically."
                : "Only " + count + " matching valid questions. At least 10 are needed. " +
                  "Try another difficulty or turn off automatic selection to create a draft.";
        } catch (error) {
            if (request !== bankAvailabilityRequest) { return; }
            info.textContent = "Could not check Question Bank: " + error.message;
        }
    }

    async function saveQuiz(event) {
        event.preventDefault();
        formError("quizFormError", "");

        var title = byId("quizTitle").value.trim();
        var topicId = Number(byId("quizTopic").value);
        var minutes = Number(byId("quizMinutes").value);

        if (!title) { return formError("quizFormError", "Please enter a quiz title."); }
        if (!topicId) { return formError("quizFormError", "Please choose a topic."); }
        if (!Number.isInteger(minutes) || minutes < 1 || minutes > 180) {
            return formError("quizFormError", "The time limit must be a whole number between 1 and 180 minutes.");
        }

        var payload = {
            quizTitle: title,
            difficulty: byId("quizDifficulty").value,
            timeLimit: toTimeSpan(minutes),
            isPublished: byId("quizPublished").checked,
            topicID: topicId
        };

        var button = byId("saveQuiz");
        button.disabled = true;
        button.textContent = "Saving...";

        try {
            var wasEditing = !!editingQuiz;
            var useBank = !wasEditing && byId("quizAutoBank").checked;
            var created = null;
            if (wasEditing) {
                await EduQuestAPI.put("/quizzes/" + editingQuiz.id, payload);
            } else if (useBank) {
                created = await EduQuestAPI.post("/quizzes/create-from-bank", payload);
            } else {
                // A quiz without questions must not become visible to learners.
                payload.isPublished = false;
                await EduQuestAPI.post("/quizzes", payload);
            }
            closeQuizModal();
            pageNotice(wasEditing
                ? "Quiz updated. Existing questions were preserved."
                : (useBank
                    ? "Quiz created with " + Number(lc(created).questionsadded || 10) +
                      " matching approved Question Bank questions."
                    : "Manual quiz created. Open Questions to add them before publishing."), "");
            await loadAll();
        } catch (err) {
            if (authLost()) { return; }
            formError("quizFormError", err.message);
        } finally {
            button.disabled = false;
            button.textContent = "Save Quiz";
        }
    }

    /* ---------- publish / delete ---------- */

    async function togglePublish(quiz) {
        var count = questionCounts[quiz.id];
        if (!quiz.published && count === 0 &&
            !window.confirm("“" + quiz.title + "” has no questions yet. Publish it anyway?")) {
            return;
        }

        try {
            await EduQuestAPI.put("/quizzes/" + quiz.id, {
                quizTitle: quiz.title,
                difficulty: quiz.difficulty,
                timeLimit: toTimeSpan(quiz.minutes || 30),
                isPublished: !quiz.published,
                topicID: quiz.topicId
            });
            pageNotice("“" + quiz.title + "” is now " + (quiz.published ? "a draft." : "published."), "");
            quiz.published = !quiz.published;
            render();
        } catch (err) {
            if (authLost()) { return; }
            pageNotice(err.message, "error");
        }
    }

    async function deleteQuiz(quiz) {
        if (!window.confirm("Delete “" + quiz.title + "” and all of its questions? This cannot be undone.")) {
            return;
        }
        try {
            await EduQuestAPI.del("/quizzes/" + quiz.id);
            pageNotice("Quiz deleted.", "");
            await loadAll();
        } catch (err) {
            if (authLost()) { return; }
            pageNotice(err.message + " If learners have already attempted this quiz it may not be deletable. " +
                "Unpublish it instead.", "error");
        }
    }

    /* ---------- questions: loading and list ---------- */

    function normQuestion(raw, options, optionsError) {
        var q = lc(raw);
        return {
            optionsError: optionsError || null,
            id: q.quizquestionid,
            text: q.questiontext || "",
            explanation: q.explanation || "",
            ai: !!q.generatedbyai,
            approved: q.approvedbyadmin === undefined ? true : !!q.approvedbyadmin,
            options: (options || []).map(function (o) {
                var l = lc(o);
                return { id: l.quizoptionid, text: l.optiontext || "", correct: !!l.iscorrect };
            })
        };
    }

    function questionHtml(question, index) {
        var optionsHtml;
        if (question.optionsError) {
            optionsHtml =
                '<li class="missing">Could not load the options for this question (' + esc(String(question.optionsError).replace(/\.$/, "")) + "). " +
                '<button type="button" class="qz-link-btn" data-qaction="reload" data-qid="' + question.id + '">Try again</button></li>';
        } else if (question.options.length === 0) {
            optionsHtml =
                '<li class="missing">No options are saved for this question. ' +
                '<button type="button" class="qz-link-btn" data-qaction="restore" data-qid="' + question.id + '">Restore from question bank</button> ' +
                'or use Edit to add them.</li>';
        } else {
            optionsHtml = question.options.map(function (o) {
                return '<li class="' + (o.correct ? "correct" : "") + '">' +
                    (o.correct ? "✓ " : "") + math(o.text) + "</li>";
            }).join("");
        }

        return (
            '<div class="qz-question">' +
                '<div class="qz-question-head">' +
                    "<p>" + (index + 1) + ". " + math(question.text) +
                        (question.ai ? ' <span class="qz-badge ai">AI</span>' : "") + "</p>" +
                    '<div class="qz-row-actions">' +
                        '<button type="button" class="qz-link-btn" data-qaction="edit" data-qid="' + question.id + '">Edit</button>' +
                        '<button type="button" class="qz-link-btn danger" data-qaction="delete" data-qid="' + question.id + '">Delete</button>' +
                    "</div>" +
                "</div>" +
                "<ul>" + optionsHtml + "</ul>" +
                (question.explanation ? '<p class="qz-explain"><strong>Explanation:</strong> ' + math(question.explanation) + "</p>" : "") +
            "</div>"
        );
    }

    function renderQuestions() {
        var list = byId("questionList");
        if (questions.length === 0) {
            list.innerHTML = '<p class="qz-empty">This quiz has no questions yet. ' +
                "Add one yourself, take some from the question bank, or generate them with AI.</p>";
        } else {
            list.innerHTML = questions.map(questionHtml).join("");
        }
        setText("questionsSub", plural(questions.length, "question") + " in this quiz");

        /* Keep the table behind the modal in step. */
        questionCounts[activeQuiz.id] = questions.length;
        render();
    }

    async function loadQuestions() {
        byId("questionList").innerHTML = '<p class="qz-empty">Loading questions...</p>';
        try {
            var raw = await EduQuestAPI.get("/quizzes/" + activeQuiz.id + "/questions");
            var optionResults = await Promise.allSettled(raw.map(function (q) {
                return EduQuestAPI.get("/questions/" + lc(q).quizquestionid + "/options");
            }));
            questions = raw.map(function (q, i) {
                var r = optionResults[i];
                if (r.status === "fulfilled") { return normQuestion(q, r.value); }
                console.error("Could not load options for question " + lc(q).quizquestionid + ":", r.reason);
                return normQuestion(q, [], r.reason && r.reason.message ? r.reason.message : "unknown error");
            });
            renderQuestions();
        } catch (err) {
            if (authLost()) { return; }
            byId("questionList").innerHTML = '<p class="qz-empty">Unable to load questions: ' + esc(err.message) + "</p>";
        }
    }

    function showListView() {
        show(byId("questionForm"), false);
        show(byId("questionsListView"), true);
    }

    function openQuestionsModal(quiz) {
        activeQuiz = quiz;
        editingQuestion = null;
        questions = [];
        closeBulk();
        show(byId("questionsNotice"), false);
        setText("questionsTitle", quiz.title);
        setText("questionsSub", "Loading...");
        showListView();
        show(byId("questionsModal"), true);
        loadQuestions();
    }

    function closeQuestionsModal() {
        show(byId("questionsModal"), false);
        activeQuiz = null;
        editingQuestion = null;
    }

    /* ---------- questions: bank / AI ---------- */

    function openBulk(mode) {
        bulkMode = mode;
        var isAi = mode === "ai";
        var count = byId("bulkCount");
        count.max = isAi ? 50 : 100;
        count.value = isAi ? 5 : 5;
        setText("bulkLabel", isAi ? "How many questions should AI write?" : "How many questions from the bank?");
        setText("bulkRun", isAi ? "Generate" : "Add");
        setText("bulkHint", isAi
            ? "Uses the study materials uploaded for this quiz's topic. This can take up to a minute."
            : "Picks random questions that match this quiz's topic and difficulty.");
        show(byId("bulkForm"), true);
        count.focus();
    }

    function closeBulk() {
        bulkMode = null;
        show(byId("bulkForm"), false);
    }

    async function runBulk(event) {
        event.preventDefault();
        var count = Number(byId("bulkCount").value);
        var max = bulkMode === "ai" ? 50 : 100;
        if (!Number.isInteger(count) || count < 1 || count > max) {
            return questionsNotice("Enter a whole number between 1 and " + max + ".", "error");
        }

        var isAi = bulkMode === "ai";
        var button = byId("bulkRun");
        var label = button.textContent;
        button.disabled = true;
        button.textContent = isAi ? "Generating..." : "Adding...";

        try {
            var result = await EduQuestAPI.post(
                "/quizzes/" + activeQuiz.id + "/questions/" + (isAi ? "generate" : "from-bank"),
                { count: count }
            );
            var r = lc(result);
            var added = (r.questions || []).length;
            closeBulk();
            await loadQuestions();
            questionsNotice(
                (isAi ? "Generated " : "Added ") + plural(added, "question") + ".",
                "",
                r.notes && r.notes.length ? r.notes : null
            );
        } catch (err) {
            if (authLost()) { return; }
            questionsNotice(err.message, "error");
        } finally {
            button.disabled = false;
            button.textContent = label;
        }
    }

    /* ---------- questions: add / edit form ---------- */

    function optionRowHtml(index, option) {
        return (
            '<div class="qz-option-row" data-option-id="' + (option && option.id ? option.id : "") + '">' +
                '<input type="radio" name="correctOption" value="' + index + '" aria-label="Correct answer"' +
                    (option && option.correct ? " checked" : "") + ">" +
                '<input type="text" maxlength="300" placeholder="Option text" aria-label="Option text" value="' +
                    esc(option ? option.text : "") + '">' +
                '<button type="button" class="qz-link-btn danger" data-remove-option title="Remove option">✕</button>' +
            "</div>"
        );
    }

    function optionRows() {
        return Array.prototype.slice.call(byId("optionRows").querySelectorAll(".qz-option-row"));
    }

    /* Radio buttons are renumbered after every add / remove so the "correct" value stays an index. */
    function renumberOptions() {
        optionRows().forEach(function (row, i) {
            row.querySelector('input[type="radio"]').value = i;
        });
        byId("addOptionBtn").disabled = optionRows().length >= MAX_OPTIONS;
    }

    function addOptionRow(option) {
        var holder = document.createElement("div");
        holder.innerHTML = optionRowHtml(optionRows().length, option);
        byId("optionRows").appendChild(holder.firstChild);
        renumberOptions();
    }

    function updatePreviews() {
        byId("questionPreview").innerHTML = math(byId("questionText").value) || "Preview appears here";
        byId("explanationPreview").innerHTML = math(byId("questionExplanation").value) || "Preview appears here";
    }

    function openQuestionForm(question) {
        editingQuestion = question || null;
        formError("questionFormError", "");
        setText("questionFormTitle", question ? "Edit question" : "Add question");
        byId("questionText").value = question ? question.text : "";
        byId("questionExplanation").value = question ? question.explanation : "";

        byId("optionRows").innerHTML = "";
        if (question && question.options.length > 0) {
            question.options.forEach(function (o) { addOptionRow(o); });
        } else {
            for (var i = 0; i < 4; i++) { addOptionRow(null); }
        }

        updatePreviews();
        show(byId("questionsNotice"), false);
        show(byId("questionsListView"), false);
        show(byId("questionForm"), true);
        byId("questionText").focus();
    }

    function readQuestionForm() {
        var text = byId("questionText").value.trim();
        var explanation = byId("questionExplanation").value.trim();
        var rows = optionRows();
        var options = [];
        var correctIndex = -1;

        rows.forEach(function (row, i) {
            var value = row.querySelector('input[type="text"]').value.trim();
            if (!value) { return; }
            if (row.querySelector('input[type="radio"]').checked) { correctIndex = options.length; }
            options.push({
                id: row.getAttribute("data-option-id") ? Number(row.getAttribute("data-option-id")) : null,
                text: value,
                correct: false
            });
        });

        if (!text) { return { error: "Please type the question." }; }
        if (options.length < MIN_OPTIONS) { return { error: "Add at least " + MIN_OPTIONS + " answer options." }; }
        if (correctIndex === -1) { return { error: "Select which option is correct (and make sure it has text)." }; }

        var seen = {};
        for (var i = 0; i < options.length; i++) {
            var key = options[i].text.toLowerCase();
            if (seen[key]) { return { error: "Two options have the same text: “" + options[i].text + "”." }; }
            seen[key] = true;
        }
        if (!explanation) { return { error: "Please add an explanation for the correct answer." }; }

        options[correctIndex].correct = true;
        return { text: text, explanation: explanation, options: options };
    }

    async function createQuestion(data) {
        var created = await EduQuestAPI.post("/quizzes/" + activeQuiz.id + "/questions", {
            questionText: data.text,
            explanation: data.explanation,
            generatedByAI: false,
            approvedByAdmin: true
        });
        var questionId = lc(created).quizquestionid;

        try {
            for (var i = 0; i < data.options.length; i++) {
                await EduQuestAPI.post("/questions/" + questionId + "/options", {
                    optionText: data.options[i].text,
                    isCorrect: data.options[i].correct
                });
            }
        } catch (err) {
            /* Don't leave a half-built question behind. */
            try { await EduQuestAPI.del("/questions/" + questionId); } catch (ignore) { /* nothing more to do */ }
            throw new Error("The question could not be saved (" + err.message + "). Nothing was added.");
        }
    }

    async function updateQuestion(data) {
        await EduQuestAPI.put("/questions/" + editingQuestion.id, {
            questionText: data.text,
            explanation: data.explanation,
            generatedByAI: editingQuestion.ai,
            approvedByAdmin: editingQuestion.approved
        });

        var keptIds = data.options.filter(function (o) { return o.id; }).map(function (o) { return o.id; });

        /* Remove first, then edit, then add: avoids "option already exists" clashes. */
        for (var i = 0; i < editingQuestion.options.length; i++) {
            if (keptIds.indexOf(editingQuestion.options[i].id) === -1) {
                await EduQuestAPI.del("/options/" + editingQuestion.options[i].id);
            }
        }
        for (var j = 0; j < data.options.length; j++) {
            var o = data.options[j];
            if (o.id) {
                await EduQuestAPI.put("/options/" + o.id, { optionText: o.text, isCorrect: o.correct });
            }
        }
        for (var k = 0; k < data.options.length; k++) {
            var added = data.options[k];
            if (!added.id) {
                await EduQuestAPI.post("/questions/" + editingQuestion.id + "/options", {
                    optionText: added.text,
                    isCorrect: added.correct
                });
            }
        }
    }

    async function saveQuestion(event) {
        event.preventDefault();
        formError("questionFormError", "");

        var data = readQuestionForm();
        if (data.error) { return formError("questionFormError", data.error); }

        var button = byId("saveQuestion");
        button.disabled = true;
        button.textContent = "Saving...";

        try {
            var wasEditing = !!editingQuestion;
            if (wasEditing) { await updateQuestion(data); } else { await createQuestion(data); }
            editingQuestion = null;
            showListView();
            await loadQuestions();
            questionsNotice(wasEditing ? "Question updated." : "Question added.", "");
        } catch (err) {
            if (authLost()) { return; }
            formError("questionFormError", err.message);
        } finally {
            button.disabled = false;
            button.textContent = "Save Question";
        }
    }

    /* Copies the options of the matching question-bank item onto a question that has none. */
    async function restoreOptions(question) {
        try {
            var bank = await EduQuestAPI.get("/questionbank");
            var wanted = question.text.trim().toLowerCase();
            var match = null;
            bank.forEach(function (item) {
                var b = lc(item);
                if (!match && String(b.questiontext || "").trim().toLowerCase() === wanted) { match = b; }
            });

            if (!match) {
                return questionsNotice("No question-bank item has exactly this question text. " +
                    "Use Edit to add the options yourself.", "error");
            }
            var bankOptions = (match.options || []).map(lc);
            if (bankOptions.length === 0) {
                return questionsNotice("The matching question-bank item has no options saved either, " +
                    "so there is nothing to restore. Use Edit to add them.", "error");
            }

            for (var i = 0; i < bankOptions.length; i++) {
                await EduQuestAPI.post("/questions/" + question.id + "/options", {
                    optionText: bankOptions[i].optiontext,
                    isCorrect: !!bankOptions[i].iscorrect
                });
            }
            await loadQuestions();
            questionsNotice("Restored " + plural(bankOptions.length, "option") + " from the question bank.", "");
        } catch (err) {
            if (authLost()) { return; }
            questionsNotice(err.message, "error");
        }
    }

    async function deleteQuestion(question) {
        if (!window.confirm("Delete this question and its options?")) { return; }
        try {
            await EduQuestAPI.del("/questions/" + question.id);
            await loadQuestions();
            questionsNotice("Question deleted.", "");
        } catch (err) {
            if (authLost()) { return; }
            questionsNotice(err.message, "error");
        }
    }

    /* ---------- wiring ---------- */

    function findQuestion(id) {
        for (var i = 0; i < questions.length; i++) {
            if (questions[i].id === id) { return questions[i]; }
        }
        return null;
    }

    function init() {
        byId("quizSearch").addEventListener("input", renderTable);
        byId("difficultyFilter").addEventListener("change", renderTable);
        byId("statusFilter").addEventListener("change", renderTable);

        byId("openAddQuiz").addEventListener("click", function () { openQuizModal(null); });
        byId("cancelQuiz").addEventListener("click", closeQuizModal);
        byId("quizForm").addEventListener("submit", saveQuiz);
        byId("quizPublished").addEventListener("change", updatePublishHint);
        byId("quizTopic").addEventListener("change", refreshBankAvailability);
        byId("quizDifficulty").addEventListener("change", refreshBankAvailability);
        byId("quizAutoBank").addEventListener("change", refreshBankAvailability);

        byId("quizTableBody").addEventListener("click", function (event) {
            var button = event.target.closest("button[data-action]");
            if (!button) { return; }
            var quiz = findQuiz(Number(button.dataset.id));
            if (!quiz) { return; }

            var action = button.dataset.action;
            if (action === "questions") { openQuestionsModal(quiz); }
            else if (action === "edit") { openQuizModal(quiz); }
            else if (action === "toggle") { togglePublish(quiz); }
            else if (action === "delete") { deleteQuiz(quiz); }
        });

        byId("closeQuestions").addEventListener("click", closeQuestionsModal);
        byId("addQuestionBtn").addEventListener("click", function () { closeBulk(); openQuestionForm(null); });
        byId("bankBtn").addEventListener("click", function () { openBulk("bank"); });
        byId("aiBtn").addEventListener("click", function () { openBulk("ai"); });
        byId("bulkForm").addEventListener("submit", runBulk);
        byId("bulkCancel").addEventListener("click", closeBulk);

        byId("questionList").addEventListener("click", function (event) {
            var button = event.target.closest("button[data-qaction]");
            if (!button) { return; }
            var question = findQuestion(Number(button.dataset.qid));
            if (!question) { return; }
            var qaction = button.dataset.qaction;
            if (qaction === "edit") { openQuestionForm(question); }
            else if (qaction === "delete") { deleteQuestion(question); }
            else if (qaction === "restore") { restoreOptions(question); }
            else if (qaction === "reload") { loadQuestions(); }
        });

        byId("questionForm").addEventListener("submit", saveQuestion);
        byId("questionText").addEventListener("input", updatePreviews);
        byId("questionExplanation").addEventListener("input", updatePreviews);
        byId("cancelQuestion").addEventListener("click", function () { editingQuestion = null; showListView(); });
        byId("addOptionBtn").addEventListener("click", function () { addOptionRow(null); });
        byId("optionRows").addEventListener("click", function (event) {
            var remove = event.target.closest("[data-remove-option]");
            if (!remove) { return; }
            if (optionRows().length <= MIN_OPTIONS) {
                return formError("questionFormError", "A question needs at least " + MIN_OPTIONS + " options.");
            }
            remove.closest(".qz-option-row").remove();
            renumberOptions();
        });

        /* Escape closes whichever dialog is on top; clicking the dark backdrop does too. */
        document.addEventListener("keydown", function (event) {
            if (event.key !== "Escape") { return; }
            if (!byId("quizModal").hidden) { closeQuizModal(); }
            else if (!byId("questionsModal").hidden) {
                if (!byId("questionForm").hidden) { editingQuestion = null; showListView(); }
                else { closeQuestionsModal(); }
            }
        });
        byId("quizModal").addEventListener("mousedown", function (event) {
            if (event.target === byId("quizModal")) { closeQuizModal(); }
        });

        loadAll();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
