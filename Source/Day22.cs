using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public partial class Day22
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day22(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Coordinate(int X, int Y, int Z)
    {
        public Coordinate Drop() => this with { Z = Z - 1 };
    }
    public record Brick(Coordinate P1, Coordinate P2)
    {
        public Brick Drop()
            => new(P1.Drop(), P2.Drop());

        public IEnumerable<Coordinate> AllCoordinates()
        {
            var length = Math.Abs(P1.X + P1.Y + P1.Z - P2.X - P2.Y - P2.Z);
            if (length == 0)
                return [P1];
            return Enumerable.Range(0, length + 1).Select(x => new Coordinate(Mix(P1.X, P2.X, x), Mix(P1.Y, P2.Y, x), Mix(P1.Z, P2.Z, x)));

            int Mix(int x1, int x2, int progress)
                => (x1 * progress + x2 * (length - progress)) / length;
        }

        public bool Hits(Brick other)
            => this.AllCoordinates().Intersect(other.AllCoordinates()).Any();

        public bool HitsAny(IEnumerable<Brick> others)
            => others.Any(Hits);

        public IEnumerable<T> RestsOn<T>(IEnumerable<(T Index, Brick Brick)> others)
        {
            var dropped = Drop();
            return others.Where(x => dropped.Hits(x.Brick)).Select(x => x.Index);
        }

        public IEnumerable<T> RestingOnThis<T>(IEnumerable<(T Index, Brick Brick)> others)
        {
            return others.Where(x => Hits(x.Brick.Drop())).Select(x => x.Index);
        }

        public bool InGround() => P1.Z < 1 || P2.Z < 1;
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var droppedBricks = DropToGround(input.OrderBy(x => x.P1.Z))
            .Select((x, i) => (Index: i, Brick: x))
            .ToArray();

        var restingOnlyOnThis = droppedBricks
            .Select(x => x.Brick.RestsOn(droppedBricks.Where(y => y.Index != x.Index)))
            .Where(x => x.Count() == 1)
            .Select(x => x.First())
            .Distinct()
            .ToArray();

        _testOutputHelper.WriteLine((input.Length - restingOnlyOnThis.Length).ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs();
        var droppedBricks = DropToGround(input.OrderBy(x => x.P1.Z))
            .Select((x, i) => (Index: i, Brick: x))
            .ToArray();

        var restingOnThis = droppedBricks
            .ToDictionary(x => x.Index, x => x.Brick.RestsOn(droppedBricks.Where(y => y.Index != x.Index)).ToArray());

        var falling = CountFalling(restingOnThis);

        _testOutputHelper.WriteLine(falling.Sum().ToString());
    }

    private int[] CountFalling(IDictionary<int, int[]> restingOn)
    {
        var result = new List<int>();

        foreach (var (index, _) in restingOn)
        {
            var continueLoop = true;
            List<int> fallen = [index];
            while (continueLoop)
            {
                continueLoop = false;
                foreach (var kvp in restingOn.Where(x => !fallen.Contains(x.Key) && x.Value.Any()))
                {
                    if (kvp.Value.All(fallen.Contains))
                    {
                        fallen.Add(kvp.Key);
                        continueLoop = true;
                    }
                }
            }
            result[index] = fallen.Except([index]).Count();
        }

        return result.ToArray();
    }

    private IEnumerable<Brick> DropToGround(IEnumerable<Brick> bricks)
    {
        var result = new List<Brick>();

        foreach (var brick in bricks)
        {
            var dropped = brick;
            var upper = brick;
            while (!dropped.HitsAny(result) && !dropped.InGround())
            {
                upper = dropped;
                dropped = upper.Drop();
            }
            result.Add(upper);
        }

        return result;
    }

    private Brick[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day22.txt");
        return lines
            .Select(x => x.Split(",~".ToCharArray()).Select(int.Parse).ToArray())
            .Select(x => new Brick(new Coordinate(x[0], x[1], x[2]), new(x[3], x[4], x[5])))
            .ToArray();
    }
}
