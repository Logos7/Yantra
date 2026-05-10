using System.Collections.ObjectModel;
using Yantra.Studio.Infrastructure;

namespace Yantra.Studio.ViewModels;

public sealed class CanvasSceneViewModel : ObservableObject
{
    private CanvasNodeViewModel? _selectedNode;
    private double _panX = 40;
    private double _panY = 40;
    private double _zoom = 1.0;

    public CanvasSceneViewModel(string systemId, string title, string boardId, string backendId)
    {
        SystemId = systemId;
        Title = title;
        BoardId = boardId;
        BackendId = backendId;
    }

    public string SystemId { get; }
    public string Title { get; }
    public string BoardId { get; }
    public string BackendId { get; }
    public ObservableCollection<CanvasNodeViewModel> Nodes { get; } = [];
    public ObservableCollection<CanvasConnectionViewModel> Connections { get; } = [];

    public CanvasNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (_selectedNode == value)
            {
                return;
            }

            if (_selectedNode is not null)
            {
                _selectedNode.IsSelected = false;
            }

            _selectedNode = value;

            if (_selectedNode is not null)
            {
                _selectedNode.IsSelected = true;
            }

            OnPropertyChanged();
        }
    }

    public double PanX
    {
        get => _panX;
        set => Set(ref _panX, value);
    }

    public double PanY
    {
        get => _panY;
        set => Set(ref _panY, value);
    }

    public double Zoom
    {
        get => _zoom;
        set => Set(ref _zoom, Math.Clamp(value, 0.25, 4.0));
    }

    public CanvasNodeViewModel AddNode(string title, string blockId, string blockName, string kind, IEnumerable<CanvasPortViewModel>? ports = null, IReadOnlyDictionary<string, string>? parameters = null)
    {
        var index = Nodes.Count + 1;
        var node = new CanvasNodeViewModel($"n{index}", title, blockId, blockName, kind, ports, parameters, 80 + index * 34, 80 + index * 24);
        Nodes.Add(node);
        SelectedNode = node;
        return node;
    }

    public void ZoomToFit()
    {
        PanX = 60;
        PanY = 60;
        Zoom = 1.0;
    }
}
