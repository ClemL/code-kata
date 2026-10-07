using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.Middleware;

[TestClass]
public class MiddlewarePipelineTests
{
    [TestMethod]
    public async Task Build_RunsComponentsInRegistrationOrder()
    {
        var log = new List<string>();

        var app = new PipelineBuilder()
            .Use(async (context, next) => { log.Add("A in"); await next(); log.Add("A out"); })
            .Use(next => async context => { log.Add("B in"); await next(context); log.Add("B out"); })
            .Run(context => { log.Add("handler"); context.Body.Append("hello"); return Task.CompletedTask; })
            .Build();

        var ctx = new HttpContextLite();
        await app(ctx);

        CollectionAssert.AreEqual(new[] { "A in", "B in", "handler", "B out", "A out" }, log);
        Assert.AreEqual("hello", ctx.Body.ToString());
        Assert.AreEqual(200, ctx.StatusCode);
    }

    [TestMethod]
    public async Task Build_Returns404WhenNothingHandlesTheRequest()
    {
        var ctx = new HttpContextLite();
        await new PipelineBuilder().Build()(ctx);
        Assert.AreEqual(404, ctx.StatusCode);

        var passThrough = new PipelineBuilder().Use((_, next) => next()).Build();
        var ctx2 = new HttpContextLite();
        await passThrough(ctx2);
        Assert.AreEqual(404, ctx2.StatusCode);
    }

    [TestMethod]
    public async Task Run_IsTerminal()
    {
        var reachedAfterRun = false;

        var app = new PipelineBuilder()
            .Run(context => { context.StatusCode = 204; return Task.CompletedTask; })
            .Use((_, next) => { reachedAfterRun = true; return next(); })
            .Build();

        var ctx = new HttpContextLite();
        await app(ctx);

        Assert.AreEqual(204, ctx.StatusCode);
        Assert.IsFalse(reachedAfterRun);
    }

    [TestMethod]
    public async Task Map_BranchesOnPathPrefix()
    {
        var app = new PipelineBuilder()
            .Map("/api", api => api.Run(context => { context.Body.Append("api"); return Task.CompletedTask; }))
            .Run(context => { context.Body.Append("site"); return Task.CompletedTask; })
            .Build();

        async Task<string> Get(string path)
        {
            var ctx = new HttpContextLite { Path = path };
            await app(ctx);
            return ctx.Body.ToString();
        }

        Assert.AreEqual("api", await Get("/api"));
        Assert.AreEqual("api", await Get("/API/orders/7"));
        Assert.AreEqual("site", await Get("/apiary"));
        Assert.AreEqual("site", await Get("/"));
    }

    [TestMethod]
    public async Task ExceptionHandler_ConvertsFailuresTo500()
    {
        var app = new PipelineBuilder()
            .Use(Middlewares.ExceptionHandler())
            .Run(context =>
            {
                context.Body.Append("partial output");
                throw new InvalidOperationException("boom");
            })
            .Build();

        var ctx = new HttpContextLite();
        await app(ctx);

        Assert.AreEqual(500, ctx.StatusCode);
        Assert.AreEqual("Internal Server Error", ctx.Body.ToString());
    }

    [TestMethod]
    public async Task ExceptionHandler_RegisteredLateDoesNotProtectEarlierComponents()
    {
        var app = new PipelineBuilder()
            .Use((_, _) => throw new InvalidOperationException("before the handler"))
            .Use(Middlewares.ExceptionHandler())
            .Build();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => app(new HttpContextLite()));
    }

    [TestMethod]
    public async Task RequireHeader_ShortCircuitsWith401()
    {
        var handled = 0;
        var app = new PipelineBuilder()
            .Use(Middlewares.RequireHeader("X-Api-Key"))
            .Run(_ => { handled++; return Task.CompletedTask; })
            .Build();

        var anonymous = new HttpContextLite();
        await app(anonymous);
        Assert.AreEqual(401, anonymous.StatusCode);

        var authorized = new HttpContextLite();
        authorized.RequestHeaders["x-api-key"] = "secret";
        await app(authorized);
        Assert.AreEqual(200, authorized.StatusCode);

        Assert.AreEqual(1, handled);
    }

    [TestMethod]
    public async Task Timing_AddsElapsedHeader()
    {
        var app = new PipelineBuilder()
            .Use(Middlewares.Timing())
            .Run(async _ => await Task.Delay(15))
            .Build();

        var ctx = new HttpContextLite();
        await app(ctx);

        Assert.IsTrue(ctx.ResponseHeaders.TryGetValue("X-Elapsed-Ms", out var elapsed));
        Assert.IsGreaterThanOrEqualTo(10, int.Parse(elapsed!));
    }
}
