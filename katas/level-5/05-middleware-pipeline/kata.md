---
title: Middleware Pipeline
level: 5
minutes: 45
summary: Rebuild ASP.NET Core's request pipeline - Use, Run, Map, short-circuiting, exception handling - from delegates and function composition.
topics: [Middleware, Delegates, Function composition, ASP.NET Core internals]
keywords: [delegate, async, await, "=>"]
apis: [RequestDelegate, "Func<RequestDelegate, RequestDelegate>", Stopwatch, StringBuilder]
docs:
  - title: ASP.NET Core middleware
    url: https://learn.microsoft.com/aspnet/core/fundamentals/middleware/
  - title: Write custom ASP.NET Core middleware
    url: https://learn.microsoft.com/aspnet/core/fundamentals/middleware/write
---

## Brief

An ASP.NET Core app is a single `RequestDelegate` built by composing middleware: each component receives the *next* delegate and returns a new one that wraps it. `app.Use`, `app.Run`, and `app.Map` are thin helpers over that one idea. Building the pipeline yourself makes ordering bugs (authentication after the endpoint, exception handling registered too late) obvious.

## Tasks

`HttpContextLite` and `RequestDelegate` are provided. Implement `PipelineBuilder` and `Middlewares`:

1. **`PipelineBuilder.Use(Func<RequestDelegate, RequestDelegate> middleware)`** registers a component.
2. **`PipelineBuilder.Use(Func<HttpContextLite, Func<Task>, Task> middleware)`** registers inline middleware in the familiar `async (context, next) => { ...; await next(); ... }` style.
3. **`PipelineBuilder.Run(RequestDelegate handler)`** registers a **terminal** component that never calls the rest of the pipeline.
4. **`PipelineBuilder.Map(string pathPrefix, Action<PipelineBuilder> configure)`** branches: requests whose path equals the prefix or starts with `prefix + "/"` (case-insensitive) go to a separately built branch; others continue down the main pipeline.
5. **`PipelineBuilder.Build()`** composes the components so the **first registered runs first**. If the request falls off the end of the pipeline, respond with status `404`.
6. **`Middlewares.ExceptionHandler()`** catches exceptions from later components, sets status `500`, clears the body, and writes `"Internal Server Error"`.
7. **`Middlewares.RequireHeader(string name)`** short-circuits with status `401` when the request header is missing.
8. **`Middlewares.Timing()`** adds a response header `X-Elapsed-Ms` with the elapsed whole milliseconds for the rest of the pipeline.

## Hints

- Keep a `List<Func<RequestDelegate, RequestDelegate>>`. `Build()` starts from the 404 delegate and wraps it **in reverse order**: `for (var i = count - 1; i >= 0; i--) app = components[i](app);`
- Inline style adapts to the delegate style: `Use(next => context => middleware(context, () => next(context)))`.
- `Run(handler)` is `Use(_ => handler)`: it ignores `next`.
- `Map` builds the branch once, at `Build` time, not per request.
- `Stopwatch.StartNew()` before `await next(context)`; set the header after it returns.

## Notes

- **Order is behavior.** Exception handling must be registered first so it wraps everything else; authentication must come before authorization, and both before endpoints. Most "middleware doesn't run" bugs are ordering bugs.
- **Short-circuiting** means not calling `next`. Static files, CORS preflight, and health checks all do it.
- **Response has started.** In real ASP.NET Core, once the response body starts streaming you can no longer change status codes or headers; `HttpResponse.HasStarted` tells you, and exception middleware has to check it.
- **Same pattern elsewhere.** `HttpClient` `DelegatingHandler`s, MediatR pipeline behaviors, gRPC interceptors, and Polly resilience pipelines all compose this way.
