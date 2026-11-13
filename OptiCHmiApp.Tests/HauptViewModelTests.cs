using Xunit;
using OptiCHmiApp.Services;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp.Tests;

public class HauptViewModelTests
{
    private sealed class KonstanterPlcService : IPlcService
    {
        public Task<int> LeseTemperaturAsync()
            => Task.FromResult(42);
    }

    [Fact]
    public void BeimStart_IstErsteSeiteUebersicht()
    {
        var viewModel = new HauptViewModel(new KonstanterPlcService());

        Assert.Equal(4, viewModel.Menue.Count);
        Assert.IsType<UebersichtViewModel>(viewModel.AktuelleSeite);
    }

    [Fact]
    public void MenueAuswahl_AendertAktuelleSeite()
    {
        var viewModel = new HauptViewModel(new KonstanterPlcService());

        viewModel.AusgewaehlterEintrag = viewModel.Menue[2];

        Assert.IsType<HandbetriebViewModel>(viewModel.AktuelleSeite);
    }
}
