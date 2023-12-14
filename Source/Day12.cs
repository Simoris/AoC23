using System.Collections.Concurrent;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day12
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day12(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();

        _testOutputHelper.WriteLine(input.Select(x => CountPossibleFills(x.Springs, x.Counts)).Sum().ToString());
    }

    [Fact(Skip = "Unfeasably slow")]
    public async Task Task2()
    {

        var input = ReadInputs().Select(Unfold).ToArray();
        var alreadySolved = File.ReadAllLines("results.txt").Select(x => x.Split(": ")).ToDictionary(x => int.Parse(x[0]), x => long.Parse(x[1]));
        var sempahore = new SemaphoreSlim(1);
        var tasks = input.Select((x, i) => Task.Run(async () =>
        {
            if (alreadySolved.ContainsKey(i))
            {
                return alreadySolved[i];
            }
            var result = CountPossibleFills(x.Springs, x.Counts);
            await sempahore.WaitAsync();
            try
            {
                File.AppendAllText("results.txt", $"{i}: {result}\r\n");
            }
            finally
            {
                sempahore.Release();
            }
            return result;
        }));
        var nums = await Task.WhenAll(tasks).ConfigureAwait(false);
        _testOutputHelper.WriteLine(nums.Sum().ToString());
    }

    (string Springs, int[] Counts) Unfold((string springs, int[] counts) input)
    {
        return (string.Join('?', Enumerable.Range(1, 5).Select(_ => input.springs)), Enumerable.Range(1, 5).SelectMany(_ => input.counts).ToArray());
    }


    const int maxCacheLength = 12;
    private ConcurrentDictionary<(string, int[]), long> _cache = new();

    private long CountPossibleFills(string springs, int[] counts)
    {
        return IterateCached(springs, counts);

        long IterateCached(string currentArr, int[] expectedCounts)
        {
            if (currentArr.Length > maxCacheLength)
                return Iterate(currentArr, expectedCounts);
            if (!_cache.ContainsKey((currentArr, expectedCounts)))
            {
                _cache[(currentArr, expectedCounts)] = Iterate(currentArr, expectedCounts);
            }
            return _cache[(currentArr, expectedCounts)];
        }

        long Iterate(string currentArr, int[] expectedCounts)
        {
            var trimmed = currentArr.Trim('.');
            if (!trimmed.Contains('?'))
            {
                // Done
                var runs = GetRuns(trimmed);
                if (runs.SequenceEqual(expectedCounts))
                {
                    return 1;
                }
                return 0;
            }

            if (expectedCounts.Length == 0)
            {
                if (!trimmed.Contains('#'))
                    return 1;
                return 0;
            }

            var numWildcard = trimmed.Count(x => x == '?');
            var numSharp = trimmed.Count(x => x == '#');
            if (numSharp > expectedCounts.Sum() || expectedCounts.Sum() > numSharp + numWildcard)
                return 0; //impossible -> early stop

            var firstWildcard = trimmed.IndexOf('?');
            var firstPoint = trimmed.IndexOf('.');
            if (firstPoint != -1 && firstPoint < firstWildcard)
            {
                if (expectedCounts.Length == 0 || firstPoint != expectedCounts.First())
                {
                    //Does not fit
                    return 0;
                }
                else
                {
                    return IterateCached(trimmed.Substring(firstPoint), expectedCounts.Skip(1).ToArray());
                }
            }
            else
            {
                StringBuilder sb = new(trimmed);
                if (firstWildcard == 0)
                {
                    long innerCount = 0;
                    sb[firstWildcard] = '.';
                    innerCount += IterateCached(sb.ToString(), expectedCounts);
                    sb[firstWildcard] = '#';
                    innerCount += IterateCached(sb.ToString(), expectedCounts);
                    return innerCount;
                }
                else
                {
                    if (firstWildcard == expectedCounts.First())
                    {
                        sb[firstWildcard] = '.';
                    }
                    else if (firstWildcard < expectedCounts.First())
                    {
                        sb[firstWildcard] = '#';
                    }
                    else
                    {
                        return 0;
                    }
                    return IterateCached(sb.ToString(), expectedCounts);
                }
            }
        }
    }

    private static int[] GetRuns(string input)
    {
        var result = new List<int>();
        var current = 0;
        foreach (var c in input)
        {
            if (c == '#')
            {
                current++;
            }
            else
            {
                if (current > 0)
                {
                    result.Add(current);
                    current = 0;
                }
            }
        }
        if (current > 0)
        {
            result.Add(current);
        }

        return result.ToArray();
    }

    internal static (string Springs, int[] Counts)[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day12.txt");
        return lines.Select(x =>
        {
            var bits = x.Split(' ');
            return (bits[0], bits[1].Split(',').Select(int.Parse).ToArray());
        }).ToArray();
    }
}
