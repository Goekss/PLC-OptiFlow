using System.Windows;
using OptiCHmiApp.Services;
using OptiCHmiApp.Themes;

namespace OptiCHmiApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        BenutzerEinstellungen.Laden();
        ThemeManager.Initialisieren();
        Sprache.Initialisieren();
        base.OnStartup(e);
    }
}
