﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFlashBold.Inventory
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

        public ItemType ItemType;

        public float WeightDry;
        public float WeightWet;
    }
}