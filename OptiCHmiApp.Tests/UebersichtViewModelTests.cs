using Xunit;
using OptiCHmiApp.Services;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp.Tests;

public class UebersichtViewModelTests
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
    public async Task Start_StopptNachStopp_AktualisiertTemperatur()
    {
        var viewModel = new UebersichtViewModel(new KonstanterPlcService());

        var startTask = viewModel.StartStoppCommand.ExecuteAsync(null);

        await Task.Delay(650);

        Assert.True(viewModel.Laeuft);
        Assert.Equal(42.0, viewModel.Temperatur);

        await viewModel.StartStoppCommand.ExecuteAsync(null);
        await startTask;

        Assert.False(viewModel.Laeuft);
    }
}
