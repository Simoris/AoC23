using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public partial class Day18
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day18(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public enum Direction
    {
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3,
    }

    private Direction ParseDirection(string c)
        => c switch
        {
            "U" => Direction.Up,
            "D" => Direction.Down,
            "L" => Direction.Left,
            "R" => Direction.Right,
            _ => throw new ArgumentException()
        };

    public enum Tile
    {
        None,
        Tunnel,
        Outer
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var corners = GetCorners(0, 0, input.Select(x => (x.Direction, x.Count)));
        var count = GetSize(corners);

        _testOutputHelper.WriteLine(count.ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs();
        var updatedSteps = input.Select(x => ParseHex(x.Color)).ToArray();
        var corners = GetCorners(0, 0, updatedSteps);
        var count = GetSize(corners);
        _testOutputHelper.WriteLine(count.ToString());

        (Direction Direction, long Count) ParseHex(string str) => (str[5] switch
        {
            '0' => Direction.Right,
            '1' => Direction.Down,
            '2' => Direction.Left,
            '3' => Direction.Up,
            _ => throw new ArgumentException()
        }, long.Parse(str[..5], System.Globalization.NumberStyles.HexNumber));
    }

    private void FloodFill(Tile[][] map, Tile fillWith, long startR, long startC)
    {
        Queue<(long R, long C)> queue = new();
        queue.Enqueue((startR, startC));
        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            try
            {
                if (map[r][c] != Tile.None)
                    continue;
                map[r][c] = fillWith;
                queue.Enqueue((r + 1, c));
                queue.Enqueue((r, c + 1));
                queue.Enqueue((r - 1, c));
                queue.Enqueue((r, c - 1));
            }
            catch (IndexOutOfRangeException) { }
        }
    }

    private (long R, long C)[] GetCorners(long startR, long startC, IEnumerable<(Direction Direction, long Count)> steps)
    {
        var corners = new List<(long, long)> { (startR, startC) };

        var r = startR;
        var c = startC;
        foreach (var step in steps)
        {
            for (long i = 0; i < step.Count; i++)
            {
                (r, c) = NeighbourInDirection(r, c, step.Direction);
            }
            corners.Add((r, c));
        }

        return corners.ToArray();
    }

    private long GetSize((long R, long C)[] corners)
    {
        var rowValues = corners.Select(x => x.R).Order().Distinct().ToArray();
        var columnValues = corners.Select(x => x.C).Order().Distinct().ToArray();
        var map = Enumerable.Range(0, 2 * rowValues.Length + 1).Select(_ => Enumerable.Range(0, 2 * columnValues.Length + 1).Select(_ => Tile.None).ToArray()).ToArray();

        foreach (var (corner, corner2) in corners.Prepend(corners.Last()).Zip(corners))
        {
            var r = BloadedIndexOf(rowValues, corner.R);
            var c = BloadedIndexOf(columnValues, corner.C);
            var r2 = BloadedIndexOf(rowValues, corner2.R);
            var c2 = BloadedIndexOf(columnValues, corner2.C);
            map[r][c] = Tile.Tunnel;
            if (r == r2)
            {
                for (long rollingC = c; rollingC != c2; rollingC += Math.Sign(c2 - c))
                    map[r][rollingC] = Tile.Tunnel;
            }
            else
            {
                for (long rollingR = r; rollingR != r2; rollingR += Math.Sign(r2 - r))
                    map[rollingR][c] = Tile.Tunnel;
            }
        }

        FloodFill(map, Tile.Outer, 0, 0);

        return map.SelectMany((x, i) => x.Select((y, j) => y is not Tile.Outer ? SizeOfTile(i, j) : 0)).Sum();

        long SizeOfTile(long r, long c)
        {
            var rLength = r % 2 == 1 ? 1 : rowValues[r / 2] - rowValues[r / 2 - 1] - 1;
            var cLength = c % 2 == 1 ? 1 : columnValues[c / 2] - columnValues[c / 2 - 1] - 1;
            return rLength * cLength;
        }

        long BloadedIndexOf(long[] array, long value)
            => 2 * IndexOf(array, value) + 1;
        long IndexOf(long[] array, long value)
            => array.Select((x, i) => (x, i)).Single(x => x.x == value).i;
    }

    private (long R, long C) NeighbourInDirection(long R, long C, Direction direction)
        => direction switch
        {
            Direction.Left => (R, C - 1),
            Direction.Right => (R, C + 1),
            Direction.Up => (R - 1, C),
            Direction.Down => (R + 1, C),
            _ => throw new NotImplementedException(),
        };

    [GeneratedRegex("(\\w) (\\d+) \\(#(\\w+)\\)")]
    private static partial Regex DecomposeInput();

    private (Direction Direction, long Count, string Color)[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day18.txt");
        return lines.Select(x => DecomposeInput().Match(x)).Select(x => (ParseDirection(x.Groups[1].Value), long.Parse(x.Groups[2].Value), x.Groups[3].Value)).ToArray();
    }
}
