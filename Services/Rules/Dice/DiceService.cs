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

public record DiceRollResult(
    string Expression,
    int Total,
    IReadOnlyList<IndividualDieRoll> Dice,
    int Modifier,
    bool IsCriticalHit = false,
    bool IsCriticalMiss = false,
    AdvantageState Advantage = AdvantageState.Normal);

public record IndividualDieRoll(int Sides, int Result)
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

/// <summary>Dice roller for expressions in the form 2d6+3 or d20-1.</summary>
public sealed class DiceService : IDiceService
{
    private static readonly Regex DiceExpressionRegex = new(
        @"^(\d*)d(\d+)([+-]\d+)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public DiceRollResult Roll(string expression) => RollInternal(expression, AdvantageState.Normal);

    public DiceRollResult Roll(int count, int sides, int modifier = 0)
    {
        ValidateDice(count, sides);
        var dice = Enumerable.Range(0, count)
            .Select(_ => new IndividualDieRoll(sides, Random.Shared.Next(1, sides + 1)))
            .ToArray();

        return new DiceRollResult(
            $"{count}d{sides}{(modifier >= 0 ? "+" : string.Empty)}{modifier}",
            dice.Sum(d => d.Result) + modifier,
            dice,
            modifier);
    }

    public DiceRollResult RollWithAdvantage(string expression) =>
        RollInternal(expression, AdvantageState.Advantage);

    public DiceRollResult RollWithDisadvantage(string expression) =>
        RollInternal(expression, AdvantageState.Disadvantage);

    public int RollD20() => Random.Shared.Next(1, 21);
    public int RollD20WithAdvantage() => Math.Max(RollD20(), RollD20());
    public int RollD20WithDisadvantage() => Math.Min(RollD20(), RollD20());

    public async Task<DiceRollResult> RollAnimatedAsync(
        string expression,
        CancellationToken cancellationToken = default)
    {
        // UI can animate during this short delay; the returned result is the actual final roll.
        await Task.Delay(350, cancellationToken);
        return Roll(expression);
    }

    private static DiceRollResult RollInternal(string expression, AdvantageState advantage)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("A dice expression is required.", nameof(expression));

        var normalized = expression.Replace(" ", string.Empty, StringComparison.Ordinal);
        var match = DiceExpressionRegex.Match(normalized);
        if (!match.Success)
            throw new ArgumentException($"Invalid dice expression '{expression}'. Use a format such as 2d6+3.", nameof(expression));

        var count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
        var sides = int.Parse(match.Groups[2].Value);
        var modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);
        ValidateDice(count, sides);

        var rolls = new List<IndividualDieRoll>();
        int subtotal;
        int selectedD20 = 0;

        if (count == 1 && sides == 20 && advantage != AdvantageState.Normal)
        {
            var first = Random.Shared.Next(1, 21);
            var second = Random.Shared.Next(1, 21);
            rolls.Add(new IndividualDieRoll(20, first));
            rolls.Add(new IndividualDieRoll(20, second));
            selectedD20 = advantage == AdvantageState.Advantage
                ? Math.Max(first, second)
                : Math.Min(first, second);
            subtotal = selectedD20;
        }
        else
        {
            for (var i = 0; i < count; i++)
                rolls.Add(new IndividualDieRoll(sides, Random.Shared.Next(1, sides + 1)));
            subtotal = rolls.Sum(die => die.Result);
            if (count == 1 && sides == 20)
                selectedD20 = rolls[0].Result;
        }

        var criticalHit = count == 1 && sides == 20 && selectedD20 == 20;
        var criticalMiss = count == 1 && sides == 20 && selectedD20 == 1;

        return new DiceRollResult(
            expression,
            subtotal + modifier,
            rolls,
            modifier,
            criticalHit,
            criticalMiss,
            advantage);
    }

    private static void ValidateDice(int count, int sides)
    {
        if (count is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(count), "Dice count must be between 1 and 100.");
        if (sides is < 2 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(sides), "Dice sides must be between 2 and 1000.");
    }
}
