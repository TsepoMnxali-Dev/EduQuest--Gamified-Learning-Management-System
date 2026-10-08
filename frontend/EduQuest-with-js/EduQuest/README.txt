# EduQuest  frontend

Pages (each page has its own script in js/ with the same name):
- index.html - landing page                      -> js/index.js
- login.html - sign-in page                      -> js/login.js
- signupp.html - three-step sign-up              -> js/signupp.js
- reset-password.html                            -> js/reset-password.js
- dashboard.html                                 -> js/dashboard.js
- classes.html                                   -> js/classes.js
- mathematics-class.html                         -> js/mathematics-class.js
- physical-sciences-class.html                   -> js/physical-sciences-class.js
- accounting-class.html                          -> js/accounting-class.js
- Quizzes.html - list of quizzes by subject      -> js/quizzes.js
- quiz.html - quiz attempt (quiz.html?id=<id>)   -> js/quiz.js
- resources.html                                 -> js/resources.js
- leaderboard.html                               -> js/leaderboard.js
- profile.html - user profile                    -> js/profile.js

Other files:
- css/style.css - shared styling
- images/eduquest-logo.png

Backend notes:
- Look for "TODO (backend)" and "DATA ACCESS" comments in js/ to find where
  server calls belong.
- js/quiz.js and js/quizzes.js keep all data access in a few Promise-returning
  functions (fetchQuiz, fetchSavedResult, submitResult, fetchResults). For now
  they use built-in sample quizzes and the browser's localStorage.
- js/signupp.js has sample school data that the backend should replace.
