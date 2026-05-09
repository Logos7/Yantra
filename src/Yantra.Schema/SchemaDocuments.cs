namespace Yantra.Schema;

public sealed class BlockDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public List<ParameterDocument> Parameters { get; set; } = [];
    public List<InterfaceDocument> Interfaces { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}

public sealed class ParameterDocument
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? Default { get; set; }
}

public sealed class InterfaceDocument
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
}

public sealed class BoardDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public List<BoardClockDocument> Clocks { get; set; } = [];
    public List<BoardLedDocument> Leds { get; set; } = [];
    public Dictionary<string, BoardConstraintBackendDocument> Constraints { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}

public sealed class BoardClockDocument
{
    public string Id { get; set; } = string.Empty;
    public long FrequencyHz { get; set; }
    public string Pin { get; set; } = string.Empty;
    public string Signal { get; set; } = string.Empty;
}

public sealed class BoardLedDocument
{
    public string Id { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string Signal { get; set; } = string.Empty;
    public string Active { get; set; } = string.Empty;
}

public sealed class BoardConstraintBackendDocument
{
    public List<string> Fragments { get; set; } = [];
}

public sealed class SystemDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Board { get; set; } = string.Empty;
    public string Backend { get; set; } = string.Empty;
    public List<InstanceDocument> Instances { get; set; } = [];
    public List<ConnectionDocument> Connections { get; set; } = [];
}

public sealed class InstanceDocument
{
    public string Id { get; set; } = string.Empty;
    public string Block { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = [];
}

public sealed class ConnectionDocument
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public sealed class BuildDocument
{
    public string Id { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public string Board { get; set; } = string.Empty;
    public string Backend { get; set; } = string.Empty;
    public string Top { get; set; } = string.Empty;
    public List<string> Constraints { get; set; } = [];
    public List<BoardBindingDocument> BoardBindings { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}

public sealed class BoardBindingDocument
{
    public string Interface { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Signal { get; set; } = string.Empty;
}
