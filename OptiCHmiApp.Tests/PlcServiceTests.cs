using Xunit;
using OptiCHmiApp.Backend;
using OptiCHmiApp.Backend.Services;

namespace OptiCHmiApp.Tests;

public class PlcServiceTests
{
    [Fact]
    public async Task LeseTemperatur_LiefertWertZwischen20Und80()
    {
        var dienst = new PlcService();

        var antwort = await dienst.LeseTemperatur(new LeseTemperaturRequest(), null!);

        Assert.InRange(antwort.Temperatur, 20, 80);
    }
}
