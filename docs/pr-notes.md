# PR notes: trade-offs and assumptions

Working notes for the PR description. Update these as decisions change.

## Dates are treated as UTC

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

## Other trade-offs

- `[BsonElement]` attributes stay on the models. This ties them to MongoDB, but avoids separate persistence models and mapping.
- Claims live in MongoDB and audits in SQL Server, so one transaction can't cover both.

## Assumptions

- Task 5: "additional" discount means 8% total for Yacht and 3% total for other types in the last period.

## Template bug found

`ClaimAudit` and `CoverAudit` had nullable properties while the migration made those columns required, which stopped the app at startup with `PendingModelChangesWarning`. Made the properties required to match the migration, because audit rows should always have these values.
