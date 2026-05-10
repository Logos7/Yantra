using Yantra.Domain;
using Yantra.Studio.ViewModels;

namespace Yantra.Studio.Services;

public static class CanvasSceneFactory
{
    public static CanvasSceneViewModel FromSystem(SystemDefinition system, IReadOnlyDictionary<BlockId, BlockDefinition> blocks)
    {
        var scene = new CanvasSceneViewModel(system.Id.Value, system.Name, system.Board.Value, system.Backend.Value);
        var positions = BuildPositions(system);

        foreach (var instance in system.Instances)
        {
            blocks.TryGetValue(instance.Block, out var block);
            var point = positions[instance.Id];
            var node = CreateNode(instance, block, point.X, point.Y);
            scene.Nodes.Add(node);
        }

        foreach (var connection in system.Connections)
        {
            scene.Connections.Add(new CanvasConnectionViewModel(
                connection.From.Instance.Value,
                connection.From.Interface.Value,
                connection.To.Instance.Value,
                connection.To.Interface.Value,
                $"{connection.From.Interface.Value} → {connection.To.Interface.Value}"));
        }

        scene.SelectedNode = scene.Nodes.FirstOrDefault();
        return scene;
    }

    public static CanvasSceneViewModel Sample()
    {
        var scene = new CanvasSceneViewModel("sample", "Sample System", "unknown", "unknown");
        var clockPorts = new[] { new CanvasPortViewModel("clk", "Output", "signal", "wire") };
        var counterPorts = new[]
        {
            new CanvasPortViewModel("clk", "Input", "signal", "wire"),
            new CanvasPortViewModel("value", "Output", "signal", "wire")
        };
        var ledPorts = new[] { new CanvasPortViewModel("input", "Input", "signal", "wire") };

        scene.Nodes.Add(new CanvasNodeViewModel("clock", "clock", "clock.27mhz", "27 MHz Clock", "Clock", clockPorts, new Dictionary<string, string>(), 60, 80));
        scene.Nodes.Add(new CanvasNodeViewModel("counter", "counter", "arithmetic.counter", "Counter", "Arithmetic", counterPorts, new Dictionary<string, string> { ["width"] = "25" }, 310, 80));
        scene.Nodes.Add(new CanvasNodeViewModel("led0", "led0", "io.led", "LED", "Io", ledPorts, new Dictionary<string, string> { ["active_low"] = "true" }, 560, 80));
        scene.Connections.Add(new CanvasConnectionViewModel("clock", "clk", "counter", "clk", "clk → clk"));
        scene.Connections.Add(new CanvasConnectionViewModel("counter", "value", "led0", "input", "value → input"));
        scene.SelectedNode = scene.Nodes.FirstOrDefault();
        return scene;
    }

    private static CanvasNodeViewModel CreateNode(BlockInstance instance, BlockDefinition? block, double x, double y)
    {
        var ports = block is null
            ? []
            : block.Interfaces.Select(port => new CanvasPortViewModel(
                port.Id.Value,
                port.Direction.ToString(),
                port.Kind,
                port.Protocol)).ToArray();

        return new CanvasNodeViewModel(
            instance.Id.Value,
            instance.Id.Value,
            instance.Block.Value,
            block?.Name ?? instance.Block.Value,
            block?.Kind.ToString() ?? "Missing",
            ports,
            instance.Parameters,
            x,
            y);
    }

    private static Dictionary<InstanceId, GraphPoint> BuildPositions(SystemDefinition system)
    {
        var result = new Dictionary<InstanceId, GraphPoint>();
        var incoming = system.Connections
            .GroupBy(x => x.To.Instance)
            .ToDictionary(x => x.Key, x => x.Count());
        var outgoing = system.Connections
            .GroupBy(x => x.From.Instance)
            .ToDictionary(x => x.Key, x => x.Count());

        var ordered = system.Instances
            .OrderBy(x => incoming.GetValueOrDefault(x.Id))
            .ThenByDescending(x => outgoing.GetValueOrDefault(x.Id))
            .ThenBy(x => x.Id.Value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        for (var i = 0; i < ordered.Length; i++)
        {
            var instance = ordered[i];
            result[instance.Id] = new GraphPoint(70 + i * 250, 95 + (i % 2) * 120);
        }

        return result;
    }

    private readonly record struct GraphPoint(double X, double Y);
}
