var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.RegisterOtel("Long Running");

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<LongRunningActorImpl>("LongRunningActor");
    //options.ReentrancyConfig = new ActorReentrancyConfig()
    //{
    //    Enabled = true,
    //    MaxStackDepth = 32
    //};
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapControllers();
app.MapActorsHandlers();

app.Run();

