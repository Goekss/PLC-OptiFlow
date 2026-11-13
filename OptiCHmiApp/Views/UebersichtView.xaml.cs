using System.Windows;
using System.Windows.Controls;

namespace OptiCHmiApp.Views;

public partial class UebersichtView : UserControl
{
    public UebersichtView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    // Passt die Kennzahlen-Karten an die verfügbare Breite an.
    // Kennzahl kartlarını mevcut genişliğe göre yeniden dizer.
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        KpiGrid.Columns = e.NewSize.Width < 560 ? 1 : e.NewSize.Width < 820 ? 2 : 3;
    }
}
