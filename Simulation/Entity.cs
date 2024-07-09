using System;
using System.Numerics;

namespace Simulation
{
    [Flags]
    public enum EntityFlags
    {
        None = 0,
        Crop = 1,
        Wild = 2,
        Grown = 4,
    }
    public struct Transform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public override bool Equals(object obj)
        {
            return obj is Transform transform &&
                   position.Equals(transform.position) &&
                   rotation.Equals(transform.rotation) &&
                   scale.Equals(transform.scale);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, rotation, scale);
        }

        public static bool operator ==(Transform left, Transform right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Transform left, Transform right)
        {
            return !(left == right);
        }
    }

    public class Entity
    {
        public int id;
        public int typeId;
        public EntityScript script;
        public string name;

        public double createAt;
        public Transform transform = new Transform()
        {
            rotation = Quaternion.Identity,
            scale = Vector3.One
        };
        public Vector3 velocity;
        public float speed;

        public float hp;
        public float maxHp;

        public int customValue;
        public float radius;

        public EntityFlags flags;

        public Item item;
        public Crop crop;
        public Character character;
        public Shop shop;
        public Projectile projectile;
        public FarmingField farmingField;

        public AbilityProperty abilityProperty;
        public EntityStatistics statistics;

        public bool dead;
        public bool destroyed;

        public string model;
        [NonSerialized]
        [MessagePack.IgnoreMember]
        public bool updateModel;
        [NonSerialized]
        [MessagePack.IgnoreMember]
        public bool needSync;

        public void CopyTo(Entity other)
        {
            other.id = id;
            other.typeId = typeId;
            other.script = script;
            other.name = name;
            other.createAt = createAt;
            other.transform = transform;
            other.velocity = velocity;
            other.speed = speed;
            other.hp = hp;
            other.maxHp = maxHp;

            other.customValue = customValue;
            other.radius = radius;
            other.flags = flags;
            other.dead = dead;
            other.destroyed = destroyed;
            other.model = model;
            if (other.character != null && other.character.isLocalPlayer)
            {
                other.character = character.Clone();
                other.character.isLocalPlayer = true;
            }
            else
            {
                other.character = character?.Clone();
            }
            other.item = item?.Clone();
            other.crop = crop?.Clone();
            other.shop = shop?.Clone();
            other.projectile = projectile?.Clone();
            other.farmingField = farmingField?.Clone();
            other.abilityProperty = abilityProperty?.Clone();
            other.updateModel = true;
        }
    }
}