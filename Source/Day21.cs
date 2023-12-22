using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public partial class Day21
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day21(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        Move(input, 64);

        _testOutputHelper.WriteLine(input.SelectMany(x => x.Where(y => y == 'E')).Count().ToString());
    }

    private void Move(char[][] matrix, int steps)
    {
        for (int i = 1; i <= steps; i++)
        {
            var reached = new List<(int R, int C)>();
            for (var r = 0; r < matrix.Length; r++)
                for (var c = 0; c < matrix[0].Length; c++)
                {
                    if (matrix[r][c] != '.')
                        continue;
                    if (SaveCheckforReached(r - 1, c) || SaveCheckforReached(r + 1, c) || SaveCheckforReached(r, c - 1) || SaveCheckforReached(r, c + 1))
                        reached.Add((r, c));
                }

            char fillWith = i % 2 == 0 ? 'E' : 'O';
            foreach (var (r, c) in reached)
                matrix[r][c] = fillWith;
        }

        bool SaveCheckforReached(int r, int c)
        {
            try
            {
                return matrix[r][c] is 'E' or 'O';
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
    }

    private char[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day21.txt");
        return lines.Select(x => x.ToCharArray()).ToArray();
    }
}
