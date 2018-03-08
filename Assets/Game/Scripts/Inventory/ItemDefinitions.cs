using System.Collections;
using System.Collections.Generic;

namespace TheFlashBold.Inventory
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
        Default = 0,
        Weapon = 1,
        Tool = 2,
        Gear = 4,
        Clothing = 8,
        Buildable = 16,
        Interactable = 32,
        Useable = 64
    };

    public enum ItemQuality
    {
        Unique,
        Rare,
        Common,
        Trash
    }
}