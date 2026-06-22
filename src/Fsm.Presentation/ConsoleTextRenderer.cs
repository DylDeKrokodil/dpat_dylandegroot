using Fsm.Domain;
using Fsm.Domain.States;
using Fsm.Domain.Visitors;

namespace Fsm.Presentation;

public sealed class ConsoleTextRenderer : IFsmRenderer, IFsmElementVisitor
{
    private readonly TextOutputBuilder _builder = new();
    private RenderContext _context = new();

    public string RenderDiagram(FsmDiagram diagram)
    {
        _builder.Clear();
        _context = new RenderContext();

        diagram.Accept(this);

        return _builder.ToString();
    }

    public string RenderState(State state)
    {
        _builder.Clear();
        _context = new RenderContext();

        RenderStateTree(state);

        return _builder.ToString();
    }

    public string RenderState(FsmDiagram diagram, State state)
    {
        _builder.Clear();
        _context = new RenderContext();

        RenderStateTree(state);
        RenderRelevantTransitions(diagram, state);

        return _builder.ToString();
    }

    public string RenderTransition(Transition transition)
    {
        _builder.Clear();
        _context = new RenderContext();

        transition.Accept(this);

        return _builder.ToString();
    }

    public void VisitDiagram(FsmDiagram diagram)
    {
        _builder.AppendLine("FSM Diagram");
        _builder.AppendLine("States:");

        var previousContext = _context;
        _context = _context.NextLevel();

        foreach (var rootState in diagram.States.Where(state => state.Parent is null))
        {
            RenderStateTree(rootState);
        }

        _context = previousContext;
        _builder.AppendLine();
        _builder.AppendLine("Transitions:");
        _context = _context.NextLevel();

        foreach (var transition in diagram.Transitions)
        {
            transition.Accept(this);
        }

        _context = previousContext;
    }

    public void VisitInitialState(InitialState state)
    {
        RenderStateHeader(state);
    }

    public void VisitSimpleState(SimpleState state)
    {
        RenderStateHeader(state);
    }

    public void VisitCompoundState(CompoundState state)
    {
        RenderStateHeader(state);
    }

    public void VisitFinalState(FinalState state)
    {
        RenderStateHeader(state);
    }

    public void VisitTransition(Transition transition)
    {
        var trigger = transition.Trigger is null ? "automatic" : transition.Trigger.Id;
        var guard = transition.Guard.IsEmpty ? "" : $" [{transition.Guard.Expression}]";
        var effect = transition.Effect is null ? "" : $" / {transition.Effect.Description}";

        _builder.AppendLine($"{_context.Indent}- {transition.Id}: {transition.Source.Id} -> {transition.Destination.Id} on {trigger}{guard}{effect}");
    }

    private void RenderStateTree(State state)
    {
        state.Accept(this);
        RenderStateActions(state);

        if (state is not CompoundState compoundState)
        {
            return;
        }

        var previousContext = _context;
        _context = _context.NextLevel();

        foreach (var child in compoundState.Children)
        {
            RenderStateTree(child);
        }

        _context = previousContext;
    }

    private void RenderStateHeader(State state)
    {
        var name = string.IsNullOrWhiteSpace(state.DisplayName) ? state.Id : state.DisplayName;

        _builder.AppendLine($"{_context.Indent}- {state.Id} ({state.Type}): {name}");
    }

    private void RenderStateActions(State state)
    {
        if (state.Actions.Count == 0)
        {
            return;
        }

        var previousContext = _context;
        _context = _context.NextLevel();

        foreach (var action in state.Actions)
        {
            _builder.AppendLine($"{_context.Indent}action {FormatActionType(action.Type)}: {action.Description}");
        }

        _context = previousContext;
    }

    private void RenderRelevantTransitions(FsmDiagram diagram, State state)
    {
        var relatedTransitions = diagram.Transitions
            .Where(transition => IsRelatedToState(transition, state))
            .ToList();

        if (relatedTransitions.Count == 0)
        {
            return;
        }

        _builder.AppendLine();
        _builder.AppendLine("Related transitions:");

        var previousContext = _context;
        _context = _context.NextLevel();

        foreach (var transition in relatedTransitions)
        {
            transition.Accept(this);
        }

        _context = previousContext;
    }

    private static bool IsRelatedToState(Transition transition, State state)
    {
        return ReferenceEquals(transition.Source, state)
            || ReferenceEquals(transition.Destination, state)
            || transition.Source.IsNestedIn(state)
            || transition.Destination.IsNestedIn(state);
    }

    private static string FormatActionType(ActionType type)
    {
        return type switch
        {
            ActionType.EntryAction => "entry",
            ActionType.DoAction => "do",
            ActionType.ExitAction => "exit",
            ActionType.TransitionAction => "transition",
            _ => type.ToString()
        };
    }
}
