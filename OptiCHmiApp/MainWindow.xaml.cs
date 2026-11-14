using System.Windows;
using OptiCHmiApp.Services;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        IPlcService plcService = new FallbackPlcService(BenutzerEinstellungen.BackendAdresse);
        DataContext = new HauptViewModel(plcService);
    }
}