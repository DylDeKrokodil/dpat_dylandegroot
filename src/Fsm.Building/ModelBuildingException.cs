namespace Fsm.Building;

public sealed class ModelBuildingException : Exception
{
    public ModelBuildingException(string message)
        : base(message)
    {
    }
}
