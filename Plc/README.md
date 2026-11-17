# TwinCAT 3.1 – Kessel-Steuerung (PLC, OOP)

Objektorientiertes PLC-Projekt für die simulierte Kesselanlage. Die Physik
entspricht `OptiCHmiApp.Backend/Services/KesselPhysik.cs` (Heizen/Kühlen,
Hysterese, 80/90 °C-Schwellen, Notaus, Ziel-Alarm + automatische Abschaltung).

> ⚠️ **Hinweis:** Der PLC-Code wurde noch **nicht** in TwinCAT XAE
> kompiliert/getestet. Die `.TcPOU`/`.TcDUT`-Dateien sind von Hand geschrieben;
> beim Öffnen in XAE können Syntax- oder Formatfehler auftreten (z. B.
> `SUPER^.` oder Property-Deklaration). Diese sind normal und korrigierbar.

## OOP-Struktur

```
I_Maschine  (Interface: Start, Stop, NotausAusloesen, NotausZuruecksetzen)
    ▲ IMPLEMENTS
FB_Maschine (Basis: _zustand, Notaus-Sperre, Property Zustand)
    ▲ EXTENDS
FB_Kessel   (Heiz-/Kühlphysik, 80/90 °C, 5 s Countdown; Methoden + Property Temperatur)

E_Zustand = (Aus, Bereit, Heizen, Warnung, Kritisch, Notaus)
```

`MAIN` ruft nur die `FB_Kessel`-Instanz auf (`fbKessel();`).

## Inhalt

| Datei | Zweck |
|---|---|
| `PlcKessel/PlcKessel.plcproj` | TwinCAT-3.1-PLC-Projekt |
| `PlcKessel/DUTs/E_Zustand.TcDUT` | Zustands-Enum |
| `PlcKessel/POUs/I_Maschine.TcPOU` | Interface (Start, Stop, Notaus*) |
| `PlcKessel/POUs/FB_Maschine.TcPOU` | Basis-FB (Zustand, Notaus-Sperre) |
| `PlcKessel/POUs/FB_Kessel.TcPOU` | Kessel-Logik (EXTENDS FB_Maschine) |
| `PlcKessel/POUs/MAIN.TcPOU` | Programm: ruft `fbKessel()` pro Zyklus auf |
| `PlcKessel/GVLs/GVL.TcGVL` | ADS-Schnittstelle (Kommandos + Zustand) |

## ADS-Schnittstelle (GVL)

### Kommandos (ADS schreibt, FB_Kessel verarbeitet)

| Symbol | Typ | Bedeutung |
|---|---|---|
| `GVL.CmdHeizung` | BOOL | Heizung manuell schalten (Flanke) |
| `GVL.CmdPumpe` | BOOL | Pumpe manuell schalten (Flanke) |
| `GVL.CmdSollwert` | LREAL | Zieltemperatur (Pegel, 0..100 °C) |
| `GVL.CmdNotaus` | BOOL | Notaus auslösen (Impuls) |
| `GVL.CmdNotausReset` | BOOL | Notaus zurücksetzen (Impuls) |

### Zustand (nur PLC schreibt, ADS liest)

| Symbol | Typ | Bedeutung |
|---|---|---|
| `GVL.Temperatur` | LREAL | Aktuelle Temperatur (°C) |
| `GVL.Heizung` | BOOL | Heizung Ein/Aus |
| `GVL.Pumpe` | BOOL | Pumpe Ein/Aus |
| `GVL.Sollwert` | LREAL | Zieltemperatur (Ist-Wert) |
| `GVL.Notaus` | BOOL | Notaus aktiv |
| `GVL.ZielAlarmSekunden` | LREAL | Countdown bis Auto-Stopp |
| `GVL.AutoGestoppt` | BOOL | Automatische Abschaltung erfolgt |

> **Notaus-Sperre:** ADS schreibt nur die `Cmd*`-Symbole. `FB_Kessel` liest sie
> und wendet die Regeln (Notaus-Sperre, Alarm, Klemmung) an. Die
> Zustandsvariablen werden ausschließlich von der PLC geschrieben und sind
> für ADS schreibgeschützt.

Das Backend liest die Zustands-Symbole und schreibt auf die Kommando-Symbole
(`AdsKesselQuelle` in `OptiCHmiApp.Backend/Services/AdsKesselQuelle.cs`).

## In TwinCAT 3.1 öffnen

1. TwinCAT 3.1 XAE (Visual-Studio-Erweiterung) installieren – die Shell ist
   kostenlos: https://www.beckhoff.com/de-de/produkte/automation/twincat/te1xxx-twincat-3-engineering/
2. Neue TwinCAT-Lösung anlegen (File → New → TwinCAT XAE Project).
3. Vorhandenes PLC-Projekt hinzufügen: Rechtsklick auf PLC-Ordner →
   *Add* → *Existing Item* → `PlcKessel/PlcKessel.plcproj` auswählen.
   Alternativ: Neues Standard-PLC-Projekt anlegen und die Dateien aus
   `DUTs/`, `GVLs/` und `POUs/` hineinkopieren (Datei → Add → Existing Item).
4. Task-Zykluszeit auf **100 ms** setzen (passend zum festen `Aktualisiere(0.1)`).
5. Build (Build → Build Solution), dann Aktivieren (Activate Configuration)
   und Start (Start/Restart TwinCAT System).

## AMS Net ID und Lizenz

- **AMS Net ID** der lokalen Runtime: i. d. R. `127.0.0.1.1.1`. Sie steht im
  TwinCAT-Tray (System-Tray-Icon → Properties → AMS Net Id). Das Backend nutzt
  diesen Wert aus `appsettings.json` (`Ads:AmsNetId`), Standard ist ebenfalls
  `127.0.0.1.1.1`.
- **Lizenz:** Die XAE (Programmierung) ist kostenlos. Für die **Runtime**
  (Ausführung auf dem lokalen Rechner) kann eine kostenlose **7-Tage-Trial-
  Lizenz** generiert werden: TwinCAT-Tray → *About* → *Manage Licenses* →
  *7 Days Trial License* (alle 7 Tage erneuerbar). Ohne Lizenz läuft die
  Runtime nicht dauerhaft.
- Der ADS-Port für die PLC-Runtime ist standardmäßig **851**.

