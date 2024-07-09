using Newtonsoft.Json;
using Simulation;
using System.Collections.Generic;

namespace ModLoader
{
    public static class GameWorldUtil
    {
        public static GameWorld ServerCreateWorld(List<IGameMod> mods, string gameRuleJson)
        {
            var world = new GameWorld();
            world.isHost = true;
            var data = GetInitializationData(mods);
            InitializeWorld(mods, gameRuleJson, world, data);

            foreach (IGameMod mod in mods)
            {
                mod.CreateWorld(world);
            }
            return world;
        }
        public static GameWorld ServerLoadWorld(string gameSavePath, List<IGameMod> mods, string gameRuleJson)
        {
            var world = new GameWorld();
            world.isHost = true;
            var data = GetInitializationData(mods);
            InitializeWorld(mods, gameRuleJson, world, data);

            GameSave gameSave1 = GameSave.Load(gameSavePath, data);
            gameSave1.ToGameWorld(world);
            return world;
        }

        public static GameWorld ClientLoadWorld(GameSave gameSave, WorldInitializationData data, List<IGameMod> mods, GameRule gameRule)
        {
            var world = new GameWorld();
            world.isHost = false;

            world.gameMods = mods;
            data.Apply(world);
            world.ApplyRule(gameRule);
            foreach (IGameMod mod in mods)
            {
                mod.Apply(world);
            }

            gameSave.ToGameWorld(world);
            return world;
        }

        public static WorldInitializationData GetInitializationData(List<IGameMod> mods)
        {
            var data = new WorldInitializationData();
            foreach (IGameMod mod in mods)
            {
                mod.InitializeScripts(data);
            }
            return data;
        }

        public static bool GetMods(GameSaveMeta meta, Dictionary<string, IGameMod> mods, out List<IGameMod> modList)
        {
            modList = new List<IGameMod>();
            bool result = true;
            foreach (var modName in meta.mods)
            {
                if (mods.TryGetValue(modName, out var mod))
                {
                    modList.Add(mod);
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        static void InitializeWorld(List<IGameMod> mods, string gameRuleJson, GameWorld world, WorldInitializationData data)
        {
            world.gameMods = mods;
            data.Apply(world);
            var gameRule = JsonConvert.DeserializeObject<GameRule>(gameRuleJson, new JsonConverter[]
            {
                SingletonJsonConverter.Create(data.EntityScripts),
                SingletonJsonConverter.Create(data.ItemScripts),
            });
            world.ApplyRule(gameRule);
            foreach (IGameMod mod in mods)
            {
                mod.Apply(world);
            }
        }
    }
}
