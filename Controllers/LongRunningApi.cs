namespace LongRunningActor.Controllers;

[ApiController]
[Route("v1/long-running")]
public class LongRunningApi : ControllerBase
{
    [HttpPost("StartLongRunningOp")]
    public async Task StartLongRunningOperationAsync()
    {
        try
        {
            await CallActorMethodAsync("StartLongRunningOperationAsync", proxy => proxy.StartLongRunningOperationAsync());
        }
        catch (TaskCanceledException)
        {
        }
    }

    [HttpPost("StopLongRunningOp")]
    public async Task StopLongRunningOperationAsync()
    {
        await CallActorMethodAsync("StopLongRunningOperationAsync", proxy => proxy.StopLongRunningOperationAsync());

    }

    [HttpPost("StartShortRunningOp")]
    public async Task StartShortOperationAsync()
    {
        await CallActorMethodAsync("StartShortOperationAsync", proxy => proxy.StartShortOperationAsync());
    }

    [HttpPost("StopReminder")]
    public async Task StopReminderAsync()
    {
        await CallActorMethodAsync("StopReminderAsync", proxy => proxy.StopReminderAsync());
    }

    private static async Task CallActorMethodAsync(string methodName, Func<ILongRunningActor, Task> func)
    {
        ActorId actorId = new("longRunningActor");

        ActorProxyOptions options = new()
        {
            RequestTimeout = TimeSpan.FromSeconds(15)
        };

        ILongRunningActor proxy = ActorProxy.Create<ILongRunningActor>(actorId, "LongRunningActor", options);

        using Activity? activity = AppActivitySources.ApiActivitySource.StartActivity(
            "CallActor.Method", ActivityKind.Client);

        activity?.SetTag("dapr.actor.type", typeof(ILongRunningActor).Name);
        activity?.SetTag("dapr.actor.id", actorId.GetId());
        activity?.SetTag("dapr.actor.method", methodName);

        await func(proxy);
    }
}