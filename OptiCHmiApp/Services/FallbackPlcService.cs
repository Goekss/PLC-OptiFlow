namespace OptiCHmiApp.Services;

// Nutzt die echte gRPC-Verbindung und wechselt bei Fehlern automatisch
// auf den Simulator, damit die Anzeige immer Daten liefert.
// Gerçek gRPC bağlantısını kullanır; hata durumunda otomatik olarak
// simülatöre geçer, böylece ekran her zaman veri gösterir.
public class FallbackPlcService : IPlcService
{
    private readonly IPlcService _grpc;
    private readonly IPlcService _simulator = new SimulierterPlcService();
    private bool _nutzeSimulator;

    public bool IstSimulation => _nutzeSimulator;

    public FallbackPlcService(string adresse)
    {
        _grpc = new GrpcPlcService(adresse);
    }

    public async Task<PlcStatus> LeseStatusAsync()
    {
        if (_nutzeSimulator)
        {
            return await _simulator.LeseStatusAsync();
        }

        try
        {
            return await _grpc.LeseStatusAsync();
        }
        catch
        {
            _nutzeSimulator = true;
            return await _simulator.LeseStatusAsync();
        }
    }

    public Task SchreibeSollwertAsync(double sollwert)
        => SchreibeAsync(s => s.SchreibeSollwertAsync(sollwert));

    public Task SchreibeHeizungAsync(bool ein)
        => SchreibeAsync(s => s.SchreibeHeizungAsync(ein));

    public Task SchreibePumpeAsync(bool ein)
        => SchreibeAsync(s => s.SchreibePumpeAsync(ein));

    public Task NotausAsync(bool ein)
        => SchreibeAsync(s => s.NotausAsync(ein));

    private async Task SchreibeAsync(Func<IPlcService, Task> aktion)
    {
        if (_nutzeSimulator)
        {
            await aktion(_simulator);
            return;
        }

        try
        {
            await aktion(_grpc);
        }
        catch
        {
            _nutzeSimulator = true;
            await aktion(_simulator);
        }
    }
}