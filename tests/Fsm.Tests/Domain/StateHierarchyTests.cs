using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Tests.Domain;

public class StateHierarchyTests
{
    [Fact]
    public void CompoundStateCanContainNestedStates()
    {
        var parent = new CompoundState("parent", "Parent");
        var child = new SimpleState("child", "Child", parent);

        parent.AddChild(child);

        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void StateCanReportWhetherItIsNestedInAnotherState()
    {
        var root = new CompoundState("root", "Root");
        var middle = new CompoundState("middle", "Middle", root);
        var leaf = new SimpleState("leaf", "Leaf", middle);

        root.AddChild(middle);
        middle.AddChild(leaf);

        Assert.True(leaf.IsNestedIn(root));
        Assert.True(leaf.IsNestedIn(middle));
        Assert.False(middle.IsNestedIn(leaf));
    }

    [Fact]
    public void StateCannotBeNestedInsideSimpleState()
    {
        var simple = new SimpleState("simple", "Simple");

        var exception = Assert.Throws<DomainException>(() => new SimpleState("child", "Child", simple));

        Assert.Contains("compound state", exception.Message);
    }
}
