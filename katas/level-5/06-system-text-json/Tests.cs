using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.Json;

[TestClass]
public class SystemTextJsonTests
{
    private static readonly JsonSerializerOptions ConverterOnly = new() { Converters = { new MoneyConverter() } };

    [TestMethod]
    public void MoneyConverter_WritesAndReadsCompactStrings()
    {
        var json = JsonSerializer.Serialize(new Money(12.5m, "USD"), ConverterOnly);
        Assert.AreEqual("\"12.50 USD\"", json);

        Assert.AreEqual(new Money(1234.5m, "EUR"), JsonSerializer.Deserialize<Money>("\"1234.50 EUR\"", ConverterOnly));
    }

    [TestMethod]
    [DataRow("\"12.50\"")]
    [DataRow("\"twelve USD\"")]
    [DataRow("\"12.50 DOLLARS\"")]
    [DataRow("12.5")]
    [DataRow("{\"amount\":1}")]
    public void MoneyConverter_RejectsMalformedInput(string json) =>
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Money>(json, ConverterOnly));

    [TestMethod]
    public void Shapes_RoundTripWithDiscriminator()
    {
        Shape[] shapes = [new Circle(1.5), new Rectangle(2, 3)];

        var json = JsonSerializer.Serialize(shapes);
        Assert.AreEqual("[{\"kind\":\"circle\",\"Radius\":1.5},{\"kind\":\"rectangle\",\"Width\":2,\"Height\":3}]", json);

        var back = JsonSerializer.Deserialize<Shape[]>(json)!;
        Assert.IsInstanceOfType<Circle>(back[0]);
        Assert.AreEqual(new Rectangle(2, 3), back[1]);
    }

    [TestMethod]
    public void Options_ProduceSnakeCaseAndOmitNulls()
    {
        var claim = new ClaimDto("C-1", ClaimStatus.PendingReview, new Money(9.99m, "USD"), Note: null);

        var json = JsonSerializer.Serialize(claim, JsonKata.Options);

        Assert.AreEqual("{\"claim_id\":\"C-1\",\"status\":\"pending_review\",\"paid_amount\":\"9.99 USD\"}", json);
    }

    [TestMethod]
    public void Options_RoundTrip()
    {
        var claim = new ClaimDto("C-2", ClaimStatus.Reversed, new Money(100m, "USD"), "duplicate");
        var options = JsonKata.Options;
        var json = JsonSerializer.Serialize(claim, options);

        Assert.AreEqual(claim, JsonSerializer.Deserialize<ClaimDto>(json, JsonKata.Options));
        Assert.AreSame(options, JsonKata.Options, "Reuse a single options instance.");
    }

    [TestMethod]
    public void SourceGeneratedContext_UsesSnakeCase()
    {
        var patient = new PatientDto("Ada", "Lovelace", new DateOnly(1815, 12, 10));

        var json = JsonSerializer.Serialize(patient, KataJsonContext.Default.PatientDto);

        Assert.AreEqual("{\"first_name\":\"Ada\",\"last_name\":\"Lovelace\",\"date_of_birth\":\"1815-12-10\"}", json);
        Assert.AreEqual(patient, JsonSerializer.Deserialize(json, KataJsonContext.Default.PatientDto));
    }

    [TestMethod]
    public void SumPaid_ReadsWithJsonDocument()
    {
        const string json = """
            [
              { "claim_id": "C-1", "paid_amount": 12.50 },
              { "claim_id": "C-2", "paid_amount": null },
              { "claim_id": "C-3" },
              { "claim_id": "C-4", "paid_amount": 7.25 }
            ]
            """;

        Assert.AreEqual(19.75m, JsonKata.SumPaid(json));
        Assert.AreEqual(0m, JsonKata.SumPaid("[]"));
    }
}
