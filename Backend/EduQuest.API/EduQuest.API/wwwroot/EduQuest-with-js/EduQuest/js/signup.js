/* =============================================================
   EduQuest - signup.js  (used by signup.html)

   Three-step sign-up: 1) account details, 2) province and school,
   3) subjects. Provinces, schools, grades and subjects are loaded
   live from the backend; the final submit sends everything to ONE
   endpoint (POST /auth/register-learner), which creates the account,
   learner profile and subject enrolments in a single transaction and
   returns a token, then sends the learner to the dashboard.
============================================================= */
(function () {
    "use strict";

    /* Shown only when the backend does not send an icon for a subject. */
    var DEFAULT_ICON = "\uD83D\uDCD8";

    function byId(id) {
        return document.getElementById(id);
    }

    function showError(message) {
        var box = byId("error");
        box.textContent = message;
        box.style.display = "block";
    }

    function clearError() {
        byId("error").style.display = "none";
    }

    function showStep(n) {
        clearError();
        document.querySelectorAll(".signup-step").forEach(function (step) {
            step.classList.remove("active");
        });
        byId("s" + n).classList.add("active");

        for (var i = 1; i <= 3; i++) {
            byId("n" + i).classList.toggle("active", i <= n);
        }
        byId("l1").classList.toggle("active", n >= 2);
        byId("l2").classList.toggle("active", n >= 3);

        window.scrollTo({ top: 0, behavior: "smooth" });
    }

    /* ---------------------------------------------------------
       Reference data: provinces, grades, subjects come from the
       backend. Schools are loaded per-province in loadSchools().
    --------------------------------------------------------- */

    function populateSelect(select, items, valueKey, textKey, placeholder) {
        select.innerHTML = "";

        var placeholderOption = document.createElement("option");
        placeholderOption.value = "";
        placeholderOption.disabled = true;
        placeholderOption.selected = true;
        placeholderOption.textContent = placeholder;
        select.appendChild(placeholderOption);

        items.forEach(function (item) {
            var option = document.createElement("option");
            option.value = item[valueKey];
            option.textContent = item[textKey];
            select.appendChild(option);
        });
    }

    function loadProvinces() {
        return EduQuestAPI.get("/provinces", { auth: false })
            .then(function (provinces) {
                if (!provinces || provinces.length === 0) {
                    populateSelect(byId("province"), [], null, null, "No provinces configured yet - contact an admin");
                    return;
                }
                populateSelect(
                    byId("province"), provinces,
                    "provinceID", "provinceName",
                    "Select your province"
                );
            })
            .catch(function (err) {
                console.error("Provinces failed to load:", err);
                populateSelect(byId("province"), [], null, null, "Could not load provinces - refresh the page");
                showError(err.message || "Could not load the list of provinces from the server.");
            });
    }

    function loadGrades() {
        return EduQuestAPI.get("/grades", { auth: false })
            .then(function (grades) {
                if (!grades || grades.length === 0) {
                    populateSelect(byId("grade"), [], null, null, "No grades configured yet - contact an admin");
                    return;
                }
                populateSelect(
                    byId("grade"), grades,
                    "gradeID", "gradeName",
                    "Select your grade"
                );
            })
            .catch(function (err) {
                console.error("Grades failed to load:", err);
                populateSelect(byId("grade"), [], null, null, "Could not load grades - refresh the page");
                showError(err.message || "Could not load the list of grades from the server.");
            });
    }

    /* Built with createElement/textContent (not innerHTML) so a
       subject name can never inject markup into the page. */
    function buildSubjectCard(subject) {
        var label = document.createElement("label");
        label.className = "subject-option";

        var input = document.createElement("input");
        input.type = "checkbox";
        input.name = "subjects";
        input.value = subject.subjectID;
        input.addEventListener("change", updateSubjects);

        var card = document.createElement("span");
        card.className = "subject-card";

        var icon = document.createElement("span");
        icon.className = "subject-icon";
        icon.textContent = subject.icon || DEFAULT_ICON;

        var name = document.createElement("strong");
        name.textContent = subject.subjectName;

        var status = document.createElement("span");
        status.className = "subject-status";
        status.textContent = "Select";

        card.appendChild(icon);
        card.appendChild(name);
        card.appendChild(status);
        label.appendChild(input);
        label.appendChild(card);
        return label;
    }

    function loadSubjects() {
        var container = document.querySelector(".subject-options");

        return EduQuestAPI.get("/subjects", { auth: false })
            .then(function (subjects) {
                container.innerHTML = "";

                if (!subjects || subjects.length === 0) {
                    var empty = document.createElement("p");
                    empty.textContent = "No subjects are configured yet - contact an admin.";
                    container.appendChild(empty);
                    return;
                }

                subjects.forEach(function (subject) {
                    container.appendChild(buildSubjectCard(subject));
                });
                updateSubjects();
            })
            .catch(function (err) {
                console.error("Subjects failed to load:", err);
                container.innerHTML = "";
                var failed = document.createElement("p");
                failed.textContent = "Could not load subjects - refresh the page.";
                container.appendChild(failed);
                showError(err.message || "Could not load the list of subjects from the server.");
            });
    }

    /* Fill the school list for the chosen province. */
    function loadSchools() {
        var provinceId = byId("province").value;
        var school = byId("school");

        school.innerHTML = "";

        if (!provinceId) {
            school.disabled = true;
            return;
        }

        school.disabled = true;
        var loading = document.createElement("option");
        loading.textContent = "Loading schools...";
        loading.value = "";
        school.appendChild(loading);

        EduQuestAPI.get("/schools?provinceId=" + encodeURIComponent(provinceId), { auth: false })
            .then(function (schools) {
                /* Ignore stale responses if the province changed meanwhile. */
                if (byId("province").value !== provinceId) return;

                if (!schools || schools.length === 0) {
                    populateSelect(school, [], null, null, "No schools configured for this province yet");
                    return;
                }
                populateSelect(
                    school, schools,
                    "schoolID", "schoolName",
                    "Select your school"
                );
                school.disabled = false;
            })
            .catch(function (err) {
                if (byId("province").value !== provinceId) return;
                console.error("Schools failed to load:", err);
                populateSelect(school, [], null, null, "Could not load schools - pick the province again");
                showError(err.message || "Could not load the list of schools for that province.");
            });
    }

    /* Step 1 -> 2: check the account details. */
    function goToStep2() {
        clearError();
        var fields = ["fullName", "email", "grade", "password", "confirmPassword", "terms"];
        for (var i = 0; i < fields.length; i++) {
            var field = byId(fields[i]);
            if (!field.checkValidity()) {
                field.reportValidity();
                return;
            }
        }
        if (byId("fullName").value.trim().split(/\s+/).length < 2) {
            showError("Please enter your first name and surname.");
            return;
        }
        if (byId("password").value !== byId("confirmPassword").value) {
            showError("Passwords do not match.");
            return;
        }
        showStep(2);
    }

    /* Step 2 -> 3: check province and school. */
    function goToStep3() {
        clearError();
        if (!byId("province").value) {
            showError("Please select your province.");
            return;
        }
        if (!byId("school").value) {
            showError("Please select your school.");
            return;
        }
        showStep(3);
    }

    function updateSubjects() {
        var selected = document.querySelectorAll('input[name="subjects"]:checked');
        byId("count").textContent = selected.length + (selected.length === 1 ? " subject selected" : " subjects selected");

        document.querySelectorAll('input[name="subjects"]').forEach(function (input) {
            input.nextElementSibling.querySelector(".subject-status").textContent = input.checked ? "\u2713 Selected" : "Select";
        });
    }

    function splitName(fullName) {
        var parts = fullName.trim().split(/\s+/);
        var firstName = parts.shift();
        var lastName = parts.join(" ");
        return { firstName: firstName, lastName: lastName };
    }

    /* One request creates the account, learner profile and subject
       enrolments atomically, so a failure can never leave a half-made
       account behind. The server returns a token, so no separate
       login call is needed. */
    async function createAccount() {
        var name = splitName(byId("fullName").value);

        var result = await EduQuestAPI.post("/auth/register-learner", {
            firstName: name.firstName,
            lastName: name.lastName,
            email: byId("email").value.trim(),
            password: byId("password").value,
            gradeID: Number(byId("grade").value),
            schoolID: Number(byId("school").value),
            subjectIDs: Array.prototype.map.call(
                document.querySelectorAll('input[name="subjects"]:checked'),
                function (input) { return Number(input.value); }
            )
        }, { auth: false });

        EduQuestAPI.saveToken(result.token);
        window.location.href = "dashboard.html";
    }

    /* Wire up the page. */
    byId("step1-next").addEventListener("click", goToStep2);
    byId("step2-back").addEventListener("click", function () { showStep(1); });
    byId("step2-next").addEventListener("click", goToStep3);
    byId("step3-back").addEventListener("click", function () { showStep(2); });
    byId("province").addEventListener("change", loadSchools);

    byId("signupForm").addEventListener("submit", function (event) {
        event.preventDefault();
        clearError();

        if (document.querySelectorAll('input[name="subjects"]:checked').length === 0) {
            showError("Please select at least one subject.");
            return;
        }

        var submitButton = event.target.querySelector("button[type='submit']");
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.textContent = "Creating account...";
        }

        createAccount().catch(function (err) {
            showError(
                err.message ||
                "Something went wrong creating your account. Please try again."
            );
        }).finally(function () {
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.textContent = "Create Account";
            }
        });
    });

    loadProvinces();
    loadGrades();
    loadSubjects();
})();