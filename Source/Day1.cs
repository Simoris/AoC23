using FluentAssertions;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day1
{
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly (int Number, string StringRepresentation)[] Numbers = new[]
    {
        (1, "one"),
        (2, "two"),
        (3, "three"),
        (4, "four"),
        (5, "five"),
        (6, "six"),
        (7, "seven"),
        (8, "eight"),
        (9, "nine")
    };

    public Day1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Task1()
    {
        var lines = ReadInputs();
        var digits = lines.Select(x => x.Where(y => char.IsDigit(y)).ToArray());
        var numbers = digits.Select(x => $"{x.First()}{x.Last()}").Select(int.Parse).ToArray();

        numbers.All(x => x > 9 && x < 100).Should().BeTrue();
        _testOutputHelper.WriteLine(numbers.Sum().ToString());
    }

    [Fact]
    public void Task2()
    {
        var lines = ReadInputs();
        var digits = lines.Select(FindStringNumnbers).Select(x => x.Where(y => char.IsDigit(y)).ToArray());
        var numbers = digits.Select(x => $"{x.First()}{x.Last()}").Select(int.Parse).ToArray();

        numbers.All(x => x > 9 && x < 100).Should().BeTrue();
        _testOutputHelper.WriteLine(numbers.Sum().ToString());
    }

    private string FindStringNumnbers(string input)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            foreach (var (num, str) in Numbers)
                if (input[i..].StartsWith(str))
                    sb.Append(num);

            sb.Append(input[i]);
        }
        return sb.ToString();
    }

    private string[] ReadInputs()
    {
        return File.ReadAllLines("Inputs/Day1.txt");
    }
}
