namespace OptiCHmiApp.Services;

// Schnittstelle zum Lesen von Daten aus der PLC.
// PLC'den veri okumak için arayüz.
public interface IPlcService
{
    // Liest den aktuellen Temperaturwert aus der PLC.
    // PLC'den güncel sıcaklık değerini okur.
    Task<int> LeseTemperaturAsync();
}
