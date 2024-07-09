using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Simulation;
using System.IO;
using System.IO.Compression;

namespace ModLoader
{
    public class DataSerializer
    {
        public readonly MessagePackSerializerOptions options;
        public DataSerializer(GameWorld world)
        {
            options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
                new IMessagePackFormatter[] {
                    new SingletonFormater<EntityScript>(world.scripts),
                    new SingletonFormater<ItemScript>(world.itemScripts),},
                new IFormatterResolver[] { ContractlessStandardResolver.Instance }));
        }
        public DataSerializer(WorldInitializationData data)
        {
            options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
                new IMessagePackFormatter[] {
                    new SingletonFormater<EntityScript>(data.EntityScripts),
                    new SingletonFormater<ItemScript>(data.ItemScripts)},
                new IFormatterResolver[] { ContractlessStandardResolver.Instance }));
        }

        public DataSerializer()
        {
            options = new MessagePackSerializerOptions(ContractlessStandardResolver.Instance);
        }

        public byte[] Serialize<T>(T data)
        {
            using var outputStream = new MemoryStream();
            MessagePackSerializer.Serialize(outputStream, data, options);
            return outputStream.ToArray();
        }

        public void Serialize<T>(Stream stream, T data)
        {
            MessagePackSerializer.Serialize(stream, data, options);
        }

        public T Deserialize<T>(Stream stream)
        {
            return MessagePackSerializer.Deserialize<T>(stream, options);
        }

        public T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data, options);
        }

        public byte[] SerializeAndCompress<T>(T data)
        {
            using var outputStream = new MemoryStream();
            using var deflate = new DeflateStream(outputStream, CompressionMode.Compress);
            MessagePackSerializer.Serialize(deflate, data, options);
            deflate.Dispose();
            return outputStream.ToArray();
        }

        public T DecompressAndDeserialize<T>(Stream compressed)
        {
            using var stream = new DeflateStream(compressed, CompressionMode.Decompress);
            return MessagePackSerializer.Deserialize<T>(stream, options);
        }

        public T DecompressAndDeserialize<T>(byte[] data)
        {
            using var stream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
            return MessagePackSerializer.Deserialize<T>(stream, options);
        }
    }
}
