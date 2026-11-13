using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OptiCHmiApp.Views;

// Yuvarlak gösterge: 0-100 arası bir değeri ibre ile gösterir.
// Rundanzeige: zeigt einen Wert zwischen 0 und 100 mit einem Zeiger an.
public partial class Rundanzeige : UserControl
{
    public static readonly DependencyProperty WertProperty =
        DependencyProperty.Register(
            nameof(Wert),
            typeof(double),
            typeof(Rundanzeige),
            new PropertyMetadata(0.0, OnWertGeaendert));

    public double Wert
    {
        get => (double)GetValue(WertProperty);
        set => SetValue(WertProperty, value);
    }

    public Rundanzeige()
    {
        InitializeComponent();
    }

    private static void OnWertGeaendert(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var anzeige = (Rundanzeige)d;
        double wert = Math.Clamp((double)e.NewValue, 0, 100);

        // 0  -> -90° (sol), 50 -> 0° (üst), 100 -> +90° (sağ)
        double zielWinkel = wert / 100.0 * 180.0 - 90.0;

        var animation = new DoubleAnimation
        {
            To = zielWinkel,
            Duration = TimeSpan.FromMilliseconds(120),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        anzeige.NadelRotation.BeginAnimation(RotateTransform.AngleProperty, animation);
    }
}
