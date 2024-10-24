using EquipmentStateManagement.Services;
using Microsoft.Data.Sqlite;
using StackExchange.Redis;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

Batteries.Init();

builder.Services.AddSingleton<SqliteConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("SQLiteConnection");
    return new SqliteConnection(connectionString);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse("localhost:6379");
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddSingleton<ISQLiteService, SQLiteService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddSingleton<IWebSocketHandler, WebSocketHandler>();

builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
        var webSocketHandler = app.Services.GetRequiredService<IWebSocketHandler>();
        await webSocketHandler.HandleWebSocketAsync(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapControllers();

app.Run();
