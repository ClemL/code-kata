---
title: Attributes & Reflection
level: 5
minutes: 45
summary: Read custom and generic attributes with reflection to build a declarative validator and a CSV exporter, caching metadata per type with a generic static class.
topics: [Reflection, Attributes, Metadata, Serialization]
keywords: [typeof, "[Attribute]", sealed, static, "new()"]
apis: [Type.GetProperties, PropertyInfo.GetValue, GetCustomAttribute, AttributeUsageAttribute, IFormattable, MetadataToken]
docs:
  - title: Attributes and reflection
    url: https://learn.microsoft.com/dotnet/csharp/advanced-topics/reflection-and-attributes/
  - title: Create custom attributes
    url: https://learn.microsoft.com/dotnet/csharp/advanced-topics/reflection-and-attributes/creating-custom-attributes
  - title: Generic attributes (C# 11)
    url: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-11#generic-attributes
  - title: RFC 4180 (CSV format)
    url: https://www.rfc-editor.org/rfc/rfc4180
---

## Brief

ASP.NET Core model validation, EF Core mapping, `System.Text.Json`, and MSTest itself are all driven by attributes read through reflection. This kata builds a small version of two of those systems: a validator driven by `[Required]`, `[Range]`, `[MaxLength]`, and a C# 11 **generic attribute** `[ValidateWith<TValidator>]`, and a CSV exporter driven by `[CsvColumn]`.

The attribute classes are provided. You write the code that reads them.

## Tasks

1. **`NdcValidator`** implements `IValueValidator`: a non-null value must look like `#####-####` (5 digits, dash, 4 digits), otherwise return `"{property} must look like 12345-6789."`. `null` is valid (that is `[Required]`'s job).
2. **`AttributeValidator.Validate(object instance)`** inspects every public instance property and returns all error messages (order does not matter):
   - `[Required]`: null, or a string that is empty/whitespace → `"{Name} is required."`
   - `[MaxLength(n)]`: a string longer than `n` → `"{Name} must be at most {n} characters."`
   - `[Range(min, max)]`: a numeric value outside the inclusive range → `"{Name} must be between {min} and {max}."`
   - `[ValidateWith<T>]`: create `T` and call its `Validate(propertyName, value)`; a non-null result is an error.
3. **`CsvExporter.Header<T>()`** returns the column names of properties marked `[CsvColumn]`, ordered by `Order`, joined with commas. Unmarked properties are skipped.
4. **`CsvExporter.Row<T>(T item)`** returns the row in the same column order:
   - `null` → empty field;
   - `IFormattable` values use the attribute's `Format` (if any) and the invariant culture;
   - other values use `ToString()`;
   - a field containing `,`, `"`, `\r`, or `\n` is wrapped in double quotes with inner quotes doubled (RFC 4180).
5. **Cache the reflection work.** Looking up properties and attributes on every row is slow; compute the column list once per type.

## Hints

- `instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)`.
- `property.GetCustomAttribute<RangeAttribute>()` returns `null` when absent.
- Generic attributes are found by their open type: `property.GetCustomAttributes().Where(a => a.GetType().IsGenericType && a.GetType().GetGenericTypeDefinition() == typeof(ValidateWithAttribute<>))`. Exposing a non-generic base class or interface on the attribute is simpler: the provided `ValidateWithAttribute<T>` derives from `ValidateWithAttribute`, which has a `CreateValidator()` method.
- `Convert.ToDouble(value, CultureInfo.InvariantCulture)` normalizes `int`, `decimal`, and `double` for range checks.
- A **generic static class** is a per-type cache: `static class Columns<T> { public static readonly IReadOnlyList<Column> All = Build(typeof(T)); }`. The runtime initializes it once per `T`, thread-safely.
- `formattable.ToString(format, CultureInfo.InvariantCulture)`.

## Notes

- **Reflection order is not guaranteed.** `GetProperties` returns declaration order in practice, but the documentation does not promise it. That is why the CSV columns carry an explicit `Order`.
- **Attribute arguments must be compile-time constants** (primitives, strings, `typeof`, enums, and arrays of those). Generic attributes (C# 11) let you pass a *type* with compile-time checking instead of `typeof(...)` plus a runtime cast.
- **Trimming and Native AOT.** Reflection over arbitrary types is not trim-safe. The modern alternative is a **source generator** that emits the same code at compile time, which is exactly what `System.Text.Json`'s `JsonSerializerContext` and the configuration binder generator do.
- **`System.ComponentModel.DataAnnotations`** contains production versions of these attributes and `Validator.TryValidateObject`; ASP.NET Core and Blazor use them.
