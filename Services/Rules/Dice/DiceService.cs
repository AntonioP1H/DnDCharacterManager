using System.Text.RegularExpressions;

namespace DnDCharacterManager.Services.Rules.Dice;

public interface IDiceService
{
    DiceRollResult Roll(string expression);
    DiceRollResult Roll(int count, int sides, int modifier = 0);
    Task<DiceRollResult> RollAnimatedAsync(string expression, CancellationToken cancellationToken = default);
    DiceRollResult RollWithAdvantage(string expression);
    DiceRollResult RollWithDisadvantage(string expression);
    int RollD20();
    int RollD20WithAdvantage();
    int RollD20WithDisadvantage();
}

public sealed record DiceRollResult(
    string Expression,
    int Total,
    IReadOnlyList<IndividualDieRoll> Dice,
    int Modifier,
    bool IsCriticalHit = false,
    bool IsCriticalMiss = false,
    AdvantageState Advantage = AdvantageState.Normal);

public sealed record IndividualDieRoll(int Sides, int Result)
{
    public bool IsMax => Result == Sides;
    public bool IsMin => Result == 1;
}

public enum AdvantageState
{
    Normal,
    Advantage,
    Disadvantage
}

public sealed class DiceService : IDiceService
{
    private static readonly Regex DiceRegex = new(
        @"^(\d*)d(\d+)([+-]\d+)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public DiceRollResult Roll(string expression)
        => RollInternal(expression, AdvantageState.Normal);

    public DiceRollResult Roll(int count, int sides, int modifier = 0)
    {
        ValidateDice(count, sides);
        var dice = new List<IndividualDieRoll>(count);
        int total = 0;

        for (int i = 0; i < count; i++)
        {
            int result = Random.Shared.Next(1, sides + 1);
            dice.Add(new IndividualDieRoll(sides, result));
            total += result;
        }

        total = checked(total + modifier);
        string suffix = modifier == 0 ? string.Empty : modifier > 0 ? $"+{modifier}" : modifier.ToString();
        return new DiceRollResult($"{count}d{sides}{suffix}", total, dice, modifier);
    }

    public DiceRollResult RollWithAdvantage(string expression)
        => RollInternal(expression, AdvantageState.Advantage);

    public DiceRollResult RollWithDisadvantage(string expression)
        => RollInternal(expression, AdvantageState.Disadvantage);

    private static DiceRollResult RollInternal(string expression, AdvantageState advantage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        string normalized = string.Concat(expression.Where(c => !char.IsWhiteSpace(c)));
        Match match = DiceRegex.Match(normalized);
        if (!match.Success)
            throw new ArgumentException("Use a dice expression such as d20, 2d6+3, or 1d8-1.", nameof(expression));

        int count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
        int sides = int.Parse(match.Groups[2].Value);
        int modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);
        ValidateDice(count, sides);

        if (advantage != AdvantageState.Normal && (count != 1 || sides != 20))
            throw new ArgumentException("Advantage and disadvantage are supported for a single d20 roll only.", nameof(expression));

        var rolls = new List<IndividualDieRoll>(advantage == AdvantageState.Normal ? count : 2);
        int subtotal;
        int selectedD20 = 0;

        if (advantage == AdvantageState.Normal)
        {
            subtotal = 0;
            for (int i = 0; i < count; i++)
            {
                int value = Random.Shared.Next(1, sides + 1);
                rolls.Add(new IndividualDieRoll(sides, value));
                subtotal = checked(subtotal + value);
            }
        }
        else
        {
            int first = Random.Shared.Next(1, 21);
            int second = Random.Shared.Next(1, 21);
            rolls.Add(new IndividualDieRoll(20, first));
            rolls.Add(new IndividualDieRoll(20, second));
            selectedD20 = advantage == AdvantageState.Advantage ? Math.Max(first, second) : Math.Min(first, second);
            subtotal = selectedD20;
        }

        int total = checked(subtotal + modifier);
        bool criticalHit = sides == 20 && (advantage == AdvantageState.Normal
            ? rolls.Count == 1 && rolls[0].Result == 20
            : selectedD20 == 20);
        bool criticalMiss = sides == 20 && (advantage == AdvantageState.Normal
            ? rolls.Count == 1 && rolls[0].Result == 1
            : selectedD20 == 1);

        return new DiceRollResult(normalized, total, rolls, modifier, criticalHit, criticalMiss, advantage);
    }

    private static void ValidateDice(int count, int sides)
    {
        if (count is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(count), "Dice count must be between 1 and 100.");
        if (sides is < 2 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(sides), "Die sides must be between 2 and 1000.");
    }

    public int RollD20() => Random.Shared.Next(1, 21);
    public int RollD20WithAdvantage() => Math.Max(RollD20(), RollD20());
    public int RollD20WithDisadvantage() => Math.Min(RollD20(), RollD20());

    public async Task<DiceRollResult> RollAnimatedAsync(
        string expression,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(350, cancellationToken);
        return Roll(expression);
    }
}
