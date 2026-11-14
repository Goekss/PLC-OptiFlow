# PLC-OptiFlow

Kleines, sauberes WPF-HMI für ein simuliertes Kessel-System – als Portfolio-/CV-Referenz gebaut. Der Fokus liegt auf wenigen, aber vollständig funktionierenden Funktionen statt auf vielen halbfertigen Features.

## Zweck

Simuliert eine PLC-gesteuerte Kesselanlage und zeigt sie in einem modernen Dashboard:

- **Temperatur** (°C) mit analogem Rundinstrument und Live-Trend
- **Heizung** und **Pumpe** im Handbetrieb schaltbar
- **Sollwert** (Zieltemperatur) einstellbar
- **Notaus** mit Verriegelung und Reset
- **Alarme** werden aus der Temperatur abgeleitet (≥ 80 °C Warnung, ≥ 90 °C kritisch)
- **Dark-/Light-Modus** und responsives Layout

## Architektur

```mermaid
graph LR
    subgraph "WPF Client (OptiCHmiApp)"
        Views[Views (XAML)]
        VMs[ViewModels]
        IPlc[IPlcService]
        F[FallbackPlcService]
        G[GrpcPlcService]
        S[SimulierterPlcService]
    end
    subgraph "Backend (OptiCHmiApp.Backend)"
        P[PlcService (gRPC)]
        K[KesselPhysik]
    end
    Views --> VMs
    VMs --> IPlc
    IPlc --> F
    F --> G
    F --> S
    G -->|gRPC| P
    P --> K
```

Grundsätze:

- **MVVM**: Views enthalten keine Geschäftslogik, Code-behind nur für reine UI-Aufgaben.
- **ViewModels kennen nur `IPlcService`** – nie die konkrete Implementierung.
- **`FallbackPlcService`** wechselt automatisch auf den Simulator, wenn das Backend nicht erreichbar ist (Anzeige „SIMULATION“).
- **Backend** ist unabhängig vom WPF-Client; beide nutzen dieselbe `plc.proto`.

## Bildschirme

| Seite | Inhalt |
|---|---|
| **Übersicht** | Temperatur, Rundinstrument, Live-Trend, Statuslampen (Heizung/Pumpe/Notaus), Start/Stopp |
| **Handbetrieb** | Pumpe, Heizung, Sollwert, Notaus |
| **Alarme** | Aktive Alarme (aus Temperatur abgeleitet), Quittieren |
| **Einstellungen** | Backend-Adresse, Abfrageintervall, Thema |

## Screenshots

![Übersicht](docs/screenshots/uebersicht.png)

## Starten

### Voraussetzungen
- .NET 11 SDK
- (Optional) Visual Studio 2026

### Backend + Client zusammen starten (empfohlen)

In Visual Studio: Lösung öffnen und beide Projekte als Startprojekte festlegen
(`OptiCHmiApp.Backend` zuerst, dann `OptiCHmiApp`).

Oder über die Kommandozeile:

```bash
# Terminal 1 – Backend
dotnet run --project OptiCHmiApp.Backend

# Terminal 2 – Client
dotnet run --project OptiCHmiApp
```

Der Client verbindet sich standardmäßig mit `http://localhost:5262`.
Ohne laufendes Backend wechselt er automatisch in den Simulationsmodus.

## Tests

```bash
dotnet test OptiCHmiApp.Tests
```
