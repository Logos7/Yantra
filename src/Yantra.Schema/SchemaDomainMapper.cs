using System.Globalization;
using Yantra.Domain;

namespace Yantra.Schema;

public static class SchemaDomainMapper
{
    public static BlockDefinition ToDomain(this BlockDocument document)
    {
        var kind = Enum.TryParse<BlockKind>(document.Kind, ignoreCase: true, out var parsedKind)
            ? parsedKind
            : BlockKind.Other;

        return new BlockDefinition(
            new BlockId(document.Id),
            document.Name,
            kind,
            document.Interfaces
                .Select(i => new BlockInterface(new InterfaceId(i.Name), i.Kind, i.Protocol))
                .ToList(),
            document.Parameters
                .Select(p => new BlockParameter(p.Name, p.Type, ToInvariantString(p.Default)))
                .ToList());
    }

    public static SystemDefinition ToDomain(this SystemDocument document)
    {
        return new SystemDefinition(
            new SystemId(document.Id),
            document.Name,
            new BoardId(document.Board),
            new BackendId(document.Backend),
            document.Instances
                .Select(i => new BlockInstance(
                    new InstanceId(i.Id),
                    new BlockId(i.Block),
                    new Dictionary<string, string>(i.Parameters, StringComparer.Ordinal)))
                .ToList(),
            document.Connections
                .Select(c => new SystemConnection(ParseEndpoint(c.From), ParseEndpoint(c.To)))
                .ToList());
    }

    private static Endpoint ParseEndpoint(string value)
    {
        var dotIndex = value.LastIndexOf('.');

        if (dotIndex <= 0 || dotIndex == value.Length - 1)
        {
            throw new FormatException($"Endpoint must have form instance.interface: {value}");
        }

        return new Endpoint(
            new InstanceId(value[..dotIndex]),
            new InterfaceId(value[(dotIndex + 1)..]));
    }

    private static string? ToInvariantString(object? value)
    {
        return value switch
        {
            null => null,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
