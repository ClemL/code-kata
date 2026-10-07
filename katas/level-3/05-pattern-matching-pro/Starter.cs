using System.Globalization;

namespace Katas.L3.PatternsPro;

public sealed record Address(string City, string State, string Country);

public sealed record Customer(string Name, bool IsPremium, Address Address);

public sealed record Order(Customer Customer, decimal Subtotal, int Items);

public static class Shipping
{
    public static decimal Cost(Order order) => throw new NotImplementedException();
}

public static class Lists
{
    public static string Describe(int[] values) => throw new NotImplementedException();
}

public static class Commands
{
    public static string Parse(string input) => throw new NotImplementedException();
}

public abstract record Expr;

public sealed record Num(double Value) : Expr;

public sealed record Var(string Name) : Expr;

public sealed record Add(Expr Left, Expr Right) : Expr;

public sealed record Mul(Expr Left, Expr Right) : Expr;

public sealed record Neg(Expr Operand) : Expr;

public static class ExprTools
{
    public static double Evaluate(Expr expr, IReadOnlyDictionary<string, double> variables) =>
        throw new NotImplementedException();

    public static Expr Simplify(Expr expr) => throw new NotImplementedException();

    public static string Print(Expr expr) => throw new NotImplementedException();
}
