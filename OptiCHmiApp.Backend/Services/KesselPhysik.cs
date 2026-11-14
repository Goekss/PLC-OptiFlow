namespace OptiCHmiApp.Backend.Services;

// Physik des simulierten Kessels: Temperatur steigt/fällt je nach Heizung,
// kleiner zufälliger Rauschanteil, Notaus sperrt alles.
// Simüle edilen kazanın fiziği: sıcaklık ısıtıcıya göre artar/azalır,
// küçük rastgele gürültü, Notaus her şeyi kilitler.
public class KesselPhysik
{
    public const double WarnSchwelle = 80.0;
    public const double KritischSchwelle = 90.0;
    public const double UmgebungsTemperatur = 20.0;

    // Raten sind austauschbar, damit Tests die Physik schnell prüfen können.
    // Oranlar testlerin fiziği hızlı doğrulaması için değiştirilebilir.
    public double HeizrateProSekunde { get; init; } = 0.5;
    public double KuehlrateProSekunde { get; init; } = 0.25;
    public double Jitter { get; init; } = 0.3;

    public double Temperatur { get; private set; } = UmgebungsTemperatur;
    public bool Heizung { get; private set; }
    public bool Pumpe { get; private set; } = true;
    public double Sollwert { get; private set; } = 60.0;
    public bool Notaus { get; private set; }
    public bool Automatik { get; private set; } = true;

    public bool IstWarnung => !Notaus && Temperatur >= WarnSchwelle;
    public bool IstKritisch => !Notaus && Temperatur >= KritischSchwelle;

    // Verbleibende Sekunden bis zur automatischen Abschaltung,
    // sobald die Solltemperatur erreicht wurde (0 = kein Alarm).
    // Hedef sıcaklığa ulaşıldığında otomatik kapanmaya kalan saniye
    // (0 = alarm yok).
    public double ZielAlarmSekunden { get; private set; }

    // True, wenn die automatische Abschaltung den Kessel gestoppt hat.
    // Otomatik kapanma kazanı durdurduysa true.
    public bool AutoGestoppt { get; private set; }

    private readonly Random _zufall = new();

    public void SchreibeSollwert(double sollwert)
    {
        if (Notaus)
        {
            return;
        }

        Sollwert = Math.Clamp(sollwert, 0.0, 100.0);
        AutoGestoppt = false;
    }

    public void SchreibeHeizung(bool ein)
    {
        if (Notaus)
        {
            return;
        }

        Heizung = ein;
        Automatik = false;
        if (ein)
        {
            AutoGestoppt = false;
        }
    }

    public void SchreibePumpe(bool ein)
    {
        if (Notaus)
        {
            return;
        }

        Pumpe = ein;
    }

    public void NotausAusloesen()
    {
        Notaus = true;
        Heizung = false;
        Pumpe = false;
        ZielAlarmSekunden = 0;
    }

    public void NotausZuruecksetzen()
    {
        Notaus = false;
        Automatik = true;
        AutoGestoppt = false;
    }

    // Schaltet die Heizung im Automatikbetrieb anhand des Sollwerts.
    // Otomatik modda ısıtıcıyı hedef değere göre açar/kapatır.
    public void Tick(double sekunden)
    {
        sekunden = Math.Clamp(sekunden, 0.0, 1.0);

        if (Notaus)
        {
            Heizung = false;
            Pumpe = false;
            ZielAlarmSekunden = 0;
            Temperatur = Annaehern(Temperatur, UmgebungsTemperatur, KuehlrateProSekunde * sekunden);
        }
        else
        {
            if (Automatik)
            {
                // Hysterese verhindert schnelles Flackern der Heizung.
                // Histerezis, ısıtıcının hızlı titremesini önler.
                const double hysterese = 0.5;
                if (Heizung && Temperatur >= Sollwert + hysterese)
                {
                    Heizung = false;
                }
                else if (!Heizung && Temperatur < Sollwert - hysterese)
                {
                    Heizung = true;
                }
            }

            if (Heizung)
            {
                Temperatur += HeizrateProSekunde * sekunden;
            }
            else
            {
                Temperatur = Annaehern(Temperatur, UmgebungsTemperatur, KuehlrateProSekunde * sekunden);
            }

            // Kritisch: ab 90 °C wird die Heizung automatisch abgeschaltet.
            // Kritik: 90 °C'den itibaren ısıtıcı otomatik kapanır.
            if (Temperatur >= KritischSchwelle)
            {
                Heizung = false;
            }

            // Ziel erreicht: Alarm + Countdown, danach automatische Abschaltung.
            // Hedef ulaşıldı: alarm + geri sayım, sonra otomatik kapanma.
            if (Heizung && Temperatur >= Sollwert && ZielAlarmSekunden <= 0)
            {
                ZielAlarmSekunden = 5.0;
            }

            if (ZielAlarmSekunden > 0)
            {
                // Nur manuelles Abschalten hebt den Alarm auf;
                // der Automatik-Zyklus lässt den Countdown weiterlaufen.
                // Sadece manuel kapatma alarmı iptal eder;
                // otomatik çevrim geri sayımı sürdürür.
                if (!Automatik && !Heizung)
                {
                    ZielAlarmSekunden = 0;
                }
                else
                {
                    ZielAlarmSekunden -= sekunden;
                    if (ZielAlarmSekunden <= 0)
                    {
                        ZielAlarmSekunden = 0;
                        Heizung = false;
                        Automatik = false; // automatische Abschaltung
                        AutoGestoppt = true;
                    }
                }
            }
        }

        Temperatur += (_zufall.NextDouble() * 2.0 - 1.0) * Jitter * sekunden;
        Temperatur = Math.Clamp(Temperatur, 10.0, 120.0);
    }

    private static double Annaehern(double aktuell, double ziel, double schritt)
    {
        if (aktuell < ziel)
        {
            return Math.Min(ziel, aktuell + schritt);
        }

        if (aktuell > ziel)
        {
            return Math.Max(ziel, aktuell - schritt);
        }

        return aktuell;
    }
}
