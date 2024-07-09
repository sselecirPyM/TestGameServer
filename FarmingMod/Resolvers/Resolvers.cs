using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "crop")]
    public class CropResolver : EntityScript
    {
        public Timer timer = new Timer() { interval = 1 };

        public Random random = new Random();

        bool updateEntity = false;

        float growthRate = 1;

        public override void FrameBegin()
        {
            updateEntity = timer.AddTime(World.fixedDeltaTime);
            switch(World.persistentData.difficulty)
            {
                case 0:
                    growthRate = 1;
                    break;
                case 1:
                    growthRate = 0.75f;
                    break;
                default:
                    growthRate = 0.5f;
                    break;
            }
        }

        public override void Update(Entity entity)
        {
            var crop = entity.crop;
            if (crop == null)
                return;

            if (updateEntity && World.IsDay())
            {
                if (crop.water >= growthRate)
                {
                    crop.water -= growthRate;
                    crop.nutrition += growthRate;
                }
                crop.nutrition += growthRate;
                crop.nutrition = MathF.Min(crop.nutrition, crop.maxNutrition);
            }
            entity.transform.scale = new Vector3(MathF.Sqrt(Math.Max(crop.nutrition / MathF.Max(crop.maxNutrition, 1), 0.01f)) * 0.2f + 1.0f);
            World.HostSyncEntity(entity);

            if (crop.growthId > 0 && crop.nutrition >= crop.growthThreshold &&
                World.entityTemplates.TryGetValue(crop.growthId, out var growthType))
            {
                World.MorphTo(entity, growthType);
            }
        }

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Harvest && source.character != null)
            {
                CutCrop(World, random, source, entity);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Harvest && source.character != null)
            {
                return true;
            }
            return false;
        }

        internal static void CutCrop(GameWorld World, Random random, Entity source, Entity entity)
        {
            if (entity.crop != null)
            {
                World.GiveItemWithNotice(source, entity.crop.lootId, entity.crop.productCount);
                if (source.character != null)
                    source.character.score += entity.crop.harvestScore;
            }

            World.DestroyEntity(entity.id);
            World.EmitEvent(new WorldEvent()
            {
                effect = new EffectEvent()
                {
                    audio = "CutCrop",
                    volume = random.NextSingle() * 0.1f + 0.7f,
                },
                startAt = World.startTime + random.NextDouble() * 0.1f,
                position = entity.transform.position
            });
            World.HostSyncEntity(source);
        }
    }


    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "matured_crop")]
    public class MaturedCropResolver : EntityScript
    {
        public Random random = new Random();


        public override void Update(Entity entity)
        {

        }

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Harvest && source.character != null)
            {
                CropResolver.CutCrop(World, random, source, entity);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Harvest && source.character != null)
            {
                return true;
            }
            return false;
        }
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "player")]
    public class PlayerResolver : EntityScript
    {
        Random random = new Random();

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.GiveItem)
            {
                if (source.TryTakeInventoryItem(meta.i, out var item1))
                {
                    World.GiveItemWithNotice(entity, item1);
                    World.HostSyncEntity(source);
                }
            }
            else if (interactType == InteractType.MoveItem)
            {
                World.MoveItem(source, entity, meta);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            switch (interactType)
            {
                case InteractType.GiveItem:
                case InteractType.MoveItem:
                    return true;
            }
            return false;
        }

        public override void Interact(Entity source, Vector3 position, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Place)
            {
                //World.CreateEntity(1000, position, Quaternion.CreateFromAxisAngle(Vector3.UnitY, (random.NextSingle() * 16 - 8) / 180.0f * MathF.PI));
            }
            else if (interactType == InteractType.DropItem)
            {
                if (source.TryTakeInventoryItem(meta.i, out var item))
                {
                    var vec = position - source.transform.position;
                    vec.Y = 0;
                    var dir = Vector3.Normalize(vec);
                    World.CreateItem(item, source.transform.position + dir);
                }
            }
            else if (interactType == InteractType.UseItem)
            {
                if (source.character != null && meta.i != null)
                {
                    World.HostUseItem(source, meta.i.Value, position);
                }
            }
        }

        public override bool ClientInteract(Entity source, Vector3 position, InteractType interactType, InteractMetaData meta)
        {
            switch (interactType)
            {
                //case InteractType.Place:
                //    World.EmitEventLocal(new WorldEvent()
                //    {
                //        effect = new EffectEvent()
                //        {
                //            audio = "Dig",
                //            volume = random.NextSingle() * 0.1f + 0.7f,
                //        },
                //        startAt = World.startTime,
                //        position = source.transform.position
                //    });
                //    return true;
                case InteractType.DropItem:
                    return true;
                case InteractType.UseItem:
                    if (source.character != null && meta.i != null)
                        World.ClientUseItem(source, meta.i.Value, position);
                    return true;
            }
            return false;
        }
    }


    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "item")]
    public class ItemResolver : EntityScript
    {
        public Timer timer = new Timer() { interval = 1 };

        bool updateEntity = false;

        public override void FrameBegin()
        {
            updateEntity = timer.AddTime(World.fixedDeltaTime);
        }

        public override void Update(Entity entity)
        {
            if (updateEntity && entity.item != null)
            {
                if (!World.specialItems.ContainsKey(entity.item.typeId))
                {
                    entity.hp -= 0.5f;
                }
                entity.hp -= 0.5f;
                if (entity.hp <= 0)
                {
                    World.DestroyEntity(entity);
                }
            }
        }

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Interact)
            {
                World.GiveItemWithNotice(source, entity.item);
                World.DestroyEntity(entity);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Interact)
            {
                return true;
            }
            return false;
        }
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "fruit_tree")]
    public class FruitTreeResolver : EntityScript
    {
        public Random random = new Random();

        public Timer timer = new Timer() { interval = 1 };

        bool updateEntity = false;


        public override void FrameBegin()
        {
            updateEntity = timer.AddTime(World.fixedDeltaTime);
        }

        public override void Update(Entity entity)
        {
            if (entity.crop == null)
                return;
            var crop = entity.crop;
            if (updateEntity && World.IsDay())
            {
                if (crop.water >= 1)
                {
                    crop.water -= 1;
                    crop.nutrition += 1;
                }
                crop.nutrition += 1;
            }

            if (crop.nutrition >= crop.maxNutrition)
            {
                crop.nutrition = 0;
                if (crop.lootId > 0)
                {
                    int count = crop.productCount + random.Next(crop.randomProductCount + 1);
                    for (int i = 0; i < count; i++)
                    {
                        var itemEntity = World.CreateItem(crop.lootId,
                            entity.transform.position + new Vector3(random.NextSingle() * 2 - 1, 0.3f, random.NextSingle() * 2 - 1));
                    }
                }
            }
        }


        //public void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        //{
        //    if (interactType == InteractType.Harvest && source.character != null && entity.crop != null)
        //    {
        //        //World.GiveItemWithNotice(source, entity.crop.lootId);

        //        //World.EmitEvent(new WorldEvent()
        //        //{
        //        //    effect = new EffectEvent()
        //        //    {
        //        //        audio = "CutCrop",
        //        //        volume = random.NextSingle() * 0.1f + 0.7f,
        //        //    },
        //        //    startAt = World.startTime + random.NextDouble() * 0.1f,
        //        //    position = entity.transform.position
        //        //});


        //        //World.DestroyEntity(entity.id);
        //        //World.HostSyncEntity(source);
        //    }
        //}
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "wild")]
    public class WildResolver : EntityScript
    {
        public override void Update(Entity entity)
        {
            if (entity.crop == null)
                return;
            if (entity.crop.maxLife == 0)
                return;
            if (entity.crop.life > entity.crop.maxLife)
            {
                World.DestroyEntity(entity);
            }
            entity.crop.life += World.fixedDeltaTime;
        }

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Harvest && source.character != null)
            {
                if (entity.crop != null)
                {
                    World.GiveItemWithNotice(source, entity.crop.lootId);
                }

                World.DestroyEntity(entity.id);
                World.EmitEvent(new WorldEvent()
                {
                    effect = new EffectEvent()
                    {
                        audio = "CutCrop",
                        volume = 0.7f,
                    },
                    startAt = World.startTime,
                    position = entity.transform.position
                });
                World.HostSyncEntity(source);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Harvest && source.character != null)
            {
                return true;
            }
            return false;
        }
    }
}