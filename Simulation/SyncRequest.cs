using System.Collections.Generic;
using System.Numerics;

namespace Simulation
{
    public class SyncRequest
    {
        public List<SyncInteractEntity> interacts;
        public List<SyncInteractPosition> interacts2;
        public List<SyncTransform> transforms;

        [MessagePack.IgnoreMember]
        public string playerName;
    }

    public class SyncInteractEntity
    {
        public int source;
        public int target;
        public InteractType type;
        public InteractMetaData meta;
    }

    public class SyncInteractPosition
    {
        public int source;
        public Vector3 target;
        public InteractType type;
        public InteractMetaData meta;
    }

    public class InteractMetaData
    {
        public string text;
        public Vector3? vec3;
        public int? i;
        public int? i2;
        public float? f;
        public float? f2;
    }

    public class SyncTransform
    {
        public int source;
        public Vector3 position;
        public Quaternion rotation;
    }
}