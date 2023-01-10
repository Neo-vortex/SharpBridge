using SharpBridge.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(typeof(Comminucator));
builder.Services.AddSingleton(typeof(Dispatcher));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets(new WebSocketOptions()
{
KeepAliveInterval = TimeSpan.FromSeconds(5),

});

app.Run();