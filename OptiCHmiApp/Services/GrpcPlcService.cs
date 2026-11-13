using Grpc.Net.Client;
using OptiCHmiApp.Grpc;

namespace OptiCHmiApp.Services;

// Stellt die Verbindung zur echten PLC über gRPC her.
// Gerçek PLC'ye gRPC üzerinden bağlantı sağlar.
public class GrpcPlcService : IPlcService
{
    private readonly Plc.PlcClient _client;

    public GrpcPlcService(string adresse)
    {
        var kanal = GrpcChannel.ForAddress(adresse);
        _client = new Plc.PlcClient(kanal);
    }

    public async Task<int> LeseTemperaturAsync()
    {
        var antwort = await _client.LeseTemperaturAsync(new LeseTemperaturRequest());
        return antwort.Temperatur;
    }
}
