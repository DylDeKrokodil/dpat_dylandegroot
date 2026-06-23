using Fsm.Application;

namespace Fsm.Tests.Application;

public class CommandOptionsTests
{
    [Theory]
    [InlineData("--simulate")]
    [InlineData("-s")]
    public void ParseRecognizesSimulationFlags(string simulationFlag)
    {
        var options = CommandOptions.Parse(["machine.fsm", simulationFlag]);

        Assert.Equal("machine.fsm", options.InputFilePath);
        Assert.True(options.Simulate);
    }

    [Fact]
    public void ParseKeepsFirstNonFlagArgumentAsInputPath()
    {
        var options = CommandOptions.Parse(["first.fsm", "second.fsm"]);

        Assert.Equal("first.fsm", options.InputFilePath);
        Assert.False(options.Simulate);
    }
}
