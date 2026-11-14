using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp.Views;

public partial class UebersichtView : UserControl
{
    public UebersichtView()
    {
        InitializeComponent();
    }

    // Reine UI-Aufgabe: Mausposition an das ViewModel für den Messwert-Cursor.
    private void Plot_MouseMove(object sender, MouseEventArgs e)
    {
        if (DataContext is UebersichtViewModel viewModel)
        {
            viewModel.ZeigeCursor(e.GetPosition((IInputElement)sender).X);
        }
    }

    private void Plot_MouseLeave(object sender, MouseEventArgs e)
    {
        (DataContext as UebersichtViewModel)?.VersteckeCursor();
    }
}
