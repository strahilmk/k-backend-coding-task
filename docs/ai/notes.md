# Notes on working with Claude Code

## What I decided
- The layering (controllers, services, repositories), and splitting POST and GET models into DTOs.
- Ids should be generated outside the services, not with `Guid.NewGuid()` in each one.
- All dates are treated as UTC.
- The validation rules and their edge cases: a period of one calendar year, both ends of a cover period included, and a 400 with my own message when a claim doesn't match its cover.
- Task 3 design: I wrote the `Channel` queue, `AuditWorker` and `docs/pr-notes.md` myself.
- Typing the queue, and using an enum for `HttpRequestType`, stored upper-case.
- Using `TimeProvider` so the StartDate tests can't fail around midnight UTC, with the rule kept in the DTO. This was for the tests only. Claude raised the risk.
- When to commit and push.

## What I used Claude for
- Moving files and fixing namespaces, and building the repository and service layers from my design. While moving the models into `Models`, it noticed that `Cover.Type` was stored as `claimType`.
- DTOs, the UTC JSON converter, the validation code and its unit tests.
- The README and code comments.
- Reviewing my Task 3 code. It found that queued entries were dropped on shutdown.
- Checking against the running app (curl, and queries against the SQL Server container) and writing commits.

## Where I rejected or corrected its output
- Put `required` on `Id` against its advice, then made `Id` nullable, because POST bodies have no id.
- Replaced its "Cover not found" message with my own wording about the claim not matching its cover.
- Its first enum conversion stored `Post`/`Delete`. I pointed out that this mixes with the old `POST`/`DELETE` rows, so we kept upper-case.
- Asked for a Mongo migration to generate ids. It explained that MongoDB has no migrations, so we used EF value generation instead.
- Wanted audit `Created` taken from the stored claim or cover. It showed that the time is already taken before queuing, so I tried passing the time in as a parameter, then reverted that on its advice.
- Wanted the auditing files moved into `Data`, `Repositories` and `Models`. Postponed, because the migrations depend on the `Claims.Auditing` namespace.

## What it got wrong that we had to fix
- Said the stored value change from `POST` to `Post` "doesn't matter" because each run starts a fresh database. It does matter for real data.
- Its `IAuditer` summary became wrong after my queue change. It was updated.
- A commit command (`git commit -F -`) failed in PowerShell 5.1. Nothing was committed, and it retried with `-m`.
- A line-ending check with `grep` wrongly reported CRLF in committed files. Re-checked with `git ls-files --eol`: all LF.
- Expected nullable `Id` to cause new compiler warnings. There were none.

## Check this
- "The worker stops before the web server on shutdown" is inferred from how .NET stops hosted services, not tested.
- The shutdown drain fix hasn't been tested with entries still in the queue.
