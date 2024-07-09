using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ModLoader
{
    public class SingletonJsonConverter<T> : JsonConverter<T> where T : class
    {
        public SingletonJsonConverter(Dictionary<string, T> entityMap)
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

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (serializeFind.TryGetValue(value, out string v))
            {
                writer.WriteValue(v);
            }
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            deserializeFind.TryGetValue((string)reader.Value, out var entityScript);
            return entityScript;
        }
    }

    public static class SingletonJsonConverter
    {
        public static SingletonJsonConverter<T> Create<T>(Dictionary<string, T> entityMap) where T : class
        {
            return new SingletonJsonConverter<T>(entityMap);
        }
    }
}
