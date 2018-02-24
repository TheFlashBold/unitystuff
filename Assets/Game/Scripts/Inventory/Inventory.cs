using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    public class Inventory : MonoBehaviour
    {
        #region Singleton
        public static Inventory instance;

        private void Awake()
        {
            instance = this;
        }
        #endregion

        public delegate void OnItemChanged();
        public OnItemChanged onItemChangedCallback;

        public int space = 20;

        public List<ItemData> items = new List<ItemData>();

        public bool Add(ItemData Item)
        {
            if (items.Count >= space)
            {
                return false;
            }

            items.Add(Item);
            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }
            return true;
        }


        public void Remove(ItemData Item)
        {
            items.Remove(Item);
            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }
        }
    }
}