# RaceDay

A full-stack web-based event management system for the South African road running, walking, and cycling community. RaceDay lets Event Organisers create and manage events, categories, routes and results, while Participants can browse events, enter categories, track their entries, and view their personal results.

**Module:** PROG6212
**Student:** Gomolemo Dizamuhupe (ST10491441)
**Group:** 4
**GitHub Repo:** [GomolemoDizamuhupe/Raceday](https://github.com/GomolemoDizamuhupe/Raceday)
**Part 3 branch:** `part3`
**Part 1 Demo Video:** https://youtu.be/ZRrgUtRMAuo
**Part 2 Demo Video:** _add YouTube link here_
**Part 3 Demo Video:** _add YouTube link here_
**Part 1 Document:** [Database Schema, API Request Methods and ERD (PDF)](https://github.com/GomolemoDizamuhupe/Raceday/blob/master/DATABASE%20SCHEMA%2C%20API%20REQUEST%20METHODS%20AND%20ERD.pdf)
**SQL Script:** [Race_day.sql](Race_day.sql)

## Project Structure

This is a Portfolio of Evidence (PoE) built progressively across three parts. Each part builds on the previous one.

- **Part 1 — System Planning and Database** *(complete)*
  ERD, API endpoint plan, and SQL database script.
- **Part 2 — RESTful API** *(complete)*
  ASP.NET Core Web API in C#, connected to SQL Server, with unit tests and GitHub Actions CI.
- **Part 3 — MVC Web Application** *(current, on the `part3` branch)*
  ASP.NET Core MVC front end that consumes the API, with session login, role-based pages and image upload.

## Roles

- **Organiser** — create, edit, and delete events; manage categories and routes; view enrolments; capture and correct results.
- **Participant** — register; browse upcoming events; enter a category; view and withdraw from their entries; view their personal results; manage their profile.

## Solution Layout

```
Part2_Raceday.sln
Part2_Raceday/                API (Part 2)
  Backend/                    Data and business logic
  Controllers/                API controllers, one per resource
  Models/                     Request and response objects (JSON)
Test_Raceday/                 xUnit tests
Part3_Raceday/                MVC web app (Part 3)
  Controllers/                Account, Home, Organiser, Participant, Profile
  Filters/                    RequireLogin (login and role check)
  Models/                     View models and API response models
  Services/                   ApiService (the only class that calls the API)
  Views/                      Razor views, one folder per controller
  wwwroot/                    CSS and uploaded profile pictures
Race_day.sql                  Database script
```

## Part 3 — MVC Web Application

### Tech Stack

- ASP.NET Core MVC (.NET 8) with Razor views
- Bootstrap 5 with a custom RaceDay theme (`wwwroot/css/site.css`)
- `HttpClient` through `ApiService` to call the Part 2 API
- Session state (`AddSession`) for the logged-in user
- Profile pictures stored in `wwwroot/uploads/profiles`

The MVC project has no database code and no copied business logic. Every read and write goes through the API.

### Authentication and Roles

1. Register and log in pages call `POST /api/Auth/Register` and `POST /api/Auth/Login`.
2. On login, the user's ID, role and name are stored in the session (30 minute idle timeout).
3. The `[RequireLogin]` filter redirects logged-out users to the login page. `[RequireLogin("organiser")]` and `[RequireLogin("participant")]` send users with the wrong role to an Access Denied page.
4. The navigation bar shows different menus for each role, and Log out clears the session.
5. Passwords are hashed with BCrypt in the API. The MVC app never sees or stores a password hash.

### Organiser Pages

| Page | What it does |
|---|---|
| Dashboard | Events created, upcoming events, total enrolments, and a table of events with enrolment counts |
| My Events | Create, edit and delete events (name, description, date, location, event type, distance) |
| Categories | Add, edit and delete categories for an event (name, distance, maximum participants) |
| Route | Add or edit a category's route (start, end, elevation gain, map link) |
| Enrolments | See everyone enrolled in an event, with category, entry date and status, and filter by category |
| Results | Capture finish times and positions for enrolled participants once the event date has arrived |

### Participant Pages

| Page | What it does |
|---|---|
| Events | Upcoming events with search, location and event type filters |
| Event detail | Event information, categories, routes with map links, and an Enter event button |
| My Events | The participant's entries with colour-coded status (green Confirmed, amber Pending) and a Withdraw button |
| My Results | Event, date, category, finish time and position (for example "1st of 12") |
| My Profile | Update name, surname and email, and upload a profile picture |

### Profile Pictures

Following the lecturer's instruction, images are handled with `wwwroot` and not Azure Blob Storage. The upload action accepts `.jpg`, `.jpeg`, `.png` and `.webp` files up to 5 MB, saves the file under a random name in `wwwroot/uploads/profiles`, and stores the path in the `Users.ProfilePictureUrl` column through `PUT /api/Users/me/picture`. Replacing a picture deletes the old file.

### Input Handling

Forms use data annotation validation. API errors (400, 403, 404, 409) come back as `{ "message": "..." }` and are shown as friendly messages. If the API cannot be reached, the user sees "Cannot reach the server" and not an error page.

### UI

A consistent RaceDay theme runs across every page: one colour palette, Barlow Condensed for headings with Public Sans for body text, and the same buttons, tables, forms and badges everywhere. Event dates appear as a race bib.

## Running the Application

Docker is not required for this part.

1. Run [`Race_day.sql`](Race_day.sql) in SSMS so the `Race_day` database exists on `(localdb)\MSSQLLocalDB`.
2. Open `Part2_Raceday.sln` in Visual Studio.
3. Right-click the solution, choose **Configure Startup Projects**, select **Multiple startup projects**, and set `Part2_Raceday` and `Part3_Raceday` to **Start** (leave `Test_Raceday` as None).
4. Press F5. Swagger opens for the API (`http://localhost:5267/swagger`) and the web app opens at `https://localhost:7104`.
5. The web app finds the API through `ApiSettings:BaseUrl` in `Part3_Raceday/appsettings.json` (`http://localhost:5267/`).

Register one **organiser** and one **participant** on the Register page to try both sides. The sample users in the SQL script use placeholder password hashes, so they cannot log in.

### Suggested walkthrough

1. Register an organiser, create an event, add a category and a route.
2. Register a participant, find the event, and enter a category.
3. As the organiser, open Enrolments, then (on or after the event date) capture a result.
4. As the participant, open My Events and My Results.

## Part 2 — RESTful API

### Tech Stack

- ASP.NET Core Web API (C#)
- SQL Server LocalDB (`Race_day` database)
- ADO.NET (`Microsoft.Data.SqlClient`) with parameterised queries
- BCrypt.Net for password hashing
- Swagger for testing endpoints
- xUnit for unit tests

### How Roles Are Enforced

Organiser-only actions (create, edit and delete events, categories, routes and results) are rejected for participants, and participant-only actions (enter or withdraw from a category) are rejected for organisers. The API looks up the caller's role in the `Users` table. Edits, deletes and result entry also check that the organiser owns the parent event.

### Endpoints

All routes start with `/api`. Requests with a body send JSON, and responses use proper status codes.

| Resource | Method | Route | Role |
|---|---|---|---|
| Auth | POST | `/Auth/Register` | Public |
| Auth | POST | `/Auth/Login` | Public |
| Users | GET | `/Users/me?userId=` | Any |
| Users | PUT | `/Users/me?userId=` | Any |
| Users | PUT | `/Users/me/picture?userId=` | Any |
| Users | GET | `/Users/{userId}` | Any |
| Events | GET | `/Events` | Any |
| Events | GET | `/Events/{eventId}` | Any |
| Events | POST | `/Events` | Organiser |
| Events | PUT | `/Events/{eventId}` | Organiser |
| Events | DELETE | `/Events/{eventId}?organiserId=` | Organiser |
| Categories | GET | `/Categories/{eventId}/Categories` | Any |
| Categories | GET | `/Categories/{categoryId}/Category` | Any |
| Categories | POST | `/Categories` | Organiser |
| Categories | PUT | `/Categories/{categoryId}` | Organiser |
| Categories | DELETE | `/Categories/{categoryId}?organiserId=` | Organiser |
| Routes | GET | `/Routes/category/{categoryId}` | Any |
| Routes | POST | `/Routes` | Organiser |
| Routes | PUT | `/Routes/{routeId}` | Organiser |
| Entries | POST | `/categories/{categoryId}/entries` | Participant |
| Entries | GET | `/Entries?participantId=` | Participant |
| Entries | GET | `/Entries/{eventId}?organiserId=` | Organiser |
| Entries | DELETE | `/Entries/{entryId}?participantId=` | Participant |
| Results | POST | `/Results/{entryId}` | Organiser |
| Results | PUT | `/Results/{resultId}` | Organiser |
| Results | GET | `/Results?participantId=` | Participant |
| Results | GET | `/Results/{eventId}?organiserId=` | Organiser |

### Validation Rules

- Registration role must be `organiser` or `participant`; the email must be valid and unique; the password must be at least 6 characters.
- Participants cannot enter an event that has already happened, enter the same category twice, or enter a full category.
- Finish times must be in `HH:mm:ss` format and positions must be positive.
- A result can only be captured once per entry.
- Events with categories, and categories with routes or entries, cannot be deleted (409 Conflict).

### Unit Tests

The `Test_Raceday` project contains 60 xUnit tests that run against the LocalDB database. Each test creates its own unique users and data, so they can be re-run without clearing the database.

| Test class | Tests |
|---|---|
| AuthTests | 8 |
| UsersTests | 7 |
| EventsTests | 11 |
| CategoriesTests | 11 |
| RoutesTests | 5 |
| EntriesTests | 9 |
| RaceResultsTests | 9 |

Run them with `dotnet test` or Test Explorer in Visual Studio.

## CI/CD

GitHub Actions (`.github/workflows/main.yml`) runs on pushes to `main`, `master` and `part3`. It builds the whole solution (API, tests and MVC app), starts LocalDB, creates the database from `Race_day.sql`, and runs the tests.

_Add the green build screenshot here:_

```
<img width="..." alt="Part 3 GitHub Actions" src="..." />
```

## Part 1 Summary

### Entity Relationship Diagram
Six entities: `User`, `Event`, `Category`, `Route`, `Entry`, `Result`.
- User organises many Events (1:*)
- Event has many Categories (1:*)
- Category has one Route (1:1)
- User submits many Entries (1:*); Category receives many Entries (1:*)
- Entry produces one Result (1:1)

### API Endpoint Plan
Covers Authentication, User Profile, Events, Categories, Routes, Entries, and Results, with method, route, role, request body, and expected responses for each.

### SQL Database Script
[`Race_day.sql`](Race_day.sql) creates the full schema in SQL Server with primary keys, foreign keys, and constraints, seeded with sample data. Later parts added `EventType`, `DistanceKm` and `BannerUrl` to `Events`, `ProfilePictureUrl` to `Users`, and a longer `MapUrl` on `Routes`.

## Part 1 GitHub Action Screenshot
<img width="1366" height="741" alt="image" src="https://github.com/user-attachments/assets/baf79d82-a07e-46c6-ad4e-6ff04a697aa2" />

## References

- diagrams.net (Draw.io), 2026. *Draw.io: Free online diagram software.* Available at: https://www.drawio.com/
- Microsoft, 2026. *SQL Server Management Studio (SSMS) documentation.* Available at: https://learn.microsoft.com/en-us/sql/ssms/
- Microsoft, 2026. *ASP.NET Core MVC documentation.* Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/
- Bootstrap, 2026. *Bootstrap documentation.* Available at: https://getbootstrap.com/docs/
