using System.Windows;
using System.Windows.Controls;

namespace OptiCHmiApp.Views;

public partial class HandbetriebView : UserControl
{
    public HandbetriebView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    // Ordnet die Karten abhängig von der verfügbaren Breite neu an.
    // Kartları mevcut genişliğe göre yeniden dizer.
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SchalterGrid.Columns = e.NewSize.Width < 560 ? 1 : e.NewSize.Width < 820 ? 2 : 3;
        ReglerGrid.Columns = e.NewSize.Width < 620 ? 1 : 2;
    }
}
