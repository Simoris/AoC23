namespace AoC23.Utils;

public class RangeSet
{
    public record Range(long Start, long Length)
    {
        public long End => Start + Length;
        public bool Overlaps(Range other)
        {
            if (Start >= other.End) return false;
            if (End <= other.Start) return false;
            return true;
        }
    }
    public record Mapping(long Start, long Length, long NewStart) : Range(Start, Length)
    {
        public Mapping Truncate(Range range)
        {
            if (Start < range.Start)
            {
                var dif = range.Start - Start;
                return (this with { Start = range.Start, Length = Length - dif, NewStart = NewStart + dif }).Truncate(range);
            }

            if (End > range.End)
            {
                return this with { Length = Length - End + range.End };
            }
            return this;
        }
        public Range ToTargetRange() => new(NewStart, Length);
    }

    public Range[] Ranges { get; set; }

    public RangeSet(Range[] ranges)
    {
        //TODO sort and merge?
        Ranges = ranges;
    }

    public long Min()
    {
        return Ranges.Select(x => x.Start).Min();
    }

    public RangeSet ApplyMappings(Mapping[] mappings)
    {
        return new(Ranges.SelectMany(x => ApplyMappings(x, mappings)).ToArray());
    }

    public static Range[] ApplyMappings(Range range, Mapping[] mappings)
    {
        var orderedMappings = mappings.Where(m => m.Overlaps(range)).OrderBy(m => m.Start).ToArray();

        if (orderedMappings.Skip(1).Zip(orderedMappings).Any(x => x.First.Start != x.Second.End))
            throw new NotImplementedException();
        //if (orderedMappings.Last().End < range.End || orderedMappings.First().Start > range.Start)
        //    throw new NotImplementedException();

        return orderedMappings.Select(x => x.Truncate(range).ToTargetRange()).ToArray();
    }
}
