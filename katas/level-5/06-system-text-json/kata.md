---
title: System.Text.Json Deep Dive
level: 5
minutes: 45
summary: Custom JsonConverter<T>, attribute-driven polymorphism, snake_case options, source-generated serializer contexts, and reading JSON with JsonDocument.
topics: [Serialization, JSON, Source generators, Polymorphism]
keywords: [partial, override, "[JsonPolymorphic]", "[JsonDerivedType]"]
apis: ["JsonConverter<T>", Utf8JsonReader, Utf8JsonWriter, JsonSerializerOptions, JsonNamingPolicy.SnakeCaseLower, JsonSerializerContext, JsonDocument]
docs:
  - title: How to write custom converters
    url: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/converters-how-to
  - title: Serialize properties of derived classes (polymorphism)
    url: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/polymorphism
  - title: Source generation in System.Text.Json
    url: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation
  - title: Use a JSON DOM (JsonDocument and JsonNode)
    url: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/use-dom
---

## Brief

`System.Text.Json` is the default serializer for ASP.NET Core, Azure Functions, and the Azure SDKs. Its defaults (case-sensitive, no polymorphism unless declared, strict about numbers in strings) surprise people coming from Newtonsoft.Json. This kata covers the four extension points used most in real services: converters, polymorphism attributes, options, and source generation.

## Tasks

1. **`MoneyConverter`** (`JsonConverter<Money>`) writes `Money(12.5m, "USD")` as the JSON string `"12.50 USD"` and reads it back. Malformed strings, or a token that is not a string, throw `JsonException`.
2. **Polymorphism.** Add attributes to the `Shape` hierarchy so that serializing as `Shape` writes a `"kind"` discriminator (`"circle"` or `"rectangle"`) and deserializing `Shape` creates the right subtype.
3. **`JsonKata.Options`** is a shared `JsonSerializerOptions` with:
   - `snake_case_lower` property names;
   - nulls omitted when writing;
   - enums written as `snake_case_lower` strings;
   - the `MoneyConverter` registered.
4. **`KataJsonContext`** is a source-generated `JsonSerializerContext` for `PatientDto` that uses `snake_case_lower` names. Add the generation options attribute to the provided partial class.
5. **`JsonKata.SumPaid(string json)`** reads an array of claim objects with `JsonDocument` and returns the sum of their `paid_amount` numbers, skipping objects where it is missing or `null`.

## Hints

- In `Read`, check `reader.TokenType == JsonTokenType.String`, then `reader.GetString()` and split on the space. Parse the amount with `decimal.TryParse(..., NumberStyles.Number, CultureInfo.InvariantCulture, ...)`.
- In `Write`, `writer.WriteStringValue(string.Create(CultureInfo.InvariantCulture, $"{value.Amount:F2} {value.Currency}"))`.
- `[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]` and one `[JsonDerivedType(typeof(Circle), "circle")]` per subtype, on the base type.
- `new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower), new MoneyConverter() } }`.
- `[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]` on the context class.
- `using var doc = JsonDocument.Parse(json); foreach (var element in doc.RootElement.EnumerateArray()) if (element.TryGetProperty("paid_amount", out var paid) && paid.ValueKind == JsonValueKind.Number) total += paid.GetDecimal();`

## Notes

- **Reuse `JsonSerializerOptions`.** Each options instance caches reflection metadata; creating one per call is a well-known performance bug, and .NET 8+ warns about it (CA1869).
- **The discriminator must come first** when deserializing polymorphic JSON unless `AllowOutOfOrderMetadataProperties` (.NET 9) is enabled, because the reader is forward-only.
- **Source generation** moves metadata creation to compile time: faster startup, lower memory, and compatibility with trimming and Native AOT. The generated code is inspectable under *Dependencies → Analyzers* in Visual Studio or Rider.
- **`JsonDocument` is pooled and disposable.** It rents buffers from `ArrayPool`; forgetting `using` loses the pooling benefit. `JsonNode` is the mutable, easier, slower alternative.
- **Newtonsoft migration traps:** case-sensitive matching by default (`PropertyNameCaseInsensitive`), no comments or trailing commas by default, and `[JsonProperty]` → `[JsonPropertyName]`.
