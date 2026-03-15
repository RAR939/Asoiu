using System;

class Program
{
    static int Distance(string s1, string s2)
    {
        int m = s1.Length;
        int n = s2.Length;

        int[,] d = new int[m + 1, n + 1];

        for (int i = 0; i <= m; i++)
            d[i, 0] = i;

        for (int j = 0; j <= n; j++)
            d[0, j] = j;

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                int insert = d[i, j - 1] + 1;
                int delete = d[i - 1, j] + 1;
                int replace = d[i - 1, j - 1] + cost;

                d[i, j] = Math.Min(Math.Min(insert, delete), replace);
            }
        }

        return d[m, n];
    }

    static void Main()
    {
        while (true)
        {
            Console.Write("Введите первую строку: ");
            string s1 = Console.ReadLine();

            if (s1 == "exit")
                break;

            Console.Write("Введите вторую строку: ");
            string s2 = Console.ReadLine();

            int result = Distance(s1, s2);

            Console.WriteLine($"Расстояние: {result}\n");
        }
    }
}