// Version B — Parallel async with concurrency control
public async Task<string[]> DownloadAllAsync(string[] urls)
{
    using var client = new HttpClient();
    var semaphore = new SemaphoreSlim(3); // max 3 concurrent requests

    var tasks = urls.Select(async url =>
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await client.GetStringAsync(url).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    });

    return await Task.WhenAll(tasks).ConfigureAwait(false);
}
