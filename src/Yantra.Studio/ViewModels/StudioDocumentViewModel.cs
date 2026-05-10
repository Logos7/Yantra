using Yantra.Studio.Infrastructure;

namespace Yantra.Studio.ViewModels;

public abstract class StudioDocumentViewModel : ObservableObject
{
    private bool _isDirty;

    protected StudioDocumentViewModel(string id, string title)
    {
        Id = id;
        Title = title;
    }

    public string Id { get; }
    public string Title { get; }

    public bool IsDirty
    {
        get => _isDirty;
        set => Set(ref _isDirty, value);
    }
}
