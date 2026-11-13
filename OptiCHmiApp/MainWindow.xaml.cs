using System.Windows;
using OptiCHmiApp.Services;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        IPlcService plcService = new GrpcPlcService("http://localhost:5262");
        DataContext = new HauptViewModel(plcService);
    }
}