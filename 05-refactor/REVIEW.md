## Context
Process an order by retrieving it from the database and applying
business logic based on its status.

## Before
See Before.cs

## After
See After.cs

## Verdict
**After is significantly better across all dimensions.**

## Justifications

- **Security**: Before concatenates `orderId` directly into SQL — SQL
  injection risk. After delegates to a repository with parameterized queries.

- **Separation of concerns**: Before mixes data access, business logic,
  and output in one method (violates Single Responsibility Principle).
  After isolates each: repository handles DB, method handles logic,
  logger handles output.

- **Magic numbers**: Before uses `"1"`, `"2"`, and `0.1` as unnamed
  constants — unreadable and fragile. After uses `OrderStatus` enum and
  named parameter `discountRate: 0.10m`.

- **Error handling**: Before silently produces wrong results if the order
  is not found (empty loop). After throws a descriptive exception immediately.

- **Async**: Before uses synchronous DB access, blocking the thread.
  After uses async/await with CancellationToken, suitable for production
  web applications.

- **Testability**: Before is untestable (hardcoded connection string).
  After depends on `IOrderRepository` (injected), fully mockable with xUnit.

- **Style**: After uses C# 8+ switch expression — concise, exhaustive,
  and idiomatic modern C#.

## Tests
```csharp
// Mock repository returning a discounted order
_mockRepo.Setup(r => r.GetByIdAsync(1, default))
    .ReturnsAsync(new Order { Status = OrderStatus.Discounted, Total = 100m });

await _service.ProcessOrderAsync(1);

_mockLogger.Verify(
    l => l.LogInformation(It.IsAny<string>(), 1, 90m), Times.Once);
```

## Possible Improvement
Add input validation (`orderId > 0`) and wrap in a Result<T> pattern
to avoid exception-driven flow for expected business cases.
