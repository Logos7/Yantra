using Yantra.Domain;

namespace Yantra.Composition;

public sealed record CompositionProblem(string Code, string Message);

public sealed record ResolvedSystem(
    SystemDefinition System,
    IReadOnlyDictionary<BlockId, BlockDefinition> Blocks,
    IReadOnlyList<CompositionProblem> Problems)
{
    public bool IsValid => Problems.Count == 0;
}
