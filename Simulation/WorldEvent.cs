using System.Collections.Generic;
using System.Numerics;

namespace Simulation
{
    public class WorldEvent
    {
        public Vector3 position;
        public double startAt;
        public float duration;
        public DestroyEntityEvent destroy;
        public EffectEvent effect;
        public ShowMenuEvent showMenu;
        public TeleportEvent teleport;
        public MessageEvent message;
        public GainItemEvent gainItem;
    }

    public enum DestroyReason
    {
        None,
        Normal,
        Burned,
        Freezing
    }

    public class DestroyEntityEvent
    {
        public int id;
        public DestroyReason reason;
    }

    public class EffectEvent
    {
        public string audio;
        public string effect;
        public float volume;
    }

    public enum ShowMenuType
    {
        Shop,
        Storage,
        Message,
        Furniture,
        YouWin,
    }

    public class TeleportEvent
    {
        public int entityId;
        public Vector3? position;
        public Quaternion? rotation;
    }

    public class ShowMenuEvent
    {
        public int entityId;
        public int targetId;

        public string title;
        public string description;
        public ShowMenuType type;

        public float range;

        public List<int> meta;
        public List<InteractOption> interactOptions;
    }

    public class InteractOption
    {
        public string title;
        public string description;
        public InteractType interactType;
        public bool disable;
    }

    public class MessageEvent
    {
        public int entityId;
        public string message;
    }

    public class GainItemEvent
    {
        public int entityId;
        public int? itemEntity;
        public int itemType;
        public int itemCount;
    }
}