/* =============================================================
   EDUQUEST LEARNER QUIZ — complete backend-connected replacement.
   - Real quiz and attempt IDs; grades only on the server.
   - Reuses the administrator's EduQuestMath formatter for all
     question text, answer options, and result explanations.
   - Uses explicit attempt IDs when reviewing older quiz results.
   - Requires js/api.js, KaTeX, and ../../js/math-format.js.
============================================================= */
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", async function () {
        const api = window.EduQuestAPI;
        const root = document.getElementById("quiz-root");
        const pageTitle = document.getElementById("quiz-title");
        const pageSubtitle = document.getElementById("quiz-subject");
        const timer = document.getElementById("quiz-timer");
        if (!api || !root) return;
        if (!api.requireLearner()) return;

        const params = new URLSearchParams(location.search);
        const quizId = Number(params.get("id"));
        const review = params.get("review") === "1";
        const requestedAttemptId = Number(params.get("attempt"));
        const currentUser = api.getCurrentUser();
        const lastAttemptKey = "eduquest.lastQuizAttempt." + currentUser.userId + "." + quizId;
        const LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        let quiz = null;
        let attemptId = null;
        let questions = [];
        let answers = [];
        let current = 0;
        let startedAt = null;
        let timerHandle = null;
        let sending = false;
        let submittedAttemptsUsed = null;
        let bestAttemptId = null;
        let attemptHistory = [];

        async function refreshAttemptUsage() {
            try {
                // One submitted attempt is a completion; unfinished starts
                // are intentionally not charged against the two allowed tries.
                const history = await api.get("/learner-quiz-progress");
                if (!Array.isArray(history)) {
                    throw new Error("Invalid learner progress response.");
                }
                const progress = history.find(item => Number(item.quizID) === quizId);
                submittedAttemptsUsed = progress ? Number(progress.completedAttempts) || 0 : 0;
                bestAttemptId = progress ? Number(progress.bestAttemptID || progress.quizAttemptID) : null;
                attemptHistory = progress && Array.isArray(progress.attempts) ? progress.attempts : [];
            } catch (error) {
                submittedAttemptsUsed = null;
                bestAttemptId = null;
                attemptHistory = [];
                console.warn("Could not verify remaining quiz attempts:", error);
            }
        }

        function escapeHtml(text) {
            return String(text ?? "").replace(/[&<>"']/g, char => ({
                "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
            })[char]);
        }

        // The formatter HTML-escapes ordinary text, while rendering $...$,
        // $$...$$, \\( ... \\), \\[ ... \\] and common unwrapped TeX.
        function math(text) {
            return window.EduQuestMath && typeof window.EduQuestMath.toHtml === "function"
                ? window.EduQuestMath.toHtml(text)
                : escapeHtml(text);
        }

        function show(html) {
            root.innerHTML = html;
            const focus = root.querySelector("[data-focus]");
            if (focus) {
                focus.setAttribute("tabindex", "-1");
                focus.focus();
            }
        }

        function notify(message) {
            show('<section class="card quiz-card"><p class="quiz-note" role="status">' +
                escapeHtml(message) + '</p></section>');
        }

        function errorPage(message, canRetry = false) {
            show('<section class="card quiz-card">' +
                '<h2 data-focus>Unable to load quiz</h2>' +
                '<p class="quiz-intro-meta">' + escapeHtml(message) + '</p>' +
                '<div class="quiz-actions">' +
                '<a href="Quizzes.html" class="btn btn-outline">Back to quizzes</a>' +
                (canRetry ? '<button class="btn btn-primary" type="button" id="reload-quiz">Try Again</button>' : '') +
                '</div></section>');
            document.getElementById("reload-quiz")?.addEventListener("click", () => location.reload());
        }

        function timeSpanSeconds(value) {
            if (typeof value !== "string") return 0;
            const match = /^(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})(?:\.\d+)?$/.exec(value);
            if (!match) return 0;
            return Number(match[1] || 0) * 86400 + Number(match[2]) * 3600 +
                Number(match[3]) * 60 + Number(match[4]);
        }

        function duration(seconds) {
            seconds = Math.max(0, Math.floor(Number(seconds) || 0));
            const hours = Math.floor(seconds / 3600);
            const mins = String(Math.floor(seconds % 3600 / 60)).padStart(2, "0");
            const secs = String(seconds % 60).padStart(2, "0");
            return hours ? hours + ":" + mins + ":" + secs : mins + ":" + secs;
        }

        function stopTimer() {
            if (timerHandle !== null) clearInterval(timerHandle);
            timerHandle = null;
        }

        function refreshTimer() {
            if (startedAt === null) return;
            const elapsed = Math.floor((Date.now() - startedAt) / 1000);
            const limit = timeSpanSeconds(quiz.timeLimit);
            const left = limit ? Math.max(0, limit - elapsed) : elapsed;
            timer.textContent = (limit ? "Time remaining " : "Time ") + duration(left);
            // The current API does NOT enforce a time limit on submission.
            // We display it but don't invent an automatic 'timed out' grading action.
            if (limit && left === 0) stopTimer();
        }

        function startTimer() {
            stopTimer();
            startedAt = Date.now();
            timer.hidden = false;
            refreshTimer();
            timerHandle = setInterval(refreshTimer, 1000);
        }

        function answeredCount() {
            return answers.filter(answer => answer !== null).length;
        }

        function findSavedId() {
            try {
                const id = Number(sessionStorage.getItem(lastAttemptKey));
                return Number.isInteger(id) && id > 0 ? id : null;
            } catch (_) {
                return null;
            }
        }

        function saveAttempt(id) {
            try {
                sessionStorage.setItem(lastAttemptKey, String(id));
            } catch (_) {
                // The explicit review link works independently of storage.
            }
        }

        // Review buttons never depend on sessionStorage: IDs come from the database.
        function reviewActions() {
            if (attemptHistory.length) {
                return attemptHistory.map((item, index) => {
                    const id = Number(item.attemptID);
                    if (!Number.isInteger(id) || id <= 0) return "";
                    return '<a class="btn btn-outline" href="quiz.html?id=' + quizId +
                        '&review=1&attempt=' + id + '">Review Attempt ' + (index + 1) + '</a>';
                }).join("");
            }
            return Number.isInteger(bestAttemptId) && bestAttemptId > 0
                ? '<a class="btn btn-outline" href="quiz.html?id=' + quizId +
                    '&review=1&attempt=' + bestAttemptId + '">Review Results</a>'
                : "";
        }

        // When an attempt is blocked, display review rather than a useless reload.
        async function showAttemptLimit(message) {
            await refreshAttemptUsage();
            stopTimer();
            timer.hidden = true;
            show('<section class="card quiz-card">' +
                '<h2 data-focus>Quiz attempts completed</h2>' +
                '<p class="quiz-intro-meta">' + escapeHtml(message) + '</p>' +
                '<div class="quiz-actions">' +
                '<a class="btn btn-outline" href="Quizzes.html">Back to quizzes</a>' +
                reviewActions() +
                '</div>' +
                (!reviewActions() ? '<p class="quiz-note">Your attempts were blocked, but the progress API did not return review IDs. Check GET /api/learner-quiz-progress in Swagger.</p>' : '') +
                '</section>');
        }

        function intro() {
            stopTimer();
            timer.hidden = true;
            const limit = timeSpanSeconds(quiz.timeLimit);
            const triesKnown = submittedAttemptsUsed !== null;
            const used = triesKnown ? Math.max(0, submittedAttemptsUsed) : 0;
            const maxUsed = triesKnown && used >= 2;
            const label = triesKnown
                ? used + " of 2 submitted attempts used"
                : "Two submissions permitted (usage unavailable)";
            const reviewLinks = reviewActions();
            show('<section class="card quiz-card">' +
                '<h2 data-focus>' + (maxUsed ? 'Quiz attempts completed' : 'Ready to start?') + '</h2>' +
                '<p class="quiz-intro-meta">' + math(quiz.quizTitle) + '</p>' +
                '<ul class="quiz-tips">' +
                '<li>Difficulty: ' + escapeHtml(quiz.difficulty) + '</li>' +
                '<li>10 randomly selected approved questions</li>' +
                '<li>Time limit: ' + (limit ? duration(limit) : "Not specified") + '</li>' +
                '<li>' + escapeHtml(label) + '</li>' +
                '<li>Only successfully submitted quizzes count as attempts; unfinished quizzes resume.</li>' +
                '<li>Your best score across the two submissions counts toward your average.</li>' +
                '<li>Choose one answer per question. You can review answers before submitting.</li>' +
                '</ul><div class="quiz-actions">' +
                '<a class="btn btn-outline" href="Quizzes.html">Back to quizzes</a>' +
                reviewLinks +
                (maxUsed ? '' : '<button class="btn btn-primary" type="button" id="start-quiz">' +
                    (used > 0 ? 'Start Final Attempt' : 'Start Quiz') + '</button>') +
                '</div></section>');
            document.getElementById("start-quiz")?.addEventListener("click", startQuiz);
        }

        async function startQuiz() {
            if (sending) return;
            notify("Preparing your quiz questions…");
            try {
                const response = await api.post("/quizzes/" + quizId + "/start", {});
                if (!response || !Number.isInteger(Number(response.quizAttemptID)) ||
                    !Array.isArray(response.questions) || !response.questions.length) {
                    throw new Error("The API did not return a valid quiz attempt.");
                }
                const loaded = response.questions.slice().sort(
                    (a, b) => Number(a.questionOrder) - Number(b.questionOrder)
                );
                for (const question of loaded) {
                    if (!Number.isInteger(Number(question.quizAttemptQuestionID)) ||
                        !Array.isArray(question.options) || question.options.length < 2) {
                        throw new Error("The quiz contains incomplete question information.");
                    }
                }
                attemptId = Number(response.quizAttemptID);
                questions = loaded;
                quiz.timeLimit = response.timeLimit || quiz.timeLimit;
                answers = questions.map(() => null);
                current = 0;
                startTimer();
                showQuestion();
            } catch (error) {
                console.error("Unable to start quiz:", error);
                if (/both attempts|used both|attempt limit|already used/i.test(error.message || "")) {
                    await showAttemptLimit(error.message);
                } else {
                    errorPage(error.message || "Could not start the quiz.", true);
                }
            }

        }

        function showQuestion() {
            const question = questions[current];
            const last = current === questions.length - 1;
            const options = question.options.map((option, index) => {
                const selected = Number(answers[current]) === Number(option.quizOptionID);
                return '<label class="quiz-option ' + (selected ? 'selected' : '') + '">' +
                    '<input type="radio" name="answer" value="' + Number(option.quizOptionID) + '" ' +
                    (selected ? 'checked' : '') + '>' +
                    '<span class="quiz-option-mark">' + (LETTERS[index] || String(index + 1)) + '</span>' +
                    '<span class="quiz-option-text">' + math(option.optionText) + '</span>' +
                    '</label>';
            }).join("");
            const dots = questions.map((_, index) =>
                '<button type="button" class="quiz-dot ' +
                (index === current ? 'current ' : '') + (answers[index] !== null ? 'answered' : '') +
                '" data-go="' + index + '" aria-label="Question ' + (index + 1) +
                (answers[index] !== null ? ' answered' : ' unanswered') + '">' + (index + 1) + '</button>'
            ).join("");

            show('<section class="card quiz-card">' +
                '<div class="quiz-progress-head"><span>Question ' + (current + 1) +
                ' of ' + questions.length + '</span><span>' + answeredCount() + ' answered</span></div>' +
                '<div class="progress-bar"><div class="progress-fill" style="width:' +
                Math.round((current + 1) / questions.length * 100) + '%"></div></div>' +
                '<h2 class="quiz-question" data-focus>' + math(question.questionText) + '</h2>' +
                '<div class="quiz-options" role="radiogroup" aria-label="Answer choices">' + options + '</div>' +
                '<div class="quiz-actions">' +
                '<button type="button" class="btn btn-outline" id="prev" ' +
                (current === 0 ? 'disabled' : '') + '>Previous</button>' +
                '<button type="button" class="btn btn-primary" id="next">' +
                (last ? 'Review Answers' : 'Next Question') + '</button></div></section>' +
                '<nav class="quiz-dots" aria-label="Question navigation">' + dots + '</nav>');

            root.querySelectorAll('input[name="answer"]').forEach(input => {
                input.addEventListener("change", () => {
                    answers[current] = Number(input.value);
                    // Redraw to update the question dots and selected styling.
                    showQuestion();
                });
            });
            root.querySelectorAll("[data-go]").forEach(button => {
                button.addEventListener("click", () => {
                    current = Number(button.dataset.go);
                    showQuestion();
                });
            });
            document.getElementById("prev").addEventListener("click", () => {
                if (current > 0) { current--; showQuestion(); }
            });
            document.getElementById("next").addEventListener("click", () => {
                if (last) showReview();
                else { current++; showQuestion(); }
            });
        }

        function showReview() {
            const missing = questions.length - answeredCount();
            const rows = questions.map((question, index) => {
                const picked = question.options.find(o => Number(o.quizOptionID) === answers[index]);
                return '<li class="quiz-review-row">' +
                    '<span class="quiz-status-icon ' + (picked ? 'done' : 'pending') + '">' +
                    (picked ? '✓' : '•') + '</span>' +
                    '<span class="quiz-review-text">Question ' + (index + 1) +
                    '<small>' + (picked ? 'Your answer: ' + math(picked.optionText) : 'Not answered') + '</small></span>' +
                    '<button type="button" class="btn btn-outline" data-edit="' + index + '">' +
                    (picked ? 'Change' : 'Answer') + '</button></li>';
            }).join("");
            show('<section class="card quiz-card"><h2 data-focus>Review Your Answers</h2>' +
                '<p class="quiz-intro-meta">You answered ' + answeredCount() + ' of ' +
                questions.length + ' questions.</p>' +
                (missing ? '<p class="quiz-note" role="status">Please answer the remaining ' +
                    missing + ' question(s) before submitting.</p>' :
                    '<p class="quiz-note">All questions are answered. You can submit now.</p>') +
                '<ul class="quiz-review-list">' + rows + '</ul>' +
                '<div class="quiz-actions">' +
                '<button class="btn btn-outline" id="back" type="button">Back to Questions</button>' +
                '<button class="btn btn-primary" id="submit" type="button" ' +
                (missing ? 'disabled' : '') + '>Submit Quiz</button></div>' +
                '<p id="submit-error" role="alert"></p></section>');
            root.querySelectorAll("[data-edit]").forEach(button => {
                button.addEventListener("click", () => {
                    current = Number(button.dataset.edit);
                    showQuestion();
                });
            });
            document.getElementById("back").addEventListener("click", () => {
                current = Math.max(0, answers.indexOf(null));
                showQuestion();
            });
            document.getElementById("submit").addEventListener("click", submit);
        }

        async function submit() {
            if (sending || answeredCount() !== questions.length) return;
            sending = true;
            const button = document.getElementById("submit");
            button.disabled = true;
            button.textContent = "Submitting…";
            try {
                await api.post("/quizattempts/" + attemptId + "/submit", {
                    answers: questions.map((question, index) => ({
                        quizAttemptQuestionID: Number(question.quizAttemptQuestionID),
                        quizOptionID: Number(answers[index])
                    }))
                });
                stopTimer();
                saveAttempt(attemptId);
                // The database is the source of truth; immediately retrieve
                // the grade and explanations returned by the server.
                await loadResult(attemptId);
            } catch (error) {
                console.error("Quiz submission or result retrieval failed:", error);
                const notice = document.getElementById("submit-error");
                if (notice) notice.textContent = error.message || "Submission failed. Please try again.";
                if (button) {
                    button.disabled = false;
                    button.textContent = "Submit Quiz";
                }
            } finally {
                sending = false;
            }
        }

        async function loadResult(id) {
            notify("Loading your marked quiz…");
            try {
                const result = await api.get("/quizattempts/" + id + "/result");
                if (!result || Number(result.quizID) !== quizId) {
                    throw new Error("These results don't belong to the selected quiz.");
                }
                stopTimer();
                timer.hidden = true;
                saveAttempt(id);
                await refreshAttemptUsage();
                showResult(result);
            } catch (error) {
                console.error("Could not load quiz results:", error);
                errorPage(error.message || "Unable to retrieve results.", true);
            }
        }

        function showResult(result) {
            const percent = Number(result.percentage) || 0;
            const score = Number(result.score) || 0;
            const total = Number(result.totalQuestions) || 0;
            const msg = percent >= 80
                ? "Excellent work!"
                : percent >= 50 ? "Good effort! Review the explanations and keep practising."
                    : "Keep practising. Review the explanations and try again.";
            const items = (result.questions || []).slice()
                .sort((a, b) => Number(a.questionOrder) - Number(b.questionOrder))
                .map((question, i) =>
                    '<li class="quiz-result-item">' +
                    '<div class="quiz-status-icon ' + (question.isCorrect ? 'done' : 'wrong') + '">' +
                    (question.isCorrect ? '✓' : '✕') + '</div>' +
                    '<div><h4>' + (i + 1) + '. ' + math(question.questionText) + '</h4>' +
                    '<p class="quiz-result-line">Your answer: <strong>' + math(question.selectedAnswer) + '</strong></p>' +
                    (!question.isCorrect ? '<p class="quiz-result-line">Correct answer: <strong>' +
                        math(question.correctAnswer) + '</strong></p>' : '') +
                    (question.explanation ? '<p class="quiz-explain">' + math(question.explanation) + '</p>' : '') +
                    '</div></li>'
                ).join("");
            show('<section class="card quiz-card quiz-result-head">' +
                '<h2 data-focus>You scored ' + Math.round(percent) + '%</h2><p>' + msg + '</p></section>' +
                '<div class="profile-stats" style="margin-top:18px">' +
                '<div class="profile-stat"><div class="stat-number">' + Math.round(percent) +
                '%</div><div class="stat-text">Score</div></div>' +
                '<div class="profile-stat"><div class="stat-number">' + score + ' / ' + total +
                '</div><div class="stat-text">Correct</div></div>' +
                '</div>' +
                '<section class="card quiz-card"><h3 class="dashboard-section-title">Your Answers</h3>' +
                '<ul class="quiz-result-list">' + items + '</ul><div class="quiz-actions">' +
                '<a href="Quizzes.html" class="btn btn-outline">Back to quizzes</a>' +
                reviewActions() +
                (submittedAttemptsUsed !== null && submittedAttemptsUsed < 2
                    ? '<button id="retry" type="button" class="btn btn-primary">' +
                      'Try Again (' + Math.max(0, 2 - submittedAttemptsUsed) + ' remaining)</button>'
                    : '') +
                '</div></section>');
            document.getElementById("retry")?.addEventListener("click", () => {
                attemptId = null;
                questions = [];
                answers = [];
                current = 0;
                startedAt = null;
                intro();
            });
        }

        if (!Number.isInteger(quizId) || quizId <= 0) {
            errorPage("The quiz link is invalid. Open a quiz from the quiz list.");
            return;
        }
        try {
            notify("Loading quiz…");
            quiz = await api.get("/quizzes/" + quizId);
            if (!quiz || !quiz.isPublished) {
                errorPage("The quiz could not be found or is not published.");
                return;
            }
            pageTitle.textContent = quiz.quizTitle;
            pageSubtitle.textContent = (quiz.difficulty || "") + " Quiz";
            document.title = "EduQuest | " + quiz.quizTitle;
            await refreshAttemptUsage();
            if (review) {
                const id = Number.isInteger(requestedAttemptId) && requestedAttemptId > 0
                    ? requestedAttemptId : (bestAttemptId || findSavedId());
                if (id) await loadResult(id);
                else errorPage("Please open a completed quiz from the Quizzes page to review its results.");
            } else {
                intro();
            }
        } catch (error) {
            console.error("Quiz initialization failed:", error);
            errorPage(error.message || "The quiz could not be loaded.", true);
        }
    });
})();
