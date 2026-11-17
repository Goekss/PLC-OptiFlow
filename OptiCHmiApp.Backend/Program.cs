using OptiCHmiApp.Backend.OpcUa;
using OptiCHmiApp.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Kessel-Datenquelle: Simulation oder echte PLC über ADS (siehe appsettings.json).
// Kazan veri kaynağı: simülasyon veya ADS üzerinden gerçek PLC (bkz. appsettings.json).
var plantSource = builder.Configuration["PlantSource"] ?? "Simulation";
if (string.Equals(plantSource, "Ads", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IKesselQuelle>(sp =>
    {
        var ads = builder.Configuration.GetSection("Ads");
        var amsNetId = ads["AmsNetId"] ?? "127.0.0.1.1.1";
        var port = int.TryParse(ads["Port"], out var p) ? p : 851;
        var timeoutMs = int.TryParse(ads["TimeoutMs"], out var t) ? t : 1000;
        return new AdsKesselQuelle(amsNetId, port, timeoutMs);
    });
}
else
{
    builder.Services.AddSingleton<IKesselQuelle, SimulationKesselQuelle>();
}

builder.Services.AddSingleton<PlcService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PlcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Optionaler minimaler OPC-UA-Server (Demo, nur lesende Werte).
// Opsiyonel minimal OPC UA server (demo, salt okunur değerler).
if (builder.Configuration.GetValue<bool>("OpcUa:Enabled"))
{
    var quelle = app.Services.GetRequiredService<IKesselQuelle>();
    var opcUa = new KesselOpcUaServer();
    await opcUa.StartAsync(quelle);
    app.Lifetime.ApplicationStopping.Register(opcUa.Stop);
}

app.Run();
