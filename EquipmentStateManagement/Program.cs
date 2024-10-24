using EquipmentStateManagement.Services;
using StackExchange.Redis;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

Batteries.Init();

builder.Services.AddSingleton(sp => builder.Configuration.GetConnectionString("SQLiteConnection"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse("localhost:6379");
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<RedisService>();
builder.Services.AddScoped<SQLiteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await WebSocketHandler.HandleWebSocketAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapControllers();

app.Run();
