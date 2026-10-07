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
    public static decimal TotalPaid(IEnumerable<Claim> claims) => throw new NotImplementedException();

    public static IReadOnlyList<(string DrugName, decimal Spend)> TopDrugsBySpend(IEnumerable<Claim> claims, int n) =>
        throw new NotImplementedException();

    public static IReadOnlyList<(DateOnly Month, decimal Spend)> MonthlySpend(IEnumerable<Claim> claims) =>
        throw new NotImplementedException();

    public static IReadOnlyList<string> PharmaciesWithReversalRateAbove(IEnumerable<Claim> claims, double threshold) =>
        throw new NotImplementedException();

    public static IReadOnlyDictionary<string, double> AverageQuantityByNdc(IEnumerable<Claim> claims) =>
        throw new NotImplementedException();

    public static IReadOnlyDictionary<string, Claim> FirstFillPerDrug(IEnumerable<Claim> claims) =>
        throw new NotImplementedException();

    public static IReadOnlyList<string> DuplicateClaimIds(IEnumerable<Claim> claims) => throw new NotImplementedException();

    public static IReadOnlyDictionary<string, int> ClaimCountByPharmacy(IEnumerable<Claim> claims) =>
        throw new NotImplementedException();

    public static IReadOnlyDictionary<string, decimal> SpendByPharmacy(IEnumerable<Claim> claims) =>
        throw new NotImplementedException();
}
