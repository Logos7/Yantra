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

public readonly record struct TargetId(string Value)
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
    Target,
    Other
}

public sealed record BlockInterface(InterfaceId Id, string Kind, string Protocol);

public sealed record Endpoint(InstanceId Instance, InterfaceId Interface);

public sealed record SystemConnection(Endpoint From, Endpoint To);

public sealed record BlockInstance(InstanceId Id, BlockId Block);

public sealed record BlockDefinition(
    BlockId Id,
    string Name,
    BlockKind Kind,
    IReadOnlyList<BlockInterface> Interfaces);

public sealed record SystemDefinition(
    SystemId Id,
    string Name,
    TargetId Target,
    IReadOnlyList<BlockInstance> Instances,
    IReadOnlyList<SystemConnection> Connections);
