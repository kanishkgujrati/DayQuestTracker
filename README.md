# DayQuestTracker 🎮

![CI Pipeline](https://github.com/kanishkgujrati/DayQuestTracker/actions/workflows/ci.yml/badge.svg)

> **Gamified Personal Growth Operating System** — Not just a todo app.
> Habit Tracking + Analytics + Gamification + Social _(coming soon)_

Turn your daily goals into a game. Track habits, earn XP, level up, and analyse your consistency — all in one platform built with production-grade architecture.

---

## 🚀 Features

- **Habit Management** — Create daily, weekly, or custom frequency tasks grouped by categories
- **Daily Check-in** — Mark tasks as completed or skipped for any date up to 7 days back
- **Streak Tracking** — Automatic streak calculation with longest streak history
- **XP & Leveling** — Earn XP on every completion based on difficulty and frequency
- **Analytics** — Consistency %, daily score trends, weakest habits, category performance
- **JWT Authentication** — Secure auth with access token + refresh token rotation
- **Background Jobs** — Nightly streak reset via Hangfire

---

## 🏗️ Architecture

Built with **Clean Architecture** — strict dependency rules ensure business logic is never polluted by infrastructure concerns.

```
DayQuestTracker.Domain          ← Entities, Enums, Business Rules (zero dependencies)
DayQuestTracker.Application     ← CQRS Commands/Queries, Interfaces, Validators
DayQuestTracker.Infrastructure  ← EF Core, PostgreSQL, JWT, BCrypt, Hangfire
DayQuestTracker.WebAPI          ← Controllers, Middleware, Swagger
```

**Dependency Direction:**

```
WebAPI → Application → Domain
WebAPI → Infrastructure → Application
```

Domain knows nothing about the outside world. Swap PostgreSQL for any database — Domain and Application don't change.

---

## 🛠️ Tech Stack

| Layer             | Technology                              |
| ----------------- | --------------------------------------- |
| Framework         | .NET 8                                  |
| Architecture      | Clean Architecture + CQRS               |
| Mediator          | MediatR 12                              |
| Database          | PostgreSQL                              |
| ORM               | Entity Framework Core 8                 |
| Validation        | FluentValidation with Pipeline Behavior |
| Authentication    | JWT Bearer + Refresh Tokens             |
| Password Hashing  | BCrypt                                  |
| Background Jobs   | Hangfire                                |
| API Documentation | Swagger / Swashbuckle                   |

---

## 📊 Database Schema

**10 tables** designed for performance and integrity:

| Table              | Purpose                                         |
| ------------------ | ----------------------------------------------- |
| `Users`            | Identity, TotalXP, Timezone, Refresh Tokens     |
| `Categories`       | Task groupings with Color and Icon              |
| `Tasks`            | Habit definitions with Frequency and Difficulty |
| `TaskSchedules`    | Scheduled days for Weekly/Custom tasks          |
| `TaskCompletions`  | Daily check/skip records                        |
| `UserTaskStreaks`  | Cached streak data per task                     |
| `DailyScores`      | Cached daily score snapshots                    |
| `Achievements`     | Badge definitions                               |
| `UserAchievements` | Earned badges per user                          |
| `XPEvents`         | Full audit log of every XP transaction          |

**Key Design Decisions:**

- UUID primary keys — globally unique, not guessable
- Soft deletes with `DeletedAt` timestamp — data never permanently lost
- `Level` computed from `TotalXP` — never stored, never out of sync
- `XPValue` computed from `Difficulty × FrequencyMultiplier` — single source of truth
- Global EF Core query filters — `WHERE DeletedAt IS NULL` automatic on every query
- Pre-computed `UserTaskStreaks` and `DailyScores` — fast reads, no heavy aggregations on demand

---

## 📡 API Endpoints

### Authentication

```
POST   /api/auth/register          Create a new account
POST   /api/auth/login             Login and receive tokens
POST   /api/auth/refresh           Refresh access token
```

### Profile

```
GET    /api/profile                Get current user profile
PATCH  /api/profile                Update username or timezone
POST   /api/profile/change-password  Change password — invalidates all sessions
```

### Categories

```
GET    /api/categories             Get all categories
GET    /api/categories/{id}        Get category by ID
POST   /api/categories             Create a category
PATCH  /api/categories/{id}        Partial update
DELETE /api/categories/{id}        Soft delete (add ?force=true to cascade delete tasks)
```

### Habit Tasks

```
GET    /api/habittasks             Get all tasks (optional ?categoryId filter)
GET    /api/habittasks/{id}        Get task by ID
GET    /api/habittasks/daily       Get tasks for a specific date with completion status
POST   /api/habittasks             Create a habit task
PATCH  /api/habittasks/{id}        Partial update
DELETE /api/habittasks/{id}        Soft delete
```

### Completions

```
POST   /api/completions            Log a task completion or skip
DELETE /api/completions/{id}       Undo a completion (deducts XP, recalculates streak)
GET    /api/completions            Get completions for a date range
```

### Analytics

```
GET    /api/analytics/consistency         Consistency % per task over a date range
GET    /api/analytics/daily-trend         Daily score trend for charts
GET    /api/analytics/streaks             Streak summary for all tasks
GET    /api/analytics/weakest-habits      Tasks with lowest consistency
GET    /api/analytics/category-performance  Performance breakdown by category
```

---

## ⚙️ Key Technical Decisions

**CQRS with MediatR**
Every operation is a self-contained Command or Query. Controllers never import handlers directly — MediatR routes requests automatically. One file, one responsibility.

**FluentValidation Pipeline Behavior**
Validation runs automatically before every handler via MediatR pipeline. Handlers contain only business logic — zero validation if blocks.

**Result Pattern over Exceptions**
Expected failures like "category not found" return `Result<T>.Failure("message")`. Exceptions are reserved for truly unexpected failures. Controllers map results cleanly to HTTP status codes.

**Streak Recalculation from Scratch**
Streaks are never incremented or decremented. Always recalculated from actual completion records. Handles out-of-order past date logging correctly — a user can log 7 days back in any order and streaks are always accurate.

**Two-Phase Save in Completions**
LogCompletion and UndoCompletion both use two SaveChanges calls:

1. First save — commits the completion/deletion to DB
2. Recalculate streak and DailyScore — queries the committed state
3. Second save — persists recalculated values

This ensures streak recalculation always sees accurate data.

**IPasswordHasher Interface**
BCrypt is abstracted behind an interface. Application layer never references BCrypt directly — only the interface. Infrastructure owns the implementation detail.

---

## 🏃 How to Run Locally

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- Git

### Setup

**1. Clone the repository**

```bash
git clone https://github.com/kanishkgujrati/DayQuestTracker.git
cd DayQuestTracker
```

**2. Configure settings**

```bash
cp DayQuestTracker.WebAPI/appsettings.example.json DayQuestTracker.WebAPI/appsettings.json
```

Edit `appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DayQuestTracker;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "Secret": "YOUR_SECRET_KEY_MINIMUM_32_CHARACTERS",
    "Issuer": "DayQuestTracker",
    "Audience": "DayQuestTrackerUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

**3. Create the database**

Create a database named `DayQuestTracker` in PostgreSQL.

**4. Run migrations**

```bash
dotnet ef database update --project DayQuestTracker.Infrastructure --startup-project DayQuestTracker.WebAPI
```

**5. Run the API**

```bash
dotnet run --project DayQuestTracker.WebAPI
```

**6. Open Swagger**

```
https://localhost:YOUR_PORT/swagger
```

Click **Authorize**, login via `/api/auth/login`, paste `Bearer YOUR_TOKEN` and all endpoints are ready to test.

**7. Open Hangfire Dashboard** _(development only)_

```
https://localhost:YOUR_PORT/hangfire
```

---

## 📁 Project Structure

```
DayQuestTracker/
├── DayQuestTracker.Domain/
│   ├── Entities/          ← User, Category, HabitTask, HabitTaskCompletion...
│   ├── Enums/             ← FrequencyType, CompletionStatus, XPReason...
│   └── Common/            ← BaseEntity
├── DayQuestTracker.Application/
│   ├── Common/
│   │   ├── Behaviors/     ← ValidationBehavior (MediatR pipeline)
│   │   ├── Interfaces/    ← ITrackerDbContext, IPasswordHasher, IJwtTokenGenerator
│   │   └── Models/        ← Result<T>, JwtSettings
│   └── Features/
│       ├── Auth/          ← Register, Login, Refresh
│       ├── Categories/    ← CRUD Commands/Queries + Validators
│       ├── Tasks/         ← CRUD Commands/Queries + Daily View + Validators
│       ├── Completions/   ← Log, Undo, Get + Validator
│       ├── Analytics/     ← 5 Query handlers + ConsistencyCalculator
│       └── Profile/       ← Get, Update, ChangePassword + Validators
├── DayQuestTracker.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/ ← EF Core entity configurations
│   ├── Services/           ← AuthService, JwtTokenGenerator, PasswordHasher
│   └── Jobs/               ← StreakResetJob (Hangfire)
└── DayQuestTracker.WebAPI/
    ├── Controllers/        ← Auth, Categories, HabitTasks, Completions, Analytics, Profile
    └── Middleware/         ← ExceptionHandlingMiddleware
```

---

## 🗺️ Roadmap

- [x] Backend API — Clean Architecture + CQRS
- [x] Authentication — JWT + Refresh Tokens
- [x] Categories + HabitTasks CRUD
- [x] TaskCompletions — Log, Undo, Streak Recalculation
- [x] Analytics — Consistency, Trends, Streaks, Weakest Habits
- [x] User Profile Management
- [x] Hangfire Background Jobs
- [ ] Docker + CI/CD Pipeline
- [ ] Angular Frontend
- [ ] NgRx State Management
- [ ] Chart.js Analytics Dashboards
- [ ] Redis Caching
- [ ] SignalR Real-time Social Feed
- [ ] Friend System + Leaderboards
- [ ] Deployment

---

## 👤 Author

**Kanishk Gujrati**
Associate Software Engineer
[GitHub](https://github.com/kanishkgujrati)

---

_Built as a portfolio project to demonstrate production-grade .NET development practices._
