using System.Collections.Generic;

namespace Simulation
{
    public class GameRule
    {
        public List<Entity> entities;
        public List<Item> items;
        public List<SpecialItem> specialItems;
        public List<Ability> abilities;
        public List<Buff> buffs;
        public int defaultItemTypeId;

        //public static GameRule MergeRules(IEnumerable<GameRule> gameRules)
        //{
        //    GameRule merged = new GameRule();

        //    Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        //    Dictionary<int, Item> items = new Dictionary<int, Item>();
        //    Dictionary<int, SpecialItem> specialItems = new Dictionary<int, SpecialItem>();
        //    Dictionary<int, Ability> abilities = new Dictionary<int, Ability>();
        //    Dictionary<int, Buff> buffs = new Dictionary<int, Buff>();
        //    foreach (GameRule rule in gameRules)
        //    {
        //        if (rule.defaultItemTypeId != 0)
        //        {
        //            merged.defaultItemTypeId = rule.defaultItemTypeId;
        //        }
        //        if (rule.entities != null)
        //        {
        //            foreach (Entity entity in rule.entities)
        //            {
        //                entities[entity.typeId] = entity;
        //            }
        //        }
        //        if (rule.items != null)
        //        {
        //            foreach (var item in rule.items)
        //            {
        //                items[item.typeId] = item;
        //            }
        //        }
        //        if (rule.specialItems != null)
        //        {
        //            foreach (var specialItem in rule.specialItems)
        //            {
        //                specialItems[specialItem.typeId] = specialItem;
        //            }
        //        }
        //        if (rule.abilities != null)
        //        {
        //            foreach (var ability in rule.abilities)
        //            {
        //                abilities[ability.typeId] = ability;
        //            }
        //        }
        //        if (rule.buffs != null)
        //        {
        //            foreach (var buff in rule.buffs)
        //            {
        //                buffs[buff.typeId] = buff;
        //            }
        //        }
        //    }
        //    merged.entities = new List<Entity>(entities.Values);
        //    merged.items = new List<Item>(items.Values);
        //    merged.specialItems = new List<SpecialItem>(specialItems.Values);
        //    merged.abilities = new List<Ability>(abilities.Values);
        //    merged.buffs = new List<Buff>(buffs.Values);

        //    return merged;
        //}
    }
}