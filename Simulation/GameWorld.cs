using System;
using System.Collections.Generic;
using System.Numerics;

namespace Simulation
{
    public class GameWorld
    {
        public List<IGameMod> gameMods;
        public GameRule rule;
        public Dictionary<int, Entity> entityTemplates = new Dictionary<int, Entity>();
        public Dictionary<int, SpecialItem> specialItems = new Dictionary<int, SpecialItem>();
        public Dictionary<int, Item> itemTemplates = new Dictionary<int, Item>();
        public Dictionary<int, Ability> abilities = new Dictionary<int, Ability>();
        public Dictionary<int, Buff> buffs = new Dictionary<int, Buff>();

        public List<Entity> entities = new List<Entity>();
        public Dictionary<int, Entity> id2Entity = new Dictionary<int, Entity>();
        public ReadOnlyKDTree3<Entity> fastSearch = new KDTree3<Entity>();
        public List<WorldEvent> worldEvents = new List<WorldEvent>();

        public Dictionary<string, EntityScript> scripts = new Dictionary<string, EntityScript>();
        public Dictionary<string, AbilityScript> abilityScripts = new Dictionary<string, AbilityScript>();
        public Dictionary<string, ItemScript> itemScripts = new Dictionary<string, ItemScript>();

        public Dictionary<int, Entity> needSyncEntities = new Dictionary<int, Entity>();
        public List<WorldEvent> needSyncEvents = new List<WorldEvent>();
        public List<SyncInteractEntity> needSyncInteract = new List<SyncInteractEntity>();
        public List<SyncInteractPosition> needSyncInteract2 = new List<SyncInteractPosition>();
        public List<SyncTransform> needSyncTransform = new List<SyncTransform>();

        public GlobalUpdateScript globalUpdateScript;

        public WorldPersistentData persistentData = new WorldPersistentData();
        public HostPersistentData hostPersistentData = new HostPersistentData();

        public int idCount = 1;

        public double startTime;

        public float fixedDeltaTime = 1 / 30.0f;

        public bool isHost;

        //public event Action<Entity> OnCreated;
        public event Action<Entity> OnUpdated;
        public event Action<Entity> OnDestroyed;
        public event Action<object> OnError;

        public event EventHandler<string> HostOnPlayerJoin;
        public event EventHandler<string> HostOnPlayerLeave;

        public Entity CreateEntity(int id, Vector3 position, Quaternion rotation)
        {
            if (entityTemplates.TryGetValue(id, out var template))
            {
                return CreateEntity(template, position, rotation, null);
            }
            return null;
        }

        public Entity CreateEntity(int id, Vector3 position, Quaternion rotation, Vector3? scale)
        {
            if (entityTemplates.TryGetValue(id, out var template))
            {
                return CreateEntity(template, position, rotation, scale);
            }
            return null;
        }
        public Entity CreateEntity(Entity template, Vector3 position, Quaternion rotation, Vector3? scale)
        {
            var entity = new Entity();
            template.CopyTo(entity);

            entity.transform.position = position;
            entity.transform.rotation = rotation;
            if (scale != null)
                entity.transform.scale = scale.Value;
            entity.createAt = startTime;
            entity.id = idCount++;

            entities.Add(entity);
            id2Entity.Add(entity.id, entity);
            HostSyncEntity(entity);
            return entity;
        }

        public bool TryGetEntity(int id, out Entity entity)
        {
            return id2Entity.TryGetValue(id, out entity);
        }
        public void AddScript(string name, EntityScript script)
        {
            script.World = this;
            scripts[name] = script;
        }
        public void AddScript(string name, AbilityScript script)
        {
            script.World = this;
            abilityScripts[name] = script;
        }
        public void AddScript(string name, ItemScript script)
        {
            script.World = this;
            itemScripts[name] = script;
        }

        public void EmitEvent(WorldEvent worldEvent)
        {
            worldEvents.Add(worldEvent);
            if (isHost)
                needSyncEvents.Add(worldEvent);
        }

        public void EmitEventLocal(WorldEvent worldEvent)
        {
            worldEvents.Add(worldEvent);
        }

        public void DestroyEntity(Entity entity)
        {
            entity.destroyed = true;
        }

        public void DestroyEntity(int entityId)
        {
            if (id2Entity.TryGetValue(entityId, out var entity))
                entity.destroyed = true;
        }

        public void HostUpdate()
        {
            startTime += fixedDeltaTime;
            globalUpdateScript.Update();
            BuildKDTree();
            foreach (var script in scripts.Values)
            {
                try
                {
                    script.FrameBegin();
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e);
                }
            }
            foreach (var script in abilityScripts.Values)
            {
                try
                {
                    script.FrameBegin();
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e);
                }
            }
            foreach (var script in itemScripts.Values)
            {
                try
                {
                    script.FrameBegin();
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e);
                }
            }

            for (int i = 0; i < entities.Count; i++)
            {
                Entity e = entities[i];
                if (e.script != null)
                {
                    try
                    {
                        e.script.Update(e);
                    }
                    catch
                    {

                    }
                }
                if (e.needSync)
                    HostSyncEntity(e);
            }

            entities.RemoveAll(e =>
            {
                if (e.destroyed)
                {
                    if (e.script != null)
                    {
                        try
                        {
                            e.script.OnDestroy(e);
                        }
                        catch (Exception err)
                        {
                            OnError?.Invoke(err);
                        }
                    }
                    OnDestroyed?.Invoke(e);

                    id2Entity.Remove(e.id);
                    HostSyncEntity(e);
                    return true;
                }
                if (e.updateModel)
                {
                    OnUpdated?.Invoke(e);
                    e.updateModel = false;
                }
                return false;
            });
        }

        public void ClientUpdate()
        {
            startTime += fixedDeltaTime;
            globalUpdateScript.ClientUpdate();
            BuildKDTree();

            foreach (Entity e in entities)
            {
                if (e.script != null)
                {
                    try
                    {
                        e.script.ClientUpdate(e);
                    }
                    catch
                    {

                    }
                }
            }

            entities.RemoveAll(e =>
            {
                if (e.destroyed)
                {
                    if (e.script != null)
                    {
                        e.script.ClientOnDestroy(e);
                    }
                    OnDestroyed?.Invoke(e);

                    id2Entity.Remove(e.id);
                    return true;
                }
                if (e.updateModel)
                {
                    OnUpdated?.Invoke(e);
                    e.updateModel = false;
                }
                return false;
            });
        }

        void BuildKDTree()
        {
            var kdTree3 = (KDTree3<Entity>)fastSearch;
            kdTree3.Clear();
            foreach (var entity in entities)
            {
                kdTree3.Add(entity, entity.transform.position);
            }
            kdTree3.Build();
        }

        public void HostInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            try
            {
                meta ??= new InteractMetaData();
                if (!source.destroyed && !entity.destroyed && entity.script != null)
                {
                    entity.script.Interact(source, entity, interactType, meta);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }
        public void HostInteract(Entity source, Vector3 position, InteractType interactType, InteractMetaData meta)
        {
            try
            {
                if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(position.Z))
                    return;
                meta ??= new InteractMetaData();
                if (!source.destroyed && source.script != null)
                {
                    source.script.Interact(source, position, interactType, meta);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }

        public bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta = null)
        {
            try
            {
                meta ??= new InteractMetaData();
                if (entity.script != null &&
                    entity.script.ClientInteract(source, entity, interactType, meta))
                {
                    needSyncInteract.Add(new SyncInteractEntity()
                    {
                        source = source.id,
                        target = entity.id,
                        type = interactType,
                        meta = meta
                    });
                    return true;
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
            return false;
        }

        public bool ClientInteract(Entity source, Vector3 position, InteractType interactType, InteractMetaData meta = null)
        {
            try
            {
                if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(position.Z))
                    return false;
                meta ??= new InteractMetaData();
                if (source.script != null &&
                    source.script.ClientInteract(source, position, interactType, meta))
                {
                    needSyncInteract2.Add(new SyncInteractPosition()
                    {
                        source = source.id,
                        target = position,
                        type = interactType,
                        meta = meta,
                    });
                    return true;
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
            return false;
        }

        public void MorphTo(Entity target, Entity template)
        {
            target.typeId = template.typeId;
            target.crop = template.crop?.Clone();
            target.shop = template.shop?.Clone();
            target.farmingField = template.farmingField?.Clone();
            target.abilityProperty = template.abilityProperty?.Clone();

            target.hp = Math.Min(target.hp / target.maxHp, 1) * template.hp;
            target.maxHp = template.maxHp;
            target.script = template.script;
            target.model = template.model;

            HostSyncEntity(target);
        }

        public void HostSyncEntity(Entity entity)
        {
            entity.updateModel = true;
            needSyncEntities[entity.id] = entity;
        }

        public void SyncInteract(SyncRequest syncRequest)
        {
            if (syncRequest.interacts != null)
                foreach (var interact in syncRequest.interacts)
                {
                    if (id2Entity.TryGetValue(interact.source, out var sourceEntity) && id2Entity.TryGetValue(interact.target, out var targetEntity))
                    {
                        HostInteract(sourceEntity, targetEntity, interact.type, interact.meta);
                    }
                }

            if (syncRequest.interacts2 != null)
                foreach (var interact in syncRequest.interacts2)
                {
                    if (id2Entity.TryGetValue(interact.source, out var sourceEntity))
                    {
                        HostInteract(sourceEntity, interact.target, interact.type, interact.meta);
                    }
                }
            if (syncRequest.transforms != null)
                foreach (var transform in syncRequest.transforms)
                {
                    if (id2Entity.TryGetValue(transform.source, out var entity))
                    {
                        var diff = transform.position - entity.transform.position;
                        float d = diff.Length();

                        if (d > 1e-3)
                        {
                            var n = Vector3.Normalize(diff);
                            entity.transform.position += n * Math.Clamp(d, 0, Math.Max(fixedDeltaTime * entity.speed + 0.1f, 0.1f));
                        }
                        entity.transform.rotation = transform.rotation;
                        HostSyncEntity(entity);
                    }
                }
        }

        public void MoveEntity(Entity entity, Vector3 position, Quaternion rotation)
        {
            if (!isHost)
            {
                needSyncTransform.Add(new SyncTransform() { source = entity.id, position = position, rotation = rotation });
            }
            entity.transform.position = position;
            entity.transform.rotation = rotation;
            entity.updateModel = true;
        }

        public void GiveItem(Entity entity, Item item)
        {
            var character = entity.character;
            var inventory = character?.inventory;
            if (inventory != null && item != null)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    Item item2 = inventory[i];

                    if (item2 != null && item.CanStack(item2))
                    {
                        StackTo(item, item2);

                        if (item.stack <= 0)
                            break;
                    }
                }
                bool added = false;
                if (item.stack > 0)
                {
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        Item item2 = inventory[i];
                        if (item2 == null)
                        {
                            inventory[i] = item;
                            added = true;
                            break;
                        }
                    }
                }
                if (item.stack > 0 && !added)
                {
                    CreateItem(item, entity.transform.position);
                }
                HostSyncEntity(entity);
            }
        }

        public void StackTo(Item item, Item target)
        {
            if (item.typeId != target.typeId)
                return;
            int maxStackCount = int.MaxValue;
            if (specialItems.TryGetValue(item.typeId, out var specialItem))
            {
                maxStackCount = specialItem.maxStackCount;
            }
            if (target != null && item.CanStack(target))
            {
                int t = Math.Min(maxStackCount - target.stack, item.stack);
                item.StackTo(target, t);
            }
        }

        public Entity CreateItem(int itemId, Vector3 position)
        {
            if (itemTemplates.TryGetValue(itemId, out var itemTemplate))
            {
                return CreateItem(itemTemplate.Clone(), position);
            }
            return null;
        }

        public Entity CreateItem(Item item, Vector3 position)
        {
            Entity entity;
            if (specialItems.TryGetValue(item.typeId, out var specialItem) &&
                entityTemplates.TryGetValue(specialItem.entityTypeId, out var entityTemplate))
            {
                entity = CreateEntity(entityTemplate, position, Quaternion.Identity, null);
            }
            else
            {
                entity = CreateEntity(rule.defaultItemTypeId, position, Quaternion.Identity);
            }
            entity.item = item;
            return entity;
        }

        bool TryGetItemScript(Item item, out SpecialItem specialItem, out ItemScript itemScript)
        {
            if (specialItems.TryGetValue(item.typeId, out specialItem) && specialItem.script != null)
            {
                itemScript = specialItem.script;
                return true;
            }
            itemScript = null;
            return false;
        }

        public AbilityStatus ClientUseItem(Entity entity, int index)
        {
            if (entity.character != null && entity.character.TryGetInventoryItem(index, out var item) &&
                TryGetItemScript(item, out var specialItem, out var itemScript))
            {
                var status = itemScript.ClientInteract(entity, index, specialItem);
                return status;
            }
            return AbilityStatus.None;
        }

        public AbilityStatus ClientUseItem(Entity entity, int index, Vector3 position)
        {
            if (entity.character != null && entity.character.TryGetInventoryItem(index, out var item) &&
                TryGetItemScript(item, out var specialItem, out var itemScript))
            {
                var status = itemScript.ClientInteract(entity, index, specialItem, position);
                return status;
            }
            return AbilityStatus.None;
        }

        public bool HostUseItem(Entity entity, int index, Vector3 position)
        {
            if (entity.character != null && entity.character.TryGetInventoryItem(index, out var item)
                && TryGetItemScript(item, out var specialItem, out var itemScript))
            {
                return itemScript.Interact(entity, index, specialItem, position);
            }
            return false;
        }

        public void PlayerJoin(string name)
        {
            HostOnPlayerJoin?.Invoke(this, name);
        }

        public void PlayerLeave(string name)
        {
            HostOnPlayerLeave?.Invoke(this, name);
        }

        public void ApplyRule(GameRule rule)
        {
            this.rule = rule;
            foreach (var e in rule.entities)
            {
                entityTemplates[e.typeId] = e;
            }
            if (rule.items != null)
            {
                foreach (var item in rule.items)
                {
                    itemTemplates[item.typeId] = item;
                }
            }
            if (rule.specialItems != null)
            {
                foreach (var item in rule.specialItems)
                {
                    specialItems[item.typeId] = item;
                }
            }
            if (rule.abilities != null)
            {
                foreach (var ability in rule.abilities)
                {
                    abilities[ability.typeId] = ability;
                }
            }
            if (rule.buffs != null)
            {
                foreach (var buff in rule.buffs)
                {
                    buffs[buff.typeId] = buff;
                }
            }
        }

        public bool IsDay()
        {
            return persistentData.timeOfDay > 6 * 60 && persistentData.timeOfDay < 18 * 60;
        }
    }
}