using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OptiCHmiApp.ViewModels;

public enum AlarmSchweregrad
{
    Info,
    Warnung,
    Kritisch
}

// Ein einzelner Alarm mit Quittierungszustand.
// Tek bir alarm ve onaylama durumu.
public class AlarmEintrag : ObservableObject
{
    private bool _istQuittiert;

    public string Zeit { get; }
    public string Meldung { get; }
    public string Bereich { get; }
    public AlarmSchweregrad Schweregrad { get; }

    public bool IstQuittiert
    {
        get => _istQuittiert;
        set => SetProperty(ref _istQuittiert, value);
    }

    public AlarmEintrag(string zeit, string meldung, string bereich, AlarmSchweregrad schweregrad)
    {
        Zeit = zeit;
        Meldung = meldung;
        Bereich = bereich;
        Schweregrad = schweregrad;
    }
}

// ViewModel der Seite "Alarme".
public class AlarmViewModel : ObservableObject
{
    public ObservableCollection<AlarmEintrag> Alarme { get; } = new();

    public IRelayCommand QuittierenCommand { get; }
    public IRelayCommand AlleQuittierenCommand { get; }

    public int AktiveAnzahl => Alarme.Count(a => !a.IstQuittiert);

    public AlarmViewModel()
    {
        QuittierenCommand = new RelayCommand<AlarmEintrag>(Quittieren);
        AlleQuittierenCommand = new RelayCommand(AlleQuittieren);

        Alarme.Add(new AlarmEintrag("12:04:12", "Kühlmitteltemperatur zu hoch", "Kühlkreis 1", AlarmSchweregrad.Kritisch));
        Alarme.Add(new AlarmEintrag("11:58:47", "Pumpendruck unter Sollwert", "Kühlmittelpumpe", AlarmSchweregrad.Warnung));
        Alarme.Add(new AlarmEintrag("11:32:03", "Ventilstellung nicht erreicht", "Ventil V-02", AlarmSchweregrad.Warnung));
        Alarme.Add(new AlarmEintrag("10:15:29", "Wartungsintervall erreicht", "Antrieb M-01", AlarmSchweregrad.Info));
        Alarme.Add(new AlarmEintrag("09:41:55", "Sensorabweichung erkannt", "Temperatursensor T-01", AlarmSchweregrad.Info));
    }

    private void Quittieren(AlarmEintrag? alarm)
    {
        if (alarm is null)
        {
            return;
        }

        alarm.IstQuittiert = true;
        OnPropertyChanged(nameof(AktiveAnzahl));
    }

    private void AlleQuittieren()
    {
        foreach (var alarm in Alarme)
        {
            alarm.IstQuittiert = true;
        }

        OnPropertyChanged(nameof(AktiveAnzahl));
    }
}
