using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day03
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day03(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record PartNumber(int Number, char Symbols);
    public record StarNumber(int Number, (int X, int Y)? StarPosition);

    [Fact]
    public void Task1()
    {
        var partNumbers = ReadInputs();
        var result = partNumbers.Where(x => x.Symbols != '.').Sum(x => x.Number);
        _testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public void Task2()
    {
        var starPositions = ReadInputs2();
        var stars = starPositions.Where(x => x.StarPosition.HasValue).ToLookup(x => x.StarPosition!.Value, x => x.Number);
        var starSum = stars.Where(x => x.Count() == 2).Select(x => x.First() * x.Last()).Sum();
        _testOutputHelper.WriteLine(starSum.ToString());
    }

    private static PartNumber[] ReadInputs()
    {
        var matrix = File.ReadAllLines("Inputs/Day03.txt").Select(x => x.ToCharArray()).ToArray();
        var dottedMatrix = matrix.Select(x => x.Select(y => char.IsDigit(y) ? '.' : y).ToArray()).ToArray();

        var width = matrix[0].Length;
        var height = matrix.Length;
        List<PartNumber> partNumbers = new();
        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                if (!char.IsDigit(matrix[i][j]))
                    continue;

                var oldJ = j;
                while (j + 1 < width && char.IsDigit(matrix[i][j + 1])) j++;
                var number = int.Parse(matrix[i][oldJ..(j + 1)]);
                partNumbers.Add(new(number, SearchPartSymbol(i, oldJ, j)));
            }
        }
        return partNumbers.ToArray();

        char SearchPartSymbol(int i, int j0, int j1)
        {
            var i0 = i > 0 ? i - 1 : i;
            var i1 = i < height - 1 ? i + 1 : i;
            j0 = j0 > 0 ? j0 - 1 : j0;
            j1 = j1 < width - 1 ? j1 + 1 : j1;
            return dottedMatrix[i0..(i1 + 1)]
                .SelectMany(x => x[j0..(j1 + 1)])
                .FirstOrDefault(x => x != '.', '.');
        }
    }

    private static StarNumber[] ReadInputs2()
    {
        var matrix = File.ReadAllLines("Inputs/Day03.txt").Select(x => x.ToCharArray()).ToArray();
        var dottedMatrix = matrix.Select(x => x.Select(y => char.IsDigit(y) ? '.' : y).ToArray()).ToArray();

        var width = matrix[0].Length;
        var height = matrix.Length;
        List<StarNumber> starNumbers = new();
        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                if (!char.IsDigit(matrix[i][j]))
                    continue;

                var oldJ = j;
                while (j + 1 < width && char.IsDigit(matrix[i][j + 1])) j++;
                var number = int.Parse(matrix[i].AsSpan()[oldJ..(j + 1)]);
                starNumbers.Add(new(number, SearchStarPosition(i, oldJ, j)));
            }
        }
        return starNumbers.ToArray();

        (int X, int Y)? SearchStarPosition(int i, int j0, int j1)
        {
            var i0 = i > 0 ? i - 1 : i;
            var i1 = i < height - 1 ? i + 1 : i;
            j0 = j0 > 0 ? j0 - 1 : j0;
            j1 = j1 < width - 1 ? j1 + 1 : j1;
            return dottedMatrix[i0..(i1 + 1)]
                .Select<char[], (int, int)?>((x, index) => (index + i0, Array.IndexOf(x[j0..(j1 + 1)], '*') + j0))
                .FirstOrDefault(x => x!.Value.Item2 != j0 - 1, null);
        }
    }
}
