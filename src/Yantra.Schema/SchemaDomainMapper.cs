using Yantra.Domain;

namespace Yantra.Schema;

public static class SchemaDomainMapper
{
    public static BlockDefinition ToDomain(this BlockDescriptorDocument document)
    {
        return new BlockDefinition(
            new BlockId(document.Id),
            document.Name,
            ParseEnum<BlockKind>(document.Kind, BlockKind.Other),
            document.Interfaces.Select(ToDomain).ToArray(),
            document.Parameters.Select(ToDomain).ToArray());
    }

    public static BoardDefinition ToDomain(this BoardDescriptorDocument document)
    {
        return new BoardDefinition(
            new BoardId(document.Id),
            document.Name,
            document.Vendor,
            document.Family,
            document.Device,
            document.Pins.Select(x => new BoardPin(x.Name, x.PhysicalPin, x.SignalKind)).ToArray());
    }

    public static SystemDefinition ToDomain(this SystemDescriptorDocument document)
    {
        return new SystemDefinition(
            new SystemId(document.Id),
            document.Name,
            new BoardId(document.Board),
            new BackendId(document.Backend),
            document.Instances.Select(ToDomain).ToArray(),
            document.Connections.Select(ToDomain).ToArray());
    }

    private static BlockInterface ToDomain(InterfaceDescriptorDocument document)
    {
        return new BlockInterface(
            new InterfaceId(document.Id),
            document.Kind,
            document.Protocol,
            ParseEnum<InterfaceDirection>(document.Direction, InterfaceDirection.InOut));
    }

    private static BlockParameter ToDomain(ParameterDescriptorDocument document)
    {
        return new BlockParameter(document.Name, document.Type, document.DefaultValue);
    }

    private static BlockInstance ToDomain(BlockInstanceDescriptorDocument document)
    {
        return new BlockInstance(new InstanceId(document.Id), new BlockId(document.Block), document.Parameters);
    }

    private static SystemConnection ToDomain(SystemConnectionDescriptorDocument document)
    {
        return new SystemConnection(ParseEndpoint(document.From), ParseEndpoint(document.To));
    }

    private static Endpoint ParseEndpoint(string value)
    {
        var parts = value.Split('.', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new FormatException($"Endpoint '{value}' must use 'instance.interface'.");
        }

        return new Endpoint(new InstanceId(parts[0]), new InterfaceId(parts[1]));
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : fallback;
    }
}
