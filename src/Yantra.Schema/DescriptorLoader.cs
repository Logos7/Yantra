using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Yantra.Schema;

public sealed class DescriptorLoader
{
    private readonly IDeserializer _deserializer;

    public DescriptorLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public BlockDescriptorDocument LoadBlock(string path)
    {
        return Load<BlockDescriptorDocument>(path);
    }

    public BoardDescriptorDocument LoadBoard(string path)
    {
        return Load<BoardDescriptorDocument>(path);
    }

    public SystemDescriptorDocument LoadSystem(string path)
    {
        return Load<SystemDescriptorDocument>(path);
    }

    public T Load<T>(string path)
    {
        var yaml = File.ReadAllText(path);
        return _deserializer.Deserialize<T>(yaml);
    }
}
