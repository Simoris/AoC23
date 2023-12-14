using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day11
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day11(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }


    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var fixedInput = Expand(input);
        var distances = GetAllDistances(fixedInput, GetDistance);
        _testOutputHelper.WriteLine(distances.Sum().ToString());

        long GetDistance((int X, int Y) p1, (int X, int Y) p2)
            => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
    }

    private long[] GetAllDistances(char[][] input, Func<(int, int), (int, int), long> distanceMeasurer)
    {
        var positions = input.SelectMany((x, i) => x.Select((y, j) => (y, i, j)).Where(y => y.y == '#').Select(y => (X: y.j, Y: y.i))).ToArray();

        var distances = new List<long>();
        for (var i = 0; i < positions.Length; i++)
        {
            for (var j = 0; j < i; j++)
            {
                distances.Add(distanceMeasurer(positions[i], positions[j]));
            }
        }
        return distances.ToArray();
    }

    private char[][] Expand(char[][] input)
    {
        int[] collumnsToDouble = GetEmptyCollumns(input);

        var withDoubledCollumns = input
            .Select(x => x
                .Select(y => y.ToString())
                .Select((y, i) => collumnsToDouble.Contains(i) ? y + y : y))
            .Select(x => string.Concat(x).ToCharArray());

        var result = new List<char[]>();
        foreach (var lines in withDoubledCollumns)
        {
            result.Add(lines);
            if (lines.All(x => x == '.'))
                result.Add(lines);
        }
        return result.ToArray();
    }

    private static int[] GetEmptyCollumns(char[][] input)
        => Enumerable.Range(0, input[0].Length)
            .Where(x => input.Select(y => y[x]).All(y => y == '.'))
            .ToArray();

    private static int[] GetEmptyRows(char[][] input)
        => input.Select((x, i) => (x, i))
            .Where(x => x.x.All(y => y == '.'))
            .Select(x => x.i)
            .ToArray();

    [Fact]
    public void Task2()
    {
        const long factor = 1000000;
        var input = ReadInputs();
        var emptyCollumns = GetEmptyCollumns(input);
        var emptyRows = GetEmptyRows(input);
        var distances = GetAllDistances(input, GetDistance);
        _testOutputHelper.WriteLine(distances.Sum().ToString());

        long GetDistance((int X, int Y) p1, (int X, int Y) p2)
            => DistanceInOneDimension(p1.X, p2.X, emptyCollumns) + DistanceInOneDimension(p1.Y, p2.Y, emptyRows);

        long DistanceInOneDimension(int p1, int p2, int[] multiplied)
        {
            if (p1 == p2)
                return 0;

            var x = GetNumbersInBetween(p1, p2).ToArray();
            return x.Length + 1 + x.Intersect(multiplied).Count() * (factor - 1);
        }

        IEnumerable<int> GetNumbersInBetween(int x, int y)
            => x == y
                ? Array.Empty<int>()
                : x < y
                    ? Enumerable.Range(x + 1, y - x - 1)
                    : Enumerable.Range(y + 1, x - y - 1);
    }

    private char[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day11.txt");
        return lines.Select(x => x.ToCharArray()).ToArray();
    }
}
