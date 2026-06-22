using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Presentation;

public interface IFsmRenderer
{
    string RenderDiagram(FsmDiagram diagram);

    string RenderState(State state);

    string RenderState(FsmDiagram diagram, State state);

    string RenderTransition(Transition transition);
}
