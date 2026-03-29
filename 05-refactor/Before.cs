// Before — Long method, magic numbers, no separation of concerns
public void ProcessOrder(int orderId)
{
    SqlConnection con = new SqlConnection("Server=myServer;Database=myDB;");
    con.Open();
    SqlCommand cmd = new SqlCommand(
        "SELECT * FROM Orders WHERE Id = " + orderId, con);
    SqlDataReader r = cmd.ExecuteReader();
    string status = "";
    double total = 0;
    while (r.Read())
    {
        status = r["Status"].ToString();
        total = Convert.ToDouble(r["Total"]);
    }
    con.Close();

    if (status == "1")
    {
        total = total - (total * 0.1);
        Console.WriteLine("Discount applied: " + total);
    }
    else if (status == "2")
    {
        Console.WriteLine("Order shipped: " + total);
    }
    else
    {
        Console.WriteLine("Unknown status");
    }
}
