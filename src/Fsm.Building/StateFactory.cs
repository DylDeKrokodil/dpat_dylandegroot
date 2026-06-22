using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class StateFactory
{
    private readonly IReadOnlyDictionary<StateType, IStateCreator> _creators;

    public StateFactory()
        : this([
            new InitialStateCreator(),
            new SimpleStateCreator(),
            new CompoundStateCreator(),
            new FinalStateCreator()
        ])
    {
    }

    public StateFactory(IEnumerable<IStateCreator> creators)
    {
        var creatorList = creators.ToList();

        if (creatorList.Count == 0)
        {
            throw new ModelBuildingException("At least one state creator is required.");
        }

        var duplicateStateType = creatorList
            .GroupBy(creator => creator.StateType)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateStateType is not null)
        {
            throw new ModelBuildingException($"State creator for type '{duplicateStateType.Key}' is registered more than once.");
        }

        _creators = creatorList.ToDictionary(creator => creator.StateType);
    }

    public State Create(string id, string name, StateType type, State? parent)
    {
        if (!_creators.TryGetValue(type, out var creator))
        {
            throw new ModelBuildingException($"Unsupported state type '{type}'.");
        }

        return creator.Create(id, name, parent);
    }
}
