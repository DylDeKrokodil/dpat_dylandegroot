namespace Fsm.Domain;

public abstract class FsmAction
{
    protected FsmAction(string description, ActionType type)
    {
        Description = description;
        Type = type;
    }

    public string Description { get; }

    public ActionType Type { get; }
}
