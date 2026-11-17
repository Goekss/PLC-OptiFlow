using Grpc.Core;

namespace OptiCHmiApp.Backend.Services;

// gRPC-Service: hält den Kesselzustand und rechnet die Physik fort.
// gRPC servisi: kazan durumunu tutar ve fiziği ilerletir.
public class PlcService : Plc.PlcBase
{
    private readonly object _sync = new();
    private readonly IKesselQuelle _quelle;

    public PlcService(IKesselQuelle quelle)
    {
        _quelle = quelle;
    }

    public override Task<LeseStatusReply> LeseStatus(LeseStatusRequest request, ServerCallContext context)
    {
        KesselZustand zustand;
        try
        {
            lock (_sync)
            {
                zustand = _quelle.LeseZustand();
            }
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }

        return Task.FromResult(new LeseStatusReply
        {
            Temperatur = zustand.Temperatur,
            Heizung = zustand.Heizung,
            Pumpe = zustand.Pumpe,
            Sollwert = zustand.Sollwert,
            Notaus = zustand.Notaus,
            ZielAlarmSekunden = zustand.ZielAlarmSekunden,
            AutoGestoppt = zustand.AutoGestoppt
        });
    }

    public override Task<SchreibeSollwertReply> SchreibeSollwert(SchreibeSollwertRequest request, ServerCallContext context)
    {
        try
        {
            lock (_sync)
            {
                _quelle.SchreibeSollwert(request.Sollwert);
            }

            return Task.FromResult(new SchreibeSollwertReply());
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
    }

    public override Task<SchreibeHeizungReply> SchreibeHeizung(SchreibeHeizungRequest request, ServerCallContext context)
    {
        try
        {
            lock (_sync)
            {
                _quelle.SchreibeHeizung(request.Ein);
            }

            return Task.FromResult(new SchreibeHeizungReply());
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
    }

    public override Task<SchreibePumpeReply> SchreibePumpe(SchreibePumpeRequest request, ServerCallContext context)
    {
        try
        {
            lock (_sync)
            {
                _quelle.SchreibePumpe(request.Ein);
            }

            return Task.FromResult(new SchreibePumpeReply());
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
    }

    public override Task<NotausReply> Notaus(NotausRequest request, ServerCallContext context)
    {
        try
        {
            lock (_sync)
            {
                if (request.Ein)
                {
                    _quelle.NotausAusloesen();
                }
                else
                {
                    _quelle.NotausZuruecksetzen();
                }
            }

            return Task.FromResult(new NotausReply());
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
        }
    }
}
