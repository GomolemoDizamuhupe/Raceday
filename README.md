# RaceDay

A full-stack web-based event management system for the South African road running, walking, and cycling community. RaceDay lets Event Organisers create and manage events, categories, and participant results, while Participants can browse events, enter categories, track their entries, and view their personal results.

**Module:** PROG6212
**Student:** Gomolemo Dizamuhupe (ST10491441)
**Group:** 4
**GitHub Repo:** [GomolemoDizamuhupe/Raceday](https://github.com/GomolemoDizamuhupe/Raceday)
**Part 1 Demo Video:** https://youtu.be/ZRrgUtRMAuo
**Part 2 Demo Video part 1:** https://youtu.be/2Glku9wPoys?si=DWxH7WVlUIkqNEPM
**Part 2 Demo Video part 2:** https://youtu.be/ecFRMHqhjps?si=bTVxoylHGI99x0gy
**Part 1 Document:** [Database Schema, API Request Methods and ERD (PDF)](https://github.com/GomolemoDizamuhupe/Raceday/blob/master/DATABASE%20SCHEMA%2C%20API%20REQUEST%20METHODS%20AND%20ERD.pdf)
**SQL Script:** [Race_day.sql](Race_day.sql)

## Project Structure

This is a Portfolio of Evidence (PoE) built progressively across three parts. Each part builds on the previous one.

- **Part 1 — System Planning and Database** *(complete)*
  ERD, API endpoint plan, and SQL database script.
- **Part 2 — RESTful API** *(current)*
  ASP.NET Core Web API in C#, connected to SQL Server, with unit tests and GitHub Actions CI/CD.
- **Part 3 — MVC Web Application** *(upcoming)*
  MVC front end consuming the API, Azure Blob Storage, and Docker.

## Roles

- **Organiser** — create, edit, and delete events; manage categories and routes; capture and correct results; view all entries for their events.
- **Participant** — register; browse events; enter a category; view their own entries and results.

## Part 2 — RESTful API

### Tech Stack

- ASP.NET Core Web API (C#)
- SQL Server LocalDB (`Race_day` database, created from the Part 1 SQL script)
- ADO.NET (`Microsoft.Data.SqlClient`) with parameterised queries
- BCrypt.Net for password hashing
- Swagger for testing endpoints
- xUnit for unit tests
- GitHub Actions for CI

### Solution Layout

```
Part2_Raceday/
  Backend/       Data and business logic (Auth, Users, Events, Categories, Routes, Entries, RaceResults)
  Controllers/   API controllers, one per resource
Test_Raceday/
  UnitTest1.cs   xUnit tests
```

Each controller is thin: it receives the request and calls the matching class in `Backend`, which runs the SQL and enforces the role rules.

### How Roles Are Enforced

Passwords are hashed with BCrypt before they are stored, and login checks the entered password against the stored hash. For protected actions, the backend looks up the caller's role in the `Users` table using their user ID. Organiser-only actions (create/edit/delete events, categories, routes, results) are rejected for participants, and participant-only actions (enter or withdraw from a category) are rejected for organisers. Edits and deletes also check that the organiser owns the parent event.

### Endpoints

All routes start with `/api`.

| Resource | Method | Route | Role |
|---|---|---|---|
| Auth | POST | `/Auth/Register` | Public |
| Auth | POST | `/Auth/Login` | Public |
| Users | GET | `/Users/me` | Any |
| Users | PUT | `/Users/me` | Any |
| Users | GET | `/Users/{userId}` | Any |
| Events | GET | `/Events` | Any |
| Events | GET | `/Events/{eventId}` | Any |
| Events | POST | `/Events` | Organiser |
| Events | PUT | `/Events/{eventId}` | Organiser |
| Events | DELETE | `/Events/{eventId}` | Organiser |
| Categories | GET | `/Categories/{eventId}/Categories` | Any |
| Categories | GET | `/Categories/{categoryId}/Category` | Any |
| Categories | POST | `/Categories` | Organiser |
| Categories | PUT | `/Categories/{categoryId}` | Organiser |
| Categories | DELETE | `/Categories/{categoryId}` | Organiser |
| Routes | GET | `/Routes/{categoryId}route` | Any |
| Routes | POST | `/Routes/{categoryId}route` | Organiser |
| Routes | PUT | `/Routes/{routeId}` | Organiser |
| Entries | POST | `/categories/{categoryId}/entries` | Participant |
| Entries | GET | `/Entries` | Participant |
| Entries | GET | `/Entries/{eventId}` | Organiser |
| Entries | DELETE | `/Entries/{entryId}` | Participant |
| Results | POST | `/Results/{entryId}` | Organiser |
| Results | PUT | `/Results/{resultId}` | Organiser |
| Results | GET | `/Results` | Participant |
| Results | GET | `/Results/{eventId}` | Organiser |

### Validation Rules

- Registration role must be `organiser` or `participant`.
- Registration emails must end in `@gmail.com` and be unique.
- Finish times must be in `HH:mm:ss` format.
- Category distance must be a valid decimal number.

### Running the API

1. Run [`Race_day.sql`](Race_day.sql) (the Part 1 script) in SSMS so the `Race_day` database exists on `(localdb)\MSSQLLocalDB`.
2. Open the solution in Visual Studio.
3. Set `Part2_Raceday` as the startup project and press F5.
4. Use Swagger (opens automatically) to try the endpoints.

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

They cover successful actions, role restrictions, duplicate emails, invalid input, and missing records.

To run them: `dotnet test`, or use Test Explorer in Visual Studio.

### CI/CD

GitHub Actions builds the solution and runs the tests on every push.

<img width="1366" height="653" alt="image" src="https://github.com/user-attachments/assets/1db71740-8708-4b36-bc8b-2c150543dffc" />


## Part 1 Summary

The full Part 1 document is here: [Database Schema, API Request Methods and ERD (PDF)](https://github.com/GomolemoDizamuhupe/Raceday/blob/master/DATABASE%20SCHEMA%2C%20API%20REQUEST%20METHODS%20AND%20ERD.pdf)

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
[`Race_day.sql`](Race_day.sql) creates the full schema in SQL Server (`Users`, `Events`, `Categories`, `Routes`, `Entries`, `Results`) with primary keys, foreign keys, and constraints, seeded with sample data.

## Part 1 GitHub Action Screenshot
<img width="1366" height="741" alt="image" src="https://github.com/user-attachments/assets/baf79d82-a07e-46c6-ad4e-6ff04a697aa2" />

## References

- diagrams.net (Draw.io), 2026. *Draw.io: Free online diagram software.* Available at: https://www.drawio.com/
- Microsoft, 2026. *SQL Server Management Studio (SSMS) documentation.* Available at: https://learn.microsoft.com/en-us/sql/ssms/
