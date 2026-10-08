/* =============================================================
   EduQuest - quiz.js  (used by quiz.html)

   Runs one quiz attempt: intro -> questions -> review -> results.
   Open it as quiz.html?id=<quiz-id>   (add &review=1 to see the
   last saved result instead of starting a new attempt).

   BACKEND: everything that talks to data lives in the "DATA ACCESS"
   section below. Each function returns a Promise, so you can swap
   the bodies for fetch() calls without touching the rest of the file.
============================================================= */
(function () {
    "use strict";

    /* =========================================================
       SAMPLE CONTENT
       Used until the backend supplies quizzes. Delete this block
       once fetchQuiz() calls your API.
       "answer" is the index (0-3) of the correct option.
    ========================================================= */
    var SAMPLE_QUIZZES = {
        "calculus": {
            title: "Basic Calculus",
            subject: "Mathematics",
            questions: [
                {
                    q: "What is the derivative of f(x) = x²?",
                    options: ["x", "x²", "2x", "2"],
                    answer: 2,
                    explain: "Bring the power down and reduce it by one: d/dx (xⁿ) = n·xⁿ⁻¹, so d/dx (x²) = 2x."
                },
                {
                    q: "Differentiate f(x) = 5x³.",
                    options: ["15x²", "5x²", "15x³", "3x²"],
                    answer: 0,
                    explain: "Multiply the coefficient by the power (5 × 3 = 15) and reduce the power by one, giving 15x²."
                },
                {
                    q: "What is the derivative of the constant function f(x) = 7?",
                    options: ["7", "1", "7x", "0"],
                    answer: 3,
                    explain: "A constant never changes, so its rate of change is zero."
                },
                {
                    q: "What does the derivative of a function tell you at a given point?",
                    options: ["The area under the curve", "The gradient (rate of change) at that point", "The y-intercept of the graph", "The largest value of the function"],
                    answer: 1,
                    explain: "The derivative gives the gradient of the tangent, which is the instantaneous rate of change."
                },
                {
                    q: "Find the gradient of f(x) = x² + 3x at x = 2.",
                    options: ["4", "3", "10", "7"],
                    answer: 3,
                    explain: "f′(x) = 2x + 3. Substituting x = 2 gives 2(2) + 3 = 7."
                },
                {
                    q: "Differentiate f(x) = 3x² − 4x + 1.",
                    options: ["6x − 4", "6x² − 4", "3x − 4", "6x − 4x"],
                    answer: 0,
                    explain: "Differentiate term by term: 3x² gives 6x, −4x gives −4, and the constant 1 gives 0."
                },
                {
                    q: "At a turning point of a smooth curve, the gradient is:",
                    options: ["Positive", "Negative", "Zero", "Undefined"],
                    answer: 2,
                    explain: "At a maximum or minimum the tangent is horizontal, so f′(x) = 0."
                },
                {
                    q: "Find the x-coordinate of the turning point of f(x) = x² − 6x + 5.",
                    options: ["x = 1", "x = 3", "x = 5", "x = 6"],
                    answer: 1,
                    explain: "f′(x) = 2x − 6. Setting 2x − 6 = 0 gives x = 3."
                },
                {
                    q: "A car's distance is s(t) = t² + 4t metres after t seconds. What is its speed at t = 3 seconds?",
                    options: ["9 m/s", "21 m/s", "13 m/s", "10 m/s"],
                    answer: 3,
                    explain: "Speed is the derivative of distance: s′(t) = 2t + 4. At t = 3 that is 2(3) + 4 = 10 m/s."
                },
                {
                    q: "If f′(x) > 0 on an interval, what is the function doing on that interval?",
                    options: ["Increasing", "Decreasing", "Constant", "Has a turning point"],
                    answer: 0,
                    explain: "A positive gradient means the graph is going up as x increases."
                }
            ]
        },
        "money-math": {
            title: "Money Math: Interest & Loans",
            subject: "Mathematics",
            questions: [
                {
                    q: "What is the simple interest on R2 000 invested at 8% per year for 3 years?",
                    options: ["R160", "R480", "R520", "R2 480"],
                    answer: 1,
                    explain: "Simple interest = P × i × n = 2 000 × 0.08 × 3 = R480. (R2 480 would be the total amount, not the interest.)"
                },
                {
                    q: "Which formula gives the final amount with compound interest?",
                    options: ["A = P(1 + in)", "A = P + in", "A = P(1 − i)ⁿ", "A = P(1 + i)ⁿ"],
                    answer: 3,
                    explain: "With compound interest the amount grows by a factor of (1 + i) each period: A = P(1 + i)ⁿ."
                },
                {
                    q: "What is the main difference between simple and compound interest?",
                    options: ["Compound interest is also earned on previously earned interest", "Simple interest is only used for loans", "Compound interest uses a lower rate", "Simple interest is calculated monthly"],
                    answer: 0,
                    explain: "Compound interest earns interest on the interest already added, which is why it grows faster over time."
                },
                {
                    q: "R5 000 is invested at 10% per year, compounded annually, for 2 years. What is the final amount?",
                    options: ["R6 000", "R6 100", "R6 050", "R5 500"],
                    answer: 2,
                    explain: "A = 5 000(1.10)² = 5 000 × 1.21 = R6 050."
                },
                {
                    q: "R1 500 is invested at 6% simple interest per year for 4 years. What is the total amount at the end?",
                    options: ["R360", "R1 590", "R1 896", "R1 860"],
                    answer: 3,
                    explain: "Interest = 1 500 × 0.06 × 4 = R360, so the total amount is 1 500 + 360 = R1 860."
                },
                {
                    q: "A bank offers 12% per year compounded monthly. What interest rate is applied each month?",
                    options: ["12%", "1%", "6%", "0.1%"],
                    answer: 1,
                    explain: "Divide the yearly rate by the 12 months: 12% ÷ 12 = 1% per month."
                },
                {
                    q: "R10 000 is borrowed for 1 year at 12% simple interest and repaid in one payment. How much is repaid?",
                    options: ["R11 200", "R10 012", "R1 200", "R12 000"],
                    answer: 0,
                    explain: "Interest = 10 000 × 0.12 × 1 = R1 200, so you repay 10 000 + 1 200 = R11 200."
                },
                {
                    q: "In an interest calculation, what is the “principal”?",
                    options: ["The interest earned so far", "The final amount at the end", "The original amount invested or borrowed", "The number of years"],
                    answer: 2,
                    explain: "The principal (P) is the starting amount that interest is calculated on."
                },
                {
                    q: "Which change would reduce the total interest paid on a loan, if the interest rate stays the same?",
                    options: ["Repaying it over a longer period", "Repaying it over a shorter period", "Borrowing the money for longer", "Making no payments for the first year"],
                    answer: 1,
                    explain: "Interest builds up over time, so paying the loan off sooner means less time for interest to accumulate."
                },
                {
                    q: "R800 earns R40 simple interest in one year. What is the yearly interest rate?",
                    options: ["4%", "40%", "8%", "5%"],
                    answer: 3,
                    explain: "i = interest ÷ (P × n) = 40 ÷ 800 = 0.05, which is 5%."
                }
            ]
        },
        "light-sound": {
            title: "Light & Sound",
            subject: "Physical Sciences",
            questions: [
                {
                    q: "Which of these can travel through a vacuum?",
                    options: ["Sound", "Light", "Both light and sound", "Neither"],
                    answer: 1,
                    explain: "Light is an electromagnetic wave and needs no medium. Sound needs particles to vibrate, so it cannot travel through a vacuum."
                },
                {
                    q: "According to the law of reflection:",
                    options: ["The angle of incidence is greater than the angle of reflection", "Light is bent towards the normal", "The angle of reflection is always 90°", "The angle of incidence equals the angle of reflection"],
                    answer: 3,
                    explain: "When a wave reflects off a surface, the angle of incidence equals the angle of reflection, both measured from the normal."
                },
                {
                    q: "What is refraction?",
                    options: ["A wave changing direction as it passes into a different medium", "A wave being absorbed by a material", "A wave bouncing off a surface", "A wave splitting into two identical waves"],
                    answer: 0,
                    explain: "The wave's speed changes when it enters a new medium, which makes it bend."
                },
                {
                    q: "What is the approximate speed of light in a vacuum?",
                    options: ["3 × 10⁶ m/s", "340 m/s", "3 × 10⁸ m/s", "3 × 10¹⁰ m/s"],
                    answer: 2,
                    explain: "Light travels at about 3 × 10⁸ m/s in a vacuum. Sound in air is only about 340 m/s."
                },
                {
                    q: "Sound travels as which type of wave?",
                    options: ["Transverse", "Stationary only", "Electromagnetic", "Longitudinal"],
                    answer: 3,
                    explain: "The particles of the medium vibrate parallel to the direction the wave travels, so sound is a longitudinal wave."
                },
                {
                    q: "If the frequency of a sound wave increases, its pitch:",
                    options: ["Gets lower", "Gets higher", "Stays the same", "Becomes louder"],
                    answer: 1,
                    explain: "Pitch depends on frequency: higher frequency means a higher pitch."
                },
                {
                    q: "Increasing the amplitude of a sound wave makes the sound:",
                    options: ["Higher in pitch", "Lower in pitch", "Louder", "Faster"],
                    answer: 2,
                    explain: "Loudness depends on amplitude. A bigger amplitude carries more energy and sounds louder."
                },
                {
                    q: "A wave has a frequency of 50 Hz and a wavelength of 2 m. What is its speed?",
                    options: ["100 m/s", "52 m/s", "25 m/s", "0.04 m/s"],
                    answer: 0,
                    explain: "Wave speed v = f × λ = 50 × 2 = 100 m/s."
                }
            ]
        },
        "chem-reactions": {
            title: "Chemistry: Reactions",
            subject: "Physical Sciences",
            questions: [
                {
                    q: "Which is the correctly balanced equation for the formation of water?",
                    options: ["H₂ + O₂ → H₂O", "2H₂ + 2O₂ → 2H₂O", "H₂ + 2O₂ → 2H₂O", "2H₂ + O₂ → 2H₂O"],
                    answer: 3,
                    explain: "2H₂ + O₂ → 2H₂O has 4 hydrogen atoms and 2 oxygen atoms on each side."
                },
                {
                    q: "What are the products of the complete combustion of methane (CH₄) in oxygen?",
                    options: ["Carbon monoxide and hydrogen", "Carbon dioxide and water", "Carbon and water", "Carbon dioxide and hydrogen"],
                    answer: 1,
                    explain: "Complete combustion of a hydrocarbon gives carbon dioxide and water: CH₄ + 2O₂ → CO₂ + 2H₂O."
                },
                {
                    q: "2Na + Cl₂ → 2NaCl is an example of which type of reaction?",
                    options: ["Synthesis (combination)", "Decomposition", "Single displacement", "Neutralisation"],
                    answer: 0,
                    explain: "Two elements join to make one compound, which is a synthesis reaction."
                },
                {
                    q: "2H₂O → 2H₂ + O₂ is an example of which type of reaction?",
                    options: ["Synthesis", "Double displacement", "Decomposition", "Combustion"],
                    answer: 2,
                    explain: "One compound breaks down into simpler substances, which is decomposition."
                },
                {
                    q: "Why must chemical equations be balanced?",
                    options: ["So that all reactions release energy", "So that the same number of atoms of each element appear on both sides", "So that reactants and products have the same state", "So that the reaction goes faster"],
                    answer: 1,
                    explain: "Atoms are not created or destroyed in a reaction (conservation of mass), so each element must balance on both sides."
                },
                {
                    q: "Zn + CuSO₄ → ZnSO₄ + Cu is an example of which type of reaction?",
                    options: ["Synthesis", "Decomposition", "Neutralisation", "Single displacement"],
                    answer: 3,
                    explain: "Zinc is more reactive than copper, so it takes the place of copper in the compound."
                },
                {
                    q: "An acid reacts with a base. What are the products?",
                    options: ["A salt and water", "A metal and hydrogen", "Two acids", "Carbon dioxide and water"],
                    answer: 0,
                    explain: "Neutralisation follows the pattern acid + base → salt + water."
                },
                {
                    q: "What coefficient is needed in front of H₂ to balance N₂ + __H₂ → 2NH₃?",
                    options: ["1", "2", "3", "6"],
                    answer: 2,
                    explain: "2NH₃ contains 6 hydrogen atoms, so 3H₂ is needed on the left side."
                },
                {
                    q: "AgNO₃ + NaCl → AgCl + NaNO₃ forms a white solid, AgCl. What type of reaction is this?",
                    options: ["Synthesis", "Decomposition", "Single displacement", "Double displacement (precipitation)"],
                    answer: 3,
                    explain: "The two compounds swap partners, and one of the new compounds is an insoluble solid (a precipitate)."
                },
                {
                    q: "A reaction that releases heat to its surroundings is called:",
                    options: ["Endothermic", "Exothermic", "Reversible", "Neutral"],
                    answer: 1,
                    explain: "Exothermic reactions give out energy, so the surroundings get warmer. Combustion is a common example."
                }
            ]
        },
        "profitability": {
            title: "Profitability Analysis",
            subject: "Accounting",
            questions: [
                {
                    q: "How is gross profit calculated?",
                    options: ["Sales − Operating expenses", "Cost of sales − Operating expenses", "Sales − Cost of sales", "Sales + Cost of sales"],
                    answer: 2,
                    explain: "Gross profit is what is left from sales after deducting the direct cost of the goods sold."
                },
                {
                    q: "Which formula gives the gross profit margin?",
                    options: ["Gross profit ÷ Sales × 100", "Gross profit ÷ Cost of sales × 100", "Net profit ÷ Sales × 100", "Sales ÷ Gross profit × 100"],
                    answer: 0,
                    explain: "Gross profit margin compares gross profit with sales: (Gross profit ÷ Sales) × 100."
                },
                {
                    q: "A business has sales of R200 000 and cost of sales of R120 000. What is its gross profit margin?",
                    options: ["30%", "66.7%", "60%", "40%"],
                    answer: 3,
                    explain: "Gross profit = 200 000 − 120 000 = R80 000. Margin = 80 000 ÷ 200 000 × 100 = 40%."
                },
                {
                    q: "Using the same business (sales R200 000, cost of sales R120 000), what is the mark-up percentage?",
                    options: ["40%", "66.7%", "60%", "150%"],
                    answer: 1,
                    explain: "Mark-up is calculated on cost of sales: 80 000 ÷ 120 000 × 100 = 66.7%."
                },
                {
                    q: "Gross profit is R80 000, operating expenses are R50 000 and there is no other income. What is the net profit?",
                    options: ["R30 000", "R80 000", "R50 000", "R130 000"],
                    answer: 0,
                    explain: "Net profit = Gross profit − Operating expenses = 80 000 − 50 000 = R30 000."
                },
                {
                    q: "Net profit is R30 000 on sales of R200 000. What is the net profit margin?",
                    options: ["6.7%", "30%", "15%", "40%"],
                    answer: 2,
                    explain: "Net profit margin = 30 000 ÷ 200 000 × 100 = 15%."
                },
                {
                    q: "A business's net profit margin fell from 12% to 8% while its sales stayed the same. What is the most likely reason?",
                    options: ["Its cost of sales decreased", "Its expenses increased", "Its prices increased", "Its expenses decreased"],
                    answer: 1,
                    explain: "With the same sales, a lower net margin means more of each rand is going out as costs and expenses."
                },
                {
                    q: "A business has a gross profit margin of 40% but a net profit margin of only 5%. What does this suggest?",
                    options: ["Its cost of sales is very high", "It sells its goods below cost", "It has no expenses", "Its operating expenses are very high"],
                    answer: 3,
                    explain: "A big gap between the two margins means operating expenses are using up most of the gross profit."
                }
            ]
        }
    };

    /* =========================================================
       DATA ACCESS  (replace these with backend calls)
    ========================================================= */
    var STORE_KEY = "eduquest.quizResults"; /* browser-only storage used for now */

    function readStore() {
        try {
            return JSON.parse(localStorage.getItem(STORE_KEY)) || {};
        } catch (e) {
            return {};
        }
    }

    /* Load one quiz: { title, subject, questions: [{ q, options, answer, explain }] }.
       Resolve with null if the quiz does not exist. */
    function fetchQuiz(id) {
        var known = Object.prototype.hasOwnProperty.call(SAMPLE_QUIZZES, id);
        return Promise.resolve(known ? SAMPLE_QUIZZES[id] : null);
    }

    /* Load the learner's latest saved result for a quiz, or null if there is none.
       Shape: { score, correct, total, seconds, date, answers: [index|null, ...] } */
    function fetchSavedResult(id) {
        return Promise.resolve(readStore()[id] || null);
    }

    /* Save a finished attempt. Reject if saving fails.
       If the server does the scoring, send only { answers, seconds } and
       use the response to build the results screen. */
    function submitResult(id, result) {
        try {
            var all = readStore();
            all[id] = result;
            localStorage.setItem(STORE_KEY, JSON.stringify(all));
            return Promise.resolve(result);
        } catch (e) {
            return Promise.reject(e);
        }
    }

    /* =========================================================
       PAGE SETUP
    ========================================================= */
    var LETTERS = ["A", "B", "C", "D"];

    var params = new URLSearchParams(window.location.search);
    var quizId = params.get("id");
    var wantsReview = params.get("review") === "1";
    var quiz = null;

    var root = document.getElementById("quiz-root");
    var titleEl = document.getElementById("quiz-title");
    var subjectEl = document.getElementById("quiz-subject");
    var timerEl = document.getElementById("quiz-timer");

    var state = {
        current: 0,
        answers: [],
        startedAt: 0,
        timerId: null
    };

    function esc(text) {
        return String(text).replace(/[&<>"']/g, function (c) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" }[c];
        });
    }

    function formatTime(totalSeconds) {
        var h = Math.floor(totalSeconds / 3600);
        var m = Math.floor((totalSeconds % 3600) / 60);
        var s = totalSeconds % 60;
        var mm = (m < 10 ? "0" : "") + m;
        var ss = (s < 10 ? "0" : "") + s;
        return h > 0 ? h + ":" + mm + ":" + ss : mm + ":" + ss;
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
    function show(html) {
        root.innerHTML = html;
        var target = root.querySelector("[data-focus]");
        if (target) {
            target.setAttribute("tabindex", "-1");
            target.focus();
        }
    }

    function on(selector, handler) {
        var el = root.querySelector(selector);
        if (el) { el.addEventListener("click", handler); }
    }

    function onAll(selector, handler) {
        root.querySelectorAll(selector).forEach(function (el) {
            el.addEventListener("click", function () { handler(el); });
        });
    }

    /* =========================================================
       VIEWS
    ========================================================= */
    function renderLoading() {
        show('<p class="quiz-note" role="status">Loading quiz\u2026</p>');
    }

    function renderNotFound() {
        titleEl.textContent = "Quiz not found";
        subjectEl.textContent = "";
        show(
            '<section class="card quiz-card">' +
            '<h2 data-focus>We couldn\u2019t find that quiz</h2>' +
            '<p class="quiz-intro-meta">The link may be out of date. Head back to the quiz list and pick one from there.</p>' +
            '<div class="quiz-actions"><a href="Quizzes.html" class="btn btn-primary">Back to quizzes</a></div>' +
            '</section>'
        );
    }

    function renderLoadError() {
        show(
            '<section class="card quiz-card">' +
            '<h2 data-focus>We couldn\u2019t load this quiz</h2>' +
            '<p class="quiz-intro-meta">Check your internet connection and try again.</p>' +
            '<div class="quiz-actions">' +
            '<a href="Quizzes.html" class="btn btn-outline">Back to quizzes</a>' +
            '<button type="button" class="btn btn-primary" id="reload-btn">Try again</button>' +
            '</div>' +
            '</section>'
        );
        on("#reload-btn", function () { window.location.reload(); });
    }

    function renderIntro() {
        var n = quiz.questions.length;
        show(
            '<section class="card quiz-card">' +
            '<h2 data-focus>Ready to start?</h2>' +
            '<p class="quiz-intro-meta">' + n + ' questions. There\u2019s no time limit, but your time is tracked so you can see how you did.</p>' +
            '<ul class="quiz-tips">' +
            '<li>Choose one answer for each question. You can change it until you submit.</li>' +
            '<li>Use Previous and Next, or tap a question number, to move around.</li>' +
            '<li>After you submit, you\u2019ll see the correct answers with a short explanation for each.</li>' +
            '</ul>' +
            '<div class="quiz-actions">' +
            '<a href="Quizzes.html" class="btn btn-outline">Back to quizzes</a>' +
            '<button type="button" class="btn btn-primary" id="start-btn">Start quiz</button>' +
            '</div>' +
            '</section>'
        );
        on("#start-btn", startAttempt);
    }

    function startAttempt() {
        state.current = 0;
        state.answers = quiz.questions.map(function () { return null; });
        startTimer();
        renderQuestion();
    }

    function renderQuestion() {
        var n = quiz.questions.length;
        var i = state.current;
        var q = quiz.questions[i];
        var isLast = i === n - 1;

        var options = q.options.map(function (text, k) {
            var selected = state.answers[i] === k;
            return (
                '<label class="quiz-option' + (selected ? " selected" : "") + '">' +
                '<input type="radio" name="answer" value="' + k + '"' + (selected ? " checked" : "") + '>' +
                '<span class="quiz-option-mark">' + LETTERS[k] + '</span>' +
                '<span class="quiz-option-text">' + esc(text) + '</span>' +
                '</label>'
            );
        }).join("");

        var dots = quiz.questions.map(function (_, k) {
            var answered = state.answers[k] !== null;
            return (
                '<button type="button" class="quiz-dot' + (k === i ? " current" : "") + (answered ? " answered" : "") + '"' +
                ' data-goto="' + k + '"' +
                ' aria-label="Question ' + (k + 1) + (answered ? ", answered" : ", not answered") + '"' +
                (k === i ? ' aria-current="step"' : "") + '>' + (k + 1) + '</button>'
            );
        }).join("");

        show(
            '<section class="card quiz-card">' +
            '<div class="quiz-progress-head"><span>Question ' + (i + 1) + ' of ' + n + '</span>' +
            '<span id="answered-count">' + answeredCount() + ' answered</span></div>' +
            '<div class="progress-bar"><div class="progress-fill" style="width:' + Math.round(((i + 1) / n) * 100) + '%"></div></div>' +
            '<h2 class="quiz-question" data-focus>' + esc(q.q) + '</h2>' +
            '<div class="quiz-options" role="radiogroup" aria-label="Answer options">' + options + '</div>' +
            '<div class="quiz-actions">' +
            '<button type="button" class="btn btn-outline" id="prev-btn"' + (i === 0 ? " disabled" : "") + '>Previous</button>' +
            '<button type="button" class="btn btn-primary" id="next-btn">' + (isLast ? "Review answers" : "Next question") + '</button>' +
            '</div>' +
            '</section>' +
            '<nav class="quiz-dots" aria-label="Question navigator">' + dots + '</nav>'
        );

        root.querySelectorAll('input[name="answer"]').forEach(function (input) {
            input.addEventListener("change", function () {
                state.answers[i] = parseInt(input.value, 10);
                root.querySelectorAll(".quiz-option").forEach(function (label) {
                    label.classList.toggle("selected", label.contains(input));
                });
                var dot = root.querySelector('.quiz-dot[data-goto="' + i + '"]');
                dot.classList.add("answered");
                dot.setAttribute("aria-label", "Question " + (i + 1) + ", answered");
                root.querySelector("#answered-count").textContent = answeredCount() + " answered";
            });
        });

        onAll("[data-goto]", function (el) {
            state.current = parseInt(el.getAttribute("data-goto"), 10);
            renderQuestion();
        });

        on("#prev-btn", function () {
            if (state.current > 0) {
                state.current -= 1;
                renderQuestion();
            }
        });

        on("#next-btn", function () {
            if (isLast) {
                renderReview();
            } else {
                state.current += 1;
                renderQuestion();
            }
        });
    }

    function renderReview() {
        var n = quiz.questions.length;
        var unanswered = n - answeredCount();

        var rows = quiz.questions.map(function (_, k) {
            var a = state.answers[k];
            var answered = a !== null;
            return (
                '<li class="quiz-review-row">' +
                '<span class="quiz-status-icon ' + (answered ? "done" : "pending") + '" aria-hidden="true">' + (answered ? "\u2713" : "\u2022") + '</span>' +
                '<span class="quiz-review-text">Question ' + (k + 1) +
                '<small>' + (answered ? "Your answer: " + LETTERS[a] : "Not answered") + '</small></span>' +
                '<button type="button" class="btn btn-outline" data-goto="' + k + '">' + (answered ? "Change" : "Answer") + '</button>' +
                '</li>'
            );
        }).join("");

        var notice = unanswered > 0
            ? '<div class="quiz-warning" role="status">You haven\u2019t answered ' + unanswered + (unanswered === 1 ? " question" : " questions") + '. Unanswered questions count as incorrect.</div>'
            : '<p class="quiz-note">You\u2019ve answered all ' + n + ' questions. Submit when you\u2019re ready.</p>';

        show(
            '<section class="card quiz-card">' +
            '<h2 data-focus>Review your answers</h2>' +
            notice +
            '<ul class="quiz-review-list">' + rows + '</ul>' +
            '<div class="quiz-actions">' +
            '<button type="button" class="btn btn-outline" id="back-btn">Back to questions</button>' +
            '<button type="button" class="btn btn-primary" id="submit-btn">Submit quiz</button>' +
            '</div>' +
            '</section>'
        );

        onAll("[data-goto]", function (el) {
            state.current = parseInt(el.getAttribute("data-goto"), 10);
            renderQuestion();
        });

        on("#back-btn", function () {
            var firstUnanswered = state.answers.indexOf(null);
            state.current = firstUnanswered === -1 ? n - 1 : firstUnanswered;
            renderQuestion();
        });

        on("#submit-btn", submitAttempt);
    }

    function submitAttempt() {
        var submitBtn = root.querySelector("#submit-btn");
        if (submitBtn) { submitBtn.disabled = true; }

        stopTimer();
        var total = quiz.questions.length;
        var correct = 0;
        quiz.questions.forEach(function (q, k) {
            if (state.answers[k] === q.answer) { correct += 1; }
        });

        var result = {
            score: Math.round((correct / total) * 100),
            correct: correct,
            total: total,
            seconds: elapsedSeconds(),
            date: new Date().toISOString(),
            answers: state.answers.slice()
        };

        submitResult(quizId, result).then(function () {
            renderResults(result, false, false);
        }).catch(function () {
            /* Still show the learner their score if saving failed. */
            renderResults(result, false, true);
        });
    }

    function renderResults(result, fromSaved, saveFailed) {
        timerEl.hidden = true;

        var message;
        if (result.score >= 80) {
            message = "Great work. You\u2019ve got a solid grip on this topic.";
        } else if (result.score >= 50) {
            message = "Good effort. Read the explanations below, then try again to improve your score.";
        } else {
            message = "This topic needs more practice. Read the explanations below, revisit the class notes, and try again.";
        }

        var dateNote = "";
        if (fromSaved && result.date) {
            dateNote = " Your last attempt was on " + new Date(result.date).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" }) + ".";
        }

        var saveWarning = saveFailed
            ? '<div class="quiz-warning" role="status">We couldn\u2019t save this result, so it won\u2019t show on your quiz list. Check your connection and try the quiz again.</div>'
            : "";

        var items = quiz.questions.map(function (q, k) {
            var a = result.answers[k];
            var ok = a === q.answer;
            var yourLine = a === null || a === undefined
                ? "You skipped this question."
                : "Your answer: <strong>" + LETTERS[a] + ". " + esc(q.options[a]) + "</strong>";
            var correctLine = ok
                ? ""
                : '<p class="quiz-result-line">Correct answer: <strong>' + LETTERS[q.answer] + ". " + esc(q.options[q.answer]) + "</strong></p>";

            return (
                '<li class="quiz-result-item">' +
                '<div class="quiz-status-icon ' + (ok ? "done" : "wrong") + '" aria-hidden="true">' + (ok ? "\u2713" : "\u2715") + '</div>' +
                '<div>' +
                '<h4><span class="sr-only">' + (ok ? "Correct. " : "Incorrect. ") + '</span>' + (k + 1) + ". " + esc(q.q) + '</h4>' +
                '<p class="quiz-result-line">' + yourLine + '</p>' +
                correctLine +
                '<p class="quiz-explain">' + esc(q.explain) + '</p>' +
                '</div>' +
                '</li>'
            );
        }).join("");

        show(
            '<section class="card quiz-card quiz-result-head">' +
            '<h2 data-focus>You scored ' + result.score + '%</h2>' +
            '<p>' + message + dateNote + '</p>' +
            saveWarning +
            '</section>' +
            '<div class="profile-stats" style="margin-top:18px">' +
            '<div class="profile-stat"><div class="stat-number">' + result.score + '%</div><div class="stat-text">Score</div></div>' +
            '<div class="profile-stat"><div class="stat-number">' + result.correct + ' / ' + result.total + '</div><div class="stat-text">Correct</div></div>' +
            '<div class="profile-stat"><div class="stat-number">' + formatTime(result.seconds) + '</div><div class="stat-text">Time taken</div></div>' +
            '</div>' +
            '<section class="card quiz-card">' +
            '<h3 class="dashboard-section-title">Your answers</h3>' +
            '<ul class="quiz-result-list">' + items + '</ul>' +
            '<div class="quiz-actions">' +
            '<a href="Quizzes.html" class="btn btn-outline">Back to quizzes</a>' +
            '<button type="button" class="btn btn-primary" id="retry-btn">Try again</button>' +
            '</div>' +
            '</section>'
        );

        on("#retry-btn", startAttempt);
    }

    /* =========================================================
       START
    ========================================================= */
    renderLoading();

    fetchQuiz(quizId).then(function (loaded) {
        if (!loaded) {
            renderNotFound();
            return null;
        }

        quiz = loaded;
        document.title = "EduQuest | " + quiz.title;
        titleEl.textContent = quiz.title;
        subjectEl.textContent = quiz.subject;

        /* A missing saved result should never stop the quiz from opening. */
        return fetchSavedResult(quizId).catch(function () { return null; }).then(function (saved) {
            if (wantsReview && saved && Array.isArray(saved.answers)) {
                renderResults(saved, true, false);
            } else {
                renderIntro();
            }
        });
    }).catch(function () {
        renderLoadError();
    });
})();
