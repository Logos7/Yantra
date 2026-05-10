using Yantra.Domain;

namespace Yantra.Composition;

public sealed class SystemComposer
{
    public ResolvedSystem Compose(SystemDefinition system, IEnumerable<BlockDefinition> blocks)
    {
        var problems = new List<CompositionProblem>();
        var blockMap = new Dictionary<BlockId, BlockDefinition>();

        foreach (var block in blocks)
        {
            if (!blockMap.TryAdd(block.Id, block))
            {
                problems.Add(new CompositionProblem("duplicate-block", $"Block '{block.Id}' is defined more than once."));
            }
        }

        var instanceMap = new Dictionary<InstanceId, BlockInstance>();
        foreach (var instance in system.Instances)
        {
            if (!instanceMap.TryAdd(instance.Id, instance))
            {
                problems.Add(new CompositionProblem("duplicate-instance", $"Instance '{instance.Id}' is defined more than once."));
            }

            if (!blockMap.ContainsKey(instance.Block))
            {
                problems.Add(new CompositionProblem("missing-block", $"Block '{instance.Block}' used by instance '{instance.Id}' was not found."));
            }
        }

        foreach (var connection in system.Connections)
        {
            ValidateEndpoint(connection.From, "from");
            ValidateEndpoint(connection.To, "to");
        }

        return new ResolvedSystem(system, blockMap, problems);

        void ValidateEndpoint(Endpoint endpoint, string role)
        {
            if (!instanceMap.TryGetValue(endpoint.Instance, out var instance))
            {
                problems.Add(new CompositionProblem("missing-instance", $"Connection {role} endpoint '{endpoint}' references missing instance '{endpoint.Instance}'."));
                return;
            }

            if (!blockMap.TryGetValue(instance.Block, out var block))
            {
                return;
            }

            if (block.Interfaces.All(x => x.Id != endpoint.Interface))
            {
                problems.Add(new CompositionProblem("missing-interface", $"Endpoint '{endpoint}' references missing interface '{endpoint.Interface}' on block '{instance.Block}'."));
            }
        }
    }
}
