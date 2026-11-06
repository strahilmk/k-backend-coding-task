# Notes on working with Claude Code

## What I decided
- Task 1: the layering (controllers, services, repositories), and splitting POST and GET models into DTOs.
- Task 1: ids should be generated outside the services, not with `Guid.NewGuid()` in each one.
- Task 2: all dates are treated as UTC.
- Task 2: the validation rules and their edge cases: a period of one calendar year, both ends of a cover period included, and a 400 with my own message when a claim doesn't match its cover.
- Task 3 design: I wrote the `Channel` queue, `AuditWorker` and `docs/pr-notes.md` myself.
- Task 3: typing the queue, and using an enum for `HttpRequestType`, stored upper-case.
- Task 4: using `TimeProvider` so the StartDate tests can't fail around midnight UTC, with the rule kept in the DTO. This was for the tests only. Claude raised the risk.
- Task 4: adding controller integration tests for 201, 404 and 400, which replace the template's TODO.
- Task 5: I fixed the premium bugs myself, and chose to charge only whole days. That's left open in the PR notes.
- When to commit and push.

## What I used Claude for
- Task 1: moving files and fixing namespaces, and building the repository and service layers from my design. While moving the models into `Models`, it noticed that `Cover.Type` was stored as `claimType`.
- Tasks 1–2: DTOs, the UTC JSON converter, the validation code and its unit tests.
- Task 1: the README and code comments.
- Task 3: reviewing my code. It found that queued entries were dropped on shutdown. After the fix, a one-off test stopped the app with 499 entries still queued, and all 500 were written.
- Task 4: unit tests with hand-written fakes, and the controller integration tests.
- Task 5: reviewing my premium fix. It found that days after 365 still got no discount and fixed that, then wrote premium tests worked out by hand from the spec. All of them fail on the original code.
- Checking against the running app (curl, and queries against the SQL Server container) and writing commits.

## Where I rejected or corrected its output
- Task 1: put `required` on `Id` against its advice, then made `Id` nullable, because POST bodies have no id.
- Task 2: replaced its "Cover not found" message with my own wording about the claim not matching its cover.
- Task 3: its first enum conversion stored `Post`/`Delete`. I pointed out that this mixes with the old `POST`/`DELETE` rows, so we kept upper-case.
- Task 1: asked for a Mongo migration to generate ids. It explained that MongoDB has no migrations, so we used EF value generation instead.
- Task 3: wanted audit `Created` taken from the stored claim or cover. It showed that the time is already taken before queuing, so I tried passing the time in as a parameter, then reverted that on its advice.
- Task 1: wanted the auditing files moved into `Data`, `Repositories` and `Models`. Postponed, because the migrations depend on the `Claims.Auditing` namespace.

## What it got wrong that we had to fix
- Task 3: said the stored value change from `POST` to `Post` "doesn't matter" because each run starts a fresh database. It does matter for real data.
- Task 3: its `IAuditer` summary became wrong after my queue change. It was updated.
- Task 1: expected nullable `Id` to cause new compiler warnings. There were none.

## Check this
- Task 3: "the worker stops before the web server on shutdown" is inferred from how .NET stops hosted services, not tested.
