using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day13
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day13(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs().ToArray();
        var horizontalLines = input.Select(FindHorizontalMirrorLine).ToArray();
        var verticalLines = input.Select(Rotate).Select(FindHorizontalMirrorLine).ToArray();
        _testOutputHelper.WriteLine((horizontalLines.Sum() * 100 + verticalLines.Sum()).ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs().ToArray();
        var horizontalLines = input.Select(FindDifferentHorizontalLine).ToArray();
        var verticalLines = input.Select(Rotate).Select(FindDifferentHorizontalLine).ToArray();
        _testOutputHelper.WriteLine((horizontalLines.Sum() * 100 + verticalLines.Sum()).ToString());
    }

    private int? FindDifferentHorizontalLine(char[][] matrix)
    {
        var old = FindHorizontalMirrorLine(matrix);
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[0].Length; j++)
            {
                matrix[i][j] = Invert(matrix[i][j]);
                var line = FindHorizontalMirrorLineWithIgnore(matrix, old);
                matrix[i][j] = Invert(matrix[i][j]);
                if (line != null)
                    return line;
            }
        }
        return null;
    }

    private char Invert(char c)
        => c == '.' ? '#' : '.';

    private char[][] Rotate(char[][] matrix)
    {
        return matrix[0].Select((_, i) => matrix.Select(x => x[i]).ToArray()).ToArray();
    }

    private int? FindHorizontalMirrorLine(char[][] matrix) //Not as optional parameter to be usable directly as lambda
        => FindHorizontalMirrorLineWithIgnore(matrix, null);
    private int? FindHorizontalMirrorLineWithIgnore(char[][] matrix, int? ignoreSolution)
    {
        for (int i = 0; i < matrix.Length - 1; i++)
        {
            if (i + 1 == ignoreSolution)
                continue;
            for (var j = 0; true; j++)
            {
                if (j > i || i + j + 1 >= matrix.Length)
                {
                    return i + 1;
                }
                if (!matrix[i - j].SequenceEqual(matrix[i + j + 1]))
                {
                    break;
                }
            }
        }
        return null;
    }

    private IEnumerable<char[][]> ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day13.txt");
        var matrix = new List<char[]>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                yield return matrix.ToArray();
                matrix = new();
            }
            else
            {
                matrix.Add(line.ToCharArray());
            }
        }
        yield return matrix.ToArray();
    }
}
