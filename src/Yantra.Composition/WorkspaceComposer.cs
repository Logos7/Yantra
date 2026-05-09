using Yantra.Schema;

namespace Yantra.Composition;

public sealed class WorkspaceComposer
{
    private readonly SystemComposer composer;

    public WorkspaceComposer()
        : this(new SystemComposer())
    {
    }

    public WorkspaceComposer(SystemComposer composer)
    {
        this.composer = composer;
    }

    public WorkspaceCompositionResult Compose(WorkspaceDescriptorSet descriptors)
    {
        var blocks = descriptors.Blocks.Select(b => b.ToDomain()).ToList();
        var systems = descriptors.Systems
            .Select(s => composer.Compose(s.ToDomain(), blocks))
            .ToList();

        return new WorkspaceCompositionResult(systems);
    }
}
