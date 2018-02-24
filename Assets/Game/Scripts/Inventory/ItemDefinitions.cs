using System.Collections;
using System.Collections.Generic;

namespace Game
{

    [System.Serializable]
    public struct SpawnChance
    {
        public float min;
        public float max;
    };

    public enum LootType
    {
        Residential = 1,
        Medical = 2,
        Military = 4
    };

    public enum ItemType
    {
        Weapon,
        Tool,
        Gear,
        Clothing,
        Buildable,
        Interactable,
        Useable
    };

    public enum ItemQuality
    {
        Unique,
        Rare,
        Common,
        Trash
    }
}