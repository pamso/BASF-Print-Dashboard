using BASF.Print.Monitor.Domain;

namespace BASF.Print.Monitor.Application;

/// <summary>
/// Aggregierte Kennzahlen über den gesamten Druckerbestand (Fleet), z. B. für Dashboard-Kacheln.
/// </summary>
public sealed record FleetSummary(int Total, int Online, int Offline, int Warning, int Unknown, int LowToner);

/// <summary>
/// Stellt den aktuellen Zustand des Drucker-Bestands bereit und sorgt für ein zeitbasiertes Caching
/// der Snapshots, damit <paramref name="source"/> nicht bei jedem Aufruf neu abgefragt werden muss.
/// </summary>
/// <param name="source">Datenquelle, aus der die aktuellen Druckerdaten gelesen werden.</param>
/// <param name="clock">Zeitquelle (abstrahiert für Testbarkeit statt DateTimeOffset.UtcNow).</param>
/// <param name="refreshInterval">Intervall, nach dem ein zwischengespeicherter Snapshot als veraltet gilt.</param>
public sealed class PrinterService(IPrinterSnapshotSource source, TimeProvider clock, TimeSpan refreshInterval)
{
    // Schützt den Zugriff auf 'snapshot', da GetSnapshot auch von mehreren Threads/Requests
    // gleichzeitig (z. B. parallele Blazor-Komponenten) aufgerufen werden kann.
    private readonly object gate = new();
    private MonitoringSnapshot? snapshot;

    /// <summary>
    /// Liefert den zwischengespeicherten Snapshot zurück oder liest einen neuen ein, falls noch
    /// keiner vorhanden, ein Refresh erzwungen wurde oder das Aktualisierungsintervall abgelaufen ist.
    /// </summary>
    /// <param name="refresh">Erzwingt das Einlesen eines neuen Snapshots, unabhängig vom Intervall.</param>
    public MonitoringSnapshot GetSnapshot(bool refresh = false)
    {
        lock (gate)
        {
            if (snapshot is null || refresh || clock.GetUtcNow() - snapshot.RetrievedAt >= refreshInterval)
            {
                snapshot = source.Read();
            }

            return snapshot;
        }
    }

    /// <summary>
    /// Erstellt eine Zusammenfassung des Druckerbestands anhand des übergebenen Snapshots.
    /// </summary>
    /// <param name="threshold">Schwellenwert (in %) unterhalb dessen ein Toner als "niedrig" gilt.</param>
    public static FleetSummary Summarize(MonitoringSnapshot snapshot, int threshold) => new(
        snapshot.Printers.Length,
        snapshot.Printers.Count(printer => printer.Status == PrinterStatus.Online),
        snapshot.Printers.Count(printer => printer.Status == PrinterStatus.Offline),
        snapshot.Printers.Count(printer => printer.Status == PrinterStatus.Warning),
        snapshot.Printers.Count(printer => printer.Status == PrinterStatus.Unknown),
        snapshot.Printers.Count(printer => IsLowToner(printer, threshold)));

    /// <summary>
    /// Prüft, ob ein Drucker einen gültigen (0-100) und gleichzeitig niedrigen Tonerstand hat.
    /// </summary>
    public static bool IsLowToner(Printer printer, int threshold) =>
        printer.BlackTonerPercent is >= 0 and <= 100 && printer.BlackTonerPercent < threshold;

    /// <summary>
    /// Filtert und sortiert die Drucker eines Snapshots anhand von Suchbegriff, Status und Standort.
    /// </summary>
    /// <param name="search">Freitextsuche über Name, IP-Adresse, Seriennummer, Asset-ID und Standort.</param>
    /// <param name="status">Optionaler Filter nach Druckerstatus.</param>
    /// <param name="location">Optionaler Filter nach Standort.</param>
    public static IEnumerable<Printer> Filter(
        MonitoringSnapshot snapshot, string? search, PrinterStatus? status, string? location)
    {
        var term = search?.Trim();
        return snapshot.Printers
            .Where(printer => status is null || printer.Status == status)
            .Where(printer => string.IsNullOrEmpty(location) || printer.Location == location)
            // Durchsucht mehrere Felder gleichzeitig, sofern ein Suchbegriff angegeben wurde.
            .Where(printer => string.IsNullOrEmpty(term) || new[]
                { printer.Name, printer.IpAddress, printer.SerialNumber, printer.AssetId, printer.Location }
                .Any(field => field?.Contains(term, StringComparison.OrdinalIgnoreCase) == true))
            .OrderBy(printer => printer.Name, StringComparer.OrdinalIgnoreCase);
    }
}
