/* Admin workflow for custom badge creation + manual learner awards. */
(function () {
    "use strict";
    if (!window.EduQuestAPI || EduQuestAPI.getCurrentUser()?.role !== "Admin") return;
    const byId = id => document.getElementById(id);
    const status = byId("achievementAdminStatus");
    if (!status) return;
    const learnerSelect = byId("achievementLearner");
    const badgeSelect = byId("achievementBadge");
    const badgeList = byId("learnerAwardedBadges");
    let badges = [];

    const info = message => { status.textContent = message; };
    function option(select, value, label) {
        const item = document.createElement("option");
        item.value = String(value);
        item.textContent = label;
        select.appendChild(item);
    }

    async function loadChoices() {
        const [learners, users, achievements] = await Promise.all([
            EduQuestAPI.get("/learners"),
            EduQuestAPI.get("/users"),
            EduQuestAPI.get("/achievements")
        ]);
        if (!Array.isArray(learners) || !Array.isArray(users) || !Array.isArray(achievements))
            throw new Error("Could not retrieve learner and badge lists.");
        badges = achievements;
        const usersById = new Map(users.map(user => [user.userID, user]));
        learnerSelect.replaceChildren();
        badgeSelect.replaceChildren();
        option(learnerSelect, "", "Select a learner");
        option(badgeSelect, "", "Select a badge");
        learners.forEach(learner => {
            const user = usersById.get(learner.userID);
            const name = user ? `${user.firstName} ${user.lastName}`.trim() : `Learner ${learner.learnerID}`;
            option(learnerSelect, learner.learnerID, `${name} (${learner.schoolName})`);
        });
        achievements.forEach(badge => option(badgeSelect, badge.achievementID,
            `${badge.badgeImage || "🏅"} ${badge.name}`));
    }

    async function showExisting() {
        badgeList.replaceChildren();
        const id = Number(learnerSelect.value);
        if (!id) return;
        const assignments = await EduQuestAPI.get(`/learners/${id}/achievements`);
        if (!Array.isArray(assignments)) throw new Error("Could not retrieve awarded badges.");
        assignments.forEach(assigned => {
            const badge = badges.find(a => a.achievementID === assigned.achievementID);
            if (!badge) return;
            const pill = document.createElement("span");
            pill.className = "eq-achievement-pill";
            pill.textContent = `${badge.badgeImage || "🏅"} ${badge.name}`;
            pill.title = badge.description || badge.name;
            badgeList.appendChild(pill);
        });
        if (!badgeList.children.length) badgeList.textContent = "No badges awarded to this learner yet.";
    }

    byId("createAchievementForm").addEventListener("submit", async event => {
        event.preventDefault();
        const btn = event.currentTarget.querySelector("button");
        btn.disabled = true;
        try {
            await EduQuestAPI.post("/achievements", {
                name: byId("achievementName").value.trim(),
                description: byId("achievementDescription").value.trim(),
                badgeImage: byId("achievementIcon").value.trim(),
                pointsRequired: 0 // manual-only for custom special badges
            });
            info("Special badge created. You can now assign it to a learner.");
            await loadChoices();
            await showExisting();
        } catch (error) {
            info("Unable to create badge: " + error.message);
        } finally {
            btn.disabled = false;
        }
    });

    byId("awardAchievementForm").addEventListener("submit", async event => {
        event.preventDefault();
        const learnerId = Number(learnerSelect.value);
        const badgeId = Number(badgeSelect.value);
        if (!learnerId || !badgeId) { info("Choose a learner and badge."); return; }
        const btn = event.currentTarget.querySelector("button");
        btn.disabled = true;
        try {
            await EduQuestAPI.post(`/learners/${learnerId}/achievements`, {
                achievementID: badgeId
            });
            info("Achievement successfully awarded.");
            await showExisting();
        } catch (error) {
            info("Unable to award badge: " + error.message);
        } finally {
            btn.disabled = false;
        }
    });

    byId("backfillAchievementBtn").addEventListener("click", async event => {
        const btn = event.currentTarget;
        btn.disabled = true;
        info("Synchronising past quiz achievements...");
        try {
            const result = await EduQuestAPI.post("/achievement-badges/backfill", {});
            info(`Achievement sync completed for ${result.learnersChecked} learner(s).`);
            await loadChoices();
            await showExisting();
        } catch (error) {
            info("Unable to sync achievements: " + error.message);
        } finally {
            btn.disabled = false;
        }
    });

    learnerSelect.addEventListener("change", () => {
        showExisting().catch(error => info(error.message));
    });
    loadChoices().catch(error => info("Unable to load badge management: " + error.message));
})();
