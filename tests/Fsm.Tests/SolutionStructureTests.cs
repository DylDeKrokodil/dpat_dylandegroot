namespace Fsm.Tests;

public class SolutionStructureTests
{
    [Fact]
    public void DomainProjectIsAvailableToTests()
    {
        Assert.Equal("Fsm.Domain", typeof(Fsm.Domain.Guard).Namespace);
    }
}
