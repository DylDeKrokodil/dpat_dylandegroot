using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class StateFactory
{
    public State Create(string id, string name, StateType type, State? parent)
    {
        return type switch
        {
            StateType.Initial => new InitialState(id, name, parent),
            StateType.Simple => new SimpleState(id, name, parent),
            StateType.Compound => new CompoundState(id, name, parent),
            StateType.Final => new FinalState(id, name, parent),
            _ => throw new ModelBuildingException($"Unsupported state type '{type}'.")
        };
    }
}
