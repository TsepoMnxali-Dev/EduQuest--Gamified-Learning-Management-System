
/* EduQuest first-login academic onboarding: live APIs, resumable subject assignment. */

(function () {
    "use strict";

    const api = window.EduQuestAPI;

    if (!api || !api.requireLearner()) return;

    const $ = id => document.getElementById(id);

    const user = api.getCurrentUser();
    const userId = Number(user && user.userId);

    if (!Number.isInteger(userId) || userId <= 0) return;

    $("setupName").textContent = user.firstName || "Learner";

    $("setupAvatar").textContent = (user.firstName || "L")
        .charAt(0)
        .toUpperCase();

    $("setupLogout").addEventListener("click", () => api.logout());

    let profile = null,
        registered = new Set(),
        gradeSubjects = [],
        ready = false;

    const val = (obj, camel, pascal) => obj[camel] ?? obj[pascal];

    function message(text, kind = "error") {
        const el = $("setupMessage");

        el.textContent = text;
        el.className = "setup-message " + kind;
    }

    function options(select, data, idKey, nameKey, placeholder) {
        select.replaceChildren(new Option(placeholder, ""));

        data.forEach(item =>
            select.add(
                new Option(
                    String(
                        val(
                            item,
                            nameKey,
                            nameKey[0].toUpperCase() + nameKey.slice(1)
                        )
                    ),
                    String(
                        val(
                            item,
                            idKey,
                            idKey[0].toUpperCase() + idKey.slice(1)
                        )
                    )
                )
            )
        );
    }

    async function loadSchools(selected) {
        const provinceId = Number($("setupProvince").value);

        $("setupSchool").disabled = true;

        options(
            $("setupSchool"),
            [],
            "schoolID",
            "schoolName",
            provinceId ? "Loading schools..." : "Select a province first"
        );

        if (!provinceId) return;

        try {
            const schools = await api.get("/Schools?provinceId=" + provinceId);

            options(
                $("setupSchool"),
                schools,
                "schoolID",
                "schoolName",
                "Select your school"
            );

            if (selected) {
                $("setupSchool").value = String(selected);
            }

            $("setupSchool").disabled = Boolean(profile);

        } catch (error) {
            message("Unable to load schools: " + error.message);
        }
    }

    async function loadSubjects() {
        const gradeId = Number($("setupGrade").value);

        gradeSubjects = [];

        $("setupSubjects").textContent = gradeId
            ? "Loading subjects..."
            : "Select your grade to see subjects.";

        if (!gradeId) return;

        try {
            gradeSubjects = await api.get("/grades/" + gradeId + "/subjects");

            const container = $("setupSubjects");
            container.replaceChildren();

            if (!gradeSubjects.length) {
                container.textContent =
                    "No subjects have been assigned to this grade. Please contact your administrator.";
                return;
            }

            gradeSubjects.forEach(item => {
                const id = Number(val(item, "subjectID", "SubjectID"));

                const label = document.createElement("label");
                label.className = "subject-choice";

                const input = document.createElement("input");
                input.type = "checkbox";
                input.value = String(id);
                input.name = "subjects";
                input.checked = registered.has(id);
                input.disabled = registered.has(id);

                const span = document.createElement("span");
                span.textContent = String(
                    val(item, "subjectName", "SubjectName")
                );

                label.append(input, span);
                container.append(label);
            });

        } catch (error) {
            message("Unable to load subjects: " + error.message);
        }
    }

    async function initialize() {
        $("setupSubmit").disabled = true;

        try {
            const state = await EduQuestProfileGate.getProfileState();

            profile = state.profile;

            registered = new Set(
                state.subjects.map(s =>
                    Number(val(s, "subjectID", "SubjectID"))
                )
            );

            const [provinces, grades] = await Promise.all([
                api.get("/Provinces"),
                api.get("/Grades")
            ]);

            options(
                $("setupProvince"),
                provinces,
                "provinceID",
                "provinceName",
                "Select your province"
            );

            options(
                $("setupGrade"),
                grades,
                "gradeID",
                "gradeName",
                "Select your grade"
            );

            if (profile) {
                $("setupProvince").value = String(
                    val(profile, "provinceID", "ProvinceID")
                );

                $("setupGrade").value = String(
                    val(profile, "gradeID", "GradeID")
                );

                await loadSchools(
                    val(profile, "schoolID", "SchoolID")
                );

                $("setupProvince").disabled = true;
                $("setupSchool").disabled = true;
                $("setupGrade").disabled = true;

                $("profileHint").textContent =
                    "Your school and grade are already saved. Select any remaining subjects to complete setup.";
            }

            await loadSubjects();

            ready = true;
            $("setupSubmit").disabled = false;

            if (profile && registered.size) {
                $("setupSubmit").textContent = "Continue to Dashboard";

                message(
                    "Your profile is complete. You can continue to the dashboard.",
                    "success"
                );
            }

        } catch (error) {
            message(error.message + " Refresh the page to retry.");
        }
    }

    $("setupProvince").addEventListener(
        "change",
        () => loadSchools()
    );

    $("setupGrade").addEventListener(
        "change",
        () => loadSubjects()
    );

    $("setupForm").addEventListener("submit", async event => {
        event.preventDefault();

        if (!ready) return;

        const chosen = [
            ...document.querySelectorAll('input[name="subjects"]:checked')
        ].map(i => Number(i.value));

        if (
            !profile &&
            (
                !Number($("setupGrade").value) ||
                !Number($("setupSchool").value) ||
                !Number($("setupProvince").value)
            )
        ) {
            message("Please select your province, school and grade.");
            return;
        }

        if (!profile && !chosen.length) {
            message("Please select at least one subject.");
            return;
        }

        const btn = $("setupSubmit");

        btn.disabled = true;
        btn.textContent = "Saving...";

        try {
            if (!profile) {
                profile = await api.post("/Learners", {
                    userID: userId,
                    gradeID: Number($("setupGrade").value),
                    schoolID: Number($("setupSchool").value)
                });

                // After profile creation, lock academic details to avoid accidental mismatches.
                $("setupProvince").disabled = true;
                $("setupSchool").disabled = true;
                $("setupGrade").disabled = true;
            }

            const learnerId = Number(
                val(profile, "learnerID", "LearnerID")
            );

            if (!Number.isInteger(learnerId) || learnerId <= 0) {
                throw new Error(
                    "Profile saved, but learner ID is missing. Refresh to resume."
                );
            }

            const failed = [];

            for (const id of chosen) {
                if (registered.has(id)) continue;

                try {
                    await api.post(
                        "/learners/" + learnerId + "/subjects/" + id
                    );

                    registered.add(id);

                } catch (error) {
                    failed.push(
                        "Subject " + id + ": " + error.message
                    );
                }
            }

            if (failed.length) {
                message(
                    "Some subjects could not be saved. Please retry.\n" +
                    failed.join("\n")
                );

                await loadSubjects();
                return;
            }

            if (!registered.size) {
                message("Select at least one subject before continuing.");
                return;
            }

            window.location.replace("dashboard.html");

        } catch (error) {
            message(
                error.message +
                " If your profile was created, refresh this page to resume subject registration."
            );

        } finally {
            btn.disabled = false;
            btn.textContent = "Save and Continue";
        }
    });

    initialize();

})();
