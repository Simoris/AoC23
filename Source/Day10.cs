using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day10
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day10(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Position(char Char, int X, int Y) //coordinates are somewhat redundant, but make everything easier
    {
        public bool IsPipe => Char != '.';
        public bool IsAnimal => Char == 'S';
        public bool ConnectsTop => Directions.Contains(Direction.Top);
        public bool ConnectsLeft => Directions.Contains(Direction.Left);
        public bool ConnectsDown => Directions.Contains(Direction.Down);
        public bool ConnectsRight => Directions.Contains(Direction.Right);

        public Direction[] Directions { get; set; } = Char switch
        {
            'L' => new[] { Direction.Top, Direction.Right },
            'F' => new[] { Direction.Down, Direction.Right },
            'J' => new[] { Direction.Top, Direction.Left },
            '7' => new[] { Direction.Down, Direction.Left },
            '|' => new[] { Direction.Top, Direction.Down },
            '-' => new[] { Direction.Left, Direction.Right },
            _ => Array.Empty<Direction>()
        };

        public Direction GetNextDirection(Direction from)
        {
            return Directions.First(x => x != from);
        }

        public (int X, int Y) GetNeighborCoordinates(Direction direction)
        {
            return direction switch
            {
                Direction.Top => (X, Y - 1),
                Direction.Left => (X - 1, Y),
                Direction.Down => (X, Y + 1),
                Direction.Right => (X + 1, Y),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public enum PointType
    {
        None,
        Line,
        Inside,
        Outside
    }

    public enum Direction
    {
        Top = 0,
        Left = 1,
        Down = 2,
        Right = 3
    }

    public Direction InvertDirection(Direction direction)
        => (Direction)(((int)direction + 2) % 4);

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var fixedInput = FixAnimalConnections(input);
        var length = GetLoopLength(fixedInput);
        _testOutputHelper.WriteLine((length / 2).ToString());
    }

    private Position FindAnimal(Position[][] matrix)
    {
        return matrix.SelectMany(x => x).Single(x => x.IsAnimal);
    }

    private Position[][] FixAnimalConnections(Position[][] matrix)
    {
        var animal = FindAnimal(matrix);
        var directions = new List<Direction>();
        if (matrix[animal.Y][animal.X - 1].ConnectsRight)
        {
            directions.Add(Direction.Left);
        }
        if (matrix[animal.Y][animal.X + 1].ConnectsLeft)
        {
            directions.Add(Direction.Right);
        }
        if (matrix[animal.Y - 1][animal.X].ConnectsDown)
        {
            directions.Add(Direction.Top);
        }
        if (matrix[animal.Y + 1][animal.X].ConnectsTop)
        {
            directions.Add(Direction.Down);
        }
        animal.Directions = directions.ToArray();
        return matrix;
    }

    private int GetLoopLength(Position[][] matrix)
    {
        var start = FindAnimal(matrix);
        var direction = Direction.Left; //Doesn't matter which direction we put in here, just start in any direction
        var moves = 0;
        do
        {
            direction = start.GetNextDirection(InvertDirection(direction));
            var nextCoords = start.GetNeighborCoordinates(direction);
            start = matrix[nextCoords.Y][nextCoords.X];
            moves++;
        } while (!start.IsAnimal);
        return moves;
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs();
        var fixedInput = FixAnimalConnections(input);
        var pointMap = ToPointMap(fixedInput);
        pointMap = InfectOutside(pointMap);
        var realInnerPoints = pointMap.Where((x, i) => i % 2 == 1).Sum(x => x.Where((y, j) => j % 2 == 1 && y == PointType.None).Count());
        _testOutputHelper.WriteLine(realInnerPoints.ToString());
    }

    PointType[][] ToPointMap(Position[][] matrix)
    {
        var height = matrix.Length;
        var width = matrix[0].Length;

        var result = Enumerable.Range(0, Stretch(height)).Select(_ => Enumerable.Range(0, Stretch(width)).Select(_ => PointType.None).ToArray()).ToArray();

        var start = FindAnimal(matrix);
        var direction = Direction.Left; //Doesn't matter which direction we put in here, just start in any direction
        do
        {
            direction = start.GetNextDirection(InvertDirection(direction));
            var nextCoords = start.GetNeighborCoordinates(direction);

            result[Stretch(start.Y)][Stretch(start.X)] = PointType.Line;
            result[StretchAverage(start.Y, nextCoords.Y)][StretchAverage(start.X, nextCoords.X)] = PointType.Line;

            start = matrix[nextCoords.Y][nextCoords.X];
        } while (!start.IsAnimal);

        return result;

        static int Stretch(int x) => 2 * x + 1;
        static int StretchAverage(int x, int y) => x + y + 1;
    }

    PointType[][] InfectOutside(PointType[][] matrix)
    {
        var queue = new Queue<(int X, int Y)>();
        SetOutsideAndEnqueue(0, 0);

        while (queue.TryDequeue(out var q))
        {
            SafeCheck(q.X + 1, q.Y);
            SafeCheck(q.X - 1, q.Y);
            SafeCheck(q.X, q.Y + 1);
            SafeCheck(q.X, q.Y - 1);
        }
        return matrix;

        void SetOutsideAndEnqueue(int x, int y)
        {
            matrix[y][x] = PointType.Outside;
            queue.Enqueue((x, y));
        }
        void SafeCheck(int x, int y)
        {
            try
            {
                if (matrix[y][x] == PointType.None)
                    SetOutsideAndEnqueue(x, y);
            }
            catch (IndexOutOfRangeException) { }
        }
    }

    private Position[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day10.txt");
        return lines.Select((x, i) => x.ToCharArray().Select((y, j) => new Position(y, j, i)).ToArray()).ToArray();
    }
}
