using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Yantra.Schema;

public sealed class DescriptorLoader
{
    private readonly IDeserializer deserializer;

    public DescriptorLoader()
    {
        deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public BlockDocument LoadBlock(string path) => LoadRequired<BlockDocument>(path);

    public BoardDocument LoadBoard(string path) => LoadRequired<BoardDocument>(path);

    public SystemDocument LoadSystem(string path) => LoadRequired<SystemDocument>(path);

    public BuildDocument LoadBuild(string path) => LoadRequired<BuildDocument>(path);

    private T LoadRequired<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Descriptor file does not exist: {path}", path);
        }

        using var reader = File.OpenText(path);
        var document = deserializer.Deserialize<T>(reader);

        return document ?? throw new InvalidDataException($"Descriptor file is empty or invalid: {path}");
    }
}

public sealed class WorkspaceDescriptorSet
{
    public required string RootPath { get; init; }
    public IReadOnlyList<BlockDocument> Blocks { get; init; } = [];
    public IReadOnlyList<BoardDocument> Boards { get; init; } = [];
    public IReadOnlyList<SystemDocument> Systems { get; init; } = [];
    public IReadOnlyList<BuildDocument> Builds { get; init; } = [];
}

public sealed class WorkspaceDescriptorLoader
{
    private readonly DescriptorLoader loader;

    public WorkspaceDescriptorLoader()
        : this(new DescriptorLoader())
    {
    }

    public WorkspaceDescriptorLoader(DescriptorLoader loader)
    {
        this.loader = loader;
    }

    public WorkspaceDescriptorSet Load(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Workspace directory does not exist: {rootPath}");
        }

        return new WorkspaceDescriptorSet
        {
            RootPath = rootPath,
            Blocks = Find(rootPath, "blocks", "block.yaml").Select(loader.LoadBlock).ToList(),
            Boards = Find(rootPath, "boards", "board.yaml").Select(loader.LoadBoard).ToList(),
            Systems = Find(rootPath, "systems", "system.yaml").Select(loader.LoadSystem).ToList(),
            Builds = Find(rootPath, "systems", "build.yaml").Select(loader.LoadBuild).ToList()
        };
    }

    private static IEnumerable<string> Find(string rootPath, string directoryName, string fileName)
    {
        var directory = Path.Combine(rootPath, directoryName);

        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateFiles(directory, fileName, SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase);
    }
}
