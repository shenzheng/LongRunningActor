namespace LongRunningActor;

public interface ILongRunningActor : IActor
{
    public Task StartLongRunningOperationAsync();

    public Task StopLongRunningOperationAsync();

    public Task StartShortOperationAsync();

    public Task StopReminderAsync();
}

