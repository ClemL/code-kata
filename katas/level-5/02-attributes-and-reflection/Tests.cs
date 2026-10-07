using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.Reflection;

public sealed class ClaimLine
{
    [Required, MaxLength(8), CsvColumn("claim_id", Order = 1)]
    public string? ClaimId { get; set; }

    [ValidateWith<NdcValidator>, CsvColumn("ndc", Order = 2)]
    public string? Ndc { get; set; }

    [Range(1, 1000), CsvColumn("qty", Order = 3)]
    public int Quantity { get; set; }

    [Range(0, 99999.99), CsvColumn("paid", Order = 5, Format = "F2")]
    public decimal Paid { get; set; }

    [CsvColumn("fill_date", Order = 4, Format = "yyyy-MM-dd")]
    public DateOnly FillDate { get; set; }

    [CsvColumn("note", Order = 6)]
    public string? Note { get; set; }

    public string? InternalComment { get; set; }
}

[TestClass]
public class AttributesAndReflectionTests
{
    private static ClaimLine Valid() => new()
    {
        ClaimId = "C-1001",
        Ndc = "00002-1433",
        Quantity = 30,
        Paid = 12.5m,
        FillDate = new DateOnly(2026, 10, 7),
        Note = null,
        InternalComment = "never exported",
    };

    [TestMethod]
    public void NdcValidator_ChecksFormat()
    {
        var validator = new NdcValidator();
        Assert.IsNull(validator.Validate("Ndc", "12345-6789"));
        Assert.IsNull(validator.Validate("Ndc", null));
        Assert.AreEqual("Ndc must look like 12345-6789.", validator.Validate("Ndc", "1234-56789"));
        Assert.AreEqual("Code must look like 12345-6789.", validator.Validate("Code", "abcde-fghi"));
    }

    [TestMethod]
    public void Validate_AcceptsValidInstance() =>
        Assert.IsEmpty(AttributeValidator.Validate(Valid()));

    [TestMethod]
    public void Validate_ReportsEveryViolation()
    {
        var line = Valid();
        line.ClaimId = "   ";
        line.Ndc = "bad";
        line.Quantity = 0;
        line.Paid = 100_000m;

        CollectionAssert.AreEquivalent(
            new[]
            {
                "ClaimId is required.",
                "Ndc must look like 12345-6789.",
                "Quantity must be between 1 and 1000.",
                "Paid must be between 0 and 99999.99.",
            },
            AttributeValidator.Validate(line).ToArray());
    }

    [TestMethod]
    public void Validate_ChecksMaxLengthAndNullRequired()
    {
        var tooLong = Valid();
        tooLong.ClaimId = "C-123456789";
        CollectionAssert.AreEqual(new[] { "ClaimId must be at most 8 characters." }, AttributeValidator.Validate(tooLong).ToArray());

        var missing = Valid();
        missing.ClaimId = null;
        CollectionAssert.AreEqual(new[] { "ClaimId is required." }, AttributeValidator.Validate(missing).ToArray());
    }

    [TestMethod]
    public void Header_UsesAttributeNamesInOrder() =>
        Assert.AreEqual("claim_id,ndc,qty,fill_date,paid,note", CsvExporter.Header<ClaimLine>());

    [TestMethod]
    public void Row_FormatsValuesInvariantly()
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            Assert.AreEqual("C-1001,00002-1433,30,2026-10-07,12.50,", CsvExporter.Row(Valid()));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }

    [TestMethod]
    public void Row_QuotesFieldsThatNeedIt()
    {
        var line = Valid();
        line.Note = "said \"hold\", then left";

        Assert.AreEqual("C-1001,00002-1433,30,2026-10-07,12.50,\"said \"\"hold\"\", then left\"", CsvExporter.Row(line));
    }
}
