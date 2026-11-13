namespace OptiCHmiApp.Services;

// Simuliert eine PLC, solange keine echte Hardware angeschlossen ist.
// Gerçek donanım bağlı olmadığı sürece bir PLC'yi simüle eder.
public class SimulierterPlcService : IPlcService
{
    private readonly Random _zufall = new();

    public Task<int> LeseTemperaturAsync()
    {
        int temperatur = _zufall.Next(20, 81);
        return Task.FromResult(temperatur);
    }
}
