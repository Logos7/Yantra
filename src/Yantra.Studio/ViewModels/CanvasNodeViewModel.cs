using Yantra.Studio.Infrastructure;

namespace Yantra.Studio.ViewModels;

public sealed class CanvasNodeViewModel : ObservableObject
{
    private double _x;
    private double _y;
    private bool _isSelected;

    public CanvasNodeViewModel(
        string id,
        string title,
        string blockId,
        string blockName,
        string blockKind,
        IEnumerable<CanvasPortViewModel>? ports,
        IReadOnlyDictionary<string, string>? parameters,
        double x,
        double y)
    {
        Id = id;
        Title = title;
        BlockId = blockId;
        BlockName = blockName;
        BlockKind = blockKind;
        Ports = ports?.ToArray() ?? [];
        Parameters = parameters ?? new Dictionary<string, string>();
        Inputs = Ports.Where(x => x.Direction.Equals("Input", StringComparison.OrdinalIgnoreCase) || x.Direction.Equals("InOut", StringComparison.OrdinalIgnoreCase)).ToArray();
        Outputs = Ports.Where(x => x.Direction.Equals("Output", StringComparison.OrdinalIgnoreCase) || x.Direction.Equals("InOut", StringComparison.OrdinalIgnoreCase)).ToArray();
        _x = x;
        _y = y;
    }

    public string Id { get; }
    public string Title { get; }
    public string BlockId { get; }
    public string BlockName { get; }
    public string BlockKind { get; }
    public IReadOnlyList<CanvasPortViewModel> Ports { get; }
    public IReadOnlyList<CanvasPortViewModel> Inputs { get; }
    public IReadOnlyList<CanvasPortViewModel> Outputs { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }
    public double Width { get; init; } = 170;
    public double Height { get; init; } = 84;

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
