namespace LongRunningActor.Otel;

public static class AppMetrics
{
    public static readonly Meter Meter = new("LongRunning.Actors.Metrics");

    public static readonly Counter<int> ActorCallCount =
        Meter.CreateCounter<int>("actors.calling.count");
}