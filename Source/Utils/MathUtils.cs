namespace AoC23.Utils;

public static class MathUtils
{
    public static long GCF(long a, long b)
    {
        while (b != 0)
        {
            long temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public static long LCM(long a, long b)
    {
        return (a / GCF(a, b)) * b;
    }

    public static int NCR(int n, int k)
    {
        int result = 1;
        for (var i = n; i > n - k; i--)
            result *= i;
        for (var i = 1; i <= k; i++)
            result /= i;
        return result;
    }
}
