using BASF.Print.Monitor.Domain;

namespace BASF.Print.Monitor.Infrastructure;

/// <summary>
/// Test-/Demo-Implementierung von <see cref="IPrinterSnapshotSource"/>, die feste Beispieldrucker
/// liefert, anstatt eine echte Monitoring-Schnittstelle abzufragen. Nützlich für lokale Entwicklung
/// und automatisierte Tests, ohne Abhängigkeit zu externer Infrastruktur.
/// </summary>
public sealed class MockPrinterSource(TimeProvider clock) : IPrinterSnapshotSource
{
    /// <inheritdoc />
    public MonitoringSnapshot Read()
    {
        var now = clock.GetUtcNow();
        return new(now,
        [
            // Online-Drucker ohne besondere Vorkommnisse.
            Sample(1, "Büro Nord", PrinterStatus.Online, 76, 1204, now, PaperTrayStatus.Available),
            // Online, aber mit Warnung wegen niedrigem Tonerstand.
            Sample(2, "Büro Nord", PrinterStatus.Warning, 12, 8740, now, PaperTrayStatus.Available,
                new("Toner niedrig", ProblemSeverity.Warning, now.AddMinutes(-8))),
            // Offline seit 20 Minuten, mit kritischem Erreichbarkeitsproblem.
            Sample(3, "Werkstatt", PrinterStatus.Offline, null, 532, now.AddMinutes(-20), PaperTrayStatus.Unknown,
                new("Drucker nicht erreichbar", ProblemSeverity.High, now.AddMinutes(-19))),
            // Status unbekannt, keine Daten von der Quelle verfügbar.
            Sample(4, "Werkstatt", PrinterStatus.Unknown, null, null, null, PaperTrayStatus.NotProvided),
            // Warnung wegen leerem Papierfach.
            Sample(5, "Büro Süd", PrinterStatus.Warning, 48, 0, now, PaperTrayStatus.Empty,
                new("Papierfach leer", ProblemSeverity.Warning, now.AddMinutes(-2)))
        ]);
    }

    /// <summary>
    /// Erzeugt einen Beispieldrucker mit durchnummerierten Stammdaten (Seriennummer, Asset-ID, IP),
    /// optional mit einem gemeldeten Problem.
    /// </summary>
    private static Printer Sample(int number, string location, PrinterStatus status, int? toner,
        long? pages, DateTimeOffset? lastSeen, PaperTrayStatus paper, Problem? problem = null) => new(
        $"demo-{number}", $"Demo-Drucker {number:00}", $"192.0.2.{number}", "HP LaserJet Pro MFP 3102fdw",
        $"DEMO-SN-{number:00}", $"DEMO-{number:00}", location, status, lastSeen, toner, pages,
        [new("Fach 1", paper)], problem is null ? [] : [problem]);
}
