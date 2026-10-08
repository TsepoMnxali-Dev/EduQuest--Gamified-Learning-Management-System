/* =============================================================
   EduQuest - signupp.js  (used by signupp.html)

   Three-step sign-up: 1) account details, 2) province and school,
   3) subjects. Provinces, schools, grades and subjects are loaded
   live from the backend; the final submit creates the account,
   signs the learner in, creates their learner profile, attaches
   their chosen subjects, then sends them to the dashboard.
============================================================= */
(function () {
    "use strict";

    var SUBJECT_ICONS = {
        "Mathematics": "\uD83D\uDCD0",
        "Physical Sciences": "\u2697\uFE0F",
        "Life Sciences": "\uD83E\uDDEC",
        "English": "\uD83D\uDCDA",
        "Geography": "\uD83C\uDF0D",
        "Life Orientation": "\uD83E\uDDED",
        "Accounting": "\uD83E\uDDEE"
    };
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
                populateSelect(
                    byId("province"), provinces,
                    "provinceID", "provinceName",
                    "Select your province"
                );
            })
            .catch(function () {
                showError("Could not load the list of provinces from the server.");
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
            .catch(function () {
                showError("Could not load the list of grades from the server.");
            });
    }

    function subjectCardHtml(subject) {
        var icon = SUBJECT_ICONS[subject.subjectName] || DEFAULT_ICON;
        return (
            '<label class="subject-option">' +
            '<input type="checkbox" name="subjects" value="' + subject.subjectID + '">' +
            '<span class="subject-card">' +
            '<span class="subject-icon">' + icon + '</span>' +
            '<strong>' + subject.subjectName + '</strong>' +
            '<span class="subject-status">Select</span>' +
            '</span></label>'
        );
    }

    function loadSubjects() {
        var container = document.querySelector(".subject-options");

        return EduQuestAPI.get("/subjects", { auth: false })
            .then(function (subjects) {
                if (!subjects || subjects.length === 0) {
                    container.innerHTML = "<p>No subjects are configured yet - contact an admin.</p>";
                    return;
                }

                container.innerHTML = subjects.map(subjectCardHtml).join("");

                container.querySelectorAll('input[name="subjects"]').forEach(function (input) {
                    input.addEventListener("change", updateSubjects);
                });
                updateSubjects();
            })
            .catch(function () {
                showError("Could not load the list of subjects from the server.");
            });
    }

    /* Fill the school list for the chosen province. */
    function loadSchools() {
        var provinceSelect = byId("province");
        var school = byId("school");
        var provinceId = provinceSelect.value;

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
            .catch(function () {
                showError("Could not load the list of schools for that province.");
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
        var lastName = parts.join(" ") || firstName;
        return { firstName: firstName, lastName: lastName };
    }

    /* Creates the account, signs in, creates the learner profile and
       attaches the chosen subjects - in that order, since each step
       needs something the previous step returned. */
    function createAccount() {
        var name = splitName(byId("fullName").value);
        var email = byId("email").value.trim();
        var password = byId("password").value;
        var gradeId = Number(byId("grade").value);
        var schoolId = Number(byId("school").value);
        var subjectIds = Array.prototype.map.call(
            document.querySelectorAll('input[name="subjects"]:checked'),
            function (input) { return Number(input.value); }
        );

        return EduQuestAPI.post("/auth/register", {
            firstName: name.firstName,
            lastName: name.lastName,
            email: email,
            password: password
        }, { auth: false })
            .then(function () {
                return EduQuestAPI.post("/auth/login", { email: email, password: password }, { auth: false });
            })
            .then(function (loginResult) {
                EduQuestAPI.saveToken(loginResult.token);
                var currentUser = EduQuestAPI.getCurrentUser();

                return EduQuestAPI.post("/learners", {
                    userID: currentUser.userId,
                    gradeID: gradeId,
                    schoolID: schoolId
                });
            })
            .then(function (learner) {
                return Promise.all(
                    subjectIds.map(function (subjectId) {
                        return EduQuestAPI.post("/learners/" + learner.learnerID + "/subjects/" + subjectId, null);
                    })
                );
            })
            .then(function () {
                window.location.href = "dashboard.html";
            });
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
