using System.Globalization;

namespace Katas.L3.PatternsPro;

public sealed record Address(string City, string State, string Country);

public sealed record Customer(string Name, bool IsPremium, Address Address);

public sealed record Order(Customer Customer, decimal Subtotal, int Items);

public static class Shipping
{
    public static decimal Cost(Order order) => order switch
    {
        { Customer.IsPremium: true } => 0m,
        { Subtotal: >= 100m } => 0m,
        { Customer.Address.Country: not "US" } => 25m,
        { Customer.Address.State: "AK" or "HI" } => 15m,
        { Items: > 10 } => 12m,
        _ => 7m,
    };
}

public static class Lists
{
    public static string Describe(int[] values) => values switch
    {
        [] => "empty",
        [var only] => $"one: {only}",
        [var first, var second] => $"pair: {first}, {second}",
        [0, ..] => "starts with zero",
        [.., < 0] => "ends negative",
        [var first, .., var last] => $"{first}..{last} ({values.Length} items)",
    };
}

public static class Commands
{
    public static string Parse(string input) =>
        input.Split(' ', StringSplitOptions.RemoveEmptyEntries) switch
        {
            ["help"] => "help",
            ["go", var direction and ("north" or "south" or "east" or "west")] => $"move {direction}",
            ["take", .. var items] when items.Length > 0 => $"take {string.Join(", ", items)}",
            ["say", _, ..] and [_, .. var words] => $"say: {string.Join(' ', words)}",
            _ => "unknown",
        };
}

public abstract record Expr;

public sealed record Num(double Value) : Expr;

public sealed record Var(string Name) : Expr;

public sealed record Add(Expr Left, Expr Right) : Expr;

public sealed record Mul(Expr Left, Expr Right) : Expr;

public sealed record Neg(Expr Operand) : Expr;

public static class ExprTools
{
    public static double Evaluate(Expr expr, IReadOnlyDictionary<string, double> variables) => expr switch
    {
        Num(var value) => value,
        Var(var name) => variables.TryGetValue(name, out var value)
            ? value
            : throw new KeyNotFoundException($"Unknown variable '{name}'."),
        Add(var left, var right) => Evaluate(left, variables) + Evaluate(right, variables),
        Mul(var left, var right) => Evaluate(left, variables) * Evaluate(right, variables),
        Neg(var operand) => -Evaluate(operand, variables),
        _ => throw new ArgumentException($"Unsupported expression {expr.GetType().Name}.", nameof(expr)),
    };

    public static Expr Simplify(Expr expr)
    {
        var withSimplifiedChildren = expr switch
        {
            Add(var left, var right) => new Add(Simplify(left), Simplify(right)),
            Mul(var left, var right) => new Mul(Simplify(left), Simplify(right)),
            Neg(var operand) => new Neg(Simplify(operand)),
            _ => expr,
        };

        return withSimplifiedChildren switch
        {
            Add(Num(0), var x) => x,
            Add(var x, Num(0)) => x,
            Mul(Num(0), _) or Mul(_, Num(0)) => new Num(0),
            Mul(Num(1), var x) => x,
            Mul(var x, Num(1)) => x,
            Neg(Neg(var x)) => x,
            Add(Num(var a), Num(var b)) => new Num(a + b),
            Mul(Num(var a), Num(var b)) => new Num(a * b),
            Neg(Num(var a)) => new Num(-a),
            var other => other,
        };
    }

    public static string Print(Expr expr) => expr switch
    {
        Num(var value) => value.ToString(CultureInfo.InvariantCulture),
        Var(var name) => name,
        Add(var left, var right) => $"({Print(left)} + {Print(right)})",
        Mul(var left, var right) => $"({Print(left)} * {Print(right)})",
        Neg(var operand) => $"-{Print(operand)}",
        _ => throw new ArgumentException($"Unsupported expression {expr.GetType().Name}.", nameof(expr)),
    };
}
