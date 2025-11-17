namespace LongRunningActor.Otel;

public static class AppActivitySources
{
    public static readonly ActivitySource ApiActivitySource =
        new("LongRunningActor.Api", "1.0.0");

    public static readonly ActivitySource ActorActivitySource =
        new("LongRunningActor.Actor", "1.0.0");
}
