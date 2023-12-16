using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day16
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day16(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Tile(char Char)
    {
        public bool BeamFromRight { get; set; }
        public bool BeamFromTop { get; set; }
        public bool BeamFromLeft { get; set; }
        public bool BeamFromBottom { get; set; }
        public bool Energized => BeamFromBottom || BeamFromLeft || BeamFromRight || BeamFromTop;
    }

    public enum Direction
    {
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3,
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        LightUp(input, 0, 0, Direction.Right);
        _testOutputHelper.WriteLine(input.Select(x => x.Count(y => y.Energized)).Sum().ToString());
    }

    [Fact]
    public void Task2()
    {
        const int size = 110;
        var maxEnergized = 0;
        foreach (var startingDirection in Enum.GetValues<Direction>())
            for (int i = 0; i < size; i++)
            {
                var input = ReadInputs();
                var startCoords = StartingCoordinate(startingDirection, i, size - 1);
                LightUp(input, startCoords.R, startCoords.C, startingDirection);
                maxEnergized = Math.Max(maxEnergized, input.Select(x => x.Count(y => y.Energized)).Sum());
            }
        _testOutputHelper.WriteLine(maxEnergized.ToString());
    }

    private (int R, int C) StartingCoordinate(Direction direction, int i, int max)
        => direction switch
        {
            Direction.Left => (i, max),
            Direction.Up => (max, i),
            Direction.Right => (i, 0),
            Direction.Down => (0, i),
            _ => throw new NotImplementedException(),
        };

    private void LightUp(Tile[][] grid, int r, int c, Direction from)
    {
        try
        {
            var currentTile = grid[r][c];
            switch (from)
            {
                case Direction.Left:
                    if (currentTile.BeamFromLeft == true)
                        return;
                    currentTile.BeamFromLeft = true;
                    break;
                case Direction.Right:
                    if (currentTile.BeamFromRight == true)
                        return;
                    currentTile.BeamFromRight = true;
                    break;
                case Direction.Up:
                    if (currentTile.BeamFromTop == true)
                        return;
                    currentTile.BeamFromTop = true;
                    break;
                case Direction.Down:
                    if (currentTile.BeamFromBottom == true)
                        return;
                    currentTile.BeamFromBottom = true;
                    break;
            }

            foreach (var dir in NextDirection(currentTile.Char, from))
            {
                var next = NeighbourInDirection(r, c, dir);
                LightUp(grid, next.R, next.C, dir);
            }
        }
        catch (IndexOutOfRangeException) { }
    }

    private Direction[] NextDirection(char symbol, Direction from)
    {
        int asInt = (int)from;
        return symbol switch
        {
            '.' => [from],
            '-' => (from is Direction.Left or Direction.Right) ? [from] : [Direction.Left, Direction.Right],
            '|' => (from is Direction.Up or Direction.Down) ? [from] : [Direction.Up, Direction.Down],
            '\\' => [(Direction)((asInt + 1) % 2 + 2 * (asInt / 2))],
            '/' => [(Direction)((asInt + 1) % 2 + 2 - 2 * (asInt / 2))],
            _ => throw new NotImplementedException(),
        };
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

    private Tile[][] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day16.txt");
        return lines.Select(x => x.Select(y => new Tile(y)).ToArray()).ToArray();
    }
}
