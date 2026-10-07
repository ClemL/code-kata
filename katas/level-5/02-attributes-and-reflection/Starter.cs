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

public sealed class NdcValidator : IValueValidator
{
    public string? Validate(string propertyName, object? value) => throw new NotImplementedException();
}

public static class AttributeValidator
{
    public static IReadOnlyList<string> Validate(object instance) => throw new NotImplementedException();
}

public static class CsvExporter
{
    public static string Header<T>() => throw new NotImplementedException();

    public static string Row<T>(T item) => throw new NotImplementedException();
}
