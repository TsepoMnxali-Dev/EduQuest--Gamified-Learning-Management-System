
(function () {
    "use strict";

    EduQuestAPI.requireAuth();

    if (!EduQuestAPI.getToken()) return;

    const el = id => document.getElementById(id);

    const put = (id, value) => {
        if (el(id)) {
            el(id).textContent =
                value == null || value === ""
                    ? "Not provided"
                    : String(value);
        }
    };

    const status = message => {
        el("profileStatus").textContent = message;
        el("profileStatus").hidden = !message;
    };

    let currentProfile = null;

    async function load() {
        status("Loading your profile...");

        try {
            const p = await EduQuestAPI.get("/profile/me");

            currentProfile = p;

            const name = [p.firstName, p.lastName]
                .filter(Boolean)
                .join(" ") || "EduQuest user";

            put("profileName", name);

            put(
                "profileAvatar",
                [p.firstName, p.lastName]
                    .filter(Boolean)
                    .map(x => x.charAt(0))
                    .join("")
                    .toUpperCase() || "U"
            );

            put("profileRole", p.role);
            put("profileSubtitle", p.email);
            put("profileFirstName", p.firstName);
            put("profileLastName", p.lastName);
            put("profileEmail", p.email);

            put(
                "profileAccountStatus",
                p.isActive ? "Active" : "Inactive"
            );

            put(
                "profileJoined",
                p.dateCreated
                    ? new Date(p.dateCreated).toLocaleDateString(
                        undefined,
                        {
                            year: "numeric",
                            month: "long",
                            day: "numeric"
                        }
                    )
                    : null
            );

            put("profileUserId", p.userID);

            const learner = p.role === "Learner";

            el("learnerDetails").hidden = !learner;

            if (learner) {
                put("profileGrade", p.grade);
                put("profileSchool", p.school);

                const subjects = el("profileSubjects");

                subjects.replaceChildren();

                if (Array.isArray(p.subjects) && p.subjects.length) {
                    p.subjects.forEach(subject => {
                        const chip = document.createElement("span");

                        chip.className = "eq-subject";
                        chip.textContent = subject;

                        subjects.appendChild(chip);
                    });
                } else {
                    subjects.textContent = "No subjects recorded yet.";
                }
            }

            status("");

        } catch (error) {
            status("Unable to load your profile: " + error.message);
        }
    }

    el("profileEdit").addEventListener("click", () => {
        if (!currentProfile) return;

        el("editProfileFirstName").value =
            currentProfile.firstName || "";

        el("editProfileLastName").value =
            currentProfile.lastName || "";

        el("editProfileEmail").value =
            currentProfile.email || "";

        el("profileEditMessage").textContent = "";
        el("profileEditPanel").hidden = false;

        el("editProfileFirstName").focus();
    });

    el("profileCancel").addEventListener("click", () => {
        el("profileEditPanel").hidden = true;
    });

    el("profileEditForm").addEventListener("submit", async event => {
        event.preventDefault();

        const save = el("profileSave");
        const message = el("profileEditMessage");

        save.disabled = true;
        message.textContent = "Saving changes...";

        try {
            const token = EduQuestAPI.getToken();

            if (!token) {
                throw new Error(
                    "Your session has expired. Please sign in again."
                );
            }

            const response = await fetch(
                EduQuestAPI.BASE_URL + "/profile/me",
                {
                    method: "PUT",
                    headers: {
                        "Authorization": "Bearer " + token,
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        firstName: el("editProfileFirstName").value.trim(),
                        lastName: el("editProfileLastName").value.trim(),
                        email: el("editProfileEmail").value.trim()
                    })
                }
            );

            if (!response.ok) {
                let detail = null;

                try {
                    detail = await response.json();
                } catch (_) {}

                throw new Error(
                    detail?.message ||
                    detail?.title ||
                    "Unable to save profile (" + response.status + ")."
                );
            }

            el("profileEditPanel").hidden = true;

            await load();

            status("Profile updated successfully.");

        } catch (error) {
            message.textContent = error.message;

        } finally {
            save.disabled = false;
        }
    });

    el("profileRefresh").addEventListener("click", load);

    el("profileLogout").addEventListener("click", () => {
        // Clear only authentication-related storage; never clear unrelated app data.
        for (const storage of [localStorage, sessionStorage]) {
            for (const key of [
                "token",
                "authToken",
                "eduquest_token",
                "eduquestToken",
                "user",
                "currentUser",
                "eduquest_user",
                "eduquestUser"
            ]) {
                storage.removeItem(key);
            }
        }

        window.location.href =
            document.location.pathname.includes("/admin/")
                ? "../../EduQuest-with-js/EduQuest/login.html"
                : "login.html";
    });

    load();

})();
