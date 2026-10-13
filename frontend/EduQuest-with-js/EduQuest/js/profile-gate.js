/* EduQuest learner onboarding gate. Only a genuine 404 means no profile. */
window.EduQuestProfileGate = (function () {
  "use strict";
  const api = window.EduQuestAPI;
  const setupUrl = new URL("../complete-profile.html", document.currentScript.src).href;
  const dashboardUrl = new URL("../dashboard.html", document.currentScript.src).href;
  async function getProfileState() {
    const token = api.getToken();
    if (!token) throw new Error("Please sign in to continue.");
    let response;
    try {
      response = await fetch(api.BASE_URL + "/Learners/me", {
        headers: { Authorization: "Bearer " + token }
      });
    } catch (_) { throw new Error("Cannot connect to EduQuest. Please check that the backend is running."); }
    if (response.status === 404) return { profile: null, subjects: [] };
    if (response.status === 401) { api.clearToken(); throw new Error("Your session has expired. Please sign in again."); }
    if (!response.ok) throw new Error("Unable to verify learner profile (HTTP " + response.status + ").");
    const profile = await response.json();
    const learnerId = profile.learnerID ?? profile.learnerId ?? profile.LearnerID;
    if (!Number.isInteger(Number(learnerId)) || Number(learnerId) <= 0) throw new Error("Learner profile returned an invalid ID.");
    const subjects = await api.get("/learners/" + learnerId + "/subjects");
    if (!Array.isArray(subjects)) throw new Error("Unable to verify learner subjects.");
    return { profile, subjects };
  }
  async function redirectAfterLogin() {
    const state = await getProfileState();
    window.location.replace(state.profile && state.subjects.length ? dashboardUrl : setupUrl);
  }
  async function protectDashboard() {
    if (!api || !api.requireLearner()) return;
    try {
      const state = await getProfileState();
      if (!state.profile || !state.subjects.length) window.location.replace(setupUrl);
    } catch (error) {
      console.error("Learner profile verification failed:", error);
      // Fail closed: hide dashboard while service cannot verify onboarding.
      document.body.innerHTML = "<main style=\"max-width:550px;margin:12vh auto;padding:30px;font-family:Arial;color:#173f67;background:white;border-radius:16px\"><h1>Unable to verify your profile</h1><p id=\"profileGateError\"></p><p>Please check your connection and refresh the page.</p></main>";
      document.getElementById("profileGateError").textContent = error.message;
    }
  }
  return { getProfileState, redirectAfterLogin, protectDashboard };
})();
