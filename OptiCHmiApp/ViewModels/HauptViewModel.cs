using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;
using OptiCHmiApp.Themes;

namespace OptiCHmiApp.ViewModels;

// Haupt-ViewModel: hält das Menü und die aktuell angezeigte Seite.
// Ana ViewModel: menüyü ve o an gösterilen sayfayı tutar.
public class HauptViewModel : ObservableObject
{
    private MenueEintrag? _ausgewaehlterEintrag;
    private object? _aktuelleSeite;

    public IReadOnlyList<MenueEintrag> Menue { get; }

    public MenueEintrag? AusgewaehlterEintrag
    {
        get => _ausgewaehlterEintrag;
        set
        {
            if (SetProperty(ref _ausgewaehlterEintrag, value) && value is not null)
            {
                AktuelleSeite = value.Seite;
            }
        }
    }

    public object? AktuelleSeite
    {
        get => _aktuelleSeite;
        set => SetProperty(ref _aktuelleSeite, value);
    }

    public string ThemeIcon => ThemeManager.AktuellesTheme == ThemeManager.Dunkel ? "☀" : "🌙";

    public string ThemeTooltip => ThemeManager.AktuellesTheme == ThemeManager.Dunkel
        ? "Zum hellen Design wechseln"
        : "Zum dunklen Design wechseln";

    public IRelayCommand ThemeUmschaltenCommand { get; }

    public HauptViewModel(IPlcService plcService)
    {
        Menue = new List<MenueEintrag>
        {
            new("Übersicht", new UebersichtViewModel(plcService)),
            new("Alarme", new AlarmViewModel(plcService)),
            new("Handbetrieb", new HandbetriebViewModel(plcService)),
            new("Einstellungen", new EinstellungenViewModel())
        };

        ThemeUmschaltenCommand = new RelayCommand(ThemeManager.Umschalten);
        ThemeManager.ThemeGeaendert += OnThemeGeaendert;

        AusgewaehlterEintrag = Menue[0];
    }

    private void OnThemeGeaendert(string thema)
    {
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeTooltip));
    }
}