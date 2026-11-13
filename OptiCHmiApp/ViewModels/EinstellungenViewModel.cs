using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;
using OptiCHmiApp.Themes;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Seite "Einstellungen".
public class EinstellungenViewModel : ObservableObject
{
    private string _backendAdresse;
    private int _abfrageIntervallMs;
    private string _speicherStatus = string.Empty;

    public string BackendAdresse
    {
        get => _backendAdresse;
        set => SetProperty(ref _backendAdresse, value);
    }

    public int AbfrageIntervallMs
    {
        get => _abfrageIntervallMs;
        set => SetProperty(ref _abfrageIntervallMs, value);
    }

    public bool IstDunkel
    {
        get => ThemeManager.AktuellesTheme == ThemeManager.Dunkel;
        set => ThemeManager.Anwenden(value ? ThemeManager.Dunkel : ThemeManager.Hell);
    }

    public string SpeicherStatus
    {
        get => _speicherStatus;
        private set => SetProperty(ref _speicherStatus, value);
    }

    public IAsyncRelayCommand SpeichernCommand { get; }

    public EinstellungenViewModel()
    {
        _backendAdresse = BenutzerEinstellungen.BackendAdresse;
        _abfrageIntervallMs = BenutzerEinstellungen.AbfrageIntervallMs;

        SpeichernCommand = new AsyncRelayCommand(SpeichernAsync);
        ThemeManager.ThemeGeaendert += OnThemeGeaendert;
    }

    private async Task SpeichernAsync()
    {
        if (AbfrageIntervallMs < 50)
        {
            AbfrageIntervallMs = 50;
        }

        BenutzerEinstellungen.BackendAdresse = BackendAdresse;
        BenutzerEinstellungen.AbfrageIntervallMs = AbfrageIntervallMs;
        BenutzerEinstellungen.Speichern();

        SpeicherStatus = "Einstellungen gespeichert.";
        await Task.Delay(2000);
        SpeicherStatus = string.Empty;
    }

    private void OnThemeGeaendert(string thema)
    {
        OnPropertyChanged(nameof(IstDunkel));
    }
}
