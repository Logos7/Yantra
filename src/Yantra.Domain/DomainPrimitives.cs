namespace Yantra.Domain;

public readonly record struct BlockId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct InstanceId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct SystemId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct BoardId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct BackendId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct InterfaceId(string Value)
{
    public override string ToString() => Value;
}

public enum BlockKind
{
    Processor,
    Arithmetic,
    Memory,
    Bus,
    Io,
    Video,
    Clock,
    Other
}

public enum InterfaceDirection
{
    Input,
    Output,
    InOut
}

public sealed record BlockInterface(
    InterfaceId Id,
    string Kind,
    string Protocol,
    InterfaceDirection Direction);

public sealed record BlockParameter(
    string Name,
    string Type,
    string? DefaultValue);

public sealed record Endpoint(InstanceId Instance, InterfaceId Interface)
{
    public override string ToString() => $"{Instance}.{Interface}";
}

public sealed record SystemConnection(Endpoint From, Endpoint To)
{
    public override string ToString() => $"{From} -> {To}";
}

public sealed record BlockInstance(
    InstanceId Id,
    BlockId Block,
    IReadOnlyDictionary<string, string> Parameters);

public sealed record BlockDefinition(
    BlockId Id,
    string Name,
    BlockKind Kind,
    IReadOnlyList<BlockInterface> Interfaces,
    IReadOnlyList<BlockParameter> Parameters);

public sealed record BoardPin(
    string Name,
    string PhysicalPin,
    string SignalKind);

public sealed record BoardDefinition(
    BoardId Id,
    string Name,
    string Vendor,
    string Family,
    string Device,
    IReadOnlyList<BoardPin> Pins);

public sealed record SystemDefinition(
    SystemId Id,
    string Name,
    BoardId Board,
    BackendId Backend,
    IReadOnlyList<BlockInstance> Instances,
    IReadOnlyList<SystemConnection> Connections);
