using System;
using System.Collections.Generic;
using System.Windows;

namespace OptiCHmiApp.Services;

// Verwaltet die aktive Sprache über austauschbare Sprachpakete (Sprachen/*.xaml).
// Değiştirilebilir dil paketleri (Sprachen/*.xaml) üzerinden aktif dili yönetir.
public static class Sprache
{
    public const string Deutsch = "de";
    public const string Englisch = "en";
    public const string Italienisch = "it";
    public const string Franzoesisch = "fr";
    public const string Tuerkisch = "tr";

    private static readonly HashSet<string> Verfuegbar = new()
    {
        Deutsch, Englisch, Italienisch, Franzoesisch, Tuerkisch
    };

    private static ResourceDictionary? _aktivesSprachpaket;

    // Meldet den Wechsel, damit ViewModels ihre berechneten Texte erneuern.
    // Değişimi bildirir; ViewModel'ler hesaplanan metinlerini yeniler.
    public static event Action<string>? SpracheGeaendert;

    public static string Aktuell { get; private set; } = Deutsch;

    public static void Initialisieren() => Anwenden(BenutzerEinstellungen.Sprache);

    public static void Anwenden(string sprache)
    {
        if (!Verfuegbar.Contains(sprache))
        {
            sprache = Deutsch;
        }

        if (_aktivesSprachpaket is not null && Aktuell == sprache)
        {
            return;
        }

        var anwendung = Application.Current;
        if (anwendung is null)
        {
            return;
        }

        var neu = new ResourceDictionary
        {
            Source = new Uri($"Sprachen/{sprache}.xaml", UriKind.Relative)
        };

        var woerterbuecher = anwendung.Resources.MergedDictionaries;
        if (_aktivesSprachpaket is not null)
        {
            woerterbuecher.Remove(_aktivesSprachpaket);
        }

        woerterbuecher.Add(neu);
        _aktivesSprachpaket = neu;
        Aktuell = sprache;

        SpracheGeaendert?.Invoke(sprache);
    }

    // Liefert den Text zum Schlüssel; fehlt ein Paket, bleibt der Schlüssel sichtbar.
    // Anahtara ait metni döndürür; paket yoksa anahtar görünür kalır.
    public static string Text(string schluessel)
        => Application.Current?.TryFindResource(schluessel) as string ?? schluessel;
}
