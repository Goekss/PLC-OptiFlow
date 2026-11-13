using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OptiCHmiApp.Services;

namespace OptiCHmiApp.ViewModels;

// ViewModel der Übersichtsseite: zeigt den Temperaturwert an.
// Genel bakış sayfasının ViewModel'i: sıcaklık değerini gösterir.
public class UebersichtViewModel : ObservableObject
{
    private readonly IPlcService _plcService;
    private int _temperatur;

    public int Temperatur
    {
        get => _temperatur;
        set => SetProperty(ref _temperatur, value);
    }

    public IAsyncRelayCommand LeseCommand { get; }

    public UebersichtViewModel(IPlcService plcService)
    {
        _plcService = plcService;
        LeseCommand = new AsyncRelayCommand(LeseAsync);
    }

    private async Task LeseAsync()
    {
        Temperatur = await _plcService.LeseTemperaturAsync();
    }
}
