using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day09
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day09(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record NextPositions(string Left, string Right);
    public record MovingPosition(string Position, long Moves, long StepSize)
    {
        public MovingPosition() : this("", long.MaxValue, 0) { }
        public MovingPosition Step()
            => new(Position, Moves + StepSize, StepSize);
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var sum = input.Select(PredictNext).Sum();
        _testOutputHelper.WriteLine(sum.ToString());
    }

    private int PredictNext(int[] sequence)
    {
        if (sequence.Length <= 1) throw new NotSupportedException();

        if (sequence.Distinct().Count() == 1)
            return sequence[0];

        var dif = PredictNext(sequence.Skip(1).Zip(sequence).Select(x => x.First - x.Second).ToArray());
        return sequence.Last() + dif;
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs();
        var sum = input.Select(PredictPrevious).Sum();
        _testOutputHelper.WriteLine(sum.ToString());
    }

    private int PredictPrevious(int[] sequence)
    {
        if (sequence.Length <= 1) throw new NotSupportedException();

        if (sequence.Distinct().Count() == 1)
            return sequence[0];

        var dif = PredictPrevious(sequence.Skip(1).Zip(sequence).Select(x => x.First - x.Second).ToArray());
        return sequence.First() - dif;
    }

    private int[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day09.txt");
        return lines.Select(x => x.Split(' ').Select(y => int.Parse(y)).ToArray()).ToArray();
    }
}
