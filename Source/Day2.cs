using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day2
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day2(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Show(int Red, int Green, int Blue);
    public record Game(int Id, Show[] Shows);

    [Fact]
    public void Task1()
    {
        var games = ReadInputs();

        var possibleGames = games.Where(x => x.Shows.All(y => y.Red <= 12 && y.Green <= 13 && y.Blue <= 14));
        _testOutputHelper.WriteLine(possibleGames.Sum(x => x.Id).ToString());
    }

    [Fact]
    public void Task2()
    {
        var games = ReadInputs();

        var powersOfGames = games.Select(x => x.Shows.Select(y => y.Red).Max() * x.Shows.Select(y => y.Green).Max() * x.Shows.Select(y => y.Blue).Max()).ToArray();
        _testOutputHelper.WriteLine(powersOfGames.Sum().ToString());
    }

    private Game[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day2.txt");

        return lines.Select(ParseGame).ToArray();

        Game ParseGame(string line)
        {
            var splitted = line.Split(new[] { ':', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var id = int.Parse(splitted[0][5..]);
            var shows = splitted.Skip(1).Select(ParseShow).ToArray();

            return new(id, shows);
        }
        Show ParseShow(string str)
        {
            var splitted = str.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            int red = 0, green = 0, blue = 0;

            foreach (var color in splitted)
            {
                if (color.Contains("red"))
                    red = int.Parse(color.TakeWhile(c => char.IsDigit(c)).ToArray());
                else if (color.Contains('b'))
                    blue = int.Parse(color.TakeWhile(c => char.IsDigit(c)).ToArray());
                else if (color.Contains('g'))
                    green = int.Parse(color.TakeWhile(c => char.IsDigit(c)).ToArray());
            }

            return new(red, green, blue);
        }
    }
}
