// Before — AI-generated, defective UserService
// Issues: hardcoded credentials, SQL injection (x2), PasswordHash exposed,
//         new HttpClient() per call, shared SqlConnection, sync reads, null risk

public class UserService
{
    // ⚠ Issue #1 — SECURITY: credentials hardcoded in source code
    SqlConnection connection = new SqlConnection(
        "Server=prod-server;Database=UsersDB;User=admin;Password=Admin1234;");

    public async Task<User> GetUserWithProfile(int userId)
    {
        // ⚠ Issue #2 — SECURITY: SQL injection via string concatenation
        string query = "SELECT * FROM Users WHERE Id = " + userId;

        connection.Open(); // ⚠ Issue #6 — PERFORMANCE: shared connection, not thread-safe

        SqlCommand cmd = new SqlCommand(query, connection);

        // ⚠ Issue #7 — LOGIC: synchronous Read() inside async method blocks thread
        SqlDataReader reader = cmd.ExecuteReader();

        User user = null;
        while (reader.Read())
        {
            user = new User();
            user.Id = (int)reader["Id"];
            user.Name = reader["Name"].ToString();
            user.Email = reader["Email"].ToString();
            user.PasswordHash = reader["PasswordHash"].ToString(); // ⚠ Issue #4 — SECURITY
        }

        // ⚠ Issue #5 — PERFORMANCE: new HttpClient() per call = socket exhaustion
        HttpClient client = new HttpClient();

        // ⚠ Issue #8 — LOGIC: NullReferenceException if no row returned
        var response = await client.GetAsync(
            "https://profile-service/api/profile/" + user.Id);

        string json = await response.Content.ReadAsStringAsync();

        // ⚠ Issue #9 — STYLE: Newtonsoft.Json instead of System.Text.Json
        user.Profile = JsonConvert.DeserializeObject<Profile>(json);

        return user;
    }

    public async Task UpdateUserEmail(int userId, string newEmail)
    {
        // ⚠ Issue #3 — SECURITY: SQL injection in UPDATE
        string query = "UPDATE Users SET Email = '" + newEmail +
                       "' WHERE Id = " + userId;

        SqlCommand cmd = new SqlCommand(query, connection);
        cmd.ExecuteNonQuery(); // ⚠ Issue #9 — STYLE: sync inside async
        connection.Close();
    }
}