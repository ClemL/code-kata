using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Katas.L5.Reflection;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxLengthAttribute(int length) : Attribute
{
    public int Length { get; } = length;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class RangeAttribute(double minimum, double maximum) : Attribute
{
    public double Minimum { get; } = minimum;

    public double Maximum { get; } = maximum;
}

public interface IValueValidator
{
    string? Validate(string propertyName, object? value);
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public abstract class ValidateWithAttribute : Attribute
{
    public abstract IValueValidator CreateValidator();
}

public sealed class ValidateWithAttribute<TValidator> : ValidateWithAttribute
    where TValidator : IValueValidator, new()
{
    public override IValueValidator CreateValidator() => new TValidator();
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class CsvColumnAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public int Order { get; init; }

    public string? Format { get; init; }
}

public sealed partial class NdcValidator : IValueValidator
{
    [GeneratedRegex(@"^\d{5}-\d{4}$")]
    private static partial Regex NdcPattern();

    public string? Validate(string propertyName, object? value) =>
        value is null || (value is string text && NdcPattern().IsMatch(text))
            ? null
            : $"{propertyName} must look like 12345-6789.";
}

public static class AttributeValidator
{
    public static IReadOnlyList<string> Validate(object instance)
    {
        var errors = new List<string>();

        foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(instance);
            var name = property.Name;

            if (property.GetCustomAttribute<RequiredAttribute>() is not null
                && (value is null || value is string s && string.IsNullOrWhiteSpace(s)))
            {
                errors.Add($"{name} is required.");
            }

            if (property.GetCustomAttribute<MaxLengthAttribute>() is { } maxLength
                && value is string text
                && text.Length > maxLength.Length)
            {
                errors.Add($"{name} must be at most {maxLength.Length} characters.");
            }

            if (property.GetCustomAttribute<RangeAttribute>() is { } range && value is IConvertible)
            {
                var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (number < range.Minimum || number > range.Maximum)
                {
                    errors.Add(string.Create(CultureInfo.InvariantCulture, $"{name} must be between {range.Minimum} and {range.Maximum}."));
                }
            }

            foreach (var validateWith in property.GetCustomAttributes<ValidateWithAttribute>())
            {
                if (validateWith.CreateValidator().Validate(name, value) is { } error)
                {
                    errors.Add(error);
                }
            }
        }

        return errors;
    }
}

public static class CsvExporter
{
    private sealed record Column(PropertyInfo Property, CsvColumnAttribute Attribute);

    // A generic static class is initialized once per closed type: a free, thread-safe per-type cache.
    private static class Columns<T>
    {
        public static readonly IReadOnlyList<Column> All = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<CsvColumnAttribute>()))
            .Where(x => x.Attribute is not null)
            .Select(x => new Column(x.Property, x.Attribute!))
            .OrderBy(c => c.Attribute.Order)
            .ToArray();

        public static readonly string Header = string.Join(',', All.Select(c => Escape(c.Attribute.Name)));
    }

    public static string Header<T>() => Columns<T>.Header;

    public static string Row<T>(T item) =>
        string.Join(',', Columns<T>.All.Select(column => Escape(Format(column.Property.GetValue(item), column.Attribute.Format))));

    private static string Format(object? value, string? format) => value switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static string Escape(string field) =>
        field.AsSpan().IndexOfAny(",\"\r\n") >= 0
            ? $"\"{field.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : field;
}
