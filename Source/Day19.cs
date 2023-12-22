using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public partial class Day19
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day19(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Part(long X, long M, long A, long S);

    public record Workflow(string Name, ConditionWithTarget[] Conditions, string DefaultTarget);

    public record Condition(char Property, bool ShouldBeGreater, long Border)
    {
        public Condition Invert()
            => this with { ShouldBeGreater = !ShouldBeGreater, Border = Border + (ShouldBeGreater ? 1 : -1) };
    }

    public record ConditionWithTarget(char Property, bool ShouldBeGreater, long Border, string Target) : Condition(Property, ShouldBeGreater, Border);

    public record MultiConditionWithTarget(Condition[] Conditions, string Target)
    {
        public MultiConditionWithTarget AddWithNewTarget(Condition condition, string newTarget)
        {
            var sameCondition = Conditions.SingleOrDefault(x => x.ShouldBeGreater == condition.ShouldBeGreater && x.Property == condition.Property);
            var newSameCondition = condition with { Border = condition.ShouldBeGreater ? Math.Max(condition.Border, sameCondition?.Border ?? 0) : Math.Min(condition.Border, sameCondition?.Border ?? long.MaxValue) };

            var oppositeCondition = Conditions.SingleOrDefault(x => x.ShouldBeGreater != condition.ShouldBeGreater && x.Property == condition.Property);
            if (oppositeCondition != null)
            {
                if (oppositeCondition.ShouldBeGreater && oppositeCondition.Border + 1 >= newSameCondition.Border)
                    return null;
                if (!oppositeCondition.ShouldBeGreater && oppositeCondition.Border - 1 <= newSameCondition.Border)
                    return null;
            }

            return new(Conditions.Where(x => x.ShouldBeGreater != condition.ShouldBeGreater || x.Property != condition.Property).Append(newSameCondition).ToArray(), newTarget);
        }

        public MultiConditionWithTarget IntersectOrNullIfEmpty(MultiConditionWithTarget other)
        {
            var conditions = Conditions.Concat(other.Conditions).GroupBy(x => x.Property).Select(x => x.GroupBy(y => y.ShouldBeGreater));
            var newConditions = new List<Condition>();
            foreach (var conditionGroup in conditions)
            {
                conditionGroup.Count().Should().Be(2);
                var lowerBound = conditionGroup.Single(x => x.Key).Select(x => x.Border).Max();
                var upperBound = conditionGroup.Single(x => !x.Key).Select(x => x.Border).Min();
                if (lowerBound >= upperBound - 1)
                    return null;
                newConditions.Add(new Condition(conditionGroup.First().First().Property, true, lowerBound));
                newConditions.Add(new Condition(conditionGroup.First().First().Property, false, upperBound));
            }
            return this with { Conditions = newConditions.ToArray() };
        }

        public long Size
        {
            get
            {
                var groups = Conditions.GroupBy(x => x.Property).Select(x => x.Max(y => y.Border) - x.Min(y => y.Border) - 1).ToArray();
                groups.Count().Should().Be(4);
                return (long)groups.Aggregate((x, y) => x * y);
            }
        }

        public static MultiConditionWithTarget Default(string target)
        {
            return new(new char[] { 'x', 'm', 'a', 's' }.SelectMany<char, Condition>(x => [new(x, true, 0), new(x, false, 4001)]).ToArray(), target);
        }
    }

    [Fact]
    public void Task1()
    {
        var input = ReadInputs();
        var result = input.Parts.Where(x => GetsAccepted(x, input.Worflows)).Select(x => x.X + x.M + x.A + x.S).Sum();
        _testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public void Task2()
    {
        var input = ReadInputs();
        var result = IterateTillAorR(input.Worflows, MultiConditionWithTarget.Default("in")).ToLookup(x => x.Target);
        var summedSize = GetOverlap(result["A"].ToArray());
        _testOutputHelper.WriteLine(summedSize.ToString());
    }

    private long GetOverlap(MultiConditionWithTarget[] sets)
    {
        long sum = 0;
        for (var i = 0; i < sets.Length; i++)
        {
            sum += sets[i].Size - Iterator(sets[i], sets.Skip(i + 1).ToArray());
        }

        return sum;

        long Iterator(MultiConditionWithTarget baseSet, MultiConditionWithTarget[] otherSets)
        {
            long sum = 0;
            for (var i = 0; i < otherSets.Length; i++)
            {
                var combined = baseSet.IntersectOrNullIfEmpty(otherSets[i]);
                if (combined == null)
                    continue;
                sum += combined.Size - Iterator(combined, otherSets.Skip(i + 1).ToArray());
            }

            return sum;
        }
    }

    private MultiConditionWithTarget[] IterateTillAorR(Dictionary<string, Workflow> workflows, MultiConditionWithTarget start)
    {
        if (workflows.ContainsKey(start.Target))
        {
            var results = ApplyWorkflow(workflows[start.Target], start);
            return results.SelectMany(x => IterateTillAorR(workflows, x)).ToArray();
        }
        else
        {
            return [start];
        }
    }

    private MultiConditionWithTarget[] ApplyWorkflow(Workflow workflow, MultiConditionWithTarget range)
    {
        var result = new List<MultiConditionWithTarget>();
        foreach (var condition in workflow.Conditions)
        {
            result.Add(range.AddWithNewTarget(condition, condition.Target));
            range = range.AddWithNewTarget(condition.Invert(), condition.Target);
            if (range == null)
                break;
        }
        return result.Where(x => x != null).Append(range with { Target = workflow.DefaultTarget }).ToArray();
    }

    private string ApplyWorkflow(Workflow workflow, Part part)
    {
        foreach (var condition in workflow.Conditions)
        {
            var compareValue = condition.Property switch
            {
                'x' => part.X,
                'm' => part.M,
                'a' => part.A,
                's' => part.S,
                _ => throw new ArgumentException()
            };
            if ((condition.ShouldBeGreater && compareValue > condition.Border) || (!condition.ShouldBeGreater && compareValue < condition.Border))
                return condition.Target;
        }
        return workflow.DefaultTarget;
    }

    private bool GetsAccepted(Part part, Dictionary<string, Workflow> workflows)
    {
        var currentFlow = "in";
        while (true)
        {
            currentFlow = ApplyWorkflow(workflows[currentFlow], part);

            if (currentFlow == "A")
                return true;
            if (currentFlow == "R")
                return false;
        }
    }

    [GeneratedRegex("{x=(\\d+),m=(\\d+),a=(\\d+),s=(\\d+)}")]
    private static partial Regex MatchPart();

    private (Dictionary<string, Workflow> Worflows, Part[] Parts) ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day19.txt");
        var workflows = new List<Workflow>();
        var parts = new List<Part>();

        var enumerator = lines.GetEnumerator();
        while (enumerator.MoveNext() && !string.IsNullOrWhiteSpace((string)enumerator.Current))
        {
            var splitted = ((string)enumerator.Current).Split(new char[] { '{', '}', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            workflows.Add(new(splitted.First(), splitted.Skip(1).SkipLast(1).Select(ParseCondition).ToArray(), splitted.Last()));
        }
        while (enumerator.MoveNext() && !string.IsNullOrWhiteSpace((string)enumerator.Current))
        {
            var groups = MatchPart().Match((string)enumerator.Current).Groups;
            parts.Add(new(long.Parse(groups[1].Value), long.Parse(groups[2].Value), long.Parse(groups[3].Value), long.Parse(groups[4].Value)));
        }

        return (workflows.ToDictionary(x => x.Name, x => x), parts.ToArray());

        ConditionWithTarget ParseCondition(string str)
        {
            var splitted = str.Split(':');
            return new(str[0], str[1] == '>', long.Parse(splitted[0][2..]), splitted[1]);
        }
    }
}
