using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.PatternsPro;

[TestClass]
public class PatternMatchingProTests
{
    private static Order MakeOrder(
        bool premium = false,
        decimal subtotal = 50m,
        int items = 1,
        string state = "MA",
        string country = "US") =>
        new(new Customer("Test", premium, new Address("Somewhere", state, country)), subtotal, items);

    [TestMethod]
    public void ShippingCost_AppliesRulesInOrder()
    {
        Assert.AreEqual(0m, Shipping.Cost(MakeOrder(premium: true, country: "CA")));
        Assert.AreEqual(0m, Shipping.Cost(MakeOrder(subtotal: 100m, country: "DE")));
        Assert.AreEqual(25m, Shipping.Cost(MakeOrder(country: "CA", items: 20)));
        Assert.AreEqual(15m, Shipping.Cost(MakeOrder(state: "HI", items: 20)));
        Assert.AreEqual(15m, Shipping.Cost(MakeOrder(state: "AK")));
        Assert.AreEqual(12m, Shipping.Cost(MakeOrder(items: 11)));
        Assert.AreEqual(7m, Shipping.Cost(MakeOrder(items: 10)));
    }

    [TestMethod]
    public void DescribeList_UsesListPatterns()
    {
        Assert.AreEqual("empty", Lists.Describe([]));
        Assert.AreEqual("one: 5", Lists.Describe([5]));
        Assert.AreEqual("pair: 1, 2", Lists.Describe([1, 2]));
        Assert.AreEqual("pair: 0, -1", Lists.Describe([0, -1]), "Two-element arrays match the pair rule first.");
        Assert.AreEqual("starts with zero", Lists.Describe([0, 4, 8]));
        Assert.AreEqual("ends negative", Lists.Describe([3, 4, -8]));
        Assert.AreEqual("1..9 (4 items)", Lists.Describe([1, 5, 7, 9]));
    }

    [TestMethod]
    [DataRow("help", "help")]
    [DataRow("go north", "move north")]
    [DataRow("go   west", "move west")]
    [DataRow("go up", "unknown")]
    [DataRow("go", "unknown")]
    [DataRow("take apple", "take apple")]
    [DataRow("take apple rope lamp", "take apple, rope, lamp")]
    [DataRow("take", "unknown")]
    [DataRow("say hello there", "say: hello there")]
    [DataRow("say", "unknown")]
    [DataRow("", "unknown")]
    [DataRow("dance", "unknown")]
    public void ParseCommand_MatchesSlices(string input, string expected) =>
        Assert.AreEqual(expected, Commands.Parse(input));

    [TestMethod]
    public void Evaluate_WalksTheTree()
    {
        // (x + 2) * -y
        var expr = new Mul(new Add(new Var("x"), new Num(2)), new Neg(new Var("y")));
        var variables = new Dictionary<string, double> { ["x"] = 3, ["y"] = 4 };

        Assert.AreEqual(-20, ExprTools.Evaluate(expr, variables), 1e-12);
    }

    [TestMethod]
    public void Evaluate_RejectsUnknownVariables() =>
        Assert.ThrowsExactly<KeyNotFoundException>(() =>
            ExprTools.Evaluate(new Var("missing"), new Dictionary<string, double>()));

    [TestMethod]
    public void Simplify_RemovesIdentities()
    {
        var x = new Var("x");

        Assert.AreEqual(x, ExprTools.Simplify(new Add(x, new Num(0))));
        Assert.AreEqual(x, ExprTools.Simplify(new Add(new Num(0), x)));
        Assert.AreEqual(x, ExprTools.Simplify(new Mul(new Num(1), x)));
        Assert.AreEqual(new Num(0), ExprTools.Simplify(new Mul(x, new Num(0))));
        Assert.AreEqual(x, ExprTools.Simplify(new Neg(new Neg(x))));
    }

    [TestMethod]
    public void Simplify_FoldsConstantsBottomUp()
    {
        // ((1 + 2) * x) + (0 * y)  =>  3 * x
        var expr = new Add(new Mul(new Add(new Num(1), new Num(2)), new Var("x")), new Mul(new Num(0), new Var("y")));

        Assert.AreEqual(new Mul(new Num(3), new Var("x")), ExprTools.Simplify(expr));
        Assert.AreEqual(new Num(-6), ExprTools.Simplify(new Neg(new Mul(new Num(2), new Num(3)))));
    }

    [TestMethod]
    public void Simplify_CascadesAcrossLevels()
    {
        // (x * (2 - 1)) where 2 - 1 is written as 2 + -1
        var expr = new Mul(new Var("x"), new Add(new Num(2), new Neg(new Num(1))));
        Assert.AreEqual(new Var("x"), ExprTools.Simplify(expr));
    }

    [TestMethod]
    public void Print_ParenthesizesBinaryOperators()
    {
        var expr = new Mul(new Add(new Var("x"), new Num(1.5)), new Neg(new Var("y")));
        Assert.AreEqual("((x + 1.5) * -y)", ExprTools.Print(expr));
    }
}
