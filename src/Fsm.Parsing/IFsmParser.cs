using Fsm.Domain;

namespace Fsm.Parsing;

public interface IFsmParser
{
    FsmDiagram ParseFile(string filePath);

    FsmDiagram ParseText(string text);
}
