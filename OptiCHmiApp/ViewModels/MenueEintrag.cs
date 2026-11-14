using CommunityToolkit.Mvvm.ComponentModel;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// Ein einzelner Eintrag im Navigationsmenü; der Titel folgt der aktiven Sprache.
// Gezinme menüsündeki tek bir öğe; başlık aktif dili takip eder.
public class MenueEintrag : ObservableObject
{
    public string TitelSchluessel { get; }
    public object Seite { get; }

    public string Titel => Sprache.Text(TitelSchluessel);

    public MenueEintrag(string titelSchluessel, object seite)
    {
        TitelSchluessel = titelSchluessel;
        Seite = seite;
    }

    public void Aktualisieren() => OnPropertyChanged(nameof(Titel));
}
