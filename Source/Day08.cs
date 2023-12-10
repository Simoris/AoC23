using AoC23.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day08
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day08(ITestOutputHelper testOutputHelper)
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

        var currentPosition = "AAA";
        var moves = 0;
        foreach (var move in input.Strategy)
        {
            moves++;
            currentPosition = move(input.Positions[currentPosition]);
            if (currentPosition == "ZZZ")
                break;
        }

        _testOutputHelper.WriteLine(moves.ToString());
    }

    [Fact(Skip = "Slow")]
    public void Task2Slow()
    {
        var input = ReadInputs();
        var startPositions = input.Positions.Keys.Where(x => x[2] == 'A').ToArray();
        var goalPositions = input.Positions.Keys.Where(x => x[2] == 'Z').ToArray();

        var zMapper = goalPositions.ToDictionary(x => x, x =>
        {
            long moves = 0;
            var endPosition = x;
            foreach (var move in input.Strategy)
            {
                moves++;
                endPosition = move(input.Positions[endPosition]);
                if (endPosition[2] == 'Z')
                    break;
            }
            return (endPosition, moves);
        });

        var positions = startPositions.Select(x =>
        {
            long moves = 0;
            var endPosition = x;
            foreach (var move in input.Strategy)
            {
                moves++;
                endPosition = move(input.Positions[endPosition]);
                if (endPosition[2] == 'Z')
                    break;
            }
            return (Position: endPosition, Moves: moves);
        }).ToArray();

        var positionsWithIndex = positions.Select((x, i) => (x.Position, x.Moves, Index: i)).ToArray();
        while (positionsWithIndex.Select(x => x.Moves).Distinct().Count() > 1)
        {
            var minMoves = positionsWithIndex.Aggregate((Position: "", Moves: long.MaxValue, Index: -1), (x, y) => x.Moves < y.Moves ? x : y);
            positionsWithIndex[minMoves.Index] = (zMapper[minMoves.Position].endPosition, minMoves.Moves + zMapper[minMoves.Position].moves, minMoves.Index);
        }
        _testOutputHelper.WriteLine(positionsWithIndex[0].Moves.ToString());
    }

    [Fact]
    public async Task Task2Optimized()
    {
        var (strategy, positions) = ReadInputs();

        var zMapper = positions.Keys.Where(x => x[2] == 'Z').ToDictionary(x => x, x =>
        {
            long moves = 0;
            var endPosition = x;
            foreach (var move in strategy)
            {
                moves++;
                endPosition = move(positions[endPosition]);
                if (endPosition[2] == 'Z')
                    break;
            }
            return (endPosition, moves);
        });

        // Prerequisite for optimization
        zMapper.Should().AllSatisfy(x => x.Key.Should().Be(x.Value.endPosition));

        var startPositions = positions.Keys.Where(x => x[2] == 'A').Select(x =>
        {
            long moves = 0;
            var endPosition = x;
            foreach (var move in strategy)
            {
                moves++;
                endPosition = move(positions[endPosition]);
                if (endPosition[2] == 'Z')
                    break;
            }
            return new MovingPosition(endPosition, moves, zMapper[endPosition].moves);
        }).ToArray();

        var task1 = Task.Run(() => CombinePositions(startPositions.Take(3).ToArray()));
        var task2 = Task.Run(() => CombinePositions(startPositions.Skip(3).ToArray()));

        var result = CombinePositions(await task1, await task2);
        _testOutputHelper.WriteLine(result.Moves.ToString());
    }

    private MovingPosition CombinePositions(params MovingPosition[] positions)
    {
        var positionsWithIndex = positions.Select((x, i) => (Position: x, Index: i)).ToArray();
        while (positionsWithIndex.Select(x => x.Position.Moves).Distinct().Count() == positions.Length)
        {
            var minMoves = positionsWithIndex.Aggregate((Position: new MovingPosition(), Index: -1), (x, y) => x.Position.Moves < y.Position.Moves ? x : y);
            positionsWithIndex[minMoves.Index].Position = minMoves.Position.Step();
        }

        var positionGrouping = positionsWithIndex.GroupBy(x => x.Position.Moves).OrderByDescending(x => x.Count()).ToArray();
        var sharedPosition = positionGrouping.First();
        var combined = new MovingPosition(string.Join('+', sharedPosition.Select(x => x.Position.Position)), sharedPosition.Key, MathUtils.LCM(sharedPosition.First().Position.Moves, sharedPosition.Last().Position.Moves));
        if (positions.Length == 2)
            return combined;

        return CombinePositions(positionGrouping.Skip(1).SelectMany(x => x.Select(y => y.Position)).Append(combined).ToArray());
    }


    private (IEnumerable<Func<NextPositions, string>> Strategy, Dictionary<string, NextPositions> Positions) ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day08.txt");
        var strategy = lines[0];
        IEnumerable<Func<NextPositions, string>> NextMove()
        {
            while (true) foreach (var move in strategy) yield return n => move == 'L' ? n.Left : n.Right;
        }

        var positions = lines.Skip(2).ToDictionary(x => x.Substring(0, 3), x => new NextPositions(x.Substring(7, 3), x.Substring(12, 3)));
        return (NextMove(), positions);
    }
}
