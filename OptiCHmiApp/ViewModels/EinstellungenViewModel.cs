using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;
using OptiCHmiApp.Themes;

namespace OptiCHmiApp.ViewModels;

// Eine auswählbare Sprache in den Einstellungen.
// Ayarlarda seçilebilir bir dil.
public sealed class SprachEintrag : ObservableObject
{
    private bool _istAktiv;

    public string Code { get; }
    public string Name { get; }

    public bool IstAktiv
    {
        get => _istAktiv;
        set => SetProperty(ref _istAktiv, value);
    }

    public SprachEintrag(string code, string name)
    {
        Code = code;
        Name = name;
    }
}

// ViewModel der Seite "Einstellungen".
public class EinstellungenViewModel : ObservableObject
{
    private string _backendAdresse;
    private int _abfrageIntervallMs;
    private bool _zeigeSpeicherStatus;

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

    public string SpeicherStatus => _zeigeSpeicherStatus ? Sprache.Text("Gespeichert") : string.Empty;

    public IReadOnlyList<SprachEintrag> Sprachen { get; }

    public IAsyncRelayCommand SpeichernCommand { get; }

    public IRelayCommand<SprachEintrag> SpracheWaehlenCommand { get; }

    public EinstellungenViewModel()
    {
        _backendAdresse = BenutzerEinstellungen.BackendAdresse;
        _abfrageIntervallMs = BenutzerEinstellungen.AbfrageIntervallMs;

        Sprachen = new List<SprachEintrag>
        {
            new(Sprache.Deutsch, "Deutsch"),
            new(Sprache.Englisch, "English"),
            new(Sprache.Italienisch, "Italiano"),
            new(Sprache.Franzoesisch, "Français"),
            new(Sprache.Tuerkisch, "Türkçe")
        };

        foreach (var eintrag in Sprachen)
        {
            eintrag.IstAktiv = eintrag.Code == Sprache.Aktuell;
        }

        SpeichernCommand = new AsyncRelayCommand(SpeichernAsync);
        SpracheWaehlenCommand = new RelayCommand<SprachEintrag>(SpracheWaehlen);
        ThemeManager.ThemeGeaendert += OnThemeGeaendert;
        Sprache.SpracheGeaendert += OnSpracheGeaendert;
    }

    private void SpracheWaehlen(SprachEintrag? eintrag)
    {
        if (eintrag is null)
        {
            return;
        }

        Sprache.Anwenden(eintrag.Code);
        BenutzerEinstellungen.Sprache = eintrag.Code;
        BenutzerEinstellungen.Speichern();
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

        _zeigeSpeicherStatus = true;
        OnPropertyChanged(nameof(SpeicherStatus));
        await Task.Delay(2000);
        _zeigeSpeicherStatus = false;
        OnPropertyChanged(nameof(SpeicherStatus));
    }

    private void OnThemeGeaendert(string thema)
    {
        OnPropertyChanged(nameof(IstDunkel));
    }

    private void OnSpracheGeaendert(string sprache)
    {
        foreach (var eintrag in Sprachen)
        {
            eintrag.IstAktiv = eintrag.Code == sprache;
        }

        OnPropertyChanged(nameof(SpeicherStatus));
    }
}
