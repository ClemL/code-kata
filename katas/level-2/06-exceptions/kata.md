---
title: Exceptions Done Right
level: 2
minutes: 30
summary: Custom exception types, wrapping with inner exceptions, exception filters for retries, aggregated validation errors, and rethrowing without losing the stack.
topics: [Exceptions, Error handling, Resilience]
keywords: [try, catch, finally, throw, when]
apis: [Exception.InnerException, KeyNotFoundException, FormatException, ExceptionDispatchInfo]
docs:
  - title: Best practices for exceptions
    url: https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions
  - title: Exception-handling statements
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements
  - title: How to create user-defined exceptions
    url: https://learn.microsoft.com/dotnet/standard/exceptions/how-to-create-user-defined-exceptions
---

## Brief

Good exception handling is mostly about what you *don't* do: don't swallow, don't reset stack traces, don't wrap without the inner exception, and don't catch what you cannot handle. Exception filters (`catch ... when`) let you decide whether to catch *before* the stack unwinds, which makes retry logic and diagnostics cleaner.

## Tasks

1. **`ConfigurationException`** derives from `Exception` and exposes the standard constructors `(string message)` and `(string message, Exception innerException)`.
2. **`Config.ReadPort(IReadOnlyDictionary<string, string> settings)`** reads the `"Port"` setting and returns it as an `int`:
   - missing key → `ConfigurationException` whose message contains `"Port"`;
   - not an integer → `ConfigurationException` whose `InnerException` is the original `FormatException`;
   - outside `1..65535` → `ConfigurationException`.
3. **`ValidationException`** carries an `IReadOnlyList<string> Errors` and a message that joins them with `"; "`.
4. **`OrderValidator.Validate(string? customerId, int quantity, decimal unitPrice)`** collects **all** problems before throwing one `ValidationException`:
   - `"CustomerId is required."` when null or whitespace;
   - `"Quantity must be between 1 and 1000."`;
   - `"UnitPrice must be positive."`.
5. **`Retry.Execute<T>(Func<T> action, int maxAttempts, Func<Exception, bool> isTransient)`** calls `action` until it succeeds. Transient failures are retried up to `maxAttempts` total attempts; the final failure propagates unchanged. Non-transient exceptions propagate immediately. Use an exception filter, not `catch` + `if` + `throw`.
6. **`Diagnostics.LogAndRethrow(Action action, List<string> log)`** runs `action`; on failure it appends `"{ExceptionType}: {Message}"` to `log` and rethrows **the same exception with its original stack trace**.

## Hints

- `if (!settings.TryGetValue("Port", out var raw)) throw new ConfigurationException(...)`.
- Wrapping: `catch (FormatException ex) { throw new ConfigurationException("...", ex); }`. Alternatively avoid the exception entirely with `int.TryParse` and construct the inner exception yourself; the test only checks the inner type.
- Build a `List<string>` of errors, then `if (errors.Count > 0) throw new ValidationException(errors);`
- `catch (Exception ex) when (attempt < maxAttempts && isTransient(ex))` inside a loop. Anything the filter rejects is never caught.
- `throw;` preserves the stack trace. `throw ex;` resets it to the current line.

## Notes

- **Filters run before unwinding.** When a filter returns `false`, the runtime keeps searching for a handler with the original stack intact, so a debugger or crash dump still shows the throwing frame. A filter that logs and returns `false` is a common way to log without catching.
- **`ExceptionDispatchInfo.Capture(ex).Throw()`** rethrows an exception captured elsewhere (another thread, a stored field) and preserves its stack. `await` uses it internally.
- **Don't catch `Exception` broadly** in library code unless you rethrow. Catching `OperationCanceledException` by accident is a common bug in async code.
- **Prefer `Try*` patterns on hot paths.** Exceptions are expensive, roughly microseconds per throw. Use them for exceptional conditions, not control flow, which is why `TryParse` exists.
- **Production retries** belong to a library such as Polly (`Microsoft.Extensions.Resilience`), which adds backoff, jitter, and circuit breakers.
