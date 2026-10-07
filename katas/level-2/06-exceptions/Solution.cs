using System.Globalization;

namespace Katas.L2.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message)
        : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ValidationException : Exception
{
    public ValidationException(IEnumerable<string> errors)
        : this(errors.ToArray())
    {
    }

    private ValidationException(string[] errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}

public static class Config
{
    public static int ReadPort(IReadOnlyDictionary<string, string> settings)
    {
        if (!settings.TryGetValue("Port", out var raw))
        {
            throw new ConfigurationException("Missing required setting 'Port'.");
        }

        int port;
        try
        {
            port = int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new ConfigurationException($"Setting 'Port' is not an integer: '{raw}'.", ex);
        }

        if (port is < 1 or > 65535)
        {
            throw new ConfigurationException($"Setting 'Port' must be between 1 and 65535, but was {port}.");
        }

        return port;
    }
}

public static class OrderValidator
{
    public static void Validate(string? customerId, int quantity, decimal unitPrice)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(customerId))
        {
            errors.Add("CustomerId is required.");
        }

        if (quantity is < 1 or > 1000)
        {
            errors.Add("Quantity must be between 1 and 1000.");
        }

        if (unitPrice <= 0)
        {
            errors.Add("UnitPrice must be positive.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}

public static class Retry
{
    public static T Execute<T>(Func<T> action, int maxAttempts, Func<Exception, bool> isTransient)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex) when (attempt < maxAttempts && isTransient(ex))
            {
                // Transient and attempts remain: loop around and try again.
            }
        }
    }
}

public static class Diagnostics
{
    public static void LogAndRethrow(Action action, List<string> log)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            log.Add($"{ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
