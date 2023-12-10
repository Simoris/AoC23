using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day04
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day04(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public record Card
    {
        public Card(int id, int[] winningNumbers, int[] ownNumbers)
        {
            Id = id;
            WinningNumbers = winningNumbers;
            OwnNumbers = ownNumbers;
            NumWinning = winningNumbers.Intersect(ownNumbers).Count();
            Value = NumWinning == 0 ? 0 : (int)Math.Pow(2, NumWinning - 1);
        }

        public int Id { get; set; }
        public int[] WinningNumbers { get; set; }
        public int[] OwnNumbers { get; set; }
        public int NumWinning { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public void Task1()
    {
        var cards = ReadInputs();

        _testOutputHelper.WriteLine(cards.Select(x => x.Value).Sum().ToString());
    }

    [Fact]
    public void Task2()
    {
        var cards = ReadInputs();
        var numCards = cards.Select(_ => 1).ToArray();
        for (int i = 0; i < cards.Length; i++)
        {
            foreach (var j in Enumerable.Range(i + 1, cards[i].NumWinning))
            {
                numCards[j] += numCards[i];
            }
        }
        _testOutputHelper.WriteLine(numCards.Sum().ToString());
    }

    private static Card[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day04.txt");
        return lines.Select(x =>
        {
            var bits = x.Split(new[] { "Card", " ", ":", "|" }, StringSplitOptions.RemoveEmptyEntries);
            return new Card(int.Parse(bits[0]), Enumerable.Range(1, 10).Select(y => int.Parse(bits[y])).ToArray(), Enumerable.Range(11, 25).Select(y => int.Parse(bits[y])).ToArray());
        }).ToArray();
    }
}
