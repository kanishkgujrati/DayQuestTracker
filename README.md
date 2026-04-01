# DayQuestTracker

Gamified Personal Growth Operating System, Not just a todo app. Habit + Tracking + Analytics + Gamification platform

## Tech Stack

- .NET 8 — Clean Architecture + CQRS + MediatR
- PostgreSQL + Entity Framework Core
- JWT Authentication with Refresh Tokens
- FluentValidation with Pipeline Behavior
- Hangfire Background Jobs

## Architecture

(paste your dbdiagram.io link here)
(paste your Clean Architecture layer diagram here)

## API Endpoints

(list your 23 endpoints)

## How to Run Locally

1. Clone the repo
2. Copy appsettings.example.json to appsettings.json
3. Update connection string and JWT secret
4. Run migrations: dotnet ef database update
5. Run: dotnet run --project DayQuestTracker.WebAPI
