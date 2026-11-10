using BASF.Print.Monitor.Domain;

namespace BASF.Print.Monitor.Web.Components;

internal static class Display
{
    public static string Status(PrinterStatus status) => status switch
    {
        PrinterStatus.Warning => "Warnung",
        PrinterStatus.Unknown => "Unbekannt",
        _ => status.ToString()
    };

    public static string Timestamp(DateTimeOffset? value, bool seconds = false) =>
        value?.ToLocalTime().ToString(seconds ? "dd.MM.yyyy HH:mm:ss" : "dd.MM.yyyy HH:mm") ?? "Nicht verfügbar";

    public static string Paper(PaperTrayStatus status) => status switch
    {
        PaperTrayStatus.Available => "Verfügbar",
        PaperTrayStatus.Empty => "Leer",
        PaperTrayStatus.NotAvailable => "Nicht verfügbar",
        PaperTrayStatus.Unknown => "Unbekannt",
        _ => "Nicht bereitgestellt"
    };

    public static string Severity(ProblemSeverity severity) => severity switch
    {
        ProblemSeverity.Information => "Information",
        ProblemSeverity.Warning => "Warnung",
        ProblemSeverity.Average => "Durchschnitt",
        ProblemSeverity.High => "Hoch",
        ProblemSeverity.Critical => "Kritisch",
        _ => "Unbekannt"
    };
}
