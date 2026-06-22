namespace Fsm.Domain;

public sealed class Trigger
{
    public Trigger(string id, string description)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("Trigger id is required.");
        }

        Id = id;
        Description = description;
    }

    public string Id { get; }

    public string Description { get; }
}
