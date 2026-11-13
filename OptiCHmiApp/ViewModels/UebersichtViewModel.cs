using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Übersichtsseite: liest die Temperatur automatisch,
// bis der Benutzer den Lesevorgang stoppt, und zeichnet den Verlauf.
// Genel bakış sayfasının ViewModel'i: kullanıcı okumayı durdurana kadar
// sıcaklığı otomatik okur ve geçmişi çizer.
public class UebersichtViewModel : ObservableObject
{
    private const int VerlaufLaenge = 60;

    private readonly IPlcService _plcService;
    private readonly IPlcService _simulierterPlc = new SimulierterPlcService();
    private CancellationTokenSource? _abbruchQuelle;
    private int _temperatur;
    private int _letzteAntwortMs;
    private int _minimum;
    private int _maximum;
    private bool _laeuft;
    private bool _istSimulation;
    private readonly Queue<int> _verlauf = new();
    private PointCollection _trendPunkte = new();

    public int Temperatur
    {
        get => _temperatur;
        set => SetProperty(ref _temperatur, value);
    }

    public int LetzteAntwortMs
    {
        get => _letzteAntwortMs;
        set => SetProperty(ref _letzteAntwortMs, value);
    }

    public int Minimum
    {
        get => _minimum;
        private set => SetProperty(ref _minimum, value);
    }

    public int Maximum
    {
        get => _maximum;
        private set => SetProperty(ref _maximum, value);
    }

    public int Messungen => _verlauf.Count;

    public PointCollection TrendPunkte
    {
        get => _trendPunkte;
        private set => SetProperty(ref _trendPunkte, value);
    }

    public bool Laeuft
    {
        get => _laeuft;
        private set
        {
            if (SetProperty(ref _laeuft, value))
            {
                OnPropertyChanged(nameof(StartStoppText));
            }
        }
    }

    // True, wenn keine echte Verbindung besteht und der Simulator Werte liefert.
    // Gerçek bağlantı yoksa ve simülatör değer sağlıyorsa true.
    public bool IstSimulation
    {
        get => _istSimulation;
        private set => SetProperty(ref _istSimulation, value);
    }

    public string StartStoppText => Laeuft ? "Stopp" : "Start";

    public IAsyncRelayCommand StartStoppCommand { get; }

    public UebersichtViewModel(IPlcService plcService)
    {
        _plcService = plcService;
        StartStoppCommand = new AsyncRelayCommand(StartStoppAsync, options: AsyncRelayCommandOptions.AllowConcurrentExecutions);
    }

    private async Task StartStoppAsync()
    {
        if (Laeuft)
        {
            _abbruchQuelle?.Cancel();
            Laeuft = false;
            return;
        }

        _abbruchQuelle = new CancellationTokenSource();
        Laeuft = true;
        await LeseSchleifeAsync(_abbruchQuelle.Token);
    }

    private async Task LeseSchleifeAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var stoppuhr = Stopwatch.StartNew();

                if (_istSimulation)
                {
                    // Simulation läuft bereits: weiterhin Simulator verwenden.
                    Temperatur = await _simulierterPlc.LeseTemperaturAsync();
                }
                else
                {
                    try
                    {
                        Temperatur = await _plcService.LeseTemperaturAsync();
                    }
                    catch (Exception)
                    {
                        // Backend nicht erreichbar: auf Simulator umschalten.
                        IstSimulation = true;
                        Temperatur = await _simulierterPlc.LeseTemperaturAsync();
                    }
                }

                stoppuhr.Stop();
                LetzteAntwortMs = (int)stoppuhr.ElapsedMilliseconds;

                AktualisiereVerlauf();
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await Task.Delay(BenutzerEinstellungen.AbfrageIntervallMs, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void AktualisiereVerlauf()
    {
        _verlauf.Enqueue(Temperatur);
        while (_verlauf.Count > VerlaufLaenge)
        {
            _verlauf.Dequeue();
        }

        Minimum = _verlauf.Min();
        Maximum = _verlauf.Max();
        OnPropertyChanged(nameof(Messungen));

        var punkte = new PointCollection();
        int i = 0;
        foreach (var wert in _verlauf)
        {
            // Negatives y: yukarıda daha yüksek sıcaklık gösterilsin.
            punkte.Add(new Point(i++, -wert));
        }

        TrendPunkte = punkte;
    }
}
