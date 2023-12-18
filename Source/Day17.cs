using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day17
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day17(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record DijkstraTile(int OwnValue)
    {
        public List<DijkstraApporach> Values { get; init; } = [];
        public int MinDistance => Values.Count == 0 ? int.MinValue : Values.Select(x => x.Distance).Min();
        public int AddIfUsefull(DijkstraApporach newValue)
        {
            if (newValue.Distance < MinDistance)
                throw new Exception();
            if (Values.Any(x => x.Direction == newValue.Direction && x.StepsInDirection == newValue.StepsInDirection))
                return -1;
            Values.Add(newValue);
            return Values.Count - 1;
        }
    }

    public record DijkstraApporach(Direction Direction, int StepsInDirection, int Distance, string Path);

    public enum Direction
    {
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3,
    }

    private Direction InvertDirection(Direction direction)
        => (Direction)(((int)direction + 2) % 4);

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var value = Dijkstra(input, input.Length - 1, input[0].Length - 1, 1, 3);
        _testOutputHelper.WriteLine(value.ToString());
    }
    [Fact]
    public void Task2()
    {
        var input = ReadInputs();
        var value = Dijkstra(input, input.Length - 1, input[0].Length - 1, 4, 10);
        _testOutputHelper.WriteLine(value.ToString());
    }

    private int Dijkstra(int[][] values, int targetR, int targetC, int minStepSize, int maxStepSize, bool printPath = false)
    {
        var tiles = values.Select(x => x.Select(y => new DijkstraTile(y)).ToArray()).ToArray();
        var queue = new PriorityQueue<(int R, int C, int Index), int>();
        tiles[0][0].Values.Add(new(Direction.Down, 0, 0, ""));
        tiles[0][0].Values.Add(new(Direction.Right, 0, 0, ""));
        queue.Enqueue((0, 0, 0), 0);
        queue.Enqueue((0, 0, 1), 0);
        while (queue.Count > 0)
        {
            var elem = queue.Dequeue();
            var item = tiles[elem.R][elem.C].Values[elem.Index];
            if (elem.R == targetR && elem.C == targetC && item.StepsInDirection >= minStepSize)
            {
                _testOutputHelper.WriteLine(item.Path);
                return item.Distance;
            }

            if (item.StepsInDirection < minStepSize)
            {
                CheckNext(elem.R, elem.C, item.Direction, item);
            }
            else
            {
                foreach (var newDirection in Enum.GetValues<Direction>().Except([InvertDirection(item.Direction)]))
                {
                    CheckNext(elem.R, elem.C, newDirection, item);
                }
            }
        }
        throw new Exception();

        void CheckNext(int oldR, int oldC, Direction newDirection, DijkstraApporach item)
        {
            var nextCoord = NeighbourInDirection(oldR, oldC, newDirection);
            try
            {
                var nextTile = tiles[nextCoord.R][nextCoord.C];
                var newStepSize = newDirection == item.Direction ? item.StepsInDirection + 1 : 1;
                if (newStepSize > maxStepSize)
                    return;
                var index = nextTile.AddIfUsefull(new(newDirection, newStepSize, item.Distance + nextTile.OwnValue, printPath ? item.Path + nextCoord : item.Path));
                if (index != -1)
                {
                    queue.Enqueue((nextCoord.R, nextCoord.C, index), item.Distance + nextTile.OwnValue);
                }
            }
            catch (IndexOutOfRangeException) { }
        }
    }

    private (int R, int C) NeighbourInDirection(int R, int C, Direction direction)
        => direction switch
        {
            Direction.Left => (R, C - 1),
            Direction.Right => (R, C + 1),
            Direction.Up => (R - 1, C),
            Direction.Down => (R + 1, C),
            _ => throw new NotImplementedException(),
        };

    private int[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day17.txt");
        return lines.Select(x => x.Select(y => int.Parse(y.ToString())).ToArray()).ToArray();
    }
}
