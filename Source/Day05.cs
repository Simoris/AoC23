using AoC23.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day05
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day05(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record FarmingData(long[] Seeds, ILookup<string, Mapping> MappingsBySource);
    public record Mapping(string Source, string Destination, long SourceStart, long DestinationStart, long Length);

    [Fact]
    public void Task1()
    {
        var farmingData = ReadInputs();
        var locations = farmingData.Seeds.Select(x => TraverseToLocations(x, farmingData.MappingsBySource)).ToArray();
        _testOutputHelper.WriteLine(locations.Min().ToString());
    }

    [Fact(Skip = "Slow")]
    public async Task Task2()
    {
        var farmingData = ReadInputs();
        var chunkTasks = farmingData.Seeds.Chunk(2).Select(x => Task.Run(() => TraverseChunk(x[0], x[1], farmingData.MappingsBySource)));
        var result = (await Task.WhenAll(chunkTasks)).Min();
        _testOutputHelper.WriteLine(result.ToString());
    }

    private long TraverseChunk(long seedStart, long seedCount, ILookup<string, Mapping> mappingsBySource)
    {
        long minLocation = long.MaxValue;
        foreach (var seed in Enumerable.Range(0, (int)seedCount).Select(y => y + seedStart))
        {
            minLocation = long.Min(minLocation, TraverseToLocations(seed, mappingsBySource) ?? long.MaxValue);
        }
        return minLocation;
    }

    private long? TraverseToLocations(long seed, ILookup<string, Mapping> mappingsBySource)
    {
        var number = seed;
        var place = "seed";
        while (place != "location")
        {
            var mapping = mappingsBySource[place].SingleOrDefault(x => x.SourceStart <= number && x.SourceStart + x.Length > number);
            if (mapping == null) return null;
            number = number - mapping.SourceStart + mapping.DestinationStart;
            place = mapping.Destination;
        }
        return number;
    }

    [Fact]
    public void Task2Optimized()
    {
        var farmingData = ReadInputs();
        var rangeSet = new RangeSet(farmingData.Seeds.Chunk(2).Select(x => new RangeSet.Range(x[0], x[1])).ToArray());

        var place = "seed";
        while (place != "location")
        {
            var mappings = farmingData.MappingsBySource[place].Select(x => new RangeSet.Mapping(x.SourceStart, x.Length, x.DestinationStart)).ToArray();

            rangeSet = rangeSet.ApplyMappings(mappings);

            place = farmingData.MappingsBySource[place].First().Destination;
        }

        _testOutputHelper.WriteLine(rangeSet.Min().ToString());
    }

    private static FarmingData ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day05.txt");
        var seeds = lines[0].Split(' ').Skip(1).Select(long.Parse).ToArray();

        var maps = new List<Mapping>();
        for (var i = 2; i < lines.Length; i++)
        {
            var bits = lines[i].Split(' ')[0].Split('-');
            var source = bits[0];
            var destination = bits[2];
            i++;

            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                bits = lines[i].Split(' ');
                maps.Add(new Mapping(source, destination, long.Parse(bits[1]), long.Parse(bits[0]), long.Parse(bits[2])));
                i++;
            }
        }
        return new(seeds, maps.ToLookup(x => x.Source));
    }
}
