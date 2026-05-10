using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Yantra.Studio.ViewModels;

namespace Yantra.Studio.Controls;

public sealed class GraphCanvasControl : Control
{
    public static readonly StyledProperty<CanvasSceneViewModel?> SceneProperty =
        AvaloniaProperty.Register<GraphCanvasControl, CanvasSceneViewModel?>(nameof(Scene));

    private CanvasNodeViewModel? _dragNode;
    private Point _dragStartScreen;
    private Point _dragStartWorld;
    private Point _nodeStart;
    private bool _isPanning;
    private double _panStartX;
    private double _panStartY;

    public GraphCanvasControl()
    {
        Focusable = true;
        ClipToBounds = true;
    }

    public CanvasSceneViewModel? Scene
    {
        get => GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SceneProperty)
        {
            if (change.OldValue is CanvasSceneViewModel oldScene)
            {
                oldScene.Nodes.CollectionChanged -= OnNodesChanged;
                oldScene.Connections.CollectionChanged -= OnConnectionsChanged;
                oldScene.PropertyChanged -= OnSceneChanged;
                foreach (var node in oldScene.Nodes)
                {
                    node.PropertyChanged -= OnNodeChanged;
                }
            }

            if (change.NewValue is CanvasSceneViewModel newScene)
            {
                newScene.Nodes.CollectionChanged += OnNodesChanged;
                newScene.Connections.CollectionChanged += OnConnectionsChanged;
                newScene.PropertyChanged += OnSceneChanged;
                foreach (var node in newScene.Nodes)
                {
                    node.PropertyChanged += OnNodeChanged;
                }
            }

            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(10, 13, 20)), bounds);
        DrawGrid(context, bounds);

        if (Scene is null)
        {
            return;
        }

        foreach (var connection in Scene.Connections)
        {
            var from = Scene.Nodes.FirstOrDefault(x => x.Id == connection.FromNodeId);
            var to = Scene.Nodes.FirstOrDefault(x => x.Id == connection.ToNodeId);
            if (from is not null && to is not null)
            {
                DrawConnection(context, from, connection.FromPortId, to, connection.ToPortId, connection.Label);
            }
        }

        foreach (var node in Scene.Nodes)
        {
            DrawNode(context, node);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        if (Scene is null)
        {
            return;
        }

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;

        if (properties.IsMiddleButtonPressed || properties.IsRightButtonPressed)
        {
            _isPanning = true;
            _dragStartScreen = point;
            _panStartX = Scene.PanX;
            _panStartY = Scene.PanY;
            e.Pointer.Capture(this);
            return;
        }

        var world = ToWorld(point);
        var hit = HitTest(world);
        Scene.SelectedNode = hit;

        if (hit is not null)
        {
            _dragNode = hit;
            _dragStartScreen = point;
            _dragStartWorld = world;
            _nodeStart = new Point(hit.X, hit.Y);
            e.Pointer.Capture(this);
        }

        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Scene is null)
        {
            return;
        }

        var point = e.GetPosition(this);

        if (_isPanning)
        {
            var delta = point - _dragStartScreen;
            Scene.PanX = _panStartX + delta.X;
            Scene.PanY = _panStartY + delta.Y;
            InvalidateVisual();
            return;
        }

        if (_dragNode is not null)
        {
            var world = ToWorld(point);
            var delta = world - _dragStartWorld;
            _dragNode.X = _nodeStart.X + delta.X;
            _dragNode.Y = _nodeStart.Y + delta.Y;
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragNode = null;
        _isPanning = false;
        e.Pointer.Capture(null);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (Scene is null)
        {
            return;
        }

        var screen = e.GetPosition(this);
        var world = ToWorld(screen);
        var factor = e.Delta.Y > 0 ? 1.1 : 0.9;
        Scene.Zoom *= factor;
        Scene.PanX = screen.X - world.X * Scene.Zoom;
        Scene.PanY = screen.Y - world.Y * Scene.Zoom;
        InvalidateVisual();
    }

    private void DrawGrid(DrawingContext context, Rect bounds)
    {
        if (Scene is null)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(Color.FromRgb(28, 34, 48)), 1);
        var majorPen = new Pen(new SolidColorBrush(Color.FromRgb(40, 48, 66)), 1);
        var step = 32 * Scene.Zoom;

        if (step < 8)
        {
            step = 8;
        }

        var offsetX = Scene.PanX % step;
        var offsetY = Scene.PanY % step;

        for (var x = offsetX; x < bounds.Width; x += step)
        {
            var major = Math.Abs(((x - Scene.PanX) / step) % 4) < 0.01;
            context.DrawLine(major ? majorPen : pen, new Point(x, 0), new Point(x, bounds.Height));
        }

        for (var y = offsetY; y < bounds.Height; y += step)
        {
            var major = Math.Abs(((y - Scene.PanY) / step) % 4) < 0.01;
            context.DrawLine(major ? majorPen : pen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    private void DrawConnection(DrawingContext context, CanvasNodeViewModel from, string fromPortId, CanvasNodeViewModel to, string toPortId, string label)
    {
        var start = GetPortPoint(from, fromPortId, true);
        var end = GetPortPoint(to, toPortId, false);
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(105, 177, 255)), 2);
        var c1 = new Point(start.X + 72 * (Scene?.Zoom ?? 1), start.Y);
        var c2 = new Point(end.X - 72 * (Scene?.Zoom ?? 1), end.Y);
        var geometry = new StreamGeometry();

        using (var stream = geometry.Open())
        {
            stream.BeginFigure(start, false);
            stream.CubicBezierTo(c1, c2, end);
            stream.EndFigure(false);
        }

        context.DrawGeometry(null, pen, geometry);
        var middle = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2 - 18);
        DrawText(context, label, middle, 11, new SolidColorBrush(Color.FromRgb(154, 201, 255)));
    }

    private void DrawNode(DrawingContext context, CanvasNodeViewModel node)
    {
        var zoom = Scene?.Zoom ?? 1;
        var p = ToScreen(new Point(node.X, node.Y));
        var rect = new Rect(p.X, p.Y, node.Width * zoom, node.Height * zoom);
        var fill = node.IsSelected
            ? new SolidColorBrush(Color.FromRgb(39, 66, 104))
            : new SolidColorBrush(Color.FromRgb(24, 31, 46));
        var border = node.IsSelected
            ? new Pen(new SolidColorBrush(Color.FromRgb(122, 187, 255)), 2)
            : new Pen(new SolidColorBrush(Color.FromRgb(62, 73, 96)), 1);
        var header = node.BlockKind switch
        {
            "Clock" => new SolidColorBrush(Color.FromRgb(95, 73, 38)),
            "Arithmetic" => new SolidColorBrush(Color.FromRgb(40, 68, 93)),
            "Io" => new SolidColorBrush(Color.FromRgb(49, 83, 58)),
            "Processor" => new SolidColorBrush(Color.FromRgb(76, 52, 94)),
            "Video" => new SolidColorBrush(Color.FromRgb(84, 54, 60)),
            _ => new SolidColorBrush(Color.FromRgb(42, 48, 64))
        };

        context.DrawRectangle(fill, border, rect, 8, 8);
        context.DrawRectangle(header, null, new Rect(rect.X, rect.Y, rect.Width, Math.Min(26 * zoom, rect.Height)), 8, 8);
        DrawText(context, node.Title, new Point(rect.X + 14 * zoom, rect.Y + 8 * zoom), 13 * zoom, Brushes.White);
        DrawText(context, node.BlockId, new Point(rect.X + 14 * zoom, rect.Y + 36 * zoom), 11 * zoom, new SolidColorBrush(Color.FromRgb(188, 198, 218)));
        DrawText(context, node.BlockKind, new Point(rect.X + 14 * zoom, rect.Y + 56 * zoom), 10 * zoom, new SolidColorBrush(Color.FromRgb(139, 151, 176)));
        DrawPorts(context, node);
    }

    private void DrawPorts(DrawingContext context, CanvasNodeViewModel node)
    {
        foreach (var port in node.Inputs)
        {
            DrawPort(context, node, port, false);
        }

        foreach (var port in node.Outputs)
        {
            DrawPort(context, node, port, true);
        }
    }

    private void DrawPort(DrawingContext context, CanvasNodeViewModel node, CanvasPortViewModel port, bool output)
    {
        var point = GetPortPoint(node, port.Id, output);
        var brush = output
            ? new SolidColorBrush(Color.FromRgb(255, 196, 92))
            : new SolidColorBrush(Color.FromRgb(128, 220, 171));
        context.FillRectangle(brush, new Rect(point.X - 4, point.Y - 4, 8, 8));
    }

    private Point GetPortPoint(CanvasNodeViewModel node, string portId, bool output)
    {
        var ports = output ? node.Outputs : node.Inputs;
        var index = Math.Max(0, ports.ToList().FindIndex(x => x.Id == portId));
        var count = Math.Max(1, ports.Count);
        var y = node.Y + 34 + index * Math.Min(18, 40.0 / count);
        var x = output ? node.X + node.Width : node.X;
        return ToScreen(new Point(x, Math.Min(node.Y + node.Height - 14, y)));
    }

    private CanvasNodeViewModel? HitTest(Point world)
    {
        if (Scene is null)
        {
            return null;
        }

        for (var i = Scene.Nodes.Count - 1; i >= 0; i--)
        {
            var node = Scene.Nodes[i];
            var rect = new Rect(node.X, node.Y, node.Width, node.Height);
            if (rect.Contains(world))
            {
                return node;
            }
        }

        return null;
    }

    private Point ToWorld(Point screen)
    {
        if (Scene is null)
        {
            return screen;
        }

        return new Point((screen.X - Scene.PanX) / Scene.Zoom, (screen.Y - Scene.PanY) / Scene.Zoom);
    }

    private Point ToScreen(Point world)
    {
        if (Scene is null)
        {
            return world;
        }

        return new Point(world.X * Scene.Zoom + Scene.PanX, world.Y * Scene.Zoom + Scene.PanY);
    }

    private static void DrawText(DrawingContext context, string text, Point origin, double size, IBrush brush)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, size, brush);
        context.DrawText(formatted, origin);
    }

    private void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (CanvasNodeViewModel node in e.NewItems)
            {
                node.PropertyChanged += OnNodeChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (CanvasNodeViewModel node in e.OldItems)
            {
                node.PropertyChanged -= OnNodeChanged;
            }
        }

        InvalidateVisual();
    }

    private void OnConnectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnSceneChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }
}
