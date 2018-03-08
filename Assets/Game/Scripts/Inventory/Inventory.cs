using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TheFlashBold.LoginSystem;

namespace TheFlashBold.Inventory
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

        private void Start()
        {
            LoginHandler.instance.HandleEvent("item", (itemData) =>
            {
                Add(JsonUtility.FromJson<ItemData>(itemData.ToString()));
            });
        }

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