using Fsm.Domain;

namespace Fsm.Application;

public interface ISimulationRunner
{
    void Run(FsmDiagram diagram);
}
