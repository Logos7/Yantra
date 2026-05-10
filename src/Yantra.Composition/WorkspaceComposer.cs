using Yantra.Domain;
using Yantra.Schema;

namespace Yantra.Composition;

public sealed class WorkspaceComposer
{
    private readonly WorkspaceDescriptorLoader _loader = new();
    private readonly SystemComposer _composer = new();

    public WorkspaceComposition Compose(string rootDirectory)
    {
        var blocks = _loader.LoadBlocks(rootDirectory);
        var boards = _loader.LoadBoards(rootDirectory);
        var systems = _loader.LoadSystems(rootDirectory);
        var resolved = systems.Select(system => _composer.Compose(system, blocks)).ToArray();

        return new WorkspaceComposition(blocks, boards, systems, resolved);
    }
}

public sealed record WorkspaceComposition(
    IReadOnlyList<BlockDefinition> Blocks,
    IReadOnlyList<BoardDefinition> Boards,
    IReadOnlyList<SystemDefinition> Systems,
    IReadOnlyList<ResolvedSystem> ResolvedSystems);
