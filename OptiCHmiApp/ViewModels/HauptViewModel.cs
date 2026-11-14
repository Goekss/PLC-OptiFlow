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

    public string ThemeTooltip => Sprache.Text(ThemeManager.AktuellesTheme == ThemeManager.Dunkel
        ? "ThemeTooltipZuHell"
        : "ThemeTooltipZuDunkel");

    public IRelayCommand ThemeUmschaltenCommand { get; }

    public IRelayCommand AlarmeAnzeigenCommand { get; }

    public AnlagenStatusViewModel Status { get; }

    public HauptViewModel(IPlcService plcService)
    {
        Status = new AnlagenStatusViewModel(plcService);

        Menue = new List<MenueEintrag>
        {
            new("MenueUebersicht", new UebersichtViewModel(plcService)),
            new("MenueAlarme", new AlarmViewModel(plcService)),
            new("MenueHandbetrieb", new HandbetriebViewModel(plcService)),
            new("MenueEinstellungen", new EinstellungenViewModel())
        };

        ThemeUmschaltenCommand = new RelayCommand(ThemeManager.Umschalten);
        AlarmeAnzeigenCommand = new RelayCommand(AlarmeAnzeigen);
        ThemeManager.ThemeGeaendert += OnThemeGeaendert;
        Sprache.SpracheGeaendert += OnSpracheGeaendert;

        AusgewaehlterEintrag = Menue[0];
    }

    // Die Alarmseite ist der zweite Menüeintrag.
    private void AlarmeAnzeigen() => AusgewaehlterEintrag = Menue[1];

    private void OnThemeGeaendert(string thema)
    {
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeTooltip));
    }

    private void OnSpracheGeaendert(string sprache)
    {
        foreach (var eintrag in Menue)
        {
            eintrag.Aktualisieren();
        }

        OnPropertyChanged(nameof(ThemeTooltip));
    }
}