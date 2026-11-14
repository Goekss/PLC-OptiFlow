using System;
using System.IO;

namespace OptiCHmiApp.Services;

// Liest und speichert einfache Benutzereinstellungen in %AppData%.
// Basit kullanıcı ayarlarını %AppData% içinde okur ve kaydeder.
public static class BenutzerEinstellungen
{
    private static readonly string Dateipfad = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OptiCHmiApp",
        "einstellungen.txt");

    public static string BackendAdresse { get; set; } = "http://localhost:5262";

    public static int AbfrageIntervallMs { get; set; } = 100;

    public static string Thema { get; set; } = "Dunkel";

    public static string Sprache { get; set; } = "de";

    public static void Laden()
    {
        try
        {
            if (!File.Exists(Dateipfad))
            {
                return;
            }

            foreach (var zeile in File.ReadAllLines(Dateipfad))
            {
                var teile = zeile.Split('=', 2);
                if (teile.Length != 2)
                {
                    continue;
                }

                var schluessel = teile[0].Trim();
                var wert = teile[1].Trim();

                switch (schluessel)
                {
                    case "BackendAdresse":
                        if (!string.IsNullOrWhiteSpace(wert))
                        {
                            BackendAdresse = wert;
                        }
                        break;
                    case "AbfrageIntervallMs":
                        if (int.TryParse(wert, out var intervall) && intervall >= 50)
                        {
                            AbfrageIntervallMs = intervall;
                        }
                        break;
                    case "Thema":
                        Thema = wert == "Hell" ? "Hell" : "Dunkel";
                        break;
                    case "Sprache":
                        if (!string.IsNullOrWhiteSpace(wert))
                        {
                            Sprache = wert;
                        }
                        break;
                }
            }
        }
        catch
        {
            // Persistenz ist optional; Fehler werden ignoriert.
        }
    }

    public static void Speichern()
    {
        try
        {
            var ordner = Path.GetDirectoryName(Dateipfad);
            if (!string.IsNullOrEmpty(ordner))
            {
                Directory.CreateDirectory(ordner);
            }

            File.WriteAllText(
                Dateipfad,
                $"BackendAdresse={BackendAdresse}{Environment.NewLine}" +
                $"AbfrageIntervallMs={AbfrageIntervallMs}{Environment.NewLine}" +
                $"Thema={Thema}{Environment.NewLine}" +
                $"Sprache={Sprache}{Environment.NewLine}");
        }
        catch
        {
            // Persistenz ist optional; Fehler werden ignoriert.
        }
    }
}
