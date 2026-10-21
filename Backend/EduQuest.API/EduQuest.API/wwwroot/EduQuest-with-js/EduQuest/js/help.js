/* =============================================================
   EduQuest - help.js  (used by help.html)

   - Filters the FAQ as the learner types in the search box.
   - Signed-in learners can send a support request
     (POST /api/support) and see their own requests and any
     reply (GET /api/support/mine).
   - Signed-out visitors can still read the FAQ; the contact form
     is replaced by a sign-in prompt.
   Server text is written with textContent, never innerHTML.
============================================================= */
(function () {
    "use strict";

    var signedIn = !!EduQuestAPI.getToken();

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

    /* ---------------------------------------------------------
       Top corner and logout: show nothing
    --------------------------------------------------------- */

    var cornerBox = byId("profileMini");
    if (cornerBox) {
        cornerBox.hidden = true;
        cornerBox.style.display = "none";
    }

    var logoutBox = byId("logoutLink");
    if (logoutBox) {
        logoutBox.hidden = true;
        logoutBox.style.display = "none";
    }

    /* ---------------------------------------------------------
       FAQ search
    --------------------------------------------------------- */

    function filterFaq() {
        var query = byId("helpSearch").value.trim().toLowerCase();
        var items = document.querySelectorAll("#faqList details");
        var visible = 0;

        items.forEach(function (item) {
            var match = query === "" || item.textContent.toLowerCase().indexOf(query) !== -1;
            item.hidden = !match;
            if (match) visible += 1;
            /* Open matching answers while searching, close them again after. */
            item.open = query !== "" && match;
        });

        byId("faqEmpty").style.display = visible === 0 ? "block" : "none";
    }

    byId("helpSearch").addEventListener("input", filterFaq);

    /* ---------------------------------------------------------
       Signed-out visitors
    --------------------------------------------------------- */

    if (!signedIn) {
        byId("signinPrompt").hidden = false;
        return;
    }

    /* ---------------------------------------------------------
       Signed-in learners
    --------------------------------------------------------- */

    byId("supportForm").hidden = false;
    byId("myRequests").hidden = false;

    function showMessage(id, text) {
        byId("formError").style.display = "none";
        byId("formSuccess").style.display = "none";
        if (id) {
            var box = byId(id);
            box.textContent = text;
            box.style.display = "block";
        }
    }

    function updateCount() {
        byId("messageCount").textContent = byId("message").value.length + " / 2000";
    }
    byId("message").addEventListener("input", updateCount);

    function formatDate(iso) {
        return new Date(iso).toLocaleDateString(undefined, {
            day: "numeric", month: "short", year: "numeric"
        });
    }

    function pillClass(status) {
        if (status === "Resolved") return "resolved";
        if (status === "In Progress") return "progress";
        return "open";
    }

    function buildRequest(request) {
        var card = el("article", "hp-request");

        var head = el("div", "hp-request-head");
        head.appendChild(el("p", "hp-request-title", request.subject));
        head.appendChild(el("span", "hp-pill " + pillClass(request.status), request.status));
        card.appendChild(head);

        card.appendChild(el(
            "p", "hp-request-meta",
            request.category + " \u00b7 Sent " + formatDate(request.dateCreated)
        ));
        card.appendChild(el("p", "hp-request-body", request.message));

        if (request.adminReply) {
            var reply = el("div", "hp-reply");
            reply.appendChild(el(
                "strong", null,
                "Reply from EduQuest" + (request.dateReplied ? " \u00b7 " + formatDate(request.dateReplied) : "")
            ));
            reply.appendChild(document.createTextNode(request.adminReply));
            card.appendChild(reply);
        }

        return card;
    }

    function loadMyRequests() {
        var list = byId("myRequestsList");

        return EduQuestAPI.get("/support/mine")
            .then(function (requests) {
                clear(list);

                if (!requests || requests.length === 0) {
                    list.appendChild(el("p", "hp-status", "You have not sent any messages yet."));
                    return;
                }

                requests.forEach(function (request) {
                    list.appendChild(buildRequest(request));
                });
            })
            .catch(function (err) {
                console.error("Support requests failed to load:", err);
                clear(list);
                list.appendChild(el("p", "hp-status", err.message || "Could not load your requests."));
            });
    }

    byId("supportForm").addEventListener("submit", function (event) {
        event.preventDefault();
        showMessage(null);

        var category = byId("category").value;
        var subject = byId("subject").value.trim();
        var message = byId("message").value.trim();

        if (!category) {
            showMessage("formError", "Please choose a category.");
            return;
        }
        if (subject.length < 3) {
            showMessage("formError", "Please enter a short subject (at least 3 characters).");
            return;
        }
        if (message.length < 10) {
            showMessage("formError", "Please describe your question in a little more detail (at least 10 characters).");
            return;
        }

        var button = byId("submitBtn");
        button.disabled = true;
        button.textContent = "Sending...";

        EduQuestAPI.post("/support", {
            category: category,
            subject: subject,
            message: message
        })
            .then(function () {
                byId("supportForm").reset();
                updateCount();
                showMessage("formSuccess", "Thank you! Your message has been sent. We will reply as soon as we can.");
                return loadMyRequests();
            })
            .catch(function (err) {
                showMessage("formError", err.message || "Could not send your message. Please try again.");
            })
            .then(function () {
                button.disabled = false;
                button.textContent = "Send Message";
            });
    });

    loadMyRequests();
})();