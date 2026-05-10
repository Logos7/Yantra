using System.Collections.ObjectModel;
using Yantra.Studio.Infrastructure;
using Yantra.Studio.Models;

namespace Yantra.Studio.ViewModels;

public sealed class StudioPanelViewModel : ObservableObject
{
    private string _subtitle = "";

    public StudioPanelViewModel(string id, string title, PanelKind kind, PanelDock defaultDock)
    {
        Id = id;
        Title = title;
        Kind = kind;
        DefaultDock = defaultDock;
    }

    public string Id { get; }
    public string Title { get; }
    public PanelKind Kind { get; }
    public PanelDock DefaultDock { get; }
    public ObservableCollection<string> Lines { get; } = [];

    public string Subtitle
    {
        get => _subtitle;
        set => Set(ref _subtitle, value);
    }

    public void ReplaceLines(IEnumerable<string> lines)
    {
        Lines.Clear();
        foreach (var line in lines)
        {
            Lines.Add(line);
        }
    }
}
