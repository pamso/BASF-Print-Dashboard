using System.Collections.Immutable;

namespace BASF.Print.Monitor.Domain;

/// <summary>Betriebsstatus eines Druckers.</summary>
public enum PrinterStatus { Online, Offline, Warning, Unknown }

/// <summary>Zustand eines einzelnen Papierfachs.</summary>
public enum PaperTrayStatus { Available, Empty, NotAvailable, Unknown, NotProvided }

/// <summary>Schweregrad eines gemeldeten Druckerproblems, aufsteigend sortiert.</summary>
public enum ProblemSeverity { Information = 1, Warning, Average, High, Critical }

/// <summary>Ein einzelnes Papierfach eines Druckers mit Namen und aktuellem Zustand.</summary>
public sealed record PaperTray(string Name, PaperTrayStatus Status);

/// <summary>Ein gemeldetes Problem an einem Drucker, inkl. Schweregrad und Zeitpunkt des Auftretens.</summary>
public sealed record Problem(string Name, ProblemSeverity Severity, DateTimeOffset OccurredAt);

/// <summary>
/// Repräsentiert einen überwachten Drucker mit Stammdaten, aktuellem Status sowie Papierfächern
/// und gemeldeten Problemen.
/// </summary>
/// <param name="HostId">Eindeutige technische ID des Druckers in der Überwachungsquelle.</param>
/// <param name="LastSeen">Zeitpunkt des letzten erfolgreichen Kontakts, sofern bekannt.</param>
/// <param name="BlackTonerPercent">Verbleibender Schwarz-Tonerstand in Prozent (0-100), sofern verfügbar.</param>
/// <param name="TotalPages">Gesamtzahl der bisher gedruckten Seiten, sofern verfügbar.</param>
public sealed record Printer(
    string HostId,
    string Name,
    string IpAddress,
    string Model,
    string? SerialNumber,
    string? AssetId,
    string? Location,
    PrinterStatus Status,
    DateTimeOffset? LastSeen,
    int? BlackTonerPercent,
    long? TotalPages,
    ImmutableArray<PaperTray> PaperTrays,
    ImmutableArray<Problem> Problems);

/// <summary>Ein zu einem Zeitpunkt eingeholter Gesamtzustand aller überwachten Drucker.</summary>
public sealed record MonitoringSnapshot(DateTimeOffset RetrievedAt, ImmutableArray<Printer> Printers);

/// <summary>
/// Abstraktion für eine Datenquelle, die den aktuellen Zustand des Druckerbestands liefert
/// (z. B. Mock-Daten oder eine echte Monitoring-API).
/// </summary>
public interface IPrinterSnapshotSource
{
    /// <summary>Liest den aktuellen Zustand aller Drucker ein.</summary>
    MonitoringSnapshot Read();
}
