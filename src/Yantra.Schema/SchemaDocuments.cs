namespace Yantra.Schema;

public sealed record BlockDocument(
    string Id,
    string Name,
    string Kind,
    IReadOnlyList<InterfaceDocument> Interfaces);

public sealed record InterfaceDocument(
    string Name,
    string Kind,
    string Protocol);

public sealed record BoardDocument(
    string Id,
    string Name,
    string Vendor,
    IReadOnlyList<BoardClockDocument> Clocks);

public sealed record BoardClockDocument(
    string Id,
    long FrequencyHz);

public sealed record SystemDocument(
    string Id,
    string Name,
    string Board,
    string Backend,
    IReadOnlyList<InstanceDocument> Instances,
    IReadOnlyList<ConnectionDocument> Connections);

public sealed record InstanceDocument(string Id, string Block);

public sealed record ConnectionDocument(string From, string To);
