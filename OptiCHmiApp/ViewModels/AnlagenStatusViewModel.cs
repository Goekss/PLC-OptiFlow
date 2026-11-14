using System.Diagnostics;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// Fester Anlagenstatus für die Kopfzeile: Verbindung, Zustand, Alarme und
// Zeitstempel der letzten Aktualisierung.
// Üst çubuk için sabit tesis durumu: bağlantı, durum, alarmlar ve son güncelleme.
public class AnlagenStatusViewModel : ObservableObject
{
    private readonly IPlcService _plcService;
    private readonly DispatcherTimer _timer;
    private bool _istSimulation = true;
    private bool _laeuft;
    private bool _notaus;
    private int _alarmAnzahl;
    private int _letzteAntwortMs;
    private string _zeitstempel = "--:--:--";

    public bool IstSimulation
    {
        get => _istSimulation;
        private set
        {
            if (SetProperty(ref _istSimulation, value))
            {
                OnPropertyChanged(nameof(VerbindungText));
            }
        }
    }

    public bool Laeuft
    {
        get => _laeuft;
        private set
        {
            if (SetProperty(ref _laeuft, value))
            {
                OnPropertyChanged(nameof(MaschineText));
            }
        }
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

    public int AlarmAnzahl
    {
        get => _alarmAnzahl;
        private set
        {
            if (SetProperty(ref _alarmAnzahl, value))
            {
                OnPropertyChanged(nameof(AlarmText));
                OnPropertyChanged(nameof(AlarmeVorhanden));
            }
        }
    }

    public int LetzteAntwortMs
    {
        get => _letzteAntwortMs;
        private set => SetProperty(ref _letzteAntwortMs, value);
    }

    public string Zeitstempel
    {
        get => _zeitstempel;
        private set => SetProperty(ref _zeitstempel, value);
    }

    public bool AlarmeVorhanden => AlarmAnzahl > 0;

    public string VerbindungText => Sprache.Text(IstSimulation ? "Getrennt" : "Verbunden");

    public string MaschineText => Sprache.Text(Notaus ? "Notaus" : (Laeuft ? "Laeuft" : "Gestoppt"));

    public string AlarmText => AlarmAnzahl > 0
        ? string.Format(Sprache.Text("AlarmAnzahl"), AlarmAnzahl)
        : Sprache.Text("KeineAlarme");

    public AnlagenStatusViewModel(IPlcService plcService)
    {
        _plcService = plcService;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => _ = AktualisiereAsync();
        _timer.Start();

        Sprache.SpracheGeaendert += _ => OnPropertyChanged(string.Empty);
    }

    private async Task AktualisiereAsync()
    {
        try
        {
            var stoppuhr = Stopwatch.StartNew();
            var status = await _plcService.LeseStatusAsync();
            stoppuhr.Stop();

            IstSimulation = _plcService.IstSimulation;
            Notaus = status.Notaus;
            Laeuft = !status.Notaus && (status.Heizung || status.Pumpe);
            LetzteAntwortMs = (int)stoppuhr.ElapsedMilliseconds;
            AlarmAnzahl = (status.Temperatur >= 80.0 ? 1 : 0) + (IstSimulation ? 1 : 0);
            Zeitstempel = DateTime.Now.ToString("HH:mm:ss");
        }
        catch
        {
            IstSimulation = true;
            Zeitstempel = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
