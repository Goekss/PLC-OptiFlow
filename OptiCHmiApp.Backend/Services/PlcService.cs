using Grpc.Core;

namespace OptiCHmiApp.Backend.Services;

// gRPC-Service: liefert simulierte PLC-Werte.
// gRPC servisi: simüle edilmiş PLC değerleri döndürür.
public class PlcService : Plc.PlcBase
{
    private readonly Random _zufall = new();

    public override Task<LeseTemperaturReply> LeseTemperatur(LeseTemperaturRequest request, ServerCallContext context)
    {
        int temperatur = _zufall.Next(20, 81);

        return Task.FromResult(new LeseTemperaturReply
        {
            Temperatur = temperatur
        });
    }
}
