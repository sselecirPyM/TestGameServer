using Simulation;
using System.Collections.Generic;
using System.Linq;

namespace ModLoader
{
    public class SyncFrame
    {
        public List<Entity> entities;
        public List<WorldEvent> worldEvents;
        public WorldPersistentData persistentData;
        public float frequency;
        public double startTime;
        public float timeOfTheDay;

        public GameRule rule;
        public GameSave gameSave;
        public SyncGameInfo gameInfo;

        public static SyncFrame SyncDelta(GameWorld world, bool fullWorld)
        {
            SyncFrame syncFrame = new SyncFrame();
            syncFrame.startTime = world.startTime;
            syncFrame.worldEvents = world.needSyncEvents;
            syncFrame.entities = world.needSyncEntities.Values.ToList();
            if (fullWorld)
            {
                syncFrame.persistentData = world.persistentData;
                foreach (var e in world.entities)
                {
                    if (!world.needSyncEntities.ContainsKey(e.id))
                        syncFrame.entities.Add(e);
                }
            }

            return syncFrame;
        }

        public static SyncFrame SyncInfo(GameWorld world)
        {
            SyncFrame syncFrame = new SyncFrame();
            syncFrame.startTime = world.startTime;
            syncFrame.gameInfo = new SyncGameInfo()
            {
                gameSaveMeta = GameSave.GetMeta(world)
            };

            return syncFrame;
        }

        public static SyncFrame SyncSave(GameWorld world)
        {
            SyncFrame syncFrame = new SyncFrame();
            syncFrame.startTime = world.startTime;
            var gameSave = new GameSave();
            gameSave.FromGameWorld(world);
            gameSave.hostPersistentData = null;

            syncFrame.gameSave = gameSave;
            syncFrame.rule = world.rule;

            return syncFrame;
        }

        public void SyncWorld(GameWorld world)
        {
            world.startTime = startTime;
            if (entities != null)
            {
                var dict = new Dictionary<int, Entity>();
                foreach (var entity in entities)
                {
                    dict[entity.id] = entity;
                }

                for (int i = 0; i < world.entities.Count; i++)
                {
                    var entity = world.entities[i];
                    if (dict.TryGetValue(entity.id, out var entity2))
                    {
                        entity2.CopyTo(entity);
                        dict.Remove(entity.id);
                    }
                }
                var remain = dict.Values;
                world.entities.AddRange(remain);
                foreach (var val in remain)
                {
                    val.updateModel = true;
                    world.id2Entity[val.id] = val;
                }
            }
            if (worldEvents != null)
            {
                world.worldEvents.AddRange(worldEvents);
            }
            if (persistentData != null)
            {
                world.persistentData = persistentData;
            }
        }
    }

}