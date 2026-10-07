using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Katas.L5.Json;

public sealed record Money(decimal Amount, string Currency);

public enum ClaimStatus
{
    Paid,
    Reversed,
    PendingReview,
}

public sealed record ClaimDto(string ClaimId, ClaimStatus Status, Money PaidAmount, string? Note);

public sealed record PatientDto(string FirstName, string LastName, DateOnly DateOfBirth);

// TODO: add polymorphism attributes so serializing as Shape includes a "kind" discriminator.
public abstract record Shape;

public sealed record Circle(double Radius) : Shape;

public sealed record Rectangle(double Width, double Height) : Shape;

public sealed class MoneyConverter : JsonConverter<Money>
{
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options) =>
        throw new NotImplementedException();
}

// TODO: configure generation options so property names are snake_case_lower.
[JsonSerializable(typeof(PatientDto))]
public sealed partial class KataJsonContext : JsonSerializerContext;

public static class JsonKata
{
    public static JsonSerializerOptions Options => throw new NotImplementedException();

    public static decimal SumPaid(string json) => throw new NotImplementedException();
}
