namespace Yantra.Studio.ViewModels;

public sealed class TextDocumentViewModel : StudioDocumentViewModel
{
    private string _text;

    public TextDocumentViewModel(string id, string title, string text)
        : base(id, title)
    {
        _text = text;
    }

    public string Text
    {
        get => _text;
        set
        {
            if (Set(ref _text, value))
            {
                IsDirty = true;
            }
        }
    }
}
