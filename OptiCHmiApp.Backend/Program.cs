using OptiCHmiApp.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
// Singleton, damit der Kesselzustand zwischen den gRPC-Aufrufen erhalten bleibt.
// gRPC çağrıları arasında kazan durumu korunsun diye singleton.
builder.Services.AddSingleton<PlcService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PlcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
