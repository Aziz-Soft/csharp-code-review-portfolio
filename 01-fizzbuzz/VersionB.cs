// Version B — Composable approach, no redundant check
for (int i = 1; i <= 20; i++)
{
    string result = string.Empty;
    if (i % 3 == 0) result += "Fizz";
    if (i % 5 == 0) result += "Buzz";
    if (result == string.Empty) result = i.ToString();
    Console.Write(result + " ");
}
