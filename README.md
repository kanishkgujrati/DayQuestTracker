# DayQuestTracker 🎮

![CI Pipeline](https://github.com/kanishkgujrati/DayQuestTracker/actions/workflows/ci.yml/badge.svg)

> **Gamified Personal Growth Operating System** — Not just a todo app.
> Habit Tracking + XP System + Analytics + Frequency-Aware Streaks

Turn your daily goals into a game. Define habits, track completions, earn XP, level up, and analyse your consistency — built with production-grade architecture from database design to frontend deployment.

---

## 🚀 Features

### Core Habit Tracking

- **5 Frequency Types** — Daily, Weekly (specific days), Custom (target per week), Once a Week (any day), Once a Month (any day)
- **Daily Check-in** — Mark tasks Completed or Skipped for any date up to 7 days back
- **Undo Completions** — Reverse a logged completion with automatic XP deduction and streak recalculation
- **Smart Daily View** — Once-a-week tasks disappear after completion and reappear next week. Once-a-month tasks reappear next month

### Gamification

- **XP System** — Earn XP on every completion based on Difficulty × FrequencyMultiplier
- **Level System** — Level = TotalXP / 500. Computed, never stored — always accurate
- **Frequency-Aware Streaks** — Daily tasks count consecutive days. Weekly/Custom count consecutive scheduled occurrences. Once-a-Week counts consecutive Mon-Sun weeks. Once-a-Month counts consecutive calendar months
- **Longest Streak** — Historical best streak per task, never decreases on undo

### Analytics Dashboard

- **Daily Score Trend** — Line chart showing score % over 7/30/90 days
- **Task Consistency** — Bar chart with green/yellow/red based on performance threshold
- **Streak Summary** — All tasks ranked by current streak
- **Weakest Habits** — Tasks needing attention with progress bars
- **Category Performance** — Average consistency, XP earned and best streak per category
- **Weekly Summary Card** — Once-a-Week task completion status for current week
- **Monthly Summary Card** — Once-a-Month task completion status for current month

### User Management

- **JWT Authentication** — Access token (15 min) + Refresh token (7 days) rotation
- **Profile Management** — Update username and timezone
- **Profile Photo** — Upload JPEG/PNG/WebP, served as static file
- **Change Password** — Invalidates all existing sessions on success

### Categories

- **Color Picker** — 12 preset hex colors
- **Icon Picker** — 12 emoji icons
- **Force Delete** — Block deletion if active tasks exist, `?force=true` cascades soft delete to all tasks
- **Add Task from Category** — Click Add Task on any category card to open pre-filled task form

---

## 🏗️ Architecture

Built with **Clean Architecture** — strict one-way dependency rules ensure business logic is never polluted by infrastructure concerns.

```
DayQuestTracker.Domain          ← Entities, Enums, Business Rules (zero dependencies)
DayQuestTracker.Application     ← CQRS Commands/Queries, Interfaces, Validators, Services
DayQuestTracker.Infrastructure  ← EF Core, PostgreSQL, JWT, BCrypt, Hangfire
DayQuestTracker.WebAPI          ← Controllers, Middleware, Swagger
```

**Dependency Direction:**

```
WebAPI → Application → Domain
WebAPI → Infrastructure → Application
```

Domain knows nothing about the outside world. Swap PostgreSQL for any database and Domain + Application do not change.

---

## 🛠️ Tech Stack

### Backend

| Technology                   | Purpose                      |
| ---------------------------- | ---------------------------- |
| .NET 8                       | API Framework                |
| Clean Architecture + CQRS    | Architectural Pattern        |
| MediatR 12                   | Request/Response Mediator    |
| PostgreSQL                   | Primary Database             |
| Entity Framework Core 8      | ORM with Npgsql              |
| FluentValidation             | Validation Pipeline Behavior |
| JWT Bearer + Refresh Tokens  | Authentication               |
| BCrypt (via IPasswordHasher) | Password Hashing             |
| Hangfire + PostgreSQL        | Background Jobs              |
| Swagger / Swashbuckle        | API Documentation            |

### Frontend

| Technology                         | Purpose          |
| ---------------------------------- | ---------------- |
| Angular 17+ (Standalone)           | SPA Framework    |
| NgRx (Store + Effects + Selectors) | State Management |
| Chart.js                           | Analytics Charts |
| Tailwind CSS                       | Utility Styling  |
| RxJS                               | Reactive Streams |

### DevOps

| Technology              | Purpose          |
| ----------------------- | ---------------- |
| Docker + docker-compose | Containerization |
| GitHub Actions          | CI/CD Pipeline   |

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

### Key Design Decisions

- **UUID primary keys** — globally unique, not guessable, future-proof
- **Soft deletes with `DeletedAt` timestamp** — data never permanently lost, auditable
- **`Level` computed from `TotalXP`** — never stored, never out of sync
- **`XPValue` computed from Difficulty × FrequencyMultiplier** — single source of truth
- **Global EF Core query filters** — `WHERE DeletedAt IS NULL` automatic on every query
- **Pre-computed `UserTaskStreaks` and `DailyScores`** — fast reads, no heavy aggregation on demand
- **`XPEvents` as audit log** — full history including negative amounts on undo
- **Task creation date boundary** — TotalTasks only counts tasks that existed on that date

---

## 📡 API Endpoints

### Authentication

```
POST   /api/auth/register - Create a new account
POST   /api/auth/login    - Login and receive tokens
POST   /api/auth/refresh  - Refresh access token
```

### Profile

```
GET    /api/profile                 - Get current user profile
PATCH  /api/profile                 - Update username or timezone
POST   /api/profile/change-password - Change password — invalidates all
POST   /api/profile/upload-photo    - Upload User Profile Picture
```

### Categories

```
GET    /api/categories             Get all categories
GET    /api/categories/{id}        Get category by ID
POST   /api/categories             Create a category
PATCH  /api/categories/{id}        Partial update
DELETE /api/categories/{id}?force={bool}  - Soft delete (add ?force=true to cascade delete tasks)
```

### Habit Tasks

```
GET /api/habittasks        - Get all tasks (optional ?categoryId filter)
GET /api/habittasks/{id}   - Get task by ID
GET /api/habittasks/daily  - Get tasks for a specific date with completion status
POST /api/habittasks       - Create a habit task
PATCH /api/habittasks/{id} - Partial update
DELETE /api/habittasks/{id} - Soft delete
```

### Completions

```
POST /api/completions                       - Log a task completion or skip
DELETE /api/completions/{id}                - Undo a completion (deducts XP, recalculates streak)
GET /api/completions/{startDate}/{endDate}  - Get completions for a date range
```

### Analytics

```
GET /api/analytics/consistency/{startDate}/{endDate}          - Consistency % per task over a date range
GET /api/analytics/daily-trend/{startDate}/{endDate}          - Daily score trend for charts
GET /api/analytics/streaks                                    - Streak summary for all tasks
GET /api/analytics/weakest-habits/{startDate}/{endDate}       - Tasks with lowest consistency
GET /api/analytics/category-performance/{startDate}/{endDate} - Performance breakdown by category
GET /api/analytics/weekly-summary/{date}                      - Once A Week Analysis
GET /api/analytics/monthly-summary/{year}/{month}             - Once A Month Analysis
```

## ⚙️ Key Technical Decisions

### CQRS with MediatR

Every operation is a self-contained Command or Query. Controllers never import handlers directly. One file, one responsibility, easy to test and extend.

### FluentValidation Pipeline Behavior

Validation runs automatically before every handler via MediatR pipeline. Handlers contain only business logic — zero if-based validation checks inside handlers.

### Result Pattern over Exceptions

Expected failures return `Result<T>.Failure("message")`. Exceptions reserved for truly unexpected failures. Controllers map results to HTTP status codes cleanly.

### Frequency-Aware Streak Calculation

Streaks are always recalculated from scratch from actual completion records — never incremented or decremented:

- **Daily/Weekly/Custom** — consecutive scheduled occurrences
- **OnceAWeek** — consecutive Mon-Sun weeks with at least one completion
- **OnceAMonth** — consecutive calendar months with at least one completion

Handles out-of-order past date logging correctly. A user can log 7 days back in any order and streaks are always accurate.

### Two-Phase Save in Completions

Both LogCompletion and UndoCompletion use two SaveChangesAsync calls — first commits the data change, then recalculates streak and DailyScore against the committed state. Prevents stale data in recalculation.

### IPasswordHasher Interface

BCrypt abstracted behind an interface. Application layer never references BCrypt directly. Infrastructure owns the implementation.

### Task Creation Date Boundary

TotalTasks in DailyScore and ConsistencyCalculator only counts tasks that existed on or before the date being scored. Tasks created today do not retroactively affect past scores.

### OnceAWeek / OnceAMonth Duplicate Prevention

- **OnceAWeek** — checks for any completion within the current Mon-Sun week
- **OnceAMonth** — checks for any completion within the current calendar month
- Once completed, task disappears from dashboard for the rest of the period and reappears next period

---

## 🏃 How to Run Locally

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- Node.js 18+
- Git

### Backend Setup

**1. Clone the repository**

```bash
git clone https://github.com/kanishkgujrati/DayQuestTracker.git
cd DayQuestTracker
```

**2. Configure settings**

```bash
cp DayQuestTracker.WebAPI/appsettings.example.json DayQuestTracker.WebAPI/appsettings.json
```

Edit `appsettings.json`:

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

**3. Run migrations**

```bash
dotnet ef database update --project DayQuestTracker.Infrastructure --startup-project DayQuestTracker.WebAPI
```

**4. Run the API**

```bash
dotnet run --project DayQuestTracker.WebAPI
```

**5. Open Swagger**

```
https://localhost:YOUR_PORT/swagger
```

**6. Hangfire Dashboard** _(development only)_

```
https://localhost:YOUR_PORT/hangfire
```

### Frontend Setup

```bash
cd client
npm install
ng serve
```

Open `http://localhost:4200`

### Docker Setup (Full Stack)

```bash
docker-compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Migrations applied automatically on startup

---

## 📁 Project Structure

```
DayQuestTracker/
├── DayQuestTracker.Domain/
│   ├── Entities/            ← User, Category, HabitTask, HabitTaskCompletion...
│   ├── Enums/               ← FrequencyType, CompletionStatus, XPReason...
│   └── Common/              ← BaseEntity (Id, CreatedAt, UpdatedAt, DeletedAt)
├── DayQuestTracker.Application/
│   ├── Common/
│   │   ├── Behaviors/       ← ValidationBehavior (MediatR pipeline)
│   │   ├── Interfaces/      ← ITrackerDbContext, IPasswordHasher, IJwtTokenGenerator
│   │   ├── Models/          ← Result<T>, JwtSettings
│   │   └── Services/        ← StreakCalculator, ConsistencyCalculator
│   └── Features/
│       ├── Auth/            ← Register, Login, Refresh
│       ├── Categories/      ← CRUD Commands/Queries + Validators
│       ├── Tasks/           ← CRUD + Daily View + Validators
│       ├── Completions/     ← Log, Undo, Get + Validator
│       ├── Analytics/       ← 7 Query handlers
│       └── Profile/         ← Get, Update, ChangePassword, UploadPhoto
├── DayQuestTracker.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/  ← EF Core IEntityTypeConfiguration per entity
│   ├── Services/            ← AuthService, JwtTokenGenerator, PasswordHasher
│   └── Jobs/                ← StreakResetJob (Hangfire, nightly 00:05 UTC)
├── DayQuestTracker.WebAPI/
│   ├── Controllers/         ← Auth, Categories, HabitTasks, Completions, Analytics, Profile
│   └── Middleware/          ← ExceptionHandlingMiddleware
├── client/                  ← Angular 17+ Frontend
│   └── src/app/
│       ├── core/
│       │   ├── guards/      ← authGuard
│       │   ├── interceptors/ ← authInterceptor (JWT + auto-refresh)
│       │   ├── models/      ← TypeScript interfaces matching API DTOs
│       │   └── services/    ← AuthService, TaskService, CategoryService...
│       ├── features/        ← Dashboard, Categories, Tasks, Analytics, Profile, Auth
│       ├── shared/          ← LayoutComponent (sidebar + router-outlet)
│       └── store/           ← NgRx slices: auth, dashboard, categories, tasks, analytics
├── .github/workflows/       ← ci.yml (GitHub Actions)
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## 🎮 XP and Level System

```
XP per Completion = Difficulty × 10 × FrequencyMultiplier

Multipliers:
  Daily        × 1.0   →  Difficulty 3 = 30 XP
  Weekly       × 1.5   →  Difficulty 3 = 45 XP
  Custom       × 1.2   →  Difficulty 3 = 36 XP
  OnceAWeek    × 2.0   →  Difficulty 3 = 60 XP
  OnceAMonth   × 3.0   →  Difficulty 3 = 90 XP

Level = floor(TotalXP / 500) + 1
Every 500 XP = 1 level up
Level 1: 0–499 XP  |  Level 2: 500–999 XP  |  Level 10: 4500+ XP
```

---

## 🗺️ Roadmap

- [x] Backend API — Clean Architecture + CQRS
- [x] Authentication — JWT + Refresh Tokens
- [x] Categories CRUD with color and icon picker
- [x] HabitTasks CRUD — 5 frequency types
- [x] TaskCompletions — Log, Undo, Streak Recalculation
- [x] Frequency-Aware Streak System (Daily, Weekly, OnceAWeek, OnceAMonth)
- [x] Analytics — Consistency, Trends, Streaks, Weakest Habits, Category Performance
- [x] Weekly and Monthly Summary Cards on Dashboard
- [x] User Profile — Edit, Change Password, Profile Photo Upload
- [x] Add Task shortcut from Category page
- [x] Hangfire Background Jobs — Nightly streak reset
- [x] Task creation date boundary in DailyScore calculation
- [x] Docker + docker-compose
- [x] GitHub Actions CI/CD Pipeline
- [x] Angular Frontend — NgRx, Chart.js, Tailwind CSS
- [ ] Redis Caching — Analytics and daily view responses
- [ ] Unit Tests — Critical handler and calculator coverage
- [ ] Deployment — Live URL on Railway or VPS
- [ ] Friend System + Leaderboards
- [ ] SignalR Real-time Social Feed

---

## 👤 Author

**Kanishk Gujrati**
Associate Software Engineer
[GitHub](https://github.com/kanishkgujrati)

---

_Built as a portfolio project to demonstrate production-grade full-stack .NET + Angular development practices._
