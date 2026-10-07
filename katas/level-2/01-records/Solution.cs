namespace Katas.L2.Records;

public sealed record Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}.");
        }

        return this with { Amount = Amount + other.Amount };
    }

    public Money Scale(decimal factor) =>
        this with { Amount = Math.Round(Amount * factor, 2, MidpointRounding.AwayFromZero) };
}

public record Person(string Name);

public record Employee(string Name, string Title) : Person(Name);

public static class RecordKata
{
    public static Employee Promote(Employee employee, string newTitle) =>
        employee with { Title = newTitle };

    public static Money Total(IEnumerable<Money> amounts, string currency) =>
        amounts.Aggregate(new Money(0m, currency), (sum, next) => sum.Add(next));

    public static IReadOnlyList<Money> DistinctAmounts(IEnumerable<Money> amounts) =>
        amounts.Distinct().ToList();
}
