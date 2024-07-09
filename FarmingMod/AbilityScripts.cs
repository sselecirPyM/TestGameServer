//using Simulation;
//using System;
//using System.Collections.Generic;
//using System.Numerics;
//using System.Text;

//namespace Mod
//{
//    public class TeleportAbility : AbilityScript
//    {
//        public override void Cast(Entity source, Ability ability, InteractMetaData meta)
//        {
//            if (meta == null || meta.vec3 == null)
//                return;
//            World.MoveEntity(source, meta.vec3.Value, source.transform.rotation);
//            World.EmitEvent(new WorldEvent()
//            {
//                teleport = new TeleportEvent()
//                {
//                    entityId = source.id,
//                }
//            });
//        }

//        public override AbilityStatus ClientCast(Entity source, Ability ability)
//        {
//            return AbilityStatus.SelectTarget;
//        }

//        public override AbilityStatus ClientCast(Entity source, Ability ability, Vector3 position)
//        {
//            return AbilityStatus.Cast;
//        }
//    }
//}
