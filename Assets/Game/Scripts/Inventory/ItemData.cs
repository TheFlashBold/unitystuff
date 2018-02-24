using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    [CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item", order = 1)]
    [System.Serializable]
    public class ItemData : ScriptableObject
    {
        public string Title;
        public string Description;

        public Sprite Icon;

        public GameObject Prefab;

        public LootType LootType;
        public SpawnChance SpawnChance;

        public float WeightDry;
        public float WeightWet;
    }
}