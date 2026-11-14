using Grpc.Core;

namespace OptiCHmiApp.Backend.Services;

// gRPC-Service: hält den Kesselzustand und rechnet die Physik fort.
// gRPC servisi: kazan durumunu tutar ve fiziği ilerletir.
public class PlcService : Plc.PlcBase
{
    private readonly object _sync = new();
    private readonly KesselPhysik _kessel = new();
    private DateTime _letzteAktualisierung = DateTime.UtcNow;

    public override Task<LeseStatusReply> LeseStatus(LeseStatusRequest request, ServerCallContext context)
    {
        LeseStatusReply antwort;
        lock (_sync)
        {
            var jetzt = DateTime.UtcNow;
            var dt = (jetzt - _letzteAktualisierung).TotalSeconds;
            _letzteAktualisierung = jetzt;
            _kessel.Tick(dt);

            antwort = new LeseStatusReply
            {
                Temperatur = _kessel.Temperatur,
                Heizung = _kessel.Heizung,
                Pumpe = _kessel.Pumpe,
                Sollwert = _kessel.Sollwert,
                Notaus = _kessel.Notaus,
                ZielAlarmSekunden = _kessel.ZielAlarmSekunden,
                AutoGestoppt = _kessel.AutoGestoppt
            };
        }

        return Task.FromResult(antwort);
    }

    public override Task<SchreibeSollwertReply> SchreibeSollwert(SchreibeSollwertRequest request, ServerCallContext context)
    {
        lock (_sync)
        {
            _kessel.SchreibeSollwert(request.Sollwert);
        }

        return Task.FromResult(new SchreibeSollwertReply());
    }

    public override Task<SchreibeHeizungReply> SchreibeHeizung(SchreibeHeizungRequest request, ServerCallContext context)
    {
        lock (_sync)
        {
            _kessel.SchreibeHeizung(request.Ein);
        }

        return Task.FromResult(new SchreibeHeizungReply());
    }

    public override Task<SchreibePumpeReply> SchreibePumpe(SchreibePumpeRequest request, ServerCallContext context)
    {
        lock (_sync)
        {
            _kessel.SchreibePumpe(request.Ein);
        }

        return Task.FromResult(new SchreibePumpeReply());
    }

    public override Task<NotausReply> Notaus(NotausRequest request, ServerCallContext context)
    {
        lock (_sync)
        {
            if (request.Ein)
            {
                _kessel.NotausAusloesen();
            }
            else
            {
                _kessel.NotausZuruecksetzen();
            }
        }

        return Task.FromResult(new NotausReply());
    }
}
