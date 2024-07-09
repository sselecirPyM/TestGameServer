using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "arrow")]
    public class ArrowResolver : EntityScript
    {
        public override void Update(Entity entity)
        {
            var projectile = entity.projectile;
            ref var transform = ref entity.transform;
            Vector3 delta = Vector3.Transform(Vector3.UnitZ, transform.rotation) * projectile.speed * World.fixedDeltaTime;
            transform.position += delta;
            //World.MoveEntity(entity, transform.position + Vector3.Transform(Vector3.UnitZ, transform.rotation) * entity.speed * World.fixedDeltaTime, transform.rotation);
            entity.hp -= World.fixedDeltaTime;
            if (entity.hp <= 0)
            {
                World.DestroyEntity(entity);
            }

            World.fastSearch.ForRange(transform.position, 2, (e) =>
            {
                if (e.flags.HasFlag(EntityFlags.Wild) && entity.projectile.hits.Add(e.id))
                {
                    World.MoveEntity(e, e.transform.position + delta * 0.2f, e.transform.rotation);
                    World.HostSyncEntity(e);
                }
            });
        }

        public override void ClientUpdate(Entity entity)
        {
            var projectile = entity.projectile;
            ref var transform = ref entity.transform;
            transform.position += Vector3.Transform(Vector3.UnitZ, transform.rotation) * projectile.speed * World.fixedDeltaTime;
            entity.updateModel = true;
        }
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "seed_projectile")]
    public class SeedProjectileResolver : EntityScript
    {
        public Random random = new Random();
        public override void Update(Entity entity)
        {
            var projectile = entity.projectile;
            ref var transform = ref entity.transform;
            float moveLength = projectile.speed * World.fixedDeltaTime;
            transform.position += Vector3.Transform(Vector3.UnitZ, transform.rotation) * moveLength;


            entity.hp -= World.fixedDeltaTime;
            if (entity.hp <= 0)
            {
                World.DestroyEntity(entity);
            }

            if (Vector3.Distance(projectile.target, entity.transform.position) < 0.5f + moveLength)
            {
                World.CreateEntity(projectile.load, projectile.target, Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)random.NextDouble() * MathF.PI * 2));
                World.DestroyEntity(entity);

            }
        }

        public override void ClientUpdate(Entity entity)
        {
            var projectile = entity.projectile;
            ref var transform = ref entity.transform;
            transform.position += Vector3.Transform(Vector3.UnitZ, transform.rotation) * projectile.speed * World.fixedDeltaTime;
            entity.updateModel = true;
        }
    }

    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "just_place")]
    public class JustPlaceResolver : EntityScript
    {
        public override void Update(Entity entity)
        {
            var projectile = entity.projectile;
            ref var transform = ref entity.transform;
            float moveLength = projectile.speed * World.fixedDeltaTime;
            transform.position += Vector3.Normalize(projectile.target - transform.position) * moveLength;


            if (Vector3.Distance(projectile.target, entity.transform.position) < 0.5f + moveLength)
            {
                entity.script = World.entityTemplates[entity.typeId].script;
                entity.transform.position = projectile.target;
                entity.projectile = null;
                World.HostSyncEntity(entity);

            }
        }

        public override void ClientUpdate(Entity entity)
        {
            var projectile = entity.projectile;
            ref var transform = ref entity.transform;
            Vector3 moveForward = projectile.target - transform.position;
            float moveLength = MathF.Min(projectile.speed * World.fixedDeltaTime, moveForward.Length());
            transform.position += Vector3.Normalize(moveForward) * moveLength;
            entity.updateModel = true;
        }
    }
}
