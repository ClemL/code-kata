using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Exceptions;

[TestClass]
public class ExceptionsTests
{
    [TestMethod]
    public void ConfigurationException_SupportsStandardConstructors()
    {
        var inner = new FormatException("bad");
        var ex = new ConfigurationException("outer", inner);

        Assert.AreEqual("outer", ex.Message);
        Assert.AreSame(inner, ex.InnerException);
        Assert.AreEqual("plain", new ConfigurationException("plain").Message);
    }

    [TestMethod]
    public void ReadPort_ReturnsValidPort() =>
        Assert.AreEqual(8080, Config.ReadPort(new Dictionary<string, string> { ["Port"] = "8080" }));

    [TestMethod]
    public void ReadPort_ReportsMissingSetting()
    {
        var ex = Assert.ThrowsExactly<ConfigurationException>(() => Config.ReadPort(new Dictionary<string, string>()));
        StringAssert.Contains(ex.Message, "Port");
        Assert.IsNull(ex.InnerException);
    }

    [TestMethod]
    public void ReadPort_WrapsParseFailure()
    {
        var ex = Assert.ThrowsExactly<ConfigurationException>(() =>
            Config.ReadPort(new Dictionary<string, string> { ["Port"] = "eighty" }));

        Assert.IsInstanceOfType<FormatException>(ex.InnerException);
    }

    [TestMethod]
    [DataRow("0")]
    [DataRow("65536")]
    [DataRow("-1")]
    public void ReadPort_RejectsOutOfRangePort(string value) =>
        Assert.ThrowsExactly<ConfigurationException>(() =>
            Config.ReadPort(new Dictionary<string, string> { ["Port"] = value }));

    [TestMethod]
    public void Validate_PassesValidOrder() =>
        OrderValidator.Validate("C-42", 3, 9.99m);

    [TestMethod]
    public void Validate_CollectsEveryError()
    {
        var ex = Assert.ThrowsExactly<ValidationException>(() => OrderValidator.Validate("  ", 0, -1m));

        CollectionAssert.AreEqual(
            new[] { "CustomerId is required.", "Quantity must be between 1 and 1000.", "UnitPrice must be positive." },
            ex.Errors.ToArray());
        Assert.AreEqual(
            "CustomerId is required.; Quantity must be between 1 and 1000.; UnitPrice must be positive.",
            ex.Message);
    }

    [TestMethod]
    public void Validate_ReportsSingleError()
    {
        var ex = Assert.ThrowsExactly<ValidationException>(() => OrderValidator.Validate("C-1", 1001, 1m));
        Assert.HasCount(1, ex.Errors);
    }

    [TestMethod]
    public void Retry_RetriesTransientFailures()
    {
        var calls = 0;
        var result = Retry.Execute(
            () => ++calls < 3 ? throw new TimeoutException() : "ok",
            maxAttempts: 5,
            isTransient: ex => ex is TimeoutException);

        Assert.AreEqual("ok", result);
        Assert.AreEqual(3, calls);
    }

    [TestMethod]
    public void Retry_StopsAtMaxAttemptsAndPropagatesLastFailure()
    {
        var calls = 0;
        var ex = Assert.ThrowsExactly<TimeoutException>(() => Retry.Execute<int>(
            () => throw new TimeoutException($"attempt {++calls}"),
            maxAttempts: 3,
            isTransient: _ => true));

        Assert.AreEqual(3, calls);
        Assert.AreEqual("attempt 3", ex.Message);
    }

    [TestMethod]
    public void Retry_DoesNotRetryPermanentFailures()
    {
        var calls = 0;
        Assert.ThrowsExactly<InvalidOperationException>(() => Retry.Execute<int>(
            () => { calls++; throw new InvalidOperationException(); },
            maxAttempts: 5,
            isTransient: ex => ex is TimeoutException));

        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void LogAndRethrow_LogsAndPreservesStackTrace()
    {
        var log = new List<string>();

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            Diagnostics.LogAndRethrow(ThrowFromDeepInside, log));

        CollectionAssert.AreEqual(new[] { "InvalidOperationException: boom" }, log);
        StringAssert.Contains(ex.StackTrace, nameof(ThrowFromDeepInside), "Use `throw;` rather than `throw ex;`.");
    }

    [TestMethod]
    public void LogAndRethrow_DoesNothingOnSuccess()
    {
        var log = new List<string>();
        Diagnostics.LogAndRethrow(() => { }, log);
        Assert.IsEmpty(log);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowFromDeepInside() => throw new InvalidOperationException("boom");
}
