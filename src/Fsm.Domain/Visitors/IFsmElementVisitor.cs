using Fsm.Domain.States;

namespace Fsm.Domain.Visitors;

public interface IFsmElementVisitor
{
    void VisitDiagram(FsmDiagram diagram);

    void VisitInitialState(InitialState state);

    void VisitSimpleState(SimpleState state);

    void VisitCompoundState(CompoundState state);

    void VisitFinalState(FinalState state);

    void VisitTransition(Transition transition);
}
