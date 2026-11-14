using Xunit;
using OptiCHmiApp.Services;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp.Tests;

public class HauptViewModelTests
{
    private sealed class KonstanterPlcService : IPlcService
    {
        public bool IstSimulation => false;

        public Task<PlcStatus> LeseStatusAsync()
            => Task.FromResult(new PlcStatus(42.0, false, true, 60.0, false, 0.0, false));

        public Task SchreibeSollwertAsync(double sollwert) => Task.CompletedTask;
        public Task SchreibeHeizungAsync(bool ein) => Task.CompletedTask;
        public Task SchreibePumpeAsync(bool ein) => Task.CompletedTask;
        public Task NotausAsync(bool ein) => Task.CompletedTask;
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
