using Grpc.Core;

namespace OptiCHmiApp.Backend.Services;

// gRPC-Service: liefert simulierte PLC-Werte.
// Die Temperatur ändert sich in kleinen Schritten (Random Walk),
// damit die Anzeige weich und realistisch wirkt.
// gRPC servisi: simüle edilmiş PLC değerleri döndürür.
// Sıcaklık, göstergenin akıcı ve gerçekçi görünmesi için küçük adımlarla değişir.
public class PlcService : Plc.PlcBase
{
    private readonly Random _zufall = new();
    private int _temperatur = 55;

    public override Task<LeseTemperaturReply> LeseTemperatur(LeseTemperaturRequest request, ServerCallContext context)
    {
        _temperatur = Math.Clamp(_temperatur + _zufall.Next(-3, 4), 20, 80);

        return Task.FromResult(new LeseTemperaturReply
        {
            Temperatur = _temperatur
        });
    }
}
