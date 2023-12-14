using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day14
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day14(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs().ToArray();
        RollNorth(input);
        _testOutputHelper.WriteLine(WeightNordBeam(input).ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs().ToArray();
        ; _testOutputHelper.WriteLine(RollAroundAndFindWeight(input, 1000000000).ToString());
    }

    private int RollAroundAndFindWeight(char[][] input, int rounds)
    {
        var comparer = new SequenceComparer();
        List<char[]> seenConfigurations = new();
        List<int> seenWeights = new();
        for (int i = 0; i < rounds; i++)
        {
            var flat = Flatten(input);
            if (seenConfigurations.Contains(flat, comparer))
            {
                var index = seenConfigurations.FindIndex(x => flat.SequenceEqual(x));
                var loopSize = i - index;
                var stepsToGo = (rounds - i) % loopSize;
                return seenWeights[index + stepsToGo];
            }
            seenConfigurations.Add(flat);
            seenWeights.Add(WeightNordBeam(input));

            RollNorth(input);
            RollWest(input);
            RollSouth(input);
            RollEast(input);
        }
        return WeightNordBeam(input);
    }

    public class SequenceComparer : IEqualityComparer<char[]>
    {
        public bool Equals(char[] x, char[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode([DisallowNull] char[] obj)
        {
            return obj.GetHashCode();
        }
    }

    private char[] Flatten(char[][] input)
        => input.SelectMany(x => x).ToArray();

    private char[][] RollNorth(char[][] input)
    {
        for (var h = 0; h < input[0].Length; h++)
        {
            for (var i = 1; i < input.Length; i++)
            {
                if (input[i][h] == 'O' && input[i - 1][h] == '.')
                {
                    var j = i - 2;
                    while (j >= 0 && input[j][h] == '.') j--;
                    input[j + 1][h] = 'O';
                    input[i][h] = '.';
                }
            }
        }
        return input;
    }

    private char[][] RollWest(char[][] input)
    {
        for (var h = 0; h < input.Length; h++)
        {
            for (var i = 1; i < input[0].Length; i++)
            {
                if (input[h][i] == 'O' && input[h][i - 1] == '.')
                {
                    var j = i - 2;
                    while (j >= 0 && input[h][j] == '.') j--;
                    input[h][j + 1] = 'O';
                    input[h][i] = '.';
                }
            }
        }
        return input;
    }

    private char[][] RollSouth(char[][] input)
    {
        for (var h = 0; h < input[0].Length; h++)
        {
            for (var i = input.Length - 2; i >= 0; i--)
            {
                if (input[i][h] == 'O' && input[i + 1][h] == '.')
                {
                    var j = i + 2;
                    while (j < input.Length && input[j][h] == '.') j++;
                    input[j - 1][h] = 'O';
                    input[i][h] = '.';
                }
            }
        }
        return input;
    }

    private char[][] RollEast(char[][] input)
    {
        for (var h = 0; h < input.Length; h++)
        {
            for (var i = input[0].Length - 2; i >= 0; i--)
            {
                if (input[h][i] == 'O' && input[h][i + 1] == '.')
                {
                    var j = i + 2;
                    while (j < input[0].Length && input[h][j] == '.') j++;
                    input[h][j - 1] = 'O';
                    input[h][i] = '.';
                }
            }
        }
        return input;
    }

    private int WeightNordBeam(char[][] input)
    {
        var weight = 0;
        for (var i = 0; i < input.Length; i++)
        {
            for (var j = 0; j < input[0].Length; j++)
            {
                if (input[i][j] == 'O')
                    weight += input.Length - i;
            }
        }
        return weight;
    }

    private char[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day14.txt");
        return lines.Select(line => line.ToCharArray()).ToArray();
    }
}
