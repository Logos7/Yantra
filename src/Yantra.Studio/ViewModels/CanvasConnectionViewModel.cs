namespace Yantra.Studio.ViewModels;

public sealed record CanvasConnectionViewModel(
    string FromNodeId,
    string FromPortId,
    string ToNodeId,
    string ToPortId,
    string Label);
