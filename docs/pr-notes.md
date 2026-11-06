# PR notes: trade-offs and assumptions

Working notes for the PR description. Update these as decisions change.

## Task 2: dates are treated as UTC

`UtcDateTimeConverter` treats a request date with no time zone as UTC and converts a date with an offset to UTC. The original offset is not kept.

- Why: MongoDB stores dates as UTC, and the driver read dates with no zone as local time, so they came back shifted.
- Cost: we can't tell which time zone a client meant. "Today" in the StartDate rule is today in UTC, so a client far from UTC can see a date rejected, or accepted, around midnight local time.
- Alternative: store the offset or time zone alongside the date. That adds a field and more logic for no need in this task.
- Known gap: query-string dates on `POST /Covers/compute` don't go through the converter.

## Task 3: in-memory audit queue

A bounded `Channel` (capacity 1000) with one `BackgroundService`. The request queues the audit entry with `TryWrite` and returns, and the worker writes it to SQL Server.

- Why: simple, no new infrastructure, and the task allows in-memory.
- Crash: a crash or killed process loses entries still in the queue. A clean shutdown stops accepting entries and writes what's queued.
- Shutdown order: the worker stops before the web server, so a request still in flight during shutdown finds the queue closed and its audit entry is dropped and logged.
- Full queue: entries are dropped and logged instead of making the request wait. This favours API availability over a complete audit trail. `TryWrite` over `WriteAsync` keeps requests from blocking when the queue is full.
- Failed writes: logged, not retried.
- Capacity: 1000 is a judgment call, not a measured number. It only has to absorb a short SQL Server outage.
- One worker: enough for this task. If audit volume outgrew it, I'd batch writes first, then add workers.
- Alternatives:
  - Outbox: save the audit message with the claim and have a worker move it to SQL Server. It survives restarts, but needs a new collection, polling and clean-up, and an extra write per request.
  - Message broker such as Azure Service Bus: durable, but adds infrastructure.

## Task 4: TimeProvider, only so tests are stable

The StartDate rule reads "today" from `TimeProvider` instead of `DateTime.UtcNow`. This is only so tests don't fail at random, and it changes nothing in how the app behaves.

- Why: the tests and `Validate` each read the real clock at slightly different moments. A test run that crossed midnight UTC could fail once and then pass on a rerun, and that kind of failure wastes time chasing something that isn't a bug. With a fixed clock, both see the same "today".
- In the app: `Program.cs` registers `TimeProvider.System`, so it is still the real clock.
- How: `Validate` gets `TimeProvider` from the `ValidationContext`, so the rule stays in the DTO. Getting a service inside a DTO is a small service locator, accepted to keep the rule where it is.

## Other trade-offs

- Task 1: `[BsonElement]` attributes stay on the models. This ties them to MongoDB, but avoids separate persistence models and mapping.
- Task 3: claims live in MongoDB and audits in SQL Server, so one transaction can't cover both.

## CI, and how I'd deploy to Azure

CI: `.github/workflows/ci.yml` builds the solution and runs every test, including the integration tests, on each pull request into `main`. GitHub's Ubuntu runners have Docker, so Testcontainers works there as it does locally.

Deploying to Azure is optional in the spec and not done. The plan I'd discuss:

- `Program.cs`: keep Testcontainers for Development and tests, and read connection strings from configuration in every other environment. This is the rewrite the spec warns about.
- Hosting: App Service on Linux. .NET 10 support there needs checking first.
- SQL: Azure SQL Database. The existing `Database.Migrate()` at startup creates the audit tables.
- MongoDB: MongoDB Atlas hosted in an Azure region, or Azure Cosmos DB for MongoDB vCore. Not checked: whether the MongoDB EF Core provider works with Cosmos DB, so Atlas is the safer choice.
- Secrets: connection strings in App Service settings or Key Vault, never in the repo.
- CD: a second job that publishes the app and deploys it with `azure/webapps-deploy`. It signs in to Azure with OIDC (a federated credential), so there's no stored secret.

## Assumptions

- Task 5: "additional" discount means 8% total for Yacht and 3% total for other types in the last period.

## Open questions to discuss

- Task 5: should a partial day be charged? Today only whole days count, so 30.5 days cost the same as 30, and on `POST /Covers/compute` a period shorter than one day (for example 12 hours) costs 0. The original code charged a partial day as a full day. The spec doesn't say, so this needs discussing: charge a partial day as a full day, charge it pro rata, or keep not charging it.
- Task 2: `DamageCost` is only capped per claim (100,000). It isn't compared with the cover, because a cover has no insured amount or limit to compare it against. Rules such as "the total of a cover's claims can't exceed its limit" would need a new field and a rule the spec doesn't give.
- Task 1: deleting a cover doesn't touch its claims. They stay, with a `CoverId` that no longer matches a cover, and can still be read. Options: reject deleting a cover that has claims, delete its claims with it, or keep the claims as history.

## Template bug found before Task 1

`ClaimAudit` and `CoverAudit` had nullable properties while the migration made those columns required, which stopped the app at startup with `PendingModelChangesWarning`. Made the properties required to match the migration, because audit rows should always have these values.
