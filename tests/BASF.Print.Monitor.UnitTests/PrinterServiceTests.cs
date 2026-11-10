using BASF.Print.Monitor.Application;
using BASF.Print.Monitor.Domain;
using BASF.Print.Monitor.Infrastructure;
using Xunit;

namespace BASF.Print.Monitor.UnitTests;

public sealed class PrinterServiceTests
{
    [Fact]
    public void SummaryPartitionsTheFleet()
    {
        var snapshot = new MockPrinterSource(TimeProvider.System).Read();
        Assert.Equal(new FleetSummary(5, 1, 1, 2, 1, 1), PrinterService.Summarize(snapshot, 20));
    }

    [Fact]
    public void FiltersAreCombinedAndSearchIgnoresCase()
    {
        var snapshot = new MockPrinterSource(TimeProvider.System).Read();
        var matches = PrinterService.Filter(snapshot, "demo-sn", PrinterStatus.Warning, "Büro Süd");
        Assert.Equal("demo-5", Assert.Single(matches).HostId);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(19, true)]
    [InlineData(20, false)]
    public void LowTonerRequiresAValidValueBelowTheThreshold(int? percent, bool expected)
    {
        var printer = new MockPrinterSource(TimeProvider.System).Read().Printers[0];
        Assert.Equal(expected, PrinterService.IsLowToner(printer with { BlackTonerPercent = percent }, 20));
    }

    [Fact]
    public void CacheSharesSnapshotsUntilExpiryOrManualRefresh()
    {
        var clock = new TestClock();
        var service = new PrinterService(new MockPrinterSource(clock), clock, TimeSpan.FromSeconds(60));
        var original = service.GetSnapshot();
        clock.Now = clock.Now.AddSeconds(59);
        Assert.Same(original, service.GetSnapshot());
        clock.Now = clock.Now.AddSeconds(1);
        var refreshed = service.GetSnapshot();
        Assert.NotSame(original, refreshed);
        Assert.NotSame(refreshed, service.GetSnapshot(refresh: true));
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UnixEpoch;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
