## Context
Retrieve all orders for a given user from a SQL Server database.

## Version A
See VersionA.cs

## Version B
See VersionB.cs

## Verdict
**Version B is better. Version A is a critical security vulnerability.**

## Justifications

- **Security (Critical)**: Version A concatenates user input directly into
  the SQL string. An attacker passing `' OR '1'='1` as userId would return
  ALL orders from the database. Worse, `'; DROP TABLE Orders; --` would
  destroy data. This is a textbook SQL Injection (OWASP Top 10).

- **Parameterization**: Version B uses `@UserId` as a named parameter.
  The database engine treats it as data, never as executable SQL.
  Injection is structurally impossible.

- **Type safety**: Version B explicitly declares `SqlDbType.NVarChar, 50`,
  enforcing the column type and max length at the application layer.

- **Performance**: Parameterized queries allow the SQL Server query planner
  to cache and reuse execution plans, improving throughput under load.

- **Style**: `const string query` signals that the SQL template never
  changes at runtime — clearer intent, easier to audit.

## Tests
```csharp
// Safe input
var orders = GetUserOrders("user123");
Assert.NotEmpty(orders);

// Injection attempt — should return empty, not all orders
var malicious = GetUserOrders("' OR '1'='1");
Assert.Empty(malicious);
```

## Possible Improvement
For larger projects, prefer Entity Framework Core or Dapper with
parameterized queries to abstract raw ADO.NET and reduce boilerplate.
