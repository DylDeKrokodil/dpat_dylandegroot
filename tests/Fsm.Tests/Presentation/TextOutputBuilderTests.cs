using Fsm.Presentation;

namespace Fsm.Tests.Presentation;

public class TextOutputBuilderTests
{
    [Fact]
    public void ToStringTrimsTrailingLineEndings()
    {
        var builder = new TextOutputBuilder();

        builder.AppendLine("line");

        Assert.Equal("line", builder.ToString());
    }
}
