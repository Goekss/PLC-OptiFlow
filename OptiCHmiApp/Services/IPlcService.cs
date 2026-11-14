namespace OptiCHmiApp.Services;

// Maschinenzustand, den die PLC liefert.
// PLC'nin sağladığı makine durumu.
public record PlcStatus(
    double Temperatur,
    bool Heizung,
    bool Pumpe,
    double Sollwert,
    bool Notaus,
    double ZielAlarmSekunden,
    bool AutoGestoppt);

// Schnittstelle zur PLC.
// PLC'ye arayüz.
public interface IPlcService
{
    // True, wenn aktuell der Simulator statt der echten Verbindung läuft.
    // Gerçek bağlantı yerine simülatör çalışıyorsa true.
    bool IstSimulation { get; }

    // Liest den vollständigen Maschinenzustand.
    // Makinenin tam durumunu okur.
    Task<PlcStatus> LeseStatusAsync();

    // Setzt den Sollwert.
    // Hedef sıcaklığı ayarlar.
    Task SchreibeSollwertAsync(double sollwert);

    // Schaltet die Heizung ein/aus.
    // Isıtıcıyı açar/kapatır.
    Task SchreibeHeizungAsync(bool ein);

    // Schaltet die Pumpe ein/aus.
    // Pompayı açar/kapatır.
    Task SchreibePumpeAsync(bool ein);

    // Löst den Notaus aus (ein=true) oder setzt ihn zurück (ein=false).
    // Acil durdurmayı tetikler (ein=true) veya sıfırlar (ein=false).
    Task NotausAsync(bool ein);
}
