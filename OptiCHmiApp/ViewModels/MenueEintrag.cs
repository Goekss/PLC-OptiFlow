namespace OptiCHmiApp.ViewModels;

// Ein einzelner Eintrag im Navigationsmenü.
// Gezinme menüsündeki tek bir öğe.
public class MenueEintrag
{
    public string Titel { get; }
    public object Seite { get; }

    public MenueEintrag(string titel, object seite)
    {
        Titel = titel;
        Seite = seite;
    }
}
