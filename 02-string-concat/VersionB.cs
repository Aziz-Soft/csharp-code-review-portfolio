// Version B — StringBuilder with pre-allocated capacity
public string BuildString(int n)
{
    var sb = new StringBuilder(n * 4); // estimated capacity

    for (int i = 0; i < n; i++)
    {
        sb.Append(i);
    }

    return sb.ToString();
}
