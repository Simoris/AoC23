using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day12DP
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day12DP(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Task1()
    {
        var input = Day12.ReadInputs();
        _testOutputHelper.WriteLine(input.Select(x => CountPossibleFills(x.Springs, x.Counts)).Sum().ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = Day12.ReadInputs().Select(Unfold).ToArray();
        _testOutputHelper.WriteLine(input.Select(x => CountPossibleFills(x.Springs, x.Counts)).Sum().ToString());
    }

    (string Springs, int[] Counts) Unfold((string springs, int[] counts) input)
    {
        return (string.Join('?', Enumerable.Range(1, 5).Select(_ => input.springs)), Enumerable.Range(1, 5).SelectMany(_ => input.counts).ToArray());
    }

    private long CountPossibleFills(string springs, int[] counts)
    {
        var options = new long[counts.Length + 1, springs.Length + 1];

        // i = 0
        for (var j = 0; j < springs.Length; j++)
        {
            options[0, j] = springs.Substring(0, j).Contains('#') ? 0 : 1;
        }
        for (int i = 1; i <= counts.Length; i++)
        {
            // j = 0
            options[i, 0] = 0;

            for (int j = 1; j <= springs.Length; j++)
            {
                //How many ways are there for springs.Substring(0, j) and count[0..i]
                options[i, j] = (springs[j - 1]) switch
                {
                    '.' => options[i, j - 1],
                    '#' => CaseSharp(i, j),
                    '?' => options[i, j - 1] + CaseSharp(i, j),
                    _ => throw new NotImplementedException(),
                };
            }
        }
        return options[counts.Length, springs.Length];

        long CaseSharp(int i, int j)
        {
            if (j < counts[i - 1])
                return 0;
            if (springs.Substring(j - counts[i - 1], counts[i - 1]).Contains('.'))
                return 0;
            if (j > counts[i - 1] && springs[j - counts[i - 1] - 1] == '#')
                return 0;
            if (j == counts[i - 1])
            {
                if (i == 1)
                    return 1;
                return 0;
            }
            return options[i - 1, j - counts[i - 1] - 1];
        }
    }
}
