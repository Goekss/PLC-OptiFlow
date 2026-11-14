using Grpc.Net.Client;
using OptiCHmiApp.Grpc;

namespace OptiCHmiApp.Services;

// Stellt die Verbindung zur PLC über gRPC her.
// PLC'ye gRPC üzerinden bağlantı sağlar.
public class GrpcPlcService : IPlcService
{
    private readonly Plc.PlcClient _client;

    public bool IstSimulation => false;

    public GrpcPlcService(string adresse)
    {
        var kanal = GrpcChannel.ForAddress(adresse);
        _client = new Plc.PlcClient(kanal);
    }

    public async Task<PlcStatus> LeseStatusAsync()
    {
        var antwort = await _client.LeseStatusAsync(new LeseStatusRequest(), deadline: Frist());
        return new PlcStatus(
            antwort.Temperatur,
            antwort.Heizung,
            antwort.Pumpe,
            antwort.Sollwert,
            antwort.Notaus,
            antwort.ZielAlarmSekunden,
            antwort.AutoGestoppt);
    }

    public async Task SchreibeSollwertAsync(double sollwert)
    {
        await _client.SchreibeSollwertAsync(new SchreibeSollwertRequest { Sollwert = sollwert }, deadline: Frist());
    }

    public async Task SchreibeHeizungAsync(bool ein)
    {
        await _client.SchreibeHeizungAsync(new SchreibeHeizungRequest { Ein = ein }, deadline: Frist());
    }

    public async Task SchreibePumpeAsync(bool ein)
    {
        await _client.SchreibePumpeAsync(new SchreibePumpeRequest { Ein = ein }, deadline: Frist());
    }

    public async Task NotausAsync(bool ein)
    {
        await _client.NotausAsync(new NotausRequest { Ein = ein }, deadline: Frist());
    }

    // Kurze Frist, damit die Oberfläche bei fehlendem Backend nicht wartet.
    // Backend yoksa arayüz beklemesin diye kısa zaman aşımı.
    private static DateTime Frist() => DateTime.UtcNow.AddSeconds(3);
}
