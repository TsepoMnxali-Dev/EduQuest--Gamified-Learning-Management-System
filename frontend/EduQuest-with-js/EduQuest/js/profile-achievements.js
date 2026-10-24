
/* Display permanent awarded badges on the signed-in learner's profile. */

(function () {
    "use strict";

    const panel = document.getElementById("profileAchievementsPanel");
    const list = document.getElementById("profileAchievements");
    const status = document.getElementById("profileAchievementsStatus");

    if (!panel || !list || !window.EduQuestAPI) return;

    const user = EduQuestAPI.getCurrentUser();

    if (!user || user.role !== "Learner") {
        panel.hidden = true;
        return;
    }

    async function loadBadges() {
        status.textContent = "Loading achievements...";
        list.replaceChildren();

        try {
            const badges = await EduQuestAPI.get("/achievement-badges/me");

            if (!Array.isArray(badges)) {
                throw new Error("Unexpected achievement response.");
            }

            if (badges.length === 0) {
                status.textContent =
                    "No badges earned yet. Complete a quiz to get started!";
                return;
            }

            status.textContent =
                `${badges.length} achievement${badges.length === 1 ? "" : "s"} unlocked`;

            badges.forEach(badge => {
                const card = document.createElement("article");
                card.className = "eq-badge-card";

                const symbol = document.createElement("span");
                symbol.className = "eq-badge-big";

                symbol.textContent = badge.badgeImage &&
                    !badge.badgeImage.startsWith("http")
                        ? badge.badgeImage
                        : "🏅";

                const content = document.createElement("div");

                const name = document.createElement("strong");
                name.textContent = badge.name;

                const description = document.createElement("p");
                description.textContent =
                    badge.description || "Achievement earned";

                content.append(name, description);
                card.append(symbol, content);

                list.appendChild(card);
            });

        } catch (error) {
            status.textContent =
                "Unable to load achievements: " + error.message;
        }
    }

    loadBadges();

    document.getElementById("profileRefresh")
        ?.addEventListener("click", loadBadges);

})();
