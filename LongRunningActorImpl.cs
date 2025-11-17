namespace LongRunningActor;

public class LongRunningActorImpl : Actor, ILongRunningActor, IRemindable
{
    private const string _reminderName = "LongRunningOperationReminder";
    private readonly ILogger _logger;
    private CancellationTokenSource? _stoppingTokenSource;

    public LongRunningActorImpl(ActorHost host, ILoggerFactory loggerFactory)
        : base(host)
    {
        this._logger = loggerFactory.CreateLogger("LongRunningActor");
    }

    public async Task StartLongRunningOperationAsync()
    {
        await CallMethodAsync(async () =>
        {
            AddCallCount();

            this._stoppingTokenSource = new CancellationTokenSource();

            await this.RegisterInnerReminderAsync();

            CancellationToken stoppingToken = this._stoppingTokenSource.Token;

            try
            {
                this._logger.LogInformation("Long running operation was started.");

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    while (stoppingToken.IsCancellationRequested == false)
                    {
                        await Task.Delay(100);

                        if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
                        {
                            this._logger.LogInformation($"Time: {DateTimeOffset.Now}. Long running operation heartbeat {DateTimeOffset.Now}.");
                            stopwatch.Restart();
                        }
                    }

                    this._logger.LogInformation($"Time: {DateTimeOffset.Now}. Long running operation completed.");
                }
                finally
                {
                    stopwatch.Stop();
                }
            }
            catch (TaskCanceledException)
            {
                this._logger.LogInformation($"Time: {DateTimeOffset.Now}. Long running operation was cancelled.");
            }
            finally
            {
                await this.StopReminderAsync();
            }
        });
    }

    public async Task StartShortOperationAsync()
    {
        await CallMethodAsync(async () =>
        {
            AddCallCount();

            this._logger.LogInformation($"Time: {DateTimeOffset.Now}.Short running operation was started.");

            await this.RegisterInnerReminderAsync();
        });
    }

    public async Task StopLongRunningOperationAsync()
    {
        await CallMethodAsync(async () =>
        {
            AddCallCount();

            this._logger.LogInformation($"Time: {DateTimeOffset.Now}. Call StopLongRunningOperationAsync.");

            if (this._stoppingTokenSource != null)
                await this._stoppingTokenSource.CancelAsync();
        });
    }

    public async Task StopReminderAsync()
    {
        await CallMethodAsync(async () =>
        {
            AddCallCount();

            this._logger.LogInformation($"Time: {DateTimeOffset.Now}. Call StopReminderAsync.");

            await this.UnregisterInnerReminderAsync();
        });
    }

    private static void AddCallCount([CallerMemberName] string callerName = "")
    {
        TagList tags = new()
        {
            { "actor.method.name", callerName }
        };

        AppMetrics.ActorCallCount.Add(1, tags);
    }

    private async Task UnregisterInnerReminderAsync()
    {
        await this.UnregisterReminderAsync(_reminderName);
        this._logger.LogInformation($"Time: {DateTimeOffset.Now}. Reminder '{_reminderName}' unregistered.");
    }

    private async Task RegisterInnerReminderAsync()
    {
        var dueTime = TimeSpan.FromSeconds(5);
        var period = TimeSpan.FromSeconds(5);
        var state = Array.Empty<byte>();

        await this.RegisterReminderAsync(_reminderName, state, dueTime, period);
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        this._logger.LogInformation($"Received reminder: {reminderName}, DueTime: {dueTime}, Period: {period}, Time: {DateTimeOffset.Now}");

        AddCallCount("Reminder");

        await Task.CompletedTask;
    }

    private async Task CallMethodAsync(Func<Task> func, [CallerMemberName] string callerName = "")
    {
        using Activity? activity = AppActivitySources.ActorActivitySource.StartActivity("Actor.Method", ActivityKind.Internal);
        activity?.SetTag("dapr.actor.type", typeof(ILongRunningActor).Name);
        activity?.SetTag("dapr.actor.id", this.Id.GetId());
        activity?.SetTag("dapr.actor.method", callerName);

        await func();
    }
}