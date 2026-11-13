using CommunityToolkit.Mvvm.ComponentModel;
using OptiCHmiApp.Services;

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

    public HauptViewModel(IPlcService plcService)
    {
        Menue = new List<MenueEintrag>
        {
            new("Übersicht", new UebersichtViewModel(plcService)),
            new("Alarme", new AlarmViewModel()),
            new("Handbetrieb", new HandbetriebViewModel()),
            new("Einstellungen", new EinstellungenViewModel())
        };

        AusgewaehlterEintrag = Menue[0];
    }
}