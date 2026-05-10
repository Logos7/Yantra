using Yantra.Studio.Infrastructure;

namespace Yantra.Studio.ViewModels;

public sealed class CanvasNodeViewModel : ObservableObject
{
    private double _x;
    private double _y;
    private bool _isSelected;

    public CanvasNodeViewModel(string id, string title, string blockKind, double x, double y)
    {
        Id = id;
        Title = title;
        BlockKind = blockKind;
        _x = x;
        _y = y;
    }

    public string Id { get; }
    public string Title { get; }
    public string BlockKind { get; }
    public double Width { get; init; } = 150;
    public double Height { get; init; } = 68;

    public double X
    {
        get => _x;
        set => Set(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => Set(ref _y, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }
}
