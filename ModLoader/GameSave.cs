using MessagePack;
using MessagePack.Resolvers;
using Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace ModLoader
{
    public class GameSave
    {
        public List<Entity> entities = new List<Entity>();
        public int idCount;
        public double startTime;
        public WorldPersistentData persistentData;
        public HostPersistentData hostPersistentData;

        public string version;

        public void FromGameWorld(GameWorld world)
        {
            idCount = world.idCount;
            startTime = world.startTime;
            entities = world.entities;
            persistentData = world.persistentData;
            hostPersistentData = world.hostPersistentData;
        }

        public void ToGameWorld(GameWorld world)
        {
            world.idCount = idCount;
            world.startTime = startTime;
            world.entities = entities;
            world.persistentData = persistentData;
            if (hostPersistentData != null)
                world.hostPersistentData = hostPersistentData;
            foreach (var entity in world.entities)
            {
                world.id2Entity[entity.id] = entity;
            }
        }

        public static GameSave Load(string path, WorldInitializationData data)
        {
            var serializer = new DataSerializer(data);

            GameSave gameSave;
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                using var stream = archive.GetEntry("save").Open();
                gameSave = serializer.Deserialize<GameSave>(stream);
            }
            return gameSave;
        }

        public static GameSaveMeta LoadMeta(string path)
        {
            var options = new MessagePackSerializerOptions(ContractlessStandardResolver.Instance);

            GameSaveMeta meta;
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                using var stream = archive.GetEntry("meta").Open();
                meta = MessagePackSerializer.Deserialize<GameSaveMeta>(stream, options);
            }
            return meta;
        }

        public static GameSaveMeta GetMeta(GameWorld world)
        {
            var meta = new GameSaveMeta()
            {
                mods = new List<string>()
            };
            foreach (var mod in world.gameMods)
            {
                foreach (var attribute in mod.GetType().GetCustomAttributes<ExportMetadataAttribute>())
                {
                    if (attribute.Name == "id")
                    {
                        meta.mods.Add(attribute.Value.ToString());
                        break;
                    }
                }
            }
            return meta;
        }

        public static void Save(string path, GameWorld world)
        {
            var serializer = new DataSerializer(world);

            GameSave gameSave = new GameSave();
            gameSave.FromGameWorld(world);

            GameSaveMeta meta = GetMeta(world);
            using (FileStream zipToOpen = new FileStream(path, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                {
                    using var stream = archive.CreateEntry("meta").Open();
                    serializer.Serialize(stream, meta);
                }
                {
                    using var stream = archive.CreateEntry("save").Open();
                    serializer.Serialize(stream, gameSave);
                }
            }
        }

        public static string GetSaveDirectory()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "farm_game_save");
        }
    }

    public class GameSaveMeta
    {
        public List<string> mods;
        public string owner;
    }
}