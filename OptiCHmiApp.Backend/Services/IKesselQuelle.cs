namespace OptiCHmiApp.Backend.Services;

// Vollständiger Kesselzustand, wie er von einer Quelle (Simulation oder ADS) kommt.
// Bir kaynaktan (simülasyon veya ADS) gelen tam kazan durumu.
public record KesselZustand(
    double Temperatur,
    bool Heizung,
    bool Pumpe,
    double Sollwert,
    bool Notaus,
    double ZielAlarmSekunden,
    bool AutoGestoppt);

// Abstraktion über die Kessel-Datenquelle: Simulation oder echte PLC über ADS.
// Kazan veri kaynağı soyutlaması: simülasyon veya ADS üzerinden gerçek PLC.
public interface IKesselQuelle
{
    KesselZustand LeseZustand();
    void SchreibeSollwert(double sollwert);
    void SchreibeHeizung(bool ein);
    void SchreibePumpe(bool ein);
    void NotausAusloesen();
    void NotausZuruecksetzen();
}
