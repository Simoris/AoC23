using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day7
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day7(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public enum Card
    {
        Two = 1,
        Three = 2,
        Four = 3,
        Five = 4,
        Six = 5,
        Seven = 6,
        Eight = 7,
        Nine = 8,
        Ten = 9,
        Jack = 10,
        Queen = 11,
        King = 12,
        Ace = 13
    }

    public static readonly Dictionary<char, Card> ParseCard = new Dictionary<char, Card>()
    {
        {'2', Card.Two },
        { '3', Card.Three },
        { '4', Card.Four },
        { '5', Card.Five },
        { '6', Card.Six },
        { '7', Card.Seven },
        { '8', Card.Eight },
        { '9', Card.Nine },
        { 'T', Card.Ten },
        { 'J', Card.Jack },
        { 'Q', Card.Queen },
        { 'K', Card.King },
        { 'A', Card.Ace }
    };

    public int Part2Value(Card card) => card is Card.Jack ? 0 : (int)card;

    public enum HandType
    {
        HighCard = 1,
        Pair = 2,
        TwoPairs = 3,
        ThreeOfAKind = 4,
        FullHouse = 5,
        FourOfAKind = 6,
        FiveOfAKind = 7,
    }

    public record Hand
    {
        public Hand(Card[] cards, int bid)
        {
            Cards = cards;
            Bid = bid;
            var sameCardGroups = cards.GroupBy(x => x).Select(x => x.Count()).OrderDescending().ToArray();
            if (sameCardGroups[0] == 3 && sameCardGroups[1] == 2)
            {
                Type = HandType.FullHouse;
            }
            else if (sameCardGroups[0] == 2 && sameCardGroups[1] == 2)
            {
                Type = HandType.TwoPairs;
            }
            else
            {
                Type = sameCardGroups[0] switch
                {
                    1 => HandType.HighCard,
                    2 => HandType.Pair,
                    3 => HandType.ThreeOfAKind,
                    4 => HandType.FourOfAKind,
                    5 => HandType.FiveOfAKind,
                    _ => throw new ArgumentException(sameCardGroups[0].ToString()),
                };
            }

            var cardsWithoutJacks = cards.Where(x => x != Card.Jack).ToArray();
            if (cardsWithoutJacks.Length == 0)
            {
                Part2Type = HandType.FiveOfAKind;
                return;
            }

            sameCardGroups = cardsWithoutJacks.GroupBy(x => x).Select(x => x.Count()).OrderDescending().ToArray();
            sameCardGroups[0] += 5 - cardsWithoutJacks.Length;
            if (sameCardGroups[0] == 3 && sameCardGroups[1] == 2)
            {
                Part2Type = HandType.FullHouse;
            }
            else if (sameCardGroups[0] == 2 && sameCardGroups[1] == 2)
            {
                Part2Type = HandType.TwoPairs;
            }
            else
            {
                Part2Type = sameCardGroups[0] switch
                {
                    1 => HandType.HighCard,
                    2 => HandType.Pair,
                    3 => HandType.ThreeOfAKind,
                    4 => HandType.FourOfAKind,
                    5 => HandType.FiveOfAKind,
                    _ => throw new ArgumentException(sameCardGroups[0].ToString()),
                };
            }
        }

        public Hand(string line) : this(line.Take(5).Select(x => ParseCard[x]).ToArray(), int.Parse(line.Substring(6)))
        {
        }

        public Card[] Cards { get; init; }
        public int Bid { get; init; }
        public HandType Type { get; private init; }
        public HandType Part2Type { get; private init; }
    }

    [Fact]
    public void Task1()
    {
        var hands = ReadInputs().OrderBy(x => x.Type).ThenBy(x => x.Cards[0]).ThenBy(x => x.Cards[1]).ThenBy(x => x.Cards[2]).ThenBy(x => x.Cards[3]).ThenBy(x => x.Cards[4]).ToArray();
        var result = hands.Select((x, i) => x.Bid * (i + 1)).Sum();
        _testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public void Task2()
    {
        var hands = ReadInputs().OrderBy(x => x.Part2Type)
            .ThenBy(x => Part2Value(x.Cards[0])).ThenBy(x => Part2Value(x.Cards[1])).ThenBy(x => Part2Value(x.Cards[2])).ThenBy(x => Part2Value(x.Cards[3])).ThenBy(x => Part2Value(x.Cards[4])).ToArray();
        var result = hands.Select((x, i) => x.Bid * (i + 1)).Sum();
        _testOutputHelper.WriteLine(result.ToString());
    }

    private Hand[] ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day7.txt");
        return lines.Select(x => new Hand(x)).ToArray();
    }
}
