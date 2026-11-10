using System.Net;
using BASF.Print.Monitor.Application;
using BASF.Print.Monitor.Domain;
using BASF.Print.Monitor.Infrastructure;
using BASF.Print.Monitor.Web.Components;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment() ||
    !builder.Configuration.GetValue<bool>("FeatureFlags:UseMockZabbixClient"))
{
    throw new InvalidOperationException("Diese Version startet ausschließlich im lokalen Demo-Modus.");
}

var refreshSeconds = builder.Configuration.GetValue("Dashboard:RefreshIntervalSeconds", 60);
var refreshInterval = TimeSpan.FromSeconds(refreshSeconds is >= 10 and <= 3600 ? refreshSeconds : 60);
var threshold = builder.Configuration.GetValue("Dashboard:TonerWarningThresholdPercent", 20);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPrinterSnapshotSource, MockPrinterSource>();
builder.Services.AddSingleton(new DashboardSettings(refreshInterval, threshold is >= 1 and <= 99 ? threshold : 20));
builder.Services.AddSingleton(services => new PrinterService(
    services.GetRequiredService<IPrinterSnapshotSource>(),
    services.GetRequiredService<TimeProvider>(),
    refreshInterval));
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();
if (threshold is < 1 or > 99)
{
    app.Logger.LogWarning("Ungültiger Tonergrenzwert {Threshold}. Standardwert 20 wird verwendet.", threshold);
}

app.Use(async (context, next) =>
{
    var remoteAddress = context.Connection.RemoteIpAddress;
    if (remoteAddress is null || !IPAddress.IsLoopback(remoteAddress))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    await next(context);
});
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

public sealed record DashboardSettings(TimeSpan RefreshInterval, int TonerWarningThreshold);
