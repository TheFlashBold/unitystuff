using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheFlashBold.Inventory
{

    [CreateAssetMenu(fileName = "Crafting Receipt", menuName = "Inventory/Receipt", order = 2)]
    public class CraftingReceipt : ScriptableObject
    {
        public List<ItemData> Requirements;
        public List<ItemData> Outcome;
        public float CraftingTime;
    }
}