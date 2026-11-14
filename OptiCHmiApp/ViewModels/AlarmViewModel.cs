using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;

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

    public string Id { get; }
    public string Zeit { get; }
    public string Meldung { get; }
    public string Bereich { get; }
    public AlarmSchweregrad Schweregrad { get; }

    public bool IstQuittiert
    {
        get => _istQuittiert;
        set => SetProperty(ref _istQuittiert, value);
    }

    public AlarmEintrag(string id, string zeit, string meldung, string bereich, AlarmSchweregrad schweregrad)
    {
        Id = id;
        Zeit = zeit;
        Meldung = meldung;
        Bereich = bereich;
        Schweregrad = schweregrad;
    }
}

// ViewModel der Seite "Alarme": leitet aktive Alarme aus dem Maschinenzustand ab.
// "Alarme" sayfasının ViewModel'i: aktif alarmları makine durumundan türetir.
public class AlarmViewModel : ObservableObject
{
    private readonly IPlcService _plcService;
    private readonly DispatcherTimer _timer;

    public ObservableCollection<AlarmEintrag> Alarme { get; } = new();

    public IRelayCommand QuittierenCommand { get; }
    public IRelayCommand AlleQuittierenCommand { get; }

    public int AktiveAnzahl => Alarme.Count(a => !a.IstQuittiert);

    public AlarmViewModel(IPlcService plcService)
    {
        _plcService = plcService;

        QuittierenCommand = new RelayCommand<AlarmEintrag>(Quittieren);
        AlleQuittierenCommand = new RelayCommand(AlleQuittieren);

        // Beim Sprachwechsel werden die Meldungen beim nächsten Tick neu erzeugt.
        // Dil değişiminde mesajlar bir sonraki okumada yeniden oluşturulur.
        Sprache.SpracheGeaendert += _ =>
        {
            Alarme.Clear();
            OnPropertyChanged(nameof(AktiveAnzahl));
        };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => _ = AktualisiereAsync();
        _timer.Start();
    }

    private async Task AktualisiereAsync()
    {
        try
        {
            var status = await _plcService.LeseStatusAsync();
            Aktualisiere(status);
        }
        catch
        {
            // Verbindungsfehler: nichts tun, nächster Versuch folgt.
        }
    }

    private void Aktualisiere(PlcStatus status)
    {
        var jetzt = DateTime.Now.ToString("HH:mm:ss");
        var gewuenscht = new List<AlarmEintrag>();

        if (status.Temperatur >= 90.0)
        {
            gewuenscht.Add(new AlarmEintrag("temp_kritisch", jetzt, Sprache.Text("AlarmTempKritisch"), Sprache.Text("BereichKessel"), AlarmSchweregrad.Kritisch));
        }
        else if (status.Temperatur >= 80.0)
        {
            gewuenscht.Add(new AlarmEintrag("temp_warnung", jetzt, Sprache.Text("AlarmTempWarnung"), Sprache.Text("BereichKessel"), AlarmSchweregrad.Warnung));
        }

        if (status.ZielAlarmSekunden > 0)
        {
            gewuenscht.Add(new AlarmEintrag("ziel", jetzt, Sprache.Text("AlarmSollwertErreicht"), Sprache.Text("BereichKessel"), AlarmSchweregrad.Warnung));
        }

        if (_plcService.IstSimulation)
        {
            gewuenscht.Add(new AlarmEintrag("verbindung", jetzt, Sprache.Text("AlarmVerbindungGetrennt"), Sprache.Text("BereichKommunikation"), AlarmSchweregrad.Warnung));
        }

        foreach (var vorhanden in Alarme.ToList())
        {
            if (!gewuenscht.Any(g => g.Id == vorhanden.Id))
            {
                Alarme.Remove(vorhanden);
            }
        }

        foreach (var neu in gewuenscht)
        {
            if (!Alarme.Any(a => a.Id == neu.Id))
            {
                Alarme.Add(neu);
            }
        }

        OnPropertyChanged(nameof(AktiveAnzahl));
    }

    private void Quittieren(AlarmEintrag? alarm)
    {
        if (alarm is null)
        {
            return;
        }

        if (alarm.Id == "ziel")
        {
            // Manuelles Abschalten der Heizung hebt den Ziel-Alarm auf.
            // Isıtıcıyı manuel kapatmak hedef alarmını iptal eder.
            _ = _plcService.SchreibeHeizungAsync(false);
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
