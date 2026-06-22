using System.Text;

namespace Fsm.Presentation;

public sealed class TextOutputBuilder
{
    private readonly StringBuilder _builder = new();

    public void AppendLine(string text = "")
    {
        _builder.AppendLine(text);
    }

    public void Clear()
    {
        _builder.Clear();
    }

    public override string ToString()
    {
        return _builder.ToString().TrimEnd();
    }
}
