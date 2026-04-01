// After — Production-ready UserService
// Fixes: DI, parameterized queries, IHttpClientFactory,
//        async throughout, null guard, PasswordHash removed,
//        input validation, ILogger, System.Text.Json

public class UserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _connectionString;
    private readonly ILogger<UserService> _logger;

    // ✅ Fix #1 & #5 — DI: no hardcoded credentials, no raw HttpClient
    public UserService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<UserService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _connectionString = configuration
            .GetConnectionString("UsersDB")
            ?? throw new InvalidOperationException(
                "Connection string 'UsersDB' not found.");
        _logger = logger;
    }

    // ✅ Fix #9 — Async suffix (C# naming convention)
    public async Task<User?> GetUserWithProfileAsync(
        int userId,
        CancellationToken ct = default)
    {
        // ✅ Fix #8 — Input validation before any DB or HTTP call
        if (userId <= 0)
            throw new ArgumentException(
                "userId must be positive.", nameof(userId));

        // ✅ Fix #2 — Parameterized query, no SQL injection
        // ✅ Fix #4 — Explicit columns, PasswordHash never loaded
        const string query =
            "SELECT Id, Name, Email FROM Users WHERE Id = @UserId";

        // ✅ Fix #6 — Local connection inside using, thread-safe
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await connection.OpenAsync(ct).ConfigureAwait(false);

        // ✅ Fix #7 — ExecuteReaderAsync + ReadAsync, non-blocking
        using var reader = await command
            .ExecuteReaderAsync(ct).ConfigureAwait(false);

        User? user = null;
        if (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            user = new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email"))
            };
        }

        // ✅ Fix #8 — Null guard: don't call HTTP if user not found
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found.", userId);
            return null;
        }

        // ✅ Fix #5 — IHttpClientFactory, no socket exhaustion
        var client = _httpClientFactory.CreateClient("ProfileService");
        var response = await client
            .GetAsync($"api/profile/{user.Id}", ct)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var json = await response.Content
            .ReadAsStringAsync(ct).ConfigureAwait(false);

        // ✅ Fix #9 — System.Text.Json (built-in, faster)
        user.Profile = JsonSerializer.Deserialize<Profile>(json);
        return user;
    }

    // ✅ Fix #9 — Async suffix
    public async Task UpdateUserEmailAsync(
        int userId,
        string newEmail,
        CancellationToken ct = default)
    {
        if (userId <= 0)
            throw new ArgumentException(
                "userId must be positive.", nameof(userId));
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException(
                "newEmail cannot be empty.", nameof(newEmail));

        // ✅ Fix #3 — Parameterized UPDATE, no SQL injection
        const string query =
            "UPDATE Users SET Email = @Email WHERE Id = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = newEmail;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await connection.OpenAsync(ct).ConfigureAwait(false);

        var rows = await command
            .ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        if (rows == 0)
            _logger.LogWarning(
                "UpdateUserEmail: no rows affected for UserId {UserId}.", userId);
    }
}