namespace Katas.L2.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) => throw new NotImplementedException();

    public ConfigurationException(string message, Exception innerException) => throw new NotImplementedException();
}

public class ValidationException : Exception
{
    public ValidationException(IEnumerable<string> errors) => throw new NotImplementedException();

    public IReadOnlyList<string> Errors => throw new NotImplementedException();
}

public static class Config
{
    public static int ReadPort(IReadOnlyDictionary<string, string> settings) => throw new NotImplementedException();
}

public static class OrderValidator
{
    public static void Validate(string? customerId, int quantity, decimal unitPrice) => throw new NotImplementedException();
}

public static class Retry
{
    public static T Execute<T>(Func<T> action, int maxAttempts, Func<Exception, bool> isTransient) =>
        throw new NotImplementedException();
}

public static class Diagnostics
{
    public static void LogAndRethrow(Action action, List<string> log) => throw new NotImplementedException();
}
