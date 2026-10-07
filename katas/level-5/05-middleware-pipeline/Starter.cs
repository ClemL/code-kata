using System.Diagnostics;
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
    public PipelineBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => throw new NotImplementedException();

    public PipelineBuilder Use(Func<HttpContextLite, Func<Task>, Task> middleware) => throw new NotImplementedException();

    public PipelineBuilder Run(RequestDelegate handler) => throw new NotImplementedException();

    public PipelineBuilder Map(string pathPrefix, Action<PipelineBuilder> configure) => throw new NotImplementedException();

    public RequestDelegate Build() => throw new NotImplementedException();
}

public static class Middlewares
{
    public static Func<RequestDelegate, RequestDelegate> ExceptionHandler() => throw new NotImplementedException();

    public static Func<RequestDelegate, RequestDelegate> RequireHeader(string name) => throw new NotImplementedException();

    public static Func<RequestDelegate, RequestDelegate> Timing() => throw new NotImplementedException();
}
