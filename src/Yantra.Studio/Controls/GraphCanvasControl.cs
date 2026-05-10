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
                oldScene.PropertyChanged -= OnSceneChanged;
                foreach (var node in oldScene.Nodes)
                {
                    node.PropertyChanged -= OnNodeChanged;
                }
            }

            if (change.NewValue is CanvasSceneViewModel newScene)
            {
                newScene.Nodes.CollectionChanged += OnNodesChanged;
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
                DrawConnection(context, from, to, connection.Label);
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

    private void DrawConnection(DrawingContext context, CanvasNodeViewModel from, CanvasNodeViewModel to, string label)
    {
        var start = ToScreen(new Point(from.X + from.Width, from.Y + from.Height / 2));
        var end = ToScreen(new Point(to.X, to.Y + to.Height / 2));
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(105, 177, 255)), 2);
        context.DrawLine(pen, start, end);

        var middle = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2 - 16);
        DrawText(context, label, middle, 11, new SolidColorBrush(Color.FromRgb(154, 201, 255)));
    }

    private void DrawNode(DrawingContext context, CanvasNodeViewModel node)
    {
        var p = ToScreen(new Point(node.X, node.Y));
        var rect = new Rect(p.X, p.Y, node.Width * (Scene?.Zoom ?? 1), node.Height * (Scene?.Zoom ?? 1));
        var fill = node.IsSelected
            ? new SolidColorBrush(Color.FromRgb(39, 66, 104))
            : new SolidColorBrush(Color.FromRgb(24, 31, 46));
        var border = node.IsSelected
            ? new Pen(new SolidColorBrush(Color.FromRgb(122, 187, 255)), 2)
            : new Pen(new SolidColorBrush(Color.FromRgb(62, 73, 96)), 1);

        context.DrawRectangle(fill, border, rect, 8, 8);
        DrawText(context, node.Title, new Point(rect.X + 14, rect.Y + 12), 13, Brushes.White);
        DrawText(context, node.BlockKind, new Point(rect.X + 14, rect.Y + 36), 11, new SolidColorBrush(Color.FromRgb(170, 181, 202)));

        var portBrush = new SolidColorBrush(Color.FromRgb(255, 196, 92));
        context.FillRectangle(portBrush, new Rect(rect.X - 4, rect.Center.Y - 4, 8, 8));
        context.FillRectangle(portBrush, new Rect(rect.Right - 4, rect.Center.Y - 4, 8, 8));
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

    private void OnSceneChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }
}
