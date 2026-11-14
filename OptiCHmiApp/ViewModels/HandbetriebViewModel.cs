using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Seite "Handbetrieb": manuelle Steuerung der Maschine.
// "Handbetrieb" (manuel kontrol) sayfasının ViewModel'i.
public class HandbetriebViewModel : ObservableObject
{
    private readonly IPlcService _plcService;
    private readonly DispatcherTimer _timer;
    private bool _synchronisiere;
    private bool _pumpeEin;
    private bool _heizungEin;
    private double _sollwert = 60.0;
    private bool _notaus;

    public bool PumpeEin
    {
        get => _pumpeEin;
        set
        {
            if (SetProperty(ref _pumpeEin, value))
            {
                if (!_synchronisiere)
                {
                    _ = _plcService.SchreibePumpeAsync(value);
                }

                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public bool HeizungEin
    {
        get => _heizungEin;
        set
        {
            if (SetProperty(ref _heizungEin, value))
            {
                if (!_synchronisiere)
                {
                    _ = _plcService.SchreibeHeizungAsync(value);
                }

                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public double Sollwert
    {
        get => _sollwert;
        set
        {
            if (SetProperty(ref _sollwert, value))
            {
                if (!_synchronisiere)
                {
                    _ = _plcService.SchreibeSollwertAsync(value);
                }
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
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(NotausText));
            }
        }
    }

    public string StatusText => Notaus
        ? "NOTAUS"
        : (PumpeEin || HeizungEin) ? "ANLAGE IN BETRIEB" : "BEREITSCHAFT";

    public string NotausText => Notaus ? "RESET" : "NOT-AUS";

    public IAsyncRelayCommand NotausCommand { get; }

    public HandbetriebViewModel(IPlcService plcService)
    {
        _plcService = plcService;
        NotausCommand = new AsyncRelayCommand(NotausUmschaltenAsync);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => _ = SynchronisiereAsync();
        _timer.Start();
    }

    private async Task SynchronisiereAsync()
    {
        try
        {
            var status = await _plcService.LeseStatusAsync();

            _synchronisiere = true;
            PumpeEin = status.Pumpe;
            HeizungEin = status.Heizung;
            Sollwert = status.Sollwert;
            Notaus = status.Notaus;
            _synchronisiere = false;
        }
        catch
        {
            // Verbindungsfehler: nächster Versuch folgt.
        }
    }

    private async Task NotausUmschaltenAsync()
    {
        var neu = !Notaus;
        await _plcService.NotausAsync(neu);
        Notaus = neu;
    }
}
