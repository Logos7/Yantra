using Yantra.Composition;
using Yantra.Domain;

namespace Yantra.Studio.Services;

public sealed class StudioWorkspaceLoader
{
    private readonly WorkspaceComposer _composer = new();

    public StudioWorkspaceSnapshot LoadNearest()
    {
        var root = FindWorkspaceRoot();
        var composition = _composer.Compose(root);
        return new StudioWorkspaceSnapshot(root, composition);
    }

    private static string FindWorkspaceRoot()
    {
        foreach (var candidate in EnumerateSearchRoots())
        {
            var directory = new DirectoryInfo(candidate);
            while (directory is not null)
            {
                if (IsWorkspaceRoot(directory.FullName))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }

    private static bool IsWorkspaceRoot(string path)
    {
        return File.Exists(Path.Combine(path, "Yantra.sln"))
            && Directory.Exists(Path.Combine(path, "blocks"))
            && Directory.Exists(Path.Combine(path, "systems"));
    }
}

public sealed class StudioWorkspaceSnapshot
{
    public StudioWorkspaceSnapshot(string rootDirectory, WorkspaceComposition composition)
    {
        RootDirectory = rootDirectory;
        Blocks = composition.Blocks;
        Boards = composition.Boards;
        Systems = composition.Systems;
        ResolvedSystems = composition.ResolvedSystems;
        BlocksById = Blocks.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());
        ResolvedBySystemId = ResolvedSystems.GroupBy(x => x.System.Id).ToDictionary(x => x.Key, x => x.First());
    }

    public string RootDirectory { get; }
    public IReadOnlyList<BlockDefinition> Blocks { get; }
    public IReadOnlyList<BoardDefinition> Boards { get; }
    public IReadOnlyList<SystemDefinition> Systems { get; }
    public IReadOnlyList<ResolvedSystem> ResolvedSystems { get; }
    public IReadOnlyDictionary<BlockId, BlockDefinition> BlocksById { get; }
    public IReadOnlyDictionary<SystemId, ResolvedSystem> ResolvedBySystemId { get; }
}
