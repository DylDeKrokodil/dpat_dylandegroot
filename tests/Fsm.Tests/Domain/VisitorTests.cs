using Fsm.Domain;
using Fsm.Domain.States;
using Fsm.Domain.Visitors;

namespace Fsm.Tests.Domain;

public class VisitorTests
{
    [Fact]
    public void DomainElementsAcceptVisitor()
    {
        var visitor = new RecordingVisitor();
        var diagram = new FsmDiagram();
        var initial = new InitialState("initial", "Initial");
        var simple = new SimpleState("simple", "Simple");
        var compound = new CompoundState("compound", "Compound");
        var final = new FinalState("final", "Final");
        var transition = new Transition("t1", initial, simple, trigger: null, Guard.Empty);

        diagram.Accept(visitor);
        initial.Accept(visitor);
        simple.Accept(visitor);
        compound.Accept(visitor);
        final.Accept(visitor);
        transition.Accept(visitor);

        Assert.Equal(
            ["diagram", "initial", "simple", "compound", "final", "transition"],
            visitor.VisitedElements);
    }

    private sealed class RecordingVisitor : IFsmElementVisitor
    {
        public List<string> VisitedElements { get; } = [];

        public void VisitDiagram(FsmDiagram diagram)
        {
            VisitedElements.Add("diagram");
        }

        public void VisitInitialState(InitialState state)
        {
            VisitedElements.Add("initial");
        }

        public void VisitSimpleState(SimpleState state)
        {
            VisitedElements.Add("simple");
        }

        public void VisitCompoundState(CompoundState state)
        {
            VisitedElements.Add("compound");
        }

        public void VisitFinalState(FinalState state)
        {
            VisitedElements.Add("final");
        }

        public void VisitTransition(Transition transition)
        {
            VisitedElements.Add("transition");
        }
    }
}
