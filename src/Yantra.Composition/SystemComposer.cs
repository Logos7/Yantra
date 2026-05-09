using Yantra.Domain;

namespace Yantra.Composition;

public sealed class SystemComposer
{
    public CompositionResult Compose(SystemDefinition system, IEnumerable<BlockDefinition> blocks)
    {
        var diagnostics = new List<CompositionDiagnostic>();
        var blockList = blocks.ToList();

        ReportDuplicateBlocks(blockList, diagnostics);
        ReportDuplicateInstances(system, diagnostics);

        var blockById = blockList
            .GroupBy(b => b.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var instanceById = system.Instances
            .GroupBy(i => i.Id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var instance in system.Instances)
        {
            if (!blockById.ContainsKey(instance.Block))
            {
                diagnostics.Add(new CompositionDiagnostic(
                    DiagnosticSeverity.Error,
                    $"Instance '{instance.Id}' references unknown block '{instance.Block}'."));
            }
        }

        foreach (var connection in system.Connections)
        {
            ValidateConnection(connection, instanceById, blockById, diagnostics);
        }

        foreach (var instance in system.Instances)
        {
            var used = system.Connections.Any(c => c.From.Instance == instance.Id || c.To.Instance == instance.Id);

            if (!used)
            {
                diagnostics.Add(new CompositionDiagnostic(
                    DiagnosticSeverity.Warning,
                    $"Instance '{instance.Id}' is not connected."));
            }
        }

        return new CompositionResult(system, diagnostics);
    }

    private static void ReportDuplicateBlocks(
        IReadOnlyList<BlockDefinition> blocks,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        foreach (var group in blocks.GroupBy(b => b.Id).Where(g => g.Count() > 1))
        {
            diagnostics.Add(new CompositionDiagnostic(
                DiagnosticSeverity.Error,
                $"Duplicate block id '{group.Key}'."));
        }
    }

    private static void ReportDuplicateInstances(
        SystemDefinition system,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        foreach (var group in system.Instances.GroupBy(i => i.Id).Where(g => g.Count() > 1))
        {
            diagnostics.Add(new CompositionDiagnostic(
                DiagnosticSeverity.Error,
                $"Duplicate instance id '{group.Key}' in system '{system.Id}'."));
        }
    }

    private static void ValidateConnection(
        SystemConnection connection,
        IReadOnlyDictionary<InstanceId, BlockInstance> instances,
        IReadOnlyDictionary<BlockId, BlockDefinition> blocks,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        var from = ResolveEndpoint(connection.From, instances, blocks, diagnostics);
        var to = ResolveEndpoint(connection.To, instances, blocks, diagnostics);

        if (from is null || to is null)
        {
            return;
        }

        if (!StringComparer.OrdinalIgnoreCase.Equals(from.Interface.Protocol, to.Interface.Protocol))
        {
            diagnostics.Add(new CompositionDiagnostic(
                DiagnosticSeverity.Error,
                $"Connection '{connection.From}' -> '{connection.To}' has incompatible protocols '{from.Interface.Protocol}' and '{to.Interface.Protocol}'."));

            return;
        }

        if (!AreDirectionsCompatible(from.Interface.Kind, to.Interface.Kind))
        {
            diagnostics.Add(new CompositionDiagnostic(
                DiagnosticSeverity.Error,
                $"Connection '{connection.From}' -> '{connection.To}' has incompatible interface kinds '{from.Interface.Kind}' and '{to.Interface.Kind}'."));
        }
    }

    private static ResolvedEndpoint? ResolveEndpoint(
        Endpoint endpoint,
        IReadOnlyDictionary<InstanceId, BlockInstance> instances,
        IReadOnlyDictionary<BlockId, BlockDefinition> blocks,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        if (!instances.TryGetValue(endpoint.Instance, out var instance))
        {
            diagnostics.Add(new CompositionDiagnostic(
                DiagnosticSeverity.Error,
                $"Connection references unknown instance '{endpoint.Instance}'."));

            return null;
        }

        if (!blocks.TryGetValue(instance.Block, out var block))
        {
            return null;
        }

        var blockInterface = block.Interfaces.FirstOrDefault(i => i.Id == endpoint.Interface);

        if (blockInterface is null)
        {
            diagnostics.Add(new CompositionDiagnostic(
                DiagnosticSeverity.Error,
                $"Connection references unknown interface '{endpoint.Interface}' on instance '{endpoint.Instance}' using block '{instance.Block}'."));

            return null;
        }

        return new ResolvedEndpoint(instance, block, blockInterface);
    }

    private static bool AreDirectionsCompatible(string fromKind, string toKind)
    {
        var from = InterfaceKindParts.Parse(fromKind);
        var to = InterfaceKindParts.Parse(toKind);

        if (!StringComparer.OrdinalIgnoreCase.Equals(from.Family, to.Family))
        {
            return false;
        }

        return IsDirectionPair(from.Direction, to.Direction);
    }

    private static bool IsDirectionPair(string from, string to)
    {
        return Is(from, "source") && Is(to, "sink")
            || Is(from, "master") && Is(to, "slave")
            || Is(from, "output") && (Is(to, "input") || Is(to, "sink"))
            || Is(from, "producer") && Is(to, "consumer");
    }

    private static bool Is(string value, string expected) =>
        StringComparer.OrdinalIgnoreCase.Equals(value, expected);

    private sealed record ResolvedEndpoint(
        BlockInstance Instance,
        BlockDefinition Block,
        BlockInterface Interface);

    private sealed record InterfaceKindParts(string Family, string Direction)
    {
        public static InterfaceKindParts Parse(string value)
        {
            var dotIndex = value.LastIndexOf('.');

            if (dotIndex <= 0 || dotIndex == value.Length - 1)
            {
                return new InterfaceKindParts(value, string.Empty);
            }

            return new InterfaceKindParts(value[..dotIndex], value[(dotIndex + 1)..]);
        }
    }
}
