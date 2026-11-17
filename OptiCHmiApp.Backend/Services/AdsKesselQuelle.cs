using TwinCAT.Ads;

namespace OptiCHmiApp.Backend.Services;

// Echte PLC-Anbindung über ADS: liest und schreibt die GVL-Symbole der TwinCAT-Steuerung.
// Gerçek PLC bağlantısı (ADS): TwinCAT kontrolünün GVL sembollerini okur/yazar.
public class AdsKesselQuelle : IKesselQuelle, IDisposable
{
    private readonly object _sync = new();
    private readonly AdsClient _client = new();
    private readonly uint _hTemperatur;
    private readonly uint _hHeizung;
    private readonly uint _hPumpe;
    private readonly uint _hSollwert;
    private readonly uint _hNotaus;
    private readonly uint _hZielAlarm;
    private readonly uint _hAutoGestoppt;
    private readonly uint _hCmdSollwert;
    private readonly uint _hCmdHeizung;
    private readonly uint _hCmdPumpe;
    private readonly uint _hCmdNotaus;
    private readonly uint _hCmdNotausReset;

    public AdsKesselQuelle(string amsNetId, int port, int timeoutMs)
    {
        _client.Timeout = timeoutMs;
        _client.Connect(AmsNetId.Parse(amsNetId), port);

        _hTemperatur = _client.CreateVariableHandle("GVL.Temperatur");
        _hHeizung = _client.CreateVariableHandle("GVL.Heizung");
        _hPumpe = _client.CreateVariableHandle("GVL.Pumpe");
        _hSollwert = _client.CreateVariableHandle("GVL.Sollwert");
        _hNotaus = _client.CreateVariableHandle("GVL.Notaus");
        _hZielAlarm = _client.CreateVariableHandle("GVL.ZielAlarmSekunden");
        _hAutoGestoppt = _client.CreateVariableHandle("GVL.AutoGestoppt");

        // Schreibzugriffe gehen auf die Kommando-Symbole (Regeln laufen in der PLC).
        _hCmdSollwert = _client.CreateVariableHandle("GVL.CmdSollwert");
        _hCmdHeizung = _client.CreateVariableHandle("GVL.CmdHeizung");
        _hCmdPumpe = _client.CreateVariableHandle("GVL.CmdPumpe");
        _hCmdNotaus = _client.CreateVariableHandle("GVL.CmdNotaus");
        _hCmdNotausReset = _client.CreateVariableHandle("GVL.CmdNotausReset");
    }

    public KesselZustand LeseZustand()
    {
        lock (_sync)
        {
            return new KesselZustand(
                _client.ReadAny<double>(_hTemperatur),
                _client.ReadAny<bool>(_hHeizung),
                _client.ReadAny<bool>(_hPumpe),
                _client.ReadAny<double>(_hSollwert),
                _client.ReadAny<bool>(_hNotaus),
                _client.ReadAny<double>(_hZielAlarm),
                _client.ReadAny<bool>(_hAutoGestoppt));
        }
    }

    public void SchreibeSollwert(double sollwert)
    {
        lock (_sync) _client.WriteAny(_hCmdSollwert, sollwert);
    }

    public void SchreibeHeizung(bool ein)
    {
        lock (_sync) _client.WriteAny(_hCmdHeizung, ein);
    }

    public void SchreibePumpe(bool ein)
    {
        lock (_sync) _client.WriteAny(_hCmdPumpe, ein);
    }

    public void NotausAusloesen()
    {
        lock (_sync) _client.WriteAny(_hCmdNotaus, true);
    }

    public void NotausZuruecksetzen()
    {
        lock (_sync) _client.WriteAny(_hCmdNotausReset, true);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
