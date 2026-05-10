using Yantra.Domain;

namespace Yantra.Schema;

public sealed class WorkspaceDescriptorLoader
{
    private readonly DescriptorLoader _loader = new();

    public IReadOnlyList<BlockDefinition> LoadBlocks(string rootDirectory)
    {
        var blocksPath = Path.Combine(rootDirectory, "blocks");
        if (!Directory.Exists(blocksPath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(blocksPath, "block.yaml", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(path => _loader.LoadBlock(path).ToDomain())
            .ToArray();
    }

    public IReadOnlyList<BoardDefinition> LoadBoards(string rootDirectory)
    {
        var boardsPath = Path.Combine(rootDirectory, "boards");
        if (!Directory.Exists(boardsPath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(boardsPath, "board.yaml", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(path => _loader.LoadBoard(path).ToDomain())
            .ToArray();
    }

    public IReadOnlyList<SystemDefinition> LoadSystems(string rootDirectory)
    {
        var systemsPath = Path.Combine(rootDirectory, "systems");
        if (!Directory.Exists(systemsPath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(systemsPath, "system.yaml", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(path => _loader.LoadSystem(path).ToDomain())
            .ToArray();
    }
}
