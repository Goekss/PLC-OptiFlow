using System;
using System.Windows;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.Themes;

// Verwaltet das aktive Design (hell/dunkel) zur Laufzeit.
// Etkin tasarımı (açık/koyu) çalışma zamanında yönetir.
public static class ThemeManager
{
    public const string Dunkel = "Dunkel";
    public const string Hell = "Hell";

    private static ResourceDictionary? _aktivesTheme;

    public static event Action<string>? ThemeGeaendert;

    public static string AktuellesTheme { get; private set; } = Dunkel;

    public static void Initialisieren()
    {
        Anwenden(BenutzerEinstellungen.Thema);
    }

    public static void Anwenden(string thema)
    {
        if (thema != Dunkel && thema != Hell)
        {
            thema = Dunkel;
        }

        if (_aktivesTheme is not null && AktuellesTheme == thema)
        {
            return;
        }

        var pfad = thema == Hell ? "Themes/Hell.xaml" : "Themes/Dunkel.xaml";
        var neu = new ResourceDictionary { Source = new Uri(pfad, UriKind.Relative) };

        var woerterbuecher = Application.Current.Resources.MergedDictionaries;
        if (_aktivesTheme is not null)
        {
            woerterbuecher.Remove(_aktivesTheme);
        }

        woerterbuecher.Add(neu);
        _aktivesTheme = neu;
        AktuellesTheme = thema;

        BenutzerEinstellungen.Thema = thema;
        BenutzerEinstellungen.Speichern();

        ThemeGeaendert?.Invoke(thema);
    }

    public static void Umschalten()
    {
        Anwenden(AktuellesTheme == Dunkel ? Hell : Dunkel);
    }
}
