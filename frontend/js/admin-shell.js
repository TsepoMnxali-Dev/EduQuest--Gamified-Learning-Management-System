/* Admin access and shared logout for pages without their own auth handling. */
(function(){"use strict";
  function init(){
    if(!window.EduQuestAPI){console.error("EduQuest API helper is missing.");return;}
    if(!EduQuestAPI.requireAdmin())return;
    const logout=document.getElementById("adminLogout");
    if(logout)logout.addEventListener("click",()=>EduQuestAPI.logout());
  }
  if(document.readyState==="loading")document.addEventListener("DOMContentLoaded",init);else init();
})();
