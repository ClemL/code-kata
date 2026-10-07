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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Circle), "circle")]
[JsonDerivedType(typeof(Rectangle), "rectangle")]
public abstract record Shape;

public sealed record Circle(double Radius) : Shape;

public sealed record Rectangle(double Width, double Height) : Shape;

public sealed class MoneyConverter : JsonConverter<Money>
{
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a string for Money but found {reader.TokenType}.");
        }

        var text = reader.GetString()!;
        var parts = text.Split(' ');

        if (parts is not [var amountText, { Length: 3 } currency]
            || !decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            throw new JsonException($"'{text}' is not a valid money value. Expected '12.34 USD'.");
        }

        return new Money(amount, currency);
    }

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options) =>
        writer.WriteStringValue(string.Create(CultureInfo.InvariantCulture, $"{value.Amount:F2} {value.Currency}"));
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(PatientDto))]
public sealed partial class KataJsonContext : JsonSerializerContext;

public static class JsonKata
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
            new MoneyConverter(),
        },
    };

    public static decimal SumPaid(string json)
    {
        using var document = JsonDocument.Parse(json);

        var total = 0m;
        foreach (var claim in document.RootElement.EnumerateArray())
        {
            if (claim.TryGetProperty("paid_amount", out var paid) && paid.ValueKind == JsonValueKind.Number)
            {
                total += paid.GetDecimal();
            }
        }

        return total;
    }
}
