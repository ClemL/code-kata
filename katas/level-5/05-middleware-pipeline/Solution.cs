using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Katas.L5.Middleware;

public sealed class HttpContextLite
{
    public string Method { get; init; } = "GET";

    public string Path { get; init; } = "/";

    public Dictionary<string, string> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public int StatusCode { get; set; } = 200;

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public StringBuilder Body { get; } = new();
}

public delegate Task RequestDelegate(HttpContextLite context);

public sealed class PipelineBuilder
{
    private readonly List<Func<RequestDelegate, RequestDelegate>> _components = [];

    public PipelineBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public PipelineBuilder Use(Func<HttpContextLite, Func<Task>, Task> middleware) =>
        Use(next => context => middleware(context, () => next(context)));

    public PipelineBuilder Run(RequestDelegate handler) => Use(_ => handler);

    public PipelineBuilder Map(string pathPrefix, Action<PipelineBuilder> configure)
    {
        var prefix = pathPrefix.TrimEnd('/');

        return Use(next =>
        {
            var branchBuilder = new PipelineBuilder();
            configure(branchBuilder);
            var branch = branchBuilder.Build();

            return context => Matches(context.Path, prefix) ? branch(context) : next(context);
        });
    }

    public RequestDelegate Build()
    {
        RequestDelegate app = static context =>
        {
            context.StatusCode = 404;
            return Task.CompletedTask;
        };

        for (var i = _components.Count - 1; i >= 0; i--)
        {
            app = _components[i](app);
        }

        return app;
    }

    private static bool Matches(string path, string prefix) =>
        path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
        && (path.Length == prefix.Length || path[prefix.Length] == '/');
}

public static class Middlewares
{
    public static Func<RequestDelegate, RequestDelegate> ExceptionHandler() => next => async context =>
    {
        try
        {
            await next(context);
        }
        catch (Exception)
        {
            context.StatusCode = 500;
            context.Body.Clear().Append("Internal Server Error");
        }
    };

    public static Func<RequestDelegate, RequestDelegate> RequireHeader(string name) => next => context =>
    {
        if (!context.RequestHeaders.ContainsKey(name))
        {
            context.StatusCode = 401;
            return Task.CompletedTask;
        }

        return next(context);
    };

    public static Func<RequestDelegate, RequestDelegate> Timing() => next => async context =>
    {
        var stopwatch = Stopwatch.StartNew();
        await next(context);
        context.ResponseHeaders["X-Elapsed-Ms"] = stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
    };
}
