# Stage 1 — Build
# Why multi-stage build: final image only contains runtime, not SDK
# SDK image is ~700MB, runtime image is ~200MB
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first
# Why: Docker caches layers — if only code changes, NuGet restore is skipped
COPY DayQuestTracker.sln .
COPY DayQuestTracker.Domain/DayQuestTracker.Domain.csproj DayQuestTracker.Domain/
COPY DayQuestTracker.Application/DayQuestTracker.Application.csproj DayQuestTracker.Application/
COPY DayQuestTracker.Infrastructure/DayQuestTracker.Infrastructure.csproj DayQuestTracker.Infrastructure/
COPY DayQuestTracker.WebAPI/DayQuestTracker.WebAPI.csproj DayQuestTracker.WebAPI/

# Restore NuGet packages
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish DayQuestTracker.WebAPI/DayQuestTracker.WebAPI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DayQuestTracker.WebAPI.dll"]