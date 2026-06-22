namespace Fsm.Presentation;

public sealed record RenderContext(int IndentationLevel = 0)
{
    public string Indent => new(' ', IndentationLevel * 2);

    public RenderContext NextLevel()
    {
        return this with { IndentationLevel = IndentationLevel + 1 };
    }
}
