## Context
Download content from multiple URLs asynchronously and return all results
as a string array.

## Version A
See VersionA.cs

## Version B
See VersionB.cs

## Verdict
**Version B is better.**

## Justifications

- **Performance**: Version A awaits each URL sequentially — if each request
  takes 1 second, 10 URLs take 10 seconds total. Version B fires all tasks
  in parallel via `Task.WhenAll`, completing in ~1 second regardless of count.

- **Concurrency control**: Version B uses `SemaphoreSlim(3)` to cap
  simultaneous requests, preventing server overload or rate-limit bans.
  Version A has no parallelism at all.

- **Deadlock prevention**: Both versions use `.ConfigureAwait(false)`,
  which avoids capturing the synchronization context — critical in library
  code or ASP.NET environments where deadlocks can occur without it.

- **Resource management**: Both correctly wrap `HttpClient` in `using`.
  However, in production, `HttpClient` should be injected via
  `IHttpClientFactory` to avoid socket exhaustion.

- **Style**: Version B uses LINQ `.Select()` idiomatically for task
  projection, which is clean and readable in modern C#.

## Tests
```csharp
var urls = new[] {
    "https://example.com/1",
    "https://example.com/2",
    "https://example.com/3"
};
var results = await DownloadAllAsync(urls);
Assert.Equal(3, results.Length);
```

## Possible Improvement
Add a `CancellationToken` parameter for timeout/cancellation support:
`client.GetStringAsync(url, cancellationToken).ConfigureAwait(false)`
