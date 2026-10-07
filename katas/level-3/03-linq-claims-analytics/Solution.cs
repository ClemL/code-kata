namespace Katas.L3.Linq;

public sealed record Claim(
    string ClaimId,
    string PharmacyId,
    string Ndc,
    string DrugName,
    DateOnly FillDate,
    int Quantity,
    decimal PaidAmount,
    bool IsReversed);

public static class ClaimsReport
{
    private static IEnumerable<Claim> Paid(IEnumerable<Claim> claims) => claims.Where(c => !c.IsReversed);

    public static decimal TotalPaid(IEnumerable<Claim> claims) =>
        Paid(claims).Sum(c => c.PaidAmount);

    public static IReadOnlyList<(string DrugName, decimal Spend)> TopDrugsBySpend(IEnumerable<Claim> claims, int n) =>
        Paid(claims)
            .GroupBy(c => c.DrugName)
            .Select(g => (DrugName: g.Key, Spend: g.Sum(c => c.PaidAmount)))
            .OrderByDescending(x => x.Spend)
            .ThenBy(x => x.DrugName, StringComparer.Ordinal)
            .Take(n)
            .ToList();

    public static IReadOnlyList<(DateOnly Month, decimal Spend)> MonthlySpend(IEnumerable<Claim> claims) =>
        Paid(claims)
            .GroupBy(c => new DateOnly(c.FillDate.Year, c.FillDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => (g.Key, g.Sum(c => c.PaidAmount)))
            .ToList();

    public static IReadOnlyList<string> PharmaciesWithReversalRateAbove(IEnumerable<Claim> claims, double threshold) =>
        claims
            .GroupBy(c => c.PharmacyId)
            .Where(g => g.Count(c => c.IsReversed) / (double)g.Count() > threshold)
            .Select(g => g.Key)
            .Order(StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyDictionary<string, double> AverageQuantityByNdc(IEnumerable<Claim> claims) =>
        Paid(claims)
            .GroupBy(c => c.Ndc)
            .ToDictionary(g => g.Key, g => g.Average(c => c.Quantity));

    public static IReadOnlyDictionary<string, Claim> FirstFillPerDrug(IEnumerable<Claim> claims) =>
        Paid(claims)
            .GroupBy(c => c.DrugName)
            .ToDictionary(g => g.Key, g => g.MinBy(c => c.FillDate)!);

    public static IReadOnlyList<string> DuplicateClaimIds(IEnumerable<Claim> claims) =>
        claims
            .CountBy(c => c.ClaimId)
            .Where(pair => pair.Value > 1)
            .Select(pair => pair.Key)
            .Order(StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyDictionary<string, int> ClaimCountByPharmacy(IEnumerable<Claim> claims) =>
        claims.CountBy(c => c.PharmacyId).ToDictionary();

    public static IReadOnlyDictionary<string, decimal> SpendByPharmacy(IEnumerable<Claim> claims) =>
        Paid(claims)
            .AggregateBy(c => c.PharmacyId, 0m, (sum, c) => sum + c.PaidAmount)
            .ToDictionary();
}
