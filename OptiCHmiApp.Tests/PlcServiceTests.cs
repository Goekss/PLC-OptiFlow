using Xunit;
using OptiCHmiApp.Backend.Services;

namespace OptiCHmiApp.Tests;

public class KesselPhysikTests
{
    [Fact]
    public void HeizungEin_ErhoehtTemperatur()
    {
        var kessel = new KesselPhysik { Jitter = 0, HeizrateProSekunde = 10.0 };
        kessel.SchreibeHeizung(true);

        var vorher = kessel.Temperatur;
        kessel.Tick(1.0);

        Assert.True(kessel.Temperatur > vorher);
    }

    [Fact]
    public void Ab80Grad_IstWarnung()
    {
        var kessel = new KesselPhysik { Jitter = 0, HeizrateProSekunde = 100.0 };
        kessel.SchreibeHeizung(true);
        kessel.Tick(1.0);

        Assert.True(kessel.Temperatur >= 80.0);
        Assert.True(kessel.IstWarnung);
    }

    [Fact]
    public void Ab90Grad_IstKritischUndHeizungAus()
    {
        var kessel = new KesselPhysik { Jitter = 0, HeizrateProSekunde = 100.0 };
        kessel.SchreibeHeizung(true);
        kessel.Tick(1.0);

        Assert.True(kessel.IstKritisch);
        Assert.False(kessel.Heizung);
    }

    [Fact]
    public void Notaus_SchaltetHeizungUndPumpeAus()
    {
        var kessel = new KesselPhysik();
        kessel.SchreibeHeizung(true);
        kessel.SchreibePumpe(true);

        kessel.NotausAusloesen();

        Assert.True(kessel.Notaus);
        Assert.False(kessel.Heizung);
        Assert.False(kessel.Pumpe);
    }

    [Fact]
    public void NotausZuruecksetzen_SchaltetAutomatikEin()
    {
        var kessel = new KesselPhysik();
        kessel.NotausAusloesen();
        kessel.NotausZuruecksetzen();

        Assert.False(kessel.Notaus);
        Assert.True(kessel.Automatik);
    }

    [Fact]
    public void ZielErreicht_SchaltetHeizungNach5SekundenAutomatischAus()
    {
        var kessel = new KesselPhysik
        {
            Jitter = 0,
            HeizrateProSekunde = 1.0,
            KuehlrateProSekunde = 0
        };
        kessel.SchreibeHeizung(true); // manuell

        for (int i = 0; i < 40; i++)
        {
            kessel.Tick(1.0); // 20 -> 60 °C
        }

        Assert.True(kessel.ZielAlarmSekunden > 0);
        Assert.True(kessel.Heizung);

        for (int i = 0; i < 6; i++)
        {
            kessel.Tick(1.0);
        }

        Assert.False(kessel.Heizung);
        Assert.Equal(0.0, kessel.ZielAlarmSekunden);
        Assert.True(kessel.AutoGestoppt);
    }

    [Fact]
    public void ZielAlarm_WirdBeimManuellenAbschaltenAufgehoben()
    {
        var kessel = new KesselPhysik
        {
            Jitter = 0,
            HeizrateProSekunde = 1.0,
            KuehlrateProSekunde = 0
        };
        kessel.SchreibeHeizung(true);

        for (int i = 0; i < 40; i++)
        {
            kessel.Tick(1.0);
        }

        Assert.True(kessel.ZielAlarmSekunden > 0);

        kessel.SchreibeHeizung(false);
        kessel.Tick(0.1);

        Assert.Equal(0.0, kessel.ZielAlarmSekunden);
    }
}
