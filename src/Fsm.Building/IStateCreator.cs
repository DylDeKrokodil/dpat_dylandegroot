using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public interface IStateCreator
{
    StateType StateType { get; }

    State Create(string id, string name, State? parent);
}
