namespace Fsm.Simulation;

public sealed class SimulationException : Exception
{
    public SimulationException(string message)
        : base(message)
    {
    }
}
