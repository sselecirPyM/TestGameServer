using MessagePack;
using MessagePack.Formatters;
using System.Collections.Generic;
using System.Text;

namespace ModLoader
{
    public class SingletonFormater<T> : IMessagePackFormatter<T> where T : class
    {
        public SingletonFormater(Dictionary<string, T> entityMap)
        {
            deserializeFind = entityMap;
            serializeFind = new Dictionary<T, string>();
            foreach (var pair in deserializeFind)
            {
                serializeFind[pair.Value] = pair.Key;
            }
        }
        Dictionary<string, T> deserializeFind;
        Dictionary<T, string> serializeFind;

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }
            if (serializeFind.TryGetValue(value, out string v))
            {
                writer.WriteString(Encoding.UTF8.GetBytes(v));
            }
            else
            {
                writer.WriteNil();
            }
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);

            var path = reader.ReadString();

            reader.Depth--;
            deserializeFind.TryGetValue(path, out var entityScript);

            return entityScript;
        }
    }
}
