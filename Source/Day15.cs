using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day15
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day15(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Lens(string Label, int FocalStrength);
    public record Step(string Label, char Operation, int? MayValue);

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var hashSum = input.Select(HASH).Sum();
        _testOutputHelper.WriteLine(hashSum.ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs().Select(ParseFor2).ToArray();
        var boxes = InitializationSequence(input);
        _testOutputHelper.WriteLine(SumFocussingPowers(boxes).ToString());
    }

    private int HASH(string input)
    {
        var currentValue = 0;
        foreach (char c in input)
        {
            currentValue += c;
            currentValue *= 17;
            currentValue %= 256;
        }
        return currentValue;
    }

    private Step ParseFor2(string input)
    {
        if (input.Last() == '-')
            return new(input.Substring(0, input.Length - 1), '-', null);

        var bits = input.Split('=');
        return new(bits[0], '=', int.Parse(bits[1]));
    }

    private List<Lens>[] InitializationSequence(Step[] steps)
    {
        var boxes = Enumerable.Range(1, 256).Select(_ => new List<Lens>()).ToArray();
        foreach (var step in steps)
        {
            var boxToWorkOn = HASH(step.Label);
            var existingIndex = boxes[boxToWorkOn]
                .Select((x, i) => (x, i))
                .Where(x => x.x.Label == step.Label)
                .Select(x => x.i)
                .SingleOrDefault(-1);
            if (step.Operation == '-')
            {
                if (existingIndex != -1)
                    boxes[boxToWorkOn].RemoveAt(existingIndex);
            }
            else if (step.Operation == '=')
            {
                if (existingIndex != -1)
                    boxes[boxToWorkOn][existingIndex] = new(step.Label, step.MayValue.Value);
                else
                    boxes[boxToWorkOn].Add(new(step.Label, step.MayValue.Value));
            }
            else
                throw new NotImplementedException();
        }
        return boxes;
    }

    private int SumFocussingPowers(List<Lens>[] boxes)
        => boxes.SelectMany((box, boxIndex) => box.Select((lens, lensIndex) => (boxIndex + 1) * (lensIndex + 1) * lens.FocalStrength)).Sum();

    private string[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day15.txt");
        lines.Count().Should().Be(1);
        return lines.First().Split(',');
    }
}
