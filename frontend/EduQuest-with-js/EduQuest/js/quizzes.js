/* =============================================================
   EduQuest - quizzes.js  (used by Quizzes.html)

   Marks quizzes the learner has finished as completed, and keeps the
   summary numbers (completed, still to do, average score) and each
   subject's "x of y quizzes completed" line in step with the list.

   BACKEND: fetchResults() is the only data access on this page.
   Swap its body for a fetch() call that returns the learner's results
   keyed by quiz id, e.g. { "calculus": { score: 90, date: "<ISO date>" } }.
   The list itself (titles, question counts, subjects) is still written
   into Quizzes.html; render it from the backend when that is ready.
============================================================= */
(function () {
    "use strict";

    var STORE_KEY = "eduquest.quizResults"; /* browser-only storage used for now */

    /* DATA ACCESS - replace with a backend call. Resolve with {} if none. */
    function fetchResults() {
        try {
            return Promise.resolve(JSON.parse(localStorage.getItem(STORE_KEY)) || {});
        } catch (e) {
            return Promise.resolve({});
        }
    }

    function timeAgo(iso) {
        var days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
        if (days < 1) { return "today"; }
        if (days < 7) { return days + (days === 1 ? " day ago" : " days ago"); }
        var weeks = Math.floor(days / 7);
        return weeks + (weeks === 1 ? " week ago" : " weeks ago");
    }

    /* Turn "Start Quiz" rows into "Review" rows for quizzes with a saved result. */
    function markCompleted(results) {
        document.querySelectorAll(".quiz-item[data-quiz-id]").forEach(function (item) {
            var id = item.getAttribute("data-quiz-id");
            var r = results[id];
            if (!r) { return; }

            var icon = item.querySelector(".quiz-status-icon");
            var info = item.querySelector(".quiz-info p");
            var badge = item.querySelector(".quiz-badge");
            var btn = item.querySelector(".btn");

            icon.className = "quiz-status-icon done";
            icon.textContent = "\u2713";
            info.textContent = info.textContent.split("\u00b7")[0].trim() + " \u00b7 Completed " + timeAgo(r.date);
            badge.className = "quiz-badge completed";
            badge.textContent = r.score + "%";
            btn.className = "btn btn-outline";
            btn.textContent = "Review";
            btn.setAttribute("href", "quiz.html?id=" + id + "&review=1");
        });
    }

    /* Recount everything from what is on the page. */
    function updateSummary() {
        var done = 0, todo = 0, sum = 0, scored = 0;

        document.querySelectorAll(".subject-block").forEach(function (block) {
            var items = block.querySelectorAll(".quiz-item");
            var d = 0;

            items.forEach(function (it) {
                if (it.querySelector(".quiz-status-icon.done")) {
                    d += 1;
                    var m = it.querySelector(".quiz-badge").textContent.match(/(\d+)%/);
                    if (m) {
                        sum += parseInt(m[1], 10);
                        scored += 1;
                    }
                }
            });

            block.querySelector(".subject-block-head p").textContent = d + " of " + items.length + " quizzes completed";
            done += d;
            todo += items.length - d;
        });

        var nums = document.querySelectorAll(".quiz-summary .stat-number");
        nums[0].textContent = done;
        nums[1].textContent = todo;
        nums[2].textContent = scored ? Math.round(sum / scored) + "%" : "\u2013";
    }

    fetchResults().catch(function () { return {}; }).then(function (results) {
        markCompleted(results);
        updateSummary();
    });
})();
