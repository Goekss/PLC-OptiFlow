using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Übersichtsseite: liest den Maschinenzustand, hält den Verlauf
// mit umschaltbarem Zeitfenster und liefert die Kennzahlen für die Anzeigen.
// Genel bakış sayfasının ViewModel'i: makine durumunu okur, seçilebilir zaman
// aralığıyla geçmişi tutar ve göstergeler için değerleri sağlar.
public class UebersichtViewModel : ObservableObject
{
    // 5 Minuten Puffer; die X-Achse läuft in Echtzeit, nicht in Abtastschritten.
    private const int VerlaufLaenge = 300;
    private const double ChartHoehe = 260.0;
    private const double ChartBreite = 300.0;
    private const double ChartMin = 0.0;
    private const double ChartMax = 120.0;
    private const double GrenzeWarnung = 80.0;
    private const double GrenzeKritisch = 90.0;

    private readonly IPlcService _plcService;
    private CancellationTokenSource? _abbruchQuelle;
    private double _temperatur;
    private int _letzteAntwortMs;
    private bool _laeuft;
    private bool _heizung;
    private bool _pumpe;
    private bool _notaus;
    private double _sollwert;
    private bool _istSimulation;
    private double _zielAlarmSekunden;
    private bool _autoGestoppt;
    private bool _vorherAutoGestoppt;
    private double _sollwertY;
    private DateTime _letzterTrendPunkt = DateTime.MinValue;
    private readonly Queue<(DateTime Zeit, double Wert)> _verlauf = new();
    private List<(DateTime Zeit, double Wert)> _sichtbareWerte = new();
    private PointCollection _trendPunkte = new();
    private int _fensterSekunden = 60;
    private bool _cursorSichtbar;
    private double _cursorX;
    private double _cursorY;
    private string _cursorText = string.Empty;

    public double Temperatur
    {
        get => _temperatur;
        private set
        {
            if (SetProperty(ref _temperatur, value))
            {
                OnPropertyChanged(nameof(TemperaturProzent));
                OnPropertyChanged(nameof(SollwertProzent));
                OnPropertyChanged(nameof(WarnungAktiv));
                OnPropertyChanged(nameof(KritischAktiv));
            }
        }
    }

    public int LetzteAntwortMs
    {
        get => _letzteAntwortMs;
        private set => SetProperty(ref _letzteAntwortMs, value);
    }

    public PointCollection TrendPunkte
    {
        get => _trendPunkte;
        private set => SetProperty(ref _trendPunkte, value);
    }

    public double SollwertY
    {
        get => _sollwertY;
        private set => SetProperty(ref _sollwertY, value);
    }

    public bool Laeuft
    {
        get => _laeuft;
        private set
        {
            if (SetProperty(ref _laeuft, value))
            {
                OnPropertyChanged(nameof(StartStoppText));
                OnPropertyChanged(nameof(MaschineText));
            }
        }
    }

    public bool Heizung
    {
        get => _heizung;
        private set => SetProperty(ref _heizung, value);
    }

    public bool Pumpe
    {
        get => _pumpe;
        private set => SetProperty(ref _pumpe, value);
    }

    public bool Notaus
    {
        get => _notaus;
        private set
        {
            if (SetProperty(ref _notaus, value))
            {
                OnPropertyChanged(nameof(MaschineText));
            }
        }
    }

    public double Sollwert
    {
        get => _sollwert;
        private set
        {
            if (SetProperty(ref _sollwert, value))
            {
                OnPropertyChanged(nameof(SollwertProzent));
            }
        }
    }

    public double ZielAlarmSekunden
    {
        get => _zielAlarmSekunden;
        private set
        {
            if (SetProperty(ref _zielAlarmSekunden, value))
            {
                OnPropertyChanged(nameof(ZielAlarmAktiv));
                OnPropertyChanged(nameof(ZielAlarmAnzeige));
            }
        }
    }

    public bool ZielAlarmAktiv => ZielAlarmSekunden > 0;

    // Aufwärts gerundete Anzeige (5, 4, 3, 2, 1, 0).
    public int ZielAlarmAnzeige => (int)Math.Ceiling(ZielAlarmSekunden);

    public bool AutoGestoppt
    {
        get => _autoGestoppt;
        private set => SetProperty(ref _autoGestoppt, value);
    }

    public bool IstSimulation
    {
        get => _istSimulation;
        private set
        {
            if (SetProperty(ref _istSimulation, value))
            {
                OnPropertyChanged(nameof(VerbindungText));
                OnPropertyChanged(nameof(AlarmAnzahl));
                OnPropertyChanged(nameof(AlarmText));
            }
        }
    }

    public int FensterSekunden
    {
        get => _fensterSekunden;
        private set
        {
            if (SetProperty(ref _fensterSekunden, value))
            {
                OnPropertyChanged(nameof(AchseLinks));
                OnPropertyChanged(nameof(AchseMitte));
                OnPropertyChanged(nameof(Fenster1Aktiv));
                OnPropertyChanged(nameof(Fenster5Aktiv));
            }
        }
    }

    public bool Fenster1Aktiv => FensterSekunden == 60;
    public bool Fenster5Aktiv => FensterSekunden == 300;

    // Achsenbeschriftung des Zeitfensters (links = ältester Punkt, rechts = jetzt).
    public string AchseLinks => $"-{FensterSekunden / 60.0:0.#} {Sprache.Text("MinuteKurz")}";
    public string AchseMitte => $"-{FensterSekunden / 120.0:0.#} {Sprache.Text("MinuteKurz")}";

    public double GrenzeWarnungY => ZuY(GrenzeWarnung);
    public double GrenzeKritischY => ZuY(GrenzeKritisch);

    public double TemperaturProzent => Math.Clamp(Temperatur, 0.0, 100.0);
    public double SollwertProzent => Math.Clamp(Sollwert, 0.0, 100.0);

    // Farbe nur bei Abweichung (ISA-101): Warnung ab 80 °C, kritisch ab 90 °C.
    public bool WarnungAktiv => Temperatur >= GrenzeWarnung && Temperatur < GrenzeKritisch;
    public bool KritischAktiv => Temperatur >= GrenzeKritisch;

    public bool CursorSichtbar
    {
        get => _cursorSichtbar;
        private set => SetProperty(ref _cursorSichtbar, value);
    }

    public double CursorX
    {
        get => _cursorX;
        private set => SetProperty(ref _cursorX, value);
    }

    public double CursorY
    {
        get => _cursorY;
        private set => SetProperty(ref _cursorY, value);
    }

    public string CursorText
    {
        get => _cursorText;
        private set => SetProperty(ref _cursorText, value);
    }

    public string StartStoppText => Sprache.Text(Laeuft ? "Stopp" : "Start");

    public string VerbindungText => Sprache.Text(IstSimulation ? "Getrennt" : "Verbunden");

    public string MaschinenName => "KESSEL-01";

    public string IpAdresse => "192.168.0.100";

    public string MaschineText => Sprache.Text(Notaus ? "Notaus" : (Laeuft ? "Laeuft" : "Gestoppt"));

    public int AlarmAnzahl => (Temperatur >= GrenzeWarnung ? 1 : 0) + (IstSimulation ? 1 : 0);

    public string AlarmText => AlarmAnzahl > 0
        ? string.Format(Sprache.Text("AlarmAnzahl"), AlarmAnzahl)
        : Sprache.Text("KeineAlarme");

    public IAsyncRelayCommand StartStoppCommand { get; }
    public IRelayCommand Fenster1MinCommand { get; }
    public IRelayCommand Fenster5MinCommand { get; }

    public UebersichtViewModel(IPlcService plcService)
    {
        _plcService = plcService;
        StartStoppCommand = new AsyncRelayCommand(StartStoppAsync, options: AsyncRelayCommandOptions.AllowConcurrentExecutions);
        Fenster1MinCommand = new RelayCommand(() => FensterWechseln(60));
        Fenster5MinCommand = new RelayCommand(() => FensterWechseln(300));
        Sprache.SpracheGeaendert += _ => OnPropertyChanged(string.Empty);

        // Sofort einen aktuellen Wert anzeigen, auch ohne gestartete Schleife.
        _ = LeseEinmalAsync();
    }

    private async Task LeseEinmalAsync()
    {
        try
        {
            var stoppuhr = Stopwatch.StartNew();
            var status = await _plcService.LeseStatusAsync();
            stoppuhr.Stop();
            Uebernehmen(status, (int)stoppuhr.ElapsedMilliseconds);
        }
        catch
        {
            // Kein Backend: die Kopfzeile zeigt den Zustand, der Simulator springt ein.
        }
    }

    private void Uebernehmen(PlcStatus status, int antwortMs)
    {
        Temperatur = status.Temperatur;
        Heizung = status.Heizung;
        Pumpe = status.Pumpe;
        Notaus = status.Notaus;
        Sollwert = status.Sollwert;
        ZielAlarmSekunden = status.ZielAlarmSekunden;
        AutoGestoppt = status.AutoGestoppt;
        IstSimulation = _plcService.IstSimulation;
        LetzteAntwortMs = antwortMs;

        OnPropertyChanged(nameof(AlarmAnzahl));
        OnPropertyChanged(nameof(AlarmText));

        AktualisiereVerlauf();
    }

    private async Task StartStoppAsync()
    {
        if (Laeuft)
        {
            _abbruchQuelle?.Cancel();
            Laeuft = false;
            return;
        }

        _abbruchQuelle = new CancellationTokenSource();
        Laeuft = true;
        await LeseSchleifeAsync(_abbruchQuelle.Token);
    }

    private async Task LeseSchleifeAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var stoppuhr = Stopwatch.StartNew();
                var status = await _plcService.LeseStatusAsync();
                stoppuhr.Stop();

                Uebernehmen(status, (int)stoppuhr.ElapsedMilliseconds);

                // Beim automatischen Stopp den Lesevorgang einmalig beenden.
                if (status.AutoGestoppt && !_vorherAutoGestoppt)
                {
                    _vorherAutoGestoppt = true;
                    Laeuft = false;
                    break;
                }
                _vorherAutoGestoppt = status.AutoGestoppt;
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await Task.Delay(BenutzerEinstellungen.AbfrageIntervallMs, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void FensterWechseln(int sekunden)
    {
        if (FensterSekunden == sekunden)
        {
            return;
        }

        FensterSekunden = sekunden;
        ZeichneVerlauf();
    }

    // Zeigt den Messwert unter dem Mauszeiger im Diagramm.
    // Fare imlecindeki ölçüm değerini grafikte gösterir.
    public void ZeigeCursor(double x)
    {
        if (_sichtbareWerte.Count == 0)
        {
            return;
        }

        var ende = _sichtbareWerte[^1].Zeit;
        var zeit = ende.AddSeconds(-(1.0 - x / ChartBreite) * FensterSekunden);

        var naechster = _sichtbareWerte[0];
        foreach (var punkt in _sichtbareWerte)
        {
            if (Math.Abs((punkt.Zeit - zeit).TotalSeconds) < Math.Abs((naechster.Zeit - zeit).TotalSeconds))
            {
                naechster = punkt;
            }
        }

        CursorX = ChartBreite * (1.0 - (ende - naechster.Zeit).TotalSeconds / FensterSekunden);
        CursorY = ZuY(naechster.Wert);
        CursorText = $"{naechster.Wert:F1} °C";
        CursorSichtbar = true;
    }

    public void VersteckeCursor() => CursorSichtbar = false;

    private void AktualisiereVerlauf()
    {
        // Ca. ein Punkt pro Sekunde; Lücken bleiben als Lücke sichtbar.
        var jetzt = DateTime.UtcNow;
        if ((jetzt - _letzterTrendPunkt).TotalMilliseconds < 800 && _verlauf.Count > 0)
        {
            return;
        }
        _letzterTrendPunkt = jetzt;

        _verlauf.Enqueue((jetzt, Temperatur));
        while (_verlauf.Count > 0 && (jetzt - _verlauf.Peek().Zeit).TotalSeconds > VerlaufLaenge)
        {
            _verlauf.Dequeue();
        }

        ZeichneVerlauf();
    }

    private void ZeichneVerlauf()
    {
        if (_verlauf.Count == 0)
        {
            return;
        }

        var ende = _verlauf.Last().Zeit;
        var start = ende.AddSeconds(-FensterSekunden);

        _sichtbareWerte = _verlauf.Where(p => p.Zeit >= start).ToList();

        var punkte = new PointCollection();
        foreach (var punkt in _sichtbareWerte)
        {
            var x = ChartBreite * (1.0 - (ende - punkt.Zeit).TotalSeconds / FensterSekunden);
            punkte.Add(new Point(x, ZuY(punkt.Wert)));
        }

        TrendPunkte = punkte;
        SollwertY = ZuY(Sollwert);
    }

    private static double ZuY(double wert)
    {
        var norm = (wert - ChartMin) / (ChartMax - ChartMin);
        return ChartHoehe - norm * ChartHoehe;
    }
}
