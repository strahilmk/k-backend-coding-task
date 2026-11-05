# Claims API

An API for creating, reading and deleting insurance claims and the covers they belong to. The task description is in [docs/README.md](docs/README.md).

## Running

Docker must be running. On startup the app starts MongoDB and SQL Server containers through Testcontainers.

```powershell
dotnet run --project Claims --launch-profile Claims
```

Swagger is at https://localhost:7052/swagger.

```powershell
dotnet test
```

## Structure

Everything is in the `Claims` project, split into folders:

| Folder | Responsibility |
| --- | --- |
| `Controllers` | HTTP only: binding requests and choosing status codes |
| `Services` | Business logic: assigning ids, computing premiums, auditing |
| `Repositories` | Reading and writing claims and covers in MongoDB |
| `Dtos` | Request and response bodies, so clients never send or see the stored models |
| `Models` | `Claim` and `Cover`, as stored in MongoDB |
| `Data` | `ClaimsContext`, the EF Core context for MongoDB. It also generates the ids |
| `Auditing` | Audit rows in SQL Server: `AuditContext`, the audit entities and `Auditer` |

Controllers depend on service interfaces, and services depend on repository interfaces and `IAuditer`. All of them are registered in `Program.cs`.

`Auditing` is kept as one folder because it is a separate store with its own database. Its namespace also must not change: the EF migrations refer to the entities by their full names.
