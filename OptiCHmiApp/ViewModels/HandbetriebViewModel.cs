using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Seite "Handbetrieb": manuelle Steuerung einzelner Aktoren.
// "Handbetrieb" (manuel kontrol) sayfasının ViewModel'i.
public class HandbetriebViewModel : ObservableObject
{
    private bool _pumpeEin;
    private bool _heizungEin;
    private bool _ventilEin;
    private double _motorDrehzahl = 60;
    private double _sollTemperatur = 45;

    public bool PumpeEin
    {
        get => _pumpeEin;
        set
        {
            if (SetProperty(ref _pumpeEin, value))
            {
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
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public bool VentilEin
    {
        get => _ventilEin;
        set
        {
            if (SetProperty(ref _ventilEin, value))
            {
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public double MotorDrehzahl
    {
        get => _motorDrehzahl;
        set => SetProperty(ref _motorDrehzahl, value);
    }

    public double SollTemperatur
    {
        get => _sollTemperatur;
        set => SetProperty(ref _sollTemperatur, value);
    }

    public IRelayCommand NotausCommand { get; }

    public string StatusText => (PumpeEin || HeizungEin || VentilEin)
        ? "ANLAGE IN BETRIEB"
        : "BEREITSCHAFT";

    public HandbetriebViewModel()
    {
        NotausCommand = new RelayCommand(Notaus);
    }

    private void Notaus()
    {
        PumpeEin = false;
        HeizungEin = false;
        VentilEin = false;
        MotorDrehzahl = 0;
        SollTemperatur = 0;
    }
}
