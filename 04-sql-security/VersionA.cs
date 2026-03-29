// Version A — SQL Injection vulnerability (NEVER use in production)
public List<string> GetUserOrders(string userId)
{
    var results = new List<string>();
    string query = "SELECT OrderId FROM Orders WHERE UserId = '" + userId + "'";

    using var connection = new SqlConnection(_connectionString);
    using var command = new SqlCommand(query, connection);
    connection.Open();

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        results.Add(reader["OrderId"].ToString());
    }

    return results;
}
