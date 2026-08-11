# EduQuest Learning Management System

## Backend Development Team Responsibilities

This document defines the backend responsibilities for each team member.
Each team member must work on their own Git branch and must not commit
directly to the `main` branch.

---

## Git Workflow

Before starting work:

```bash
git switch main
git pull origin main
git switch -c feature/your-feature-name
```

After completing work:

```bash
git add .
git commit -m "Describe your changes"
git push -u origin feature/your-feature-name
```

Then create a Pull Request from your feature branch into `main`.

### Important Rules

- Do not work directly on `main`.
- Do not push directly to `main`.
- Pull the latest `main` before starting new work.
- Only modify files related to your assigned functionality where possible.
- Test your API through Swagger before creating a Pull Request.
- Coordinate with other members when your functionality depends on their entities.

---

# Gcinikhaya Mabe — Users, Authentication & Learners

## Responsibilities

Gcinikhaya is responsible for the application's users, roles,
authentication, authorization and learner management.

### Database Entities

- Users
- Roles
- Learners
- ActivityLogs

### APIs

#### Authentication API

Base route: `/api/auth`

- `POST /api/auth/register` — Register a new user.
- `POST /api/auth/login` — Authenticate a user.
- `POST /api/auth/logout` — Log out the current user if applicable.

#### Users API

Base route: `/api/users`

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`

#### Learners API

Base route: `/api/learners`

- `GET /api/learners`
- `GET /api/learners/{id}`
- `POST /api/learners`
- `PUT /api/learners/{id}`
- `DELETE /api/learners/{id}`

#### Activity Logs API

Base route: `/api/activitylogs`

- `GET /api/activitylogs`
- `GET /api/activitylogs/{id}`

---

# Liqhamile Silinga — Academic Content & Study Materials

## Responsibilities

Liqhamile Silinga is responsible for the academic structure of EduQuest
and the study resources available to learners.

### Database Entities

- Grades
- Subjects
- GradeSubjects
- LearnerSubjects
- Topics
- StudyMaterials

### APIs

#### Grades API

Base route: `/api/grades`

- `GET /api/grades`
- `GET /api/grades/{id}`
- `POST /api/grades`
- `PUT /api/grades/{id}`
- `DELETE /api/grades/{id}`

#### Subjects API

Base route: `/api/subjects`

- `GET /api/subjects`
- `GET /api/subjects/{id}`
- `POST /api/subjects`
- `PUT /api/subjects/{id}`
- `DELETE /api/subjects/{id}`

#### Grade Subjects API

Base route: `/api/grades/{gradeId}/subjects`

- `GET /api/grades/{gradeId}/subjects`
- `POST /api/grades/{gradeId}/subjects/{subjectId}`
- `DELETE /api/grades/{gradeId}/subjects/{subjectId}`

#### Learner Subjects API

Base route: `/api/learners/{learnerId}/subjects`

- `GET /api/learners/{learnerId}/subjects`
- `POST /api/learners/{learnerId}/subjects/{subjectId}`
- `DELETE /api/learners/{learnerId}/subjects/{subjectId}`

#### Topics API

Base route: `/api/topics`

- `GET /api/topics`
- `GET /api/topics/{id}`
- `POST /api/topics`
- `PUT /api/topics/{id}`
- `DELETE /api/topics/{id}`

#### Study Materials API

Base route: `/api/studymaterials`

- `GET /api/studymaterials`
- `GET /api/studymaterials/{id}`
- `POST /api/studymaterials`
- `PUT /api/studymaterials/{id}`
- `DELETE /api/studymaterials/{id}`

---

# Tsepo Mnxali — Quizzes, Gamification & Competitions

## Responsibilities

Tsepo Mnxali is responsible for the assessment and gamification
features of EduQuest.

### Database Entities

- Quizzes
- QuizQuestions
- QuizOptions
- QuizAttempts
- QuizAttemptAnswers
- Leaderboards
- Achievements
- LearnerAchievements
- Competitions
- CompetitionParticipants
- Prizes
- Sponsors
- Notifications

### APIs

#### Quizzes API

Base route: `/api/quizzes`

- `GET /api/quizzes`
- `GET /api/quizzes/{id}`
- `POST /api/quizzes`
- `PUT /api/quizzes/{id}`
- `DELETE /api/quizzes/{id}`

#### Quiz Questions API

Base route: `/api/quizzes/{quizId}/questions`

- `GET /api/quizzes/{quizId}/questions`
- `POST /api/quizzes/{quizId}/questions`
- `PUT /api/questions/{id}`
- `DELETE /api/questions/{id}`

#### Quiz Options API

Base route: `/api/questions/{questionId}/options`

- `GET /api/questions/{questionId}/options`
- `POST /api/questions/{questionId}/options`
- `PUT /api/options/{id}`
- `DELETE /api/options/{id}`

#### Quiz Attempts API

Base route: `/api/quizattempts`

- `GET /api/quizattempts`
- `GET /api/quizattempts/{id}`
- `POST /api/quizattempts`
- `GET /api/learners/{learnerId}/quizattempts`

#### Quiz Attempt Answers API

Base route: `/api/quizattemptanswers`

- `POST /api/quizattemptanswers`
- `GET /api/quizattempts/{attemptId}/answers`

#### Leaderboards API

Base route: `/api/leaderboards`

- `GET /api/leaderboards`
- `GET /api/leaderboards/{id}`
- `GET /api/leaderboards/grade/{gradeId}`

#### Achievements API

Base route: `/api/achievements`

- `GET /api/achievements`
- `GET /api/achievements/{id}`
- `POST /api/achievements`
- `PUT /api/achievements/{id}`
- `DELETE /api/achievements/{id}`

#### Learner Achievements API

Base route: `/api/learners/{learnerId}/achievements`

- `GET /api/learners/{learnerId}/achievements`
- `POST /api/learners/{learnerId}/achievements`

#### Competitions API

Base route: `/api/competitions`

- `GET /api/competitions`
- `GET /api/competitions/{id}`
- `POST /api/competitions`
- `PUT /api/competitions/{id}`
- `DELETE /api/competitions/{id}`

#### Competition Participants API

Base route: `/api/competitions/{competitionId}/participants`

- `GET /api/competitions/{competitionId}/participants`
- `POST /api/competitions/{competitionId}/participants`
- `DELETE /api/competitions/{competitionId}/participants/{learnerId}`

#### Prizes API

Base route: `/api/prizes`

- `GET /api/prizes`
- `GET /api/prizes/{id}`
- `POST /api/prizes`
- `PUT /api/prizes/{id}`
- `DELETE /api/prizes/{id}`

#### Sponsors API

Base route: `/api/sponsors`

- `GET /api/sponsors`
- `GET /api/sponsors/{id}`
- `POST /api/sponsors`
- `GET /api/sponsors/{id}`
- `PUT /api/sponsors/{id}`
- `DELETE /api/sponsors/{id}`

#### Notifications API

Base route: `/api/notifications`

- `GET /api/notifications`
- `GET /api/notifications/{id}`
- `POST /api/notifications`
- `PUT /api/notifications/{id}`
- `DELETE /api/notifications/{id}`
- `PATCH /api/notifications/{id}/read`

---

# Shared Backend Responsibilities

Although each member has assigned entities, some backend decisions must
be coordinated.

## Database

The team must agree on:

- Required and optional fields
- Naming conventions
- Data types


## Authentication & Authorization

Team Member 1 owns the authentication implementation, but all members
must use the agreed authentication and authorization system when
protecting their endpoints.

## Swagger

Every API endpoint should appear in Swagger and be testable through
Swagger UI.

Development URL:

`https://localhost:7021/swagger`

## API Naming

Use consistent REST-style naming:

```text
GET     /api/resources
GET     /api/resources/{id}
POST    /api/resources
PUT     /api/resources/{id}
DELETE  /api/resources/{id}
```

## Testing

Before submitting a Pull Request:

1. Build the API.
2. Run the API.
3. Open Swagger.
4. Test your endpoints.
5. Verify successful responses.
6. Test invalid input.
7. Test not-found cases.
8. Check that existing functionality still works.

---

# Branch Naming

Use descriptive branch names.

Examples:

```text
feature/user-management
feature/learner-management
feature/grades
feature/subjects
feature/study-materials
feature/quizzes
feature/achievements
feature/competitions
feature/authentication
```

Avoid vague names such as:

```text
branch1
test
mybranch
stuff
new
```

---

# Commit Message Examples

Good:

```text
Add learner CRUD endpoints
Implement quiz attempt API
Add subject and grade relationships
Implement JWT authentication
```

Avoid vague messages such as:

```text
changes
update
stuff
fixed things
final
```

---

# Pull Request Process

When your work is ready:

```text
Feature Branch
      |
      v
   Commit
      |
      v
 Push to GitHub
      |
      v
 Pull Request
      |
      v
 Code Review
      |
      v
 Merge into main
```

Never merge unfinished or untested functionality.

---


Swagger is available at:

`https://localhost:7021/swagger`


---

# Goal

The objective is for all three members to develop their assigned
functionality independently while maintaining one shared, working
EduQuest backend.

Each member is responsible for:

1. Their assigned models.
2. Their assigned database relationships.
3. Their assigned controllers.
4. Their assigned API endpoints.
5. Validation and error handling.
6. Testing through Swagger.
7. Git commits and feature branches.
8. Pull Requests into `main`.

**Communication between team members is required whenever one API depends
on another member's entities or functionality.**

