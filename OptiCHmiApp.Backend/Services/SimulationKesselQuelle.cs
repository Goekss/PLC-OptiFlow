namespace OptiCHmiApp.Backend.Services;

// Simulation: nutzt KesselPhysik und rechnet die Physik bei jedem Lesevorgang fort.
// Simülasyon: KesselPhysik kullanır ve her okumada fiziği ilerletir.
public class SimulationKesselQuelle : IKesselQuelle
{
    private readonly object _sync = new();
    private readonly KesselPhysik _kessel = new();
    private DateTime _letzte = DateTime.UtcNow;

    public KesselZustand LeseZustand()
    {
        lock (_sync)
        {
            var jetzt = DateTime.UtcNow;
            var dt = (jetzt - _letzte).TotalSeconds;
            _letzte = jetzt;
            _kessel.Tick(dt);

            return new KesselZustand(
                _kessel.Temperatur,
                _kessel.Heizung,
                _kessel.Pumpe,
                _kessel.Sollwert,
                _kessel.Notaus,
                _kessel.ZielAlarmSekunden,
                _kessel.AutoGestoppt);
        }
    }

    public void SchreibeSollwert(double sollwert)
    {
        lock (_sync) _kessel.SchreibeSollwert(sollwert);
    }

    public void SchreibeHeizung(bool ein)
    {
        lock (_sync) _kessel.SchreibeHeizung(ein);
    }

    public void SchreibePumpe(bool ein)
    {
        lock (_sync) _kessel.SchreibePumpe(ein);
    }

    public void NotausAusloesen()
    {
        lock (_sync) _kessel.NotausAusloesen();
    }

    public void NotausZuruecksetzen()
    {
        lock (_sync) _kessel.NotausZuruecksetzen();
    }
}
