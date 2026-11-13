using Xunit;
using OptiCHmiApp.Services;
using OptiCHmiApp.ViewModels;

namespace OptiCHmiApp.Tests;

public class UebersichtViewModelTests
{
    private sealed class KonstanterPlcService : IPlcService
    {
        public Task<int> LeseTemperaturAsync()
            => Task.FromResult(42);
    }

    [Fact]
    public async Task Start_StopptNachStopp_AktualisiertTemperatur()
    {
        var viewModel = new UebersichtViewModel(new KonstanterPlcService());

        var startTask = viewModel.StartStoppCommand.ExecuteAsync(null);

        await Task.Delay(650);

        Assert.True(viewModel.Laeuft);
        Assert.Equal(42, viewModel.Temperatur);

        await viewModel.StartStoppCommand.ExecuteAsync(null);
        await startTask;

        Assert.False(viewModel.Laeuft);
    }
}
