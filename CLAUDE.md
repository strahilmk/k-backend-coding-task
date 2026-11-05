# Claims API

Backend coding task. The spec is `docs/README.md`. Reviewers read this code before the interview, so keep changes small and easy to follow.

## How we work

Strahil writes the design and business logic. Claude handles:

- mechanical work, such as moving files and fixing namespaces
- unit tests
- comments and documentation, where they are needed

Don't refactor, rename or add anything beyond what was asked. If something looks wrong, say so instead of changing it.

## Structure

Standard three layers in the `Claims` project, using folders rather than separate projects:

- `Controllers`: HTTP only
- `Services`: validation and business logic
- `Repositories`: data access
- `Data`: `ClaimsContext`
- `Auditing`: audit code and `AuditContext`. Don't change its namespace, because the migrations reference it.

Add interfaces only on services, repositories and the auditer. No CQRS, MediatR or other extra patterns.

## Stack

.NET 10, C#, xUnit v3. Claims and covers are stored in MongoDB, audit rows in SQL Server. Both run through Testcontainers, so Docker must be running to start the app or run tests that go through `Program.cs`.

## Commands

```powershell
dotnet build
dotnet test
dotnet run --project Claims --launch-profile Claims
```

## Tests

- Write tests from the rules in `docs/README.md`, not from what the code currently does.
- Unit tests must not need Docker. Fake the repositories and the auditer.
- If a rule is ambiguous, ask rather than guess.

## Git

- Work on `claims-solution`. `main` stays on the initial commit, and the PR goes into `main`.
- Don't commit or push unless asked.
- One commit per task, with a message saying what changed.
- Files use LF line endings, enforced by `.gitattributes`. Check that `git diff --stat` shows only the lines you meant to change.
