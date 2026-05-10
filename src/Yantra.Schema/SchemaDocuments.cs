namespace Yantra.Schema;

public sealed class BlockDescriptorDocument
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "Other";
    public List<InterfaceDescriptorDocument> Interfaces { get; set; } = [];
    public List<ParameterDescriptorDocument> Parameters { get; set; } = [];
}

public sealed class InterfaceDescriptorDocument
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "signal";
    public string Protocol { get; set; } = "wire";
    public string Direction { get; set; } = "InOut";
}

public sealed class ParameterDescriptorDocument
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public string? DefaultValue { get; set; }
}

public sealed class BoardDescriptorDocument
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Family { get; set; } = "";
    public string Device { get; set; } = "";
    public List<BoardPinDescriptorDocument> Pins { get; set; } = [];
}

public sealed class BoardPinDescriptorDocument
{
    public string Name { get; set; } = "";
    public string PhysicalPin { get; set; } = "";
    public string SignalKind { get; set; } = "gpio";
}

public sealed class SystemDescriptorDocument
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Board { get; set; } = "";
    public string Backend { get; set; } = "";
    public List<BlockInstanceDescriptorDocument> Instances { get; set; } = [];
    public List<SystemConnectionDescriptorDocument> Connections { get; set; } = [];
}

public sealed class BlockInstanceDescriptorDocument
{
    public string Id { get; set; } = "";
    public string Block { get; set; } = "";
    public Dictionary<string, string> Parameters { get; set; } = [];
}

public sealed class SystemConnectionDescriptorDocument
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
}
