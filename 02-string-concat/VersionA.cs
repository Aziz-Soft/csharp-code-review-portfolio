// Version A — Naive string concatenation in a loop (avoid in production)
public string BuildString(int n)
{
    string result = "";

    for (int i = 0; i < n; i++)
    {
        result += i.ToString();
    }

    return result;
}
