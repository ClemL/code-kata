namespace Katas.L2.Records;

// TODO: Two Money values with the same amount and currency should be equal. Make this a positional record.
public sealed class Money(decimal amount, string currency)
{
    public decimal Amount { get; } = amount;

    public string Currency { get; } = currency;

    // A positional record generates this for you.
    public void Deconstruct(out decimal amount, out string currency) => throw new NotImplementedException();

    public Money Add(Money other) => throw new NotImplementedException();

    public Money Scale(decimal factor) => throw new NotImplementedException();
}

// TODO: Make Person and Employee records. Employee derives from Person.
public class Person(string name)
{
    public string Name { get; } = name;
}

public class Employee(string name, string title) : Person(name)
{
    public string Title { get; } = title;
}

public static class RecordKata
{
    public static Employee Promote(Employee employee, string newTitle) => throw new NotImplementedException();

    public static Money Total(IEnumerable<Money> amounts, string currency) => throw new NotImplementedException();

    public static IReadOnlyList<Money> DistinctAmounts(IEnumerable<Money> amounts) => throw new NotImplementedException();
}
