using Fsm.Domain;

namespace Fsm.Tests.Domain;

public class GuardTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void IsEmptyReturnsTrueForBlankExpressions(string expression)
    {
        var guard = new Guard(expression);

        Assert.True(guard.IsEmpty);
    }

    [Fact]
    public void IsEmptyReturnsFalseForMeaningfulExpression()
    {
        var guard = new Guard("attempts >= 3");

        Assert.False(guard.IsEmpty);
    }
}
