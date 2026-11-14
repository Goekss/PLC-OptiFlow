using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Übersichtsseite: liest den Maschinenzustand automatisch,
// zeichnet den Verlauf und zeigt den Status als Badges an.
// Genel bakış sayfasının ViewModel'i: makine durumunu otomatik okur,
// geçmişi çizer ve durumu rozet olarak gösterir.
public class UebersichtViewModel : ObservableObject
{
    private const int VerlaufLaenge = 60;
    private const double ChartHoehe = 360.0;
    private const double ChartBreite = 240.0;
    private const double ChartMin = 0.0;
    private const double ChartMax = 120.0;

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
    private readonly Queue<double> _verlauf = new();
    private PointCollection _trendPunkte = new();

    public double Temperatur
    {
        get => _temperatur;
        private set => SetProperty(ref _temperatur, value);
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
        private set => SetProperty(ref _sollwert, value);
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
    // Yukarı yuvarlanmış gösterim (5, 4, 3, 2, 1, 0).
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

    public string StartStoppText => Laeuft ? "Stopp" : "Start";

    public string VerbindungText => IstSimulation ? "GETRENNT" : "VERBUNDEN";

    public string MaschinenName => "KESSEL-01";

    public string IpAdresse => "192.168.0.100";

    public string MaschineText => Notaus ? "NOTAUS" : (Laeuft ? "LÄUFT" : "GESTOPPT");

    public int AlarmAnzahl => (Temperatur >= 80.0 ? 1 : 0) + (IstSimulation ? 1 : 0);

    public string AlarmText => AlarmAnzahl > 0 ? $"{AlarmAnzahl} ALARM" : "0 ALARME";

    public IAsyncRelayCommand StartStoppCommand { get; }

    public UebersichtViewModel(IPlcService plcService)
    {
        _plcService = plcService;
        StartStoppCommand = new AsyncRelayCommand(StartStoppAsync, options: AsyncRelayCommandOptions.AllowConcurrentExecutions);
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

                Temperatur = status.Temperatur;
                Heizung = status.Heizung;
                Pumpe = status.Pumpe;
                Notaus = status.Notaus;
                Sollwert = status.Sollwert;
                ZielAlarmSekunden = status.ZielAlarmSekunden;
                AutoGestoppt = status.AutoGestoppt;
                IstSimulation = _plcService.IstSimulation;
                LetzteAntwortMs = (int)stoppuhr.ElapsedMilliseconds;

                OnPropertyChanged(nameof(AlarmAnzahl));
                OnPropertyChanged(nameof(AlarmText));

                AktualisiereVerlauf();

                // Beim automatischen Stopp den Lesevorgang einmalig beenden.
                // Otomatik durmada okuma döngüsünü bir kez sonlandır.
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

    private void AktualisiereVerlauf()
    {
        // Ca. ein Punkt pro Sekunde, damit der Verlauf sichtbar steigt.
        // Görünür şekilde tırmansın diye saniyede bir nokta.
        var jetzt = DateTime.UtcNow;
        if ((jetzt - _letzterTrendPunkt).TotalMilliseconds < 800 && _verlauf.Count > 0)
        {
            return;
        }
        _letzterTrendPunkt = jetzt;

        _verlauf.Enqueue(Temperatur);
        while (_verlauf.Count > VerlaufLaenge)
        {
            _verlauf.Dequeue();
        }

        var schritt = ChartBreite / (VerlaufLaenge - 1);
        var punkte = new PointCollection();
        int i = 0;
        foreach (var wert in _verlauf)
        {
            punkte.Add(new Point(i * schritt, ZuY(wert)));
            i++;
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
