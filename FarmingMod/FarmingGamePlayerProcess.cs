using Simulation;
using System.Numerics;

namespace FarmingMod
{
    public static class FarmingGamePlayerProcess
    {
        public static void PlayerJoin(object sender, string playerName)
        {
            GameWorld World = (GameWorld)sender;
            var playerEntity = World.entities.Find(e => e.character != null && e.character.name == playerName);
            if (playerEntity == null)
            {
                int moneyItemId = 20001;
                playerEntity = World.CreateEntity(10000, new Vector3(0, 1, 0), Quaternion.Identity, Vector3.One);

                playerEntity.character.name = playerName;
                //playerEntity.character.money = 10000;
                playerEntity.character.isPlayer = true;
                World.GiveItemWithNotice(playerEntity, moneyItemId, 10000);
            }
        }

        public static void PlayerLeave(object sender, string playerName)
        {
            GameWorld World = (GameWorld)sender;
            var playerEntity = World.entities.Find(e => e.character != null && e.character.name == playerName);
            if (playerEntity != null)
            {
                playerEntity.destroyed = true;
            }
        }
    }
}
