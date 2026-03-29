// Version B — Parameterized query (safe and correct)
public List<string> GetUserOrders(string userId)
{
    var results = new List<string>();
    const string query = "SELECT OrderId FROM Orders WHERE UserId = @UserId";

    using var connection = new SqlConnection(_connectionString);
    using var command = new SqlCommand(query, connection);
    command.Parameters.Add("@UserId", SqlDbType.NVarChar, 50).Value = userId;
    connection.Open();

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        results.Add(reader["OrderId"].ToString());
    }

    return results;
}
