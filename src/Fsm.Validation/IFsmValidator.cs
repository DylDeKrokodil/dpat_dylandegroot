using Fsm.Domain;

namespace Fsm.Validation;

public interface IFsmValidator
{
    IEnumerable<ValidationError> Validate(FsmDiagram diagram);
}
