using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Numerics;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "shop")]
    public class ShopResolver : EntityScript
    {
        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Sell)
            {
                SellItem(source, meta);
            }
            else if (interactType == InteractType.Buy)
            {
                BuyItem(source, entity, meta);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.Interact)
            {
                if (entity.shop != null)
                {
                    World.ShowMenuLocal(source, entity, ShowMenuType.Shop, entity.shop.range);
                }
            }
            else if (interactType == InteractType.Sell)
            {
                return true;
            }
            else if (interactType == InteractType.Buy)
            {
                return true;
            }
            return false;
        }

        void SellItem(Entity source, InteractMetaData meta)
        {
            if (source.TryTakeInventoryItem(meta.i, out var item))
            {
                int moneyGain = (int)(item.cost * 0.5f);
                if (moneyGain > 0)
                {
                    World.GiveItemWithNotice(source, 20001, moneyGain);
                }

                World.EmitEvent(new WorldEvent()
                {
                    effect = new EffectEvent()
                    {
                        audio = "Sell",
                        volume = 0.7f,
                    },
                    startAt = World.startTime,
                    position = source.transform.position
                });
                World.HostSyncEntity(source);
            }
        }

        void BuyItem(Entity source, Entity shop, InteractMetaData meta)
        {
            if (shop.shop == null || meta.i == null)
                return;

            if (source.character?.inventory == null)
                return;
            int moneyItemId = 20001;
            if (World.itemTemplates.TryGetValue(shop.shop.GetShopItemId(meta.i.Value), out var template) && source.ItemCount(moneyItemId) >= template.cost)
            {
                //source.character.money -= template.cost;
                source.RemoveItems(moneyItemId, template.cost);
                World.GiveItemWithNotice(source, template.Clone());
            }
        }
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "storage")]
    public class StorageResolver : EntityScript
    {
        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.MoveItem)
            {
                World.MoveItem(source, entity, meta);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            switch (interactType)
            {
                case InteractType.Interact:
                    World.ShowMenuLocal(source, entity, ShowMenuType.Storage, 5);
                    return false;
                case InteractType.MoveItem:
                    return true;
            }
            return false;
        }
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "sweeper")]
    public class SweeperResolver : EntityScript
    {
        public Timer timer = new Timer() { interval = 2 };

        bool trigger;

        public override void FrameBegin()
        {
            trigger = timer.AddTime(World.fixedDeltaTime);
        }

        public override void Update(Entity entity)
        {
            if (trigger)
            {
                var inventory = entity.character.inventory;
                World.fastSearch.Nearest(entity.transform.position, 4, (e) =>
                {
                    if (e.item != null && !e.destroyed)
                    {
                        return inventory.Any(a => a == null || e.item.CanStack(a));
                    };
                    return false;
                }, (e) =>
                {
                    World.GiveItem(entity, e.item);
                    World.DestroyEntity(e);
                });
            }
        }

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.MoveItem)
            {
                World.MoveItem(source, entity, meta);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            switch (interactType)
            {
                case InteractType.Interact:
                    World.ShowMenuLocal(source, entity, ShowMenuType.Storage, 5);
                    return false;
                case InteractType.MoveItem:
                    return true;
            }
            return false;
        }
    }


    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "random_dispenser")]
    public class RandomdDispenserResolver : EntityScript
    {
        public Timer timer = new Timer() { interval = 2 };
        public Random random = new Random();

        bool trigger;

        float speed = 4.0f;

        public override void FrameBegin()
        {
            trigger = timer.AddTime(World.fixedDeltaTime);
        }

        public override void Update(Entity entity)
        {
            var abilityProperty = entity.abilityProperty;
            if (trigger && abilityProperty != null)
            {
                //var inventory = entity.character.inventory;
                //float angle = (random.NextSingle() - 0.5f) * abilityProperty.angle1;
                float x = (random.NextSingle() - 0.5f) * abilityProperty.range1;
                float y = random.NextSingle() * abilityProperty.range2;
                Vector3 target = Vector3.Transform(new Vector3(x, 0, y), entity.transform.rotation);

                //Vector3 position = new Vector3((random.NextSingle() - 0.5f), 0, (random.NextSingle() - 0.5f)) * 5 + entity.transform.position;
                Vector3 position = target + entity.transform.position;

                World.HostUseItem(entity, 0, position);
            }

            if (entity.transform.position.Y > 0.2f)
            {
                entity.transform.position.Y -= speed * World.fixedDeltaTime;
                World.HostSyncEntity(entity);
            }
            if (entity.transform.position.Y < 0.0f)
            {
                entity.transform.position.Y += speed * World.fixedDeltaTime;
                World.HostSyncEntity(entity);
            }
        }

        public override void ClientUpdate(Entity entity)
        {
            if (entity.transform.position.Y > 0.2f)
            {
                entity.transform.position.Y -= speed * World.fixedDeltaTime;
            }
            if (entity.transform.position.Y < 0.0f)
            {
                entity.transform.position.Y += speed * World.fixedDeltaTime;
            }
        }

        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            if (interactType == InteractType.MoveItem)
            {
                World.MoveItem(source, entity, meta);
            }
        }

        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            switch (interactType)
            {
                case InteractType.Interact:
                    World.ShowMenuLocal(source, entity, ShowMenuType.Storage, 5);
                    return false;
                case InteractType.MoveItem:
                    return true;
            }
            return false;
        }
    }
}
