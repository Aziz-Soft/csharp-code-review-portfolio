// Version A — Sequential async (slow)
public async Task<string[]> DownloadAllAsync(string[] urls)
{
    var results = new string[urls.Length];
    using var client = new HttpClient();

    for (int i = 0; i < urls.Length; i++)
    {
        results[i] = await client.GetStringAsync(urls[i]);
    }

    return results;
}
