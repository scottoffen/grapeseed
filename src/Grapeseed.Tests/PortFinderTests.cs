namespace Grapeseed.Tests;

public class PortFinderTests
{
    [Fact]
    public void GetPortsInUse_ReturnsNonEmptyList()
    {
        PortFinder.GetPortsInUse().ShouldNotBeEmpty();
    }

    [Fact]
    public void WhenStartIndexIsNotInRange_ThrowsException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
        {
            _ = PortFinder.FindNextLocalOpenPort(PortFinder.MinPortNumber - 1, PortFinder.MaxPortNumber);
        });
    }

    [Fact]
    public void WhenEndIndexIsNotInRange_ThrowsException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
        {
            _ = PortFinder.FindNextLocalOpenPort(PortFinder.MinPortNumber, PortFinder.MaxPortNumber + 1);
        });
    }

    [Fact]
    public void WhenNoMatchingPortIsFound_ThrowsException()
    {
        var firstPortInUse = PortFinder.GetPortsInUse().First();

        Should.Throw<IndexOutOfRangeException>(() =>
        {
            _ = PortFinder.FindNextLocalOpenPort(firstPortInUse, firstPortInUse);
        });
    }

    [Fact]
    public void FindNextLocalOpenPort_ReturnsValueFromMin()
    {
        var firstPortInUse = PortFinder.GetPortsInUse().First(x => x > PortFinder.MinPortNumber);

        var port = PortFinder.FindNextLocalOpenPort(PortFinder.MinPortNumber, firstPortInUse);

        int.Parse(port).ShouldBeLessThan(firstPortInUse);
    }

    [Fact]
    public void FindNextLocalOpenPort_ReturnsValueFromMax()
    {
        var firstPortInUse = PortFinder.GetPortsInUse().Reverse().First(x => x < PortFinder.MaxPortNumber);

        var port = PortFinder.FindNextLocalOpenPort(PortFinder.MaxPortNumber, firstPortInUse);

        int.Parse(port).ShouldBeGreaterThan(firstPortInUse);
    }

    [Fact]
    public void FindNextLocalOpenPort_UsingDefaults()
    {
        var firstOpenPort = PortFinder.FindNextLocalOpenPort(PortFinder.MinPortNumber, PortFinder.MaxPortNumber);
        var lastOpenPort = PortFinder.FindNextLocalOpenPort(PortFinder.MaxPortNumber, PortFinder.MinPortNumber);

        int.Parse(firstOpenPort).ShouldBeLessThan(int.Parse(lastOpenPort));
    }

    [Fact]
    public void FindNextLocalOpenPort_AscDescReturnsDifferentValues()
    {
        var portsInUse = PortFinder.GetPortsInUse();
        portsInUse.ShouldNotBeEmpty();

        var startIndex = portsInUse.First();
        var endIndex = PortFinder.MaxPortNumber;

        foreach (var p in portsInUse.Skip(1))
        {
            if (p > (startIndex + 2))
            {
                endIndex = p;
                break;
            }
            else
            {
                startIndex = p;
            }
        }

        var firstOpenPort = int.Parse(PortFinder.FindNextLocalOpenPort(startIndex, endIndex));
        var lastOpenPort = int.Parse(PortFinder.FindNextLocalOpenPort(endIndex, startIndex));

        firstOpenPort.ShouldBe(startIndex + 1);
        lastOpenPort.ShouldBe(endIndex - 1);
        firstOpenPort.ShouldBeLessThan(lastOpenPort);
    }
}
