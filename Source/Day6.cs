using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day6
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day6(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Race(long Duration, long Record);

    [Fact]
    public void Task1()
    {
        var races = ReadInputs();
        var boundaries = races.Select(BeatsRecordBorders);
        var result = boundaries.Select(x => x.High - x.Low + 1).Aggregate((x, y) => x * y);
        _testOutputHelper.WriteLine(result.ToString());
    }

    private (long Low, long High) BeatsRecordBorders(Race race)
    {
        var minusPHalfs = ((double)race.Duration) / 2;
        double root = Math.Sqrt(minusPHalfs * minusPHalfs - race.Record);
        var xMin = minusPHalfs - root;
        var xMax = minusPHalfs + root;
        var cappedXMax = (long)xMax;
        return ((long)xMin + 1, cappedXMax == xMax ? cappedXMax - 1 : cappedXMax);
    }

    [Fact]
    public void Task2()
    {
        var races = ReadInputs();
        var duration = long.Parse(string.Concat(races.Select(x => x.Duration.ToString())));
        var record = long.Parse(string.Concat(races.Select(x => x.Record.ToString())));
        var result = BeatsRecordBorders(new(duration, record));
        _testOutputHelper.WriteLine((result.High - result.Low + 1).ToString());
    }

    private Race[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day6.txt");
        var times = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(long.Parse);
        var records = lines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(long.Parse);
        return times.Zip(records).Select(x => new Race(x.First, x.Second)).ToArray();
    }
}
