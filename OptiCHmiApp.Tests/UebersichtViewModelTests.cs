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
    public async Task LeseCommand_AktualisiertTemperatur()
    {
        var viewModel = new UebersichtViewModel(new KonstanterPlcService());

        await viewModel.LeseCommand.ExecuteAsync(null);

        Assert.Equal(42, viewModel.Temperatur);
    }
}
