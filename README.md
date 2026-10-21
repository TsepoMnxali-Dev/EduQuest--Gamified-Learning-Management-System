# 🎓 EduQuest — Gamified Learning Management System



**Enhancing Education Through Tech**



A full-stack learning platform for South African learners in Grades 10–12, built with ASP.NET Core, SQL Server and vanilla JavaScript.



EduQuest lets learners enrol in subjects, take timed quizzes, review their results, earn points and achievements, and climb leaderboards across the country. Administrators manage users, quizzes, questions and study resources, and can generate quiz questions with Google Gemini.

![image alt](https://github.com/TsepoMnxali-Dev/EduQuest--Gamified-Learning-Management-System/blob/a8f5525257cb114f187da69891f9c842864a2c30/Eduquest%20Gamified%20learning%20management%20landing%20page.png)

## 🎥 Demo

<p align="center">
  <a href="https://www.dropbox.com/scl/fi/upf7cpb86rn0dontlo8bq/CoolRides-Vehicle-Manufacturing-System-Demo.mov?rlkey=vl66r4flgdgetgbosdjczuyzl&st=lq0jha0l&dl=0">
    ▶️ <strong>Watch the EduQuest Demo</strong>
  </a>
</p>


---



## 🏫 System Overview



EduQuest consists of:



- 🎓 **Learner portal**: sign-up, dashboard, classes, quizzes, leaderboard, profile, resources and help

- 🛠️ **Admin portal**: users, quizzes, questions, resources and leaderboard

- 🌐 **REST API**: ASP.NET Core Web API secured with JWT

- 🗄️ **SQL Server database** accessed through Entity Framework Core

- 🤖 **AI question generator** powered by Google Gemini

- ➗ **KaTeX** for properly typeset mathematics

- 🏠 **Public home page** with live platform statistics



Every figure shown in the interface comes from the database. There is no hard-coded sample data.



### How the Pieces Fit Together



```

        ┌──────────────────────────────────┐

        │     Browser (HTML / CSS / JS)    │

        │   Learner pages · Admin pages    │

        └────────────────┬─────────────────┘

                         │

                 EduQuestAPI (js/api.js)

                 adds the sign-in token

                         │

                         ▼

        ┌──────────────────────────────────┐

        │      ASP.NET Core Web API        │

        │  Controllers · DTOs · Services   │

        └───────┬──────────────────┬───────┘

                │                  │

                ▼                  ▼

       ┌────────────────┐  ┌─────────────────┐

       │   EF Core      │  │  Google Gemini  │

       │  SQL Server    │  │ (quiz questions)│

       └────────────────┘  └─────────────────┘

```



---



## ✨ Features



### 🎓 Learners



| Area | What learners can do |
|---|---|
| **Sign-up** | Three steps: account details, province and school, then subjects. Everything is loaded from the database. |
| **Sign-in** | Sign in, or reset a forgotten password with a one-time link that expires after one hour. |
| **Dashboard** | Recent activity, enrolled classes, total points, ranking and a weekly quiz goal. |
| **Classes** | Every enrolled subject with progress, topics, quizzes and study materials. |
| **Quizzes** | 10 random questions per attempt, a countdown timer, a full review with explanations, and unlimited retakes. |
| **Leaderboard** | This week, this month or all time, with a podium, subject tabs, a province filter and a top learner per province. |
| **Profile** | Edit your name, change your password, manage your subjects and view your achievements. |
| **Resources** | View or download the study materials published by administrators. |
| **Help & Support** | A searchable FAQ and a contact form. Administrator replies appear on the same page. |

### 🛠️ Administrators



| Area | What administrators can do |
|---|---|
| **Dashboard** | Platform statistics and quick actions. |
| **Users** | Add, edit, view, deactivate and reactivate users. |
| **Quizzes** | Create, edit, publish and unpublish quizzes, and manage their questions and answer options. |
| **Question sources** | Write questions by hand, draw them from the question bank, or generate them with Gemini from the study materials uploaded for a topic. |
| **Resources** | Upload and manage the materials learners can view and download. |
| **Leaderboard** | View rankings across subjects and provinces. |
| **Support** | Review and answer learner requests through the support API. |



---



## 🔄 Key Workflows



### 📝 Taking a Quiz



```

 Learner opens a quiz

          │

          ▼

 POST /quizzes/{id}/start

 10 random approved questions

 (the response contains no answer keys)

          │

          ▼

 Learner answers · countdown timer runs

          │

          ▼

 POST /quizattempts/{id}/submit

 Score is calculated and saved

          │

          ▼

 GET /quizattempts/{id}/result

 Review with correct answers and explanations

          │

          ▼

 Points, ranking and achievements update

```



### 🧑‍🏫 Building a Quiz



```

 Admin creates a quiz for a topic

          │

          ▼

 ┌────────────┬─────────────┬──────────────┐

 │ Write by   │ Draw from   │ Generate     │

 │ hand       │ question    │ with Gemini  │

 │            │ bank        │              │

 └─────┬──────┴──────┬──────┴──────┬───────┘

       └─────────────┼─────────────┘

                     ▼

        At least 10 approved questions

                     │

                     ▼

              Admin publishes

                     │

                     ▼

 Visible to learners in that grade and subject

```



### 🧾 Signing Up



```

 Learner completes the 3-step form

          │

          ▼

 POST /auth/register-learner

          │

   ┌──────┴──────────────────────────┐

   │  One database transaction:      │

   │  User → Learner → Subjects      │

   └──────┬──────────────────────────┘

          │

          ▼

 Token returned · learner signed in

```



---



## 🧩 Architecture and Design Decisions



### 🔐 1. JWT Authentication and Role-Based Authorisation



Every learner and administrator endpoint requires a signed token. The token carries the user's id, email and role, and administrator actions are restricted with the `Admin` role.



### 📦 2. Data Transfer Objects (DTOs)



Controllers never expose database entities directly. Requests and responses use DTOs, which keeps sensitive fields (such as password hashes) out of API responses and lets the database change without breaking the front end.



### 🗄️ 3. Entity Framework Core with Migrations



The schema is defined by entity classes and created with migrations. A `DbSeeder` loads reference data (grades, subjects, topics, a starter question bank and demo learners) when the API starts in Development.



### 💉 4. Dependency Injection



The database context, token service, email service and Gemini service are registered in `Program.cs` and injected into controllers, so each part can be replaced or tested on its own.



### 🌐 5. A Shared Front-End Client



`js/api.js` exposes `window\.EduQuestAPI`, the single place that:



- stores the sign-in token,

- adds it to every request,

- times out slow requests,

- and turns server errors into readable messages.



Every page uses it, so authentication and error handling are written once.



### 🛡️ 6. Safe Rendering



Pages write server text with `textContent`, never `innerHTML`, which prevents script injection.



---



## 🛠️ Tech Stack



| Technology | Purpose |
|---|---|
| **C# / ASP.NET Core Web API** | Back-end application and REST endpoints |
| **Entity Framework Core** | Object-relational mapping and migrations |
| **Microsoft SQL Server** | Relational database |
| **JWT** | Stateless authentication |
| **BCrypt** | Password hashing |
| **Google Gemini** | AI-generated quiz questions |
| **HTML, CSS, JavaScript** | Front end (no framework) |
| **KaTeX** | Mathematics typesetting |


---



## 📁 Project Structure



```

Backend/

└── EduQuest.API/

    ├── EduQuest.API.slnx

    └── EduQuest.API/              # the API project (contains the .csproj)

        ├── Controllers/           # REST endpoints

        ├── Data/                  # ApplicationDBContext and DbSeeder

        ├── DTOs/                  # request and response models

        ├── Models/Entities/       # database entities

        ├── Services/              # token, email and Gemini quiz-generator services

        ├── Migrations/

        ├── appsettings.json

        └── Program.cs

```



**Front end:** learner pages (`index`, `signup`, `login`, `reset-password`, `dashboard`, `classes`, `class`, `Quizzes`, `quiz`, `leaderboard`, `profile`, `resources`, `help`), administrator pages (`pages/admin`), and the shared `js/`, `css/` and `images/` folders.



---



## 🚀 Getting Started



### Prerequisites



- The .NET SDK matching the project's `TargetFramework`

- SQL Server Express (or any SQL Server instance)

- The EF Core command-line tools:
   **dotnet tool install --global dotnet-ef**

- Visual Studio or VS Code with the **Live Server** extension



### 1. Get the Code



Clone the repository and open the folder in your editor.



### 2. Set Up the Database



In `Backend/EduQuest.API/EduQuest.API/appsettings.json`, point `DefaultConnection` at your SQL Server:



```json

"ConnectionStrings": {

  "DefaultConnection": "Server=YOUR-ServerName;Database=EduQuest;Trusted_Connection=True;TrustServerCertificate=True;"

}

```



Then create the database from the project folder (the one containing `EduQuest.API.csproj`):



```bash

cd Backend/EduQuest.API/EduQuest.API

dotnet ef database update

```



### 3. Add Your Secrets



Store the secrets described in [Configuration and Secrets](#-configuration-and-secrets).



### 4. Run the API



```bash

dotnet dev-certs https --trust     *# once, so the browser trusts https\://localhost*

dotnet run --launch-profile https

```



The API listens on `https\://localhost:7021`. When it starts in **Development**, it seeds the grades, subjects, topics, a starter question bank, study-material links and demo learners, and creates the first administrator from your secrets. Swagger is available at `https\://localhost:7021/swagger`.



### 5. Run the Front End



Open `index.html` with **Live Server**. The pages are served from `http\://127.0.0.1:5500`.



> Pages must be served over `http`. Opening an HTML file directly from disk (`file://`) blocks API calls.



The API address is set in `js/api.js`:



```js

var BASE_URL = "https\://localhost:7021/api";

```



The front-end origins the API accepts are listed in the CORS policy in `Program.cs`. Add yours there if you serve the pages from a different address or port.



### 6. Sign In



- **Administrator:** the email and password stored in `AdminSeed:Email` and `AdminSeed:Password`.

- **Learner:** sign up on `signup.html`, or use one of the seeded demo learners listed in `Data/DbSeeder.cs`.



---



## 🔑 Configuration and Secrets



Secrets are kept out of the repository with .NET user-secrets. Run these from the folder that contains `EduQuest.API.csproj`:



```bash

dotnet user-secrets set "Jwt:Key" "a-long-random-string-of-at-least-32-characters"

dotnet user-secrets set "AdminSeed:Email" "admin\@example.com"

dotnet user-secrets set "AdminSeed:Password" "choose-a-strong-password"

dotnet user-secrets set "Email:Password" "your-smtp-password-or-key"

```



The Gemini API key is read from configuration by `Services/GeminiQuizGeneratorService.cs`. Store it with `dotnet user-secrets set` under the setting name that file uses.



Settings kept in `appsettings.json`:



| Setting | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` | Token settings |
| `Frontend:BaseUrl` | Address used in password-reset links (`http\://127.0.0.1:5500` by default) |
| `Email:Host`, `Port`, `User`, `From`, `EnableSsl` | SMTP settings for outgoing email |


---



## 📏 How the Main Rules Work



| Rule | Behaviour |
|---|---|
| **Quiz attempts** | Each attempt gives the learner **10 random approved questions**. A quiz needs at least 10 approved questions before learners can start it. |
| **What a learner sees** | A quiz is visible only if it is published, its topic belongs to the learner's grade, and the learner is enrolled in that subject. |
| **Points** | **10 points per correct answer**, counting only the learner's **best attempt on each quiz**, so retaking a quiz improves a score without collecting extra points. |
| **Rankings** | Only learners with points are ranked, and equal points share a rank. Other learners appear as a first name and a surname initial. |
| **Weekly goal** | Complete 5 quizzes in the current week (the week starts on Monday). |
| **Achievements** | Earned automatically from quiz activity: a first quiz, ten quizzes, a perfect score, a seven-day streak, a top-10 place, and a star badge for each subject. |


---



## 🔌 API Overview



All routes are under `/api`. Learner routes require a valid token, and administrator routes require the `Admin` role.



| Area | Routes |
|---|---|
| **Authentication** | `POST /auth/register-learner`<br>`POST /auth/login`<br>`POST /auth/forgot-password`<br>`POST /auth/reset-password` |
| **Reference data (public)** | `GET /provinces`<br>`GET /grades`<br>`GET /subjects`<br>`GET /schools?provinceId=` |
| **Home page (public)** | `GET /landing-stats` |
| **Learner** | `GET /learner/me`<br>`GET /learner/dashboard`<br>`GET /learner/classes`<br>`GET /learner/classes/{subjectId}`<br>`GET /learner/quizzes`<br>`GET /learner/leaderboard` |
| **Learner profile** | `GET /learner/profile`<br>`PUT /learner/profile`<br>`POST /learner/profile/password`<br>`GET /learner/profile/subjects`<br>`PUT /learner/profile/subjects` |
| **Quizzes** | `POST /quizzes/{id}/start`<br>`POST /quizattempts/{id}/submit`<br>`GET /quizattempts/{id}/result` |
| **Support** | `POST /support`<br>`GET /support/mine` (learners)<br>`GET /support` (administrators)<br>`PUT /support/{id}` (administrators) |
| **Administration** | Users, quizzes, questions, answer options, question bank, study materials and leaderboard |


Swagger lists every endpoint, with its request and response shapes, while the API runs in Development.


---



## 🛡️ Security



- 🔒 Passwords are hashed with **BCrypt** and are never stored or returned in plain text.

- 🎫 **JWT** bearer tokens protect every learner and administrator endpoint, and administrator actions require the `Admin` role.

- 🔁 Password-reset tokens are random, stored only as a hash, expire after one hour and can be used once. The "forgot password" response is the same whether or not an email address has an account.

- 🙈 The response that starts a quiz contains no answer keys.

- 👤 Learners can read only their own results, profile and support requests.

- 🧼 Pages write server text with `textContent`, never `innerHTML`, which prevents script injection.



---



## 🎯 Project Objective



The objective of EduQuest is to give learners a motivating place to practise what they study, and to give administrators the tools to run it.



Rather than building the system as one tightly coupled application, responsibilities are separated into a secured API, a data layer and a lightweight front end that all share a single client for authentication and error handling.



---



## 🚀 Key Concepts Demonstrated



- ✅ RESTful API design

- ✅ JWT authentication

- ✅ Role-based authorisation

- ✅ Password hashing with BCrypt

- ✅ Entity Framework Core and migrations

- ✅ Data Transfer Objects

- ✅ Dependency injection

- ✅ Database transactions

- ✅ Asynchronous programming (`async` / `await`)

- ✅ Third-party AI integration (Google Gemini)

- ✅ Separation of responsibilities

- ✅ Secure handling of secrets

- ✅ Gamification: points, ranks, leaderboards and achievements



---



## 📌 Conclusion



EduQuest shows how a secured back end, a relational database and a framework-free front end can work together as one complete platform.



A learner signs up in one transaction, takes a timed quiz built by an administrator by hand, from the question bank or with Gemini, and sees the result reflected in their points, ranking and achievements. Administrators control what learners see through publishing, user management and study resources.



Three subjects. Two roles. One learning platform.



---



## 👨‍💻 Project



**EduQuest** — Gamified Learning Management System



Built to demonstrate a complete, secure and data-driven web application for Grade 10–12 learners.
