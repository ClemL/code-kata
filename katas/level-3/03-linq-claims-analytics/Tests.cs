using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.Linq;

[TestClass]
public class LinqClaimsAnalyticsTests
{
    private static readonly Claim[] Claims =
    [
        new("C001", "PH1", "00002-1433", "Atorvastatin", new DateOnly(2026, 1, 5), 30, 12.50m, false),
        new("C002", "PH1", "00071-0155", "Lisinopril", new DateOnly(2026, 1, 12), 30, 8.00m, false),
        new("C003", "PH2", "00002-1433", "Atorvastatin", new DateOnly(2026, 1, 20), 90, 35.00m, false),
        new("C004", "PH2", "50090-3352", "Eliquis", new DateOnly(2026, 2, 3), 60, 520.00m, false),
        new("C005", "PH2", "50090-3352", "Eliquis", new DateOnly(2026, 2, 3), 60, 520.00m, true),
        new("C006", "PH3", "00071-0155", "Lisinopril", new DateOnly(2026, 2, 14), 90, 21.00m, false),
        new("C007", "PH3", "50090-3352", "Eliquis", new DateOnly(2026, 3, 1), 60, 515.00m, false),
        new("C008", "PH1", "00002-1433", "Atorvastatin", new DateOnly(2026, 3, 9), 30, 12.50m, true),
        new("C009", "PH1", "61958-2501", "Ozempic", new DateOnly(2026, 3, 15), 1, 935.00m, false),
        new("C010", "PH3", "61958-2501", "Ozempic", new DateOnly(2026, 3, 28), 1, 940.00m, false),
        new("C011", "PH2", "00071-0155", "Lisinopril", new DateOnly(2026, 3, 30), 30, 8.00m, false),
        new("C003", "PH2", "00002-1433", "Atorvastatin", new DateOnly(2026, 1, 21), 90, 35.00m, false),
    ];

    [TestMethod]
    public void TotalPaid_ExcludesReversals()
    {
        Assert.AreEqual(3029.50m, ClaimsReport.TotalPaid(Claims));
        Assert.AreEqual(0m, ClaimsReport.TotalPaid([]));
    }

    [TestMethod]
    public void TopDrugsBySpend_OrdersBySpendThenName()
    {
        var top = ClaimsReport.TopDrugsBySpend(Claims, 3);

        Assert.HasCount(3, top);
        Assert.AreEqual(("Ozempic", 1875.00m), top[0]);
        Assert.AreEqual(("Eliquis", 1035.00m), top[1]);
        Assert.AreEqual(("Atorvastatin", 82.50m), top[2]);
    }

    [TestMethod]
    public void TopDrugsBySpend_BreaksTiesByName()
    {
        Claim[] tied =
        [
            new("X1", "PH9", "1", "Zeta", new DateOnly(2026, 1, 1), 1, 10m, false),
            new("X2", "PH9", "2", "Alpha", new DateOnly(2026, 1, 1), 1, 10m, false),
        ];

        var top = ClaimsReport.TopDrugsBySpend(tied, 2);

        Assert.AreEqual("Alpha", top[0].DrugName);
        Assert.AreEqual("Zeta", top[1].DrugName);
    }

    [TestMethod]
    public void MonthlySpend_IsChronological()
    {
        var months = ClaimsReport.MonthlySpend(Claims);

        Assert.HasCount(3, months);
        Assert.AreEqual((new DateOnly(2026, 1, 1), 90.50m), months[0]);
        Assert.AreEqual((new DateOnly(2026, 2, 1), 541.00m), months[1]);
        Assert.AreEqual((new DateOnly(2026, 3, 1), 2398.00m), months[2]);
    }

    [TestMethod]
    public void PharmaciesWithReversalRateAbove_UsesStrictComparison()
    {
        CollectionAssert.AreEqual(new[] { "PH1" }, ClaimsReport.PharmaciesWithReversalRateAbove(Claims, 0.2).ToArray());
        CollectionAssert.AreEqual(new[] { "PH1", "PH2" }, ClaimsReport.PharmaciesWithReversalRateAbove(Claims, 0.1).ToArray());
        Assert.IsEmpty(ClaimsReport.PharmaciesWithReversalRateAbove(Claims, 0.5));
    }

    [TestMethod]
    public void AverageQuantityByNdc_AveragesPaidClaims()
    {
        var averages = ClaimsReport.AverageQuantityByNdc(Claims);

        Assert.HasCount(4, averages);
        Assert.AreEqual(70.0, averages["00002-1433"], 1e-9);
        Assert.AreEqual(50.0, averages["00071-0155"], 1e-9);
        Assert.AreEqual(60.0, averages["50090-3352"], 1e-9);
    }

    [TestMethod]
    public void FirstFillPerDrug_ReturnsEarliestPaidClaim()
    {
        var first = ClaimsReport.FirstFillPerDrug(Claims);

        Assert.AreEqual("C001", first["Atorvastatin"].ClaimId);
        Assert.AreEqual("C002", first["Lisinopril"].ClaimId);
        Assert.AreEqual("C004", first["Eliquis"].ClaimId);
        Assert.AreEqual("C009", first["Ozempic"].ClaimId);
    }

    [TestMethod]
    public void DuplicateClaimIds_FindsRepeats()
    {
        CollectionAssert.AreEqual(new[] { "C003" }, ClaimsReport.DuplicateClaimIds(Claims).ToArray());
        Assert.IsEmpty(ClaimsReport.DuplicateClaimIds(Claims.Take(5)));
    }

    [TestMethod]
    public void ClaimCountByPharmacy_CountsAllClaims()
    {
        var counts = ClaimsReport.ClaimCountByPharmacy(Claims);

        Assert.AreEqual(4, counts["PH1"]);
        Assert.AreEqual(5, counts["PH2"]);
        Assert.AreEqual(3, counts["PH3"]);
    }

    [TestMethod]
    public void SpendByPharmacy_ExcludesReversals()
    {
        var spend = ClaimsReport.SpendByPharmacy(Claims);

        Assert.AreEqual(955.50m, spend["PH1"]);
        Assert.AreEqual(598.00m, spend["PH2"]);
        Assert.AreEqual(1476.00m, spend["PH3"]);
    }
}
