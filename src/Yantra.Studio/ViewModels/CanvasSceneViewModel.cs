using System.Collections.ObjectModel;
using Yantra.Studio.Infrastructure;

namespace Yantra.Studio.ViewModels;

public sealed class CanvasSceneViewModel : ObservableObject
{
    private CanvasNodeViewModel? _selectedNode;
    private double _panX = 40;
    private double _panY = 40;
    private double _zoom = 1.0;

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

    public CanvasNodeViewModel AddNode(string title, string kind)
    {
        var index = Nodes.Count + 1;
        var node = new CanvasNodeViewModel($"n{index}", title, kind, 80 + index * 30, 80 + index * 20);
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
