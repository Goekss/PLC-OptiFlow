namespace OptiCHmiApp.Services;

// Simuliert den Kessel lokal, solange keine echte Verbindung besteht.
// Gerçek bağlantı yokken kazanı yerel olarak simüle eder.
public class SimulierterPlcService : IPlcService
{
    private const double Heizrate = 0.5;
    private const double Kuehlrate = 0.25;
    private const double Umgebung = 20.0;
    private const double Jitter = 0.3;

    private readonly object _sync = new();
    private readonly Random _zufall = new();
    private double _temperatur = Umgebung;
    private bool _heizung;
    private bool _pumpe = true;
    private double _sollwert = 60.0;
    private bool _notaus;
    private bool _automatik = true;
    private double _zielAlarmSekunden;
    private bool _autoGestoppt;
    private DateTime _letzte = DateTime.UtcNow;

    public bool IstSimulation => true;

    public Task<PlcStatus> LeseStatusAsync()
    {
        lock (_sync)
        {
            Tick();
            return Task.FromResult(new PlcStatus(_temperatur, _heizung, _pumpe, _sollwert, _notaus, _zielAlarmSekunden, _autoGestoppt));
        }
    }

    public Task SchreibeSollwertAsync(double sollwert)
    {
        lock (_sync)
        {
            if (!_notaus)
            {
                _sollwert = Math.Clamp(sollwert, 0.0, 100.0);
                _autoGestoppt = false;
            }
        }

        return Task.CompletedTask;
    }

    public Task SchreibeHeizungAsync(bool ein)
    {
        lock (_sync)
        {
            if (!_notaus)
            {
                _heizung = ein;
                _automatik = false;
                if (ein)
                {
                    _autoGestoppt = false;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task SchreibePumpeAsync(bool ein)
    {
        lock (_sync)
        {
            if (!_notaus)
            {
                _pumpe = ein;
            }
        }

        return Task.CompletedTask;
    }

    public Task NotausAsync(bool ein)
    {
        lock (_sync)
        {
            if (ein)
            {
                _notaus = true;
                _heizung = false;
                _pumpe = false;
                _zielAlarmSekunden = 0;
            }
            else
            {
                _notaus = false;
                _automatik = true;
                _autoGestoppt = false;
            }
        }

        return Task.CompletedTask;
    }

    private void Tick()
    {
        var jetzt = DateTime.UtcNow;
        var dt = Math.Clamp((jetzt - _letzte).TotalSeconds, 0.0, 1.0);
        _letzte = jetzt;

        if (_notaus)
        {
            _heizung = false;
            _pumpe = false;
            _zielAlarmSekunden = 0;
            _temperatur = Annaehern(_temperatur, Umgebung, Kuehlrate * dt);
        }
        else
        {
            if (_automatik)
            {
                // Hysterese verhindert schnelles Flackern der Heizung.
                const double hysterese = 0.5;
                if (_heizung && _temperatur >= _sollwert + hysterese)
                {
                    _heizung = false;
                }
                else if (!_heizung && _temperatur < _sollwert - hysterese)
                {
                    _heizung = true;
                }
            }

            if (_heizung)
            {
                _temperatur += Heizrate * dt;
            }
            else
            {
                _temperatur = Annaehern(_temperatur, Umgebung, Kuehlrate * dt);
            }

            if (_temperatur >= 90.0)
            {
                _heizung = false;
            }

            // Ziel erreicht: Alarm + Countdown, danach automatische Abschaltung.
            if (_heizung && _temperatur >= _sollwert && _zielAlarmSekunden <= 0)
            {
                _zielAlarmSekunden = 5.0;
            }

            if (_zielAlarmSekunden > 0)
            {
                if (!_automatik && !_heizung)
                {
                    _zielAlarmSekunden = 0;
                }
                else
                {
                    _zielAlarmSekunden -= dt;
                    if (_zielAlarmSekunden <= 0)
                    {
                        _zielAlarmSekunden = 0;
                        _heizung = false;
                        _automatik = false;
                        _autoGestoppt = true;
                    }
                }
            }
        }

        _temperatur += (_zufall.NextDouble() * 2.0 - 1.0) * Jitter * dt;
        _temperatur = Math.Clamp(_temperatur, 10.0, 120.0);
    }

    private static double Annaehern(double aktuell, double ziel, double schritt)
    {
        if (aktuell < ziel)
        {
            return Math.Min(ziel, aktuell + schritt);
        }

        if (aktuell > ziel)
        {
            return Math.Max(ziel, aktuell - schritt);
        }

        return aktuell;
    }
}
