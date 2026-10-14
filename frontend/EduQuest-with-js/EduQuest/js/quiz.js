/* =============================================================
   EduQuest - quiz.js  (used by quiz.html)

   Runs one quiz attempt: intro -> questions -> review -> results.
   Open it as quiz.html?id=<quiz-id>   (add &review=1 to see the
   learner's last submitted result instead of starting a new attempt).

   Backend:
     GET  /api/learner/quizzes             quiz title, subject, latest attempt
     POST /api/quizzes/{id}/start          creates the attempt, sends questions
     POST /api/quizattempts/{id}/submit    { answers: [{ quizAttemptQuestionID, quizOptionID }] }
     GET  /api/quizattempts/{id}/result    server-scored result with explanations

   The server does the scoring, so correct answers never reach the
   browser before the quiz is submitted.
   Server text is written with textContent, never innerHTML.
============================================================= */
(function () {
    "use strict";

    if (!EduQuestAPI.requireLearner()) {
        return;
    }

    var LETTERS = ["A", "B", "C", "D", "E", "F", "G", "H"];

    var params = new URLSearchParams(window.location.search);
    var quizId = Number(params.get("id"));
    var wantsReview = params.get("review") === "1";

    var root = document.getElementById("quiz-root");
    var titleEl = document.getElementById("quiz-title");
    var subjectEl = document.getElementById("quiz-subject");
    var timerEl = document.getElementById("quiz-timer");

    var info = null;        /* this quiz from the learner's list */
    var attempt = null;     /* { id, questions: [{ attemptQuestionId, text, options: [{ id, text }] }] } */

    var state = {
        current: 0,
        answers: [],        /* chosen option id per question, or null */
        startedAt: 0,
        timerId: null
    };

    /* =========================================================
       HELPERS
    ========================================================= */

    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null) node.textContent = text;
        return node;
    }

    function clear(node) {
        while (node.firstChild) node.removeChild(node.firstChild);
    }

    /* Lower-cases every key so the page works whether the API sends
       camelCase or PascalCase. */
    function norm(value) {
        if (Array.isArray(value)) return value.map(norm);
        if (value && typeof value === "object") {
            var out = {};
            Object.keys(value).forEach(function (key) {
                out[key.toLowerCase()] = norm(value[key]);
            });
            return out;
        }
        return value;
    }

    /* Writes text, turning x^2 into x with a superscript 2 (built as
       DOM nodes, so no HTML is ever parsed). */
    function appendMath(node, text) {
        var s = String(text === null || text === undefined ? "" : text);
        var re = /\^(\([^)]*\)|-?[A-Za-z0-9]+)/g;
        var last = 0;
        var m;

        while ((m = re.exec(s)) !== null) {
            if (m.index > last) {
                node.appendChild(document.createTextNode(s.slice(last, m.index)));
            }
            var exponent = m[1];
            if (exponent.charAt(0) === "(" && exponent.charAt(exponent.length - 1) === ")") {
                exponent = exponent.slice(1, -1);
            }
            node.appendChild(el("sup", null, exponent));
            last = re.lastIndex;
        }

        if (last < s.length) {
            node.appendChild(document.createTextNode(s.slice(last)));
        }
        return node;
    }

    function formatTime(totalSeconds) {
        var h = Math.floor(totalSeconds / 3600);
        var m = Math.floor((totalSeconds % 3600) / 60);
        var s = totalSeconds % 60;
        var mm = (m < 10 ? "0" : "") + m;
        var ss = (s < 10 ? "0" : "") + s;
        return h > 0 ? h + ":" + mm + ":" + ss : mm + ":" + ss;
    }

    /* "00:12:30", "00:12:30.5" or "1.00:00:00" -> seconds. */
    function parseSeconds(value) {
        var m = /^(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})/.exec(String(value || ""));
        if (!m) return 0;
        return (Number(m[1] || 0) * 86400) + (Number(m[2]) * 3600) + (Number(m[3]) * 60) + Number(m[4]);
    }

    function elapsedSeconds() {
        return Math.round((Date.now() - state.startedAt) / 1000);
    }

    function updateTimer() {
        timerEl.textContent = "Time " + formatTime(elapsedSeconds());
    }

    function startTimer() {
        stopTimer();
        state.startedAt = Date.now();
        timerEl.hidden = false;
        updateTimer();
        state.timerId = window.setInterval(updateTimer, 1000);
    }

    function stopTimer() {
        if (state.timerId !== null) {
            window.clearInterval(state.timerId);
            state.timerId = null;
        }
    }

    function answeredCount() {
        return state.answers.filter(function (a) { return a !== null; }).length;
    }

    /* Replace the page content and move focus to the new heading so
       keyboard and screen-reader users know the view changed. */
    function show() {
        clear(root);
        for (var i = 0; i < arguments.length; i++) {
            root.appendChild(arguments[i]);
        }
        var target = root.querySelector("[data-focus]");
        if (target) {
            target.setAttribute("tabindex", "-1");
            target.focus();
        }
    }

    function button(label, className, handler) {
        var b = el("button", className, label);
        b.type = "button";
        if (handler) b.addEventListener("click", handler);
        return b;
    }

    function link(label, className, href) {
        var a = el("a", className, label);
        a.href = href;
        return a;
    }

    function card(extraClass) {
        return el("section", "card quiz-card" + (extraClass ? " " + extraClass : ""));
    }

    /* =========================================================
       VIEWS
    ========================================================= */

    function renderLoading() {
        var p = el("p", "quiz-note", "Loading quiz\u2026");
        p.setAttribute("role", "status");
        show(p);
    }

    function renderMessage(title, text, withRetry) {
        var c = card();
        var h = el("h2", null, title);
        h.setAttribute("data-focus", "");
        c.appendChild(h);
        c.appendChild(el("p", "quiz-intro-meta", text));

        var actions = el("div", "quiz-actions");
        actions.appendChild(link("Back to quizzes", "btn btn-outline", "Quizzes.html"));
        if (withRetry) {
            actions.appendChild(button("Try again", "btn btn-primary", function () {
                window.location.reload();
            }));
        }
        c.appendChild(actions);
        show(c);
    }

    function renderNotFound() {
        titleEl.textContent = "Quiz not found";
        subjectEl.textContent = "";
        renderMessage(
            "We couldn\u2019t find that quiz",
            "It may not be available for your grade and subjects, or it may no longer be published. Head back to the quiz list and pick one from there.",
            false
        );
    }

    function renderIntro(errorMessage) {
        var c = card();
        var h = el("h2", null, "Ready to start?");
        h.setAttribute("data-focus", "");
        c.appendChild(h);

        var meta = info.questioncount + " questions.";
        if (info.timelimitminutes) {
            meta += " Suggested time: " + info.timelimitminutes + " minutes.";
        }
        meta += " Your time is tracked so you can see how you did.";
        c.appendChild(el("p", "quiz-intro-meta", meta));

        var tips = el("ul", "quiz-tips");
        tips.appendChild(el("li", null, "Choose one answer for each question. You can change it until you submit."));
        tips.appendChild(el("li", null, "Use Previous and Next, or tap a question number, to move around."));
        tips.appendChild(el("li", null, "You need to answer every question before you can submit."));
        tips.appendChild(el("li", null, "After you submit, you\u2019ll see the correct answers with a short explanation for each."));
        c.appendChild(tips);

        if (errorMessage) {
            var warning = el("div", "quiz-warning", errorMessage);
            warning.setAttribute("role", "status");
            c.appendChild(warning);
        }

        var actions = el("div", "quiz-actions");
        actions.appendChild(link("Back to quizzes", "btn btn-outline", "Quizzes.html"));
        var start = button("Start quiz", "btn btn-primary", function () {
            startAttempt(start);
        });
        actions.appendChild(start);
        c.appendChild(actions);

        show(c);
    }

    /* Creates the attempt on the server and shows the first question. */
    function startAttempt(startButton) {
        if (startButton) {
            startButton.disabled = true;
            startButton.textContent = "Starting\u2026";
        }

        EduQuestAPI.post("/quizzes/" + quizId + "/start")
            .then(function (raw) {
                var data = norm(raw);

                attempt = {
                    id: data.quizattemptid,
                    questions: (data.questions || [])
                        .sort(function (a, b) { return a.questionorder - b.questionorder; })
                        .map(function (q) {
                            return {
                                attemptQuestionId: q.quizattemptquestionid,
                                text: q.questiontext,
                                options: (q.options || []).map(function (o) {
                                    return { id: o.quizoptionid, text: o.optiontext };
                                })
                            };
                        })
                };

                if (attempt.questions.length === 0) {
                    renderIntro("This quiz has no questions yet.");
                    return;
                }

                state.current = 0;
                state.answers = attempt.questions.map(function () { return null; });
                startTimer();
                renderQuestion();
            })
            .catch(function (err) {
                console.error("Quiz could not be started:", err);
                renderIntro(err.message || "This quiz could not be started.");
            });
    }

    function renderQuestion() {
        var n = attempt.questions.length;
        var i = state.current;
        var q = attempt.questions[i];
        var isLast = i === n - 1;

        var c = card();

        var head = el("div", "quiz-progress-head");
        head.appendChild(el("span", null, "Question " + (i + 1) + " of " + n));
        var countEl = el("span", null, answeredCount() + " answered");
        head.appendChild(countEl);
        c.appendChild(head);

        var bar = el("div", "progress-bar");
        var fill = el("div", "progress-fill");
        fill.style.width = Math.round(((i + 1) / n) * 100) + "%";
        bar.appendChild(fill);
        c.appendChild(bar);

        var question = el("h2", "quiz-question");
        question.setAttribute("data-focus", "");
        appendMath(question, q.text);
        c.appendChild(question);

        var options = el("div", "quiz-options");
        options.setAttribute("role", "radiogroup");
        options.setAttribute("aria-label", "Answer options");

        var dotsNav = el("nav", "quiz-dots");
        dotsNav.setAttribute("aria-label", "Question navigator");

        q.options.forEach(function (option, k) {
            var selected = state.answers[i] === option.id;

            var label = el("label", "quiz-option" + (selected ? " selected" : ""));

            var input = document.createElement("input");
            input.type = "radio";
            input.name = "answer";
            input.value = String(option.id);
            input.checked = selected;
            label.appendChild(input);

            label.appendChild(el("span", "quiz-option-mark", LETTERS[k] || String(k + 1)));

            var text = el("span", "quiz-option-text");
            appendMath(text, option.text);
            label.appendChild(text);

            input.addEventListener("change", function () {
                state.answers[i] = option.id;

                options.querySelectorAll(".quiz-option").forEach(function (other) {
                    other.classList.toggle("selected", other === label);
                });

                var dot = dotsNav.querySelector('[data-goto="' + i + '"]');
                if (dot) {
                    dot.classList.add("answered");
                    dot.setAttribute("aria-label", "Question " + (i + 1) + ", answered");
                }
                countEl.textContent = answeredCount() + " answered";
            });

            options.appendChild(label);
        });
        c.appendChild(options);

        var actions = el("div", "quiz-actions");
        var prev = button("Previous", "btn btn-outline", function () {
            if (state.current > 0) {
                state.current -= 1;
                renderQuestion();
            }
        });
        if (i === 0) prev.disabled = true;
        actions.appendChild(prev);

        actions.appendChild(button(
            isLast ? "Review answers" : "Next question",
            "btn btn-primary",
            function () {
                if (isLast) {
                    renderReview();
                } else {
                    state.current += 1;
                    renderQuestion();
                }
            }
        ));
        c.appendChild(actions);

        attempt.questions.forEach(function (_, k) {
            var answered = state.answers[k] !== null;
            var dot = el(
                "button",
                "quiz-dot" + (k === i ? " current" : "") + (answered ? " answered" : ""),
                String(k + 1)
            );
            dot.type = "button";
            dot.setAttribute("data-goto", String(k));
            dot.setAttribute("aria-label", "Question " + (k + 1) + (answered ? ", answered" : ", not answered"));
            if (k === i) dot.setAttribute("aria-current", "step");
            dot.addEventListener("click", function () {
                state.current = k;
                renderQuestion();
            });
            dotsNav.appendChild(dot);
        });

        show(c, dotsNav);
    }

    function renderReview(errorMessage) {
        var n = attempt.questions.length;
        var unanswered = n - answeredCount();

        var c = card();
        var h = el("h2", null, "Review your answers");
        h.setAttribute("data-focus", "");
        c.appendChild(h);

        if (errorMessage) {
            var failed = el("div", "quiz-warning", errorMessage);
            failed.setAttribute("role", "status");
            c.appendChild(failed);
        }

        if (unanswered > 0) {
            var notice = el(
                "div", "quiz-warning",
                "You haven\u2019t answered " + unanswered + (unanswered === 1 ? " question" : " questions") +
                ". Answer every question to submit."
            );
            notice.setAttribute("role", "status");
            c.appendChild(notice);
        } else {
            c.appendChild(el("p", "quiz-note", "You\u2019ve answered all " + n + " questions. Submit when you\u2019re ready."));
        }

        var list = el("ul", "quiz-review-list");
        attempt.questions.forEach(function (q, k) {
            var chosen = state.answers[k];
            var answered = chosen !== null;

            var row = el("li", "quiz-review-row");

            var icon = el("span", "quiz-status-icon " + (answered ? "done" : "pending"), answered ? "\u2713" : "\u2022");
            icon.setAttribute("aria-hidden", "true");
            row.appendChild(icon);

            var textBox = el("span", "quiz-review-text", "Question " + (k + 1));
            var letter = "";
            q.options.forEach(function (o, idx) {
                if (o.id === chosen) letter = LETTERS[idx] || String(idx + 1);
            });
            textBox.appendChild(el("small", null, answered ? "Your answer: " + letter : "Not answered"));
            row.appendChild(textBox);

            row.appendChild(button(answered ? "Change" : "Answer", "btn btn-outline", function () {
                state.current = k;
                renderQuestion();
            }));

            list.appendChild(row);
        });
        c.appendChild(list);

        var actions = el("div", "quiz-actions");
        actions.appendChild(button("Back to questions", "btn btn-outline", function () {
            var firstUnanswered = state.answers.indexOf(null);
            state.current = firstUnanswered === -1 ? n - 1 : firstUnanswered;
            renderQuestion();
        }));

        var submit = button("Submit quiz", "btn btn-primary", function () {
            submitAttempt(submit);
        });
        if (unanswered > 0) submit.disabled = true;
        actions.appendChild(submit);
        c.appendChild(actions);

        show(c);
    }

    function submitAttempt(submitButton) {
        submitButton.disabled = true;
        submitButton.textContent = "Submitting\u2026";
        stopTimer();

        var seconds = elapsedSeconds();

        var payload = {
            answers: attempt.questions.map(function (q, k) {
                return {
                    quizAttemptQuestionID: q.attemptQuestionId,
                    quizOptionID: state.answers[k]
                };
            })
        };

        EduQuestAPI.post("/quizattempts/" + attempt.id + "/submit", payload)
            .then(function () {
                return EduQuestAPI.get("/quizattempts/" + attempt.id + "/result");
            })
            .then(function (raw) {
                renderResults(norm(raw), seconds, false);
            })
            .catch(function (err) {
                console.error("Quiz could not be submitted:", err);
                renderReview(err.message || "Your answers could not be submitted. Please try again.");
            });
    }

    function renderResults(result, justTakenSeconds, fromSaved) {
        stopTimer();
        timerEl.hidden = true;

        var percent = Math.round(result.percentage || 0);
        var total = result.totalquestions || 0;

        var message;
        if (percent >= 80) {
            message = "Great work. You\u2019ve got a solid grip on this topic.";
        } else if (percent >= 50) {
            message = "Good effort. Read the explanations below, then try again to improve your score.";
        } else {
            message = "This topic needs more practice. Read the explanations below, revisit the class notes, and try again.";
        }

        if (fromSaved && result.datetaken) {
            message += " Your last attempt was on " +
                new Date(result.datetaken).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" }) + ".";
        }

        var head = card("quiz-result-head");
        var h = el("h2", null, "You scored " + percent + "%");
        h.setAttribute("data-focus", "");
        head.appendChild(h);
        head.appendChild(el("p", null, message));

        var seconds = parseSeconds(result.timetaken);
        if (seconds === 0 && justTakenSeconds) seconds = justTakenSeconds;

        var stats = el("div", "profile-stats");
        stats.style.marginTop = "18px";

        function stat(value, label) {
            var box = el("div", "profile-stat");
            box.appendChild(el("div", "stat-number", value));
            box.appendChild(el("div", "stat-text", label));
            return box;
        }

        stats.appendChild(stat(percent + "%", "Score"));
        stats.appendChild(stat(result.score + " / " + total, "Correct"));
        stats.appendChild(stat(seconds > 0 ? formatTime(seconds) : "\u2013", "Time taken"));

        var answersCard = card();
        answersCard.appendChild(el("h3", "dashboard-section-title", "Your answers"));

        var list = el("ul", "quiz-result-list");
        (result.questions || [])
            .sort(function (a, b) { return a.questionorder - b.questionorder; })
            .forEach(function (q, k) {
                var ok = !!q.iscorrect;

                var item = el("li", "quiz-result-item");

                var icon = el("div", "quiz-status-icon " + (ok ? "done" : "wrong"), ok ? "\u2713" : "\u2715");
                icon.setAttribute("aria-hidden", "true");
                item.appendChild(icon);

                var body = el("div");

                var title = el("h4");
                title.appendChild(el("span", "sr-only", ok ? "Correct. " : "Incorrect. "));
                title.appendChild(document.createTextNode((k + 1) + ". "));
                appendMath(title, q.questiontext);
                body.appendChild(title);

                var yours = el("p", "quiz-result-line", "Your answer: ");
                var yoursStrong = el("strong");
                appendMath(yoursStrong, q.selectedanswer);
                yours.appendChild(yoursStrong);
                body.appendChild(yours);

                if (!ok) {
                    var correct = el("p", "quiz-result-line", "Correct answer: ");
                    var correctStrong = el("strong");
                    appendMath(correctStrong, q.correctanswer);
                    correct.appendChild(correctStrong);
                    body.appendChild(correct);
                }

                if (q.explanation) {
                    var explain = el("p", "quiz-explain");
                    appendMath(explain, q.explanation);
                    body.appendChild(explain);
                }

                item.appendChild(body);
                list.appendChild(item);
            });
        answersCard.appendChild(list);

        var actions = el("div", "quiz-actions");
        actions.appendChild(link("Back to quizzes", "btn btn-outline", "Quizzes.html"));
        var retry = button("Try again", "btn btn-primary", function () {
            startAttempt(retry);
        });
        actions.appendChild(retry);
        answersCard.appendChild(actions);

        show(head, stats, answersCard);
    }

    /* =========================================================
       START
    ========================================================= */

    function findQuiz(data) {
        var found = null;
        (data.subjects || []).forEach(function (subject) {
            (subject.quizzes || []).forEach(function (quiz) {
                if (quiz.quizid === quizId) {
                    found = quiz;
                    found.subjectname = subject.subjectname;
                }
            });
        });
        return found;
    }

    renderLoading();

    if (!quizId) {
        renderNotFound();
        return;
    }

    EduQuestAPI.get("/learner/quizzes")
        .then(function (raw) {
            info = findQuiz(norm(raw));

            if (!info) {
                renderNotFound();
                return;
            }

            document.title = "EduQuest | " + info.title;
            titleEl.textContent = info.title;
            subjectEl.textContent = info.subjectname;

            if (wantsReview && info.latestattemptid) {
                return EduQuestAPI.get("/quizattempts/" + info.latestattemptid + "/result")
                    .then(function (result) {
                        renderResults(norm(result), 0, true);
                    })
                    .catch(function (err) {
                        console.error("Saved result failed to load:", err);
                        renderIntro();
                    });
            }

            renderIntro();
        })
        .catch(function (err) {
            console.error("Quiz failed to load:", err);
            renderMessage(
                "We couldn\u2019t load this quiz",
                err.message || "Check your internet connection and try again.",
                true
            );
        });
})();