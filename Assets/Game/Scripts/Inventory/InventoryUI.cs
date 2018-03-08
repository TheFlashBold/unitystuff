using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;

namespace TheFlashBold.Inventory.UI
{
    #region Declarations
    public enum UIArrows
    {
        None = 0,
        RisingSlow = 1,
        RisingMedium = 2,
        RisingFast = 3,
        FallingSlow = 4,
        FallingMedium = 5,
        FallingFast = 6
    };
    #endregion
    
    #region Status Indicators
    [System.Serializable]
    public struct StatusIndicators
    {
        public Sprite EmptySprite;
        public Sprite[] Arrows;
        public void Init()
        {
            Health Health = Health.instance;
            StatusIndicators SI = this;
            Health.temp.onValueChangedCallback += delegate (float value) {
                SI.SetTemp((int)value);
            };
            Health.temp.onArrowChangedCallback += delegate (int arrow)
            {
                SI.TempArrow.sprite = SI.Arrows[arrow];
            };
            Health.water.onValueChangedCallback += delegate (float value) {
                SI.SetWater((int)value);
            };
            Health.water.onArrowChangedCallback += delegate (int arrow)
            {
                SI.WaterArrow.sprite = SI.Arrows[arrow];
            };
            Health.food.onValueChangedCallback += delegate (float value) {
                SI.SetFood((int)value);
            };
            Health.food.onArrowChangedCallback += delegate (int arrow)
            {
                SI.FoodArrow.sprite = SI.Arrows[arrow];
            };
            Health.blood.onValueChangedCallback += delegate (float value) {
                SI.SetBlood((int)(value/120));
            };
            Health.blood.onArrowChangedCallback += delegate (int arrow)
            {
                SI.BloodArrow.sprite = SI.Arrows[arrow];
            };
            SetBrokenBone(false);
            SetSickness(false);
        }
        #region Temp
        public Image Temp;
        public Image TempArrow;
        public Sprite[] TempSprites;
        public void SetTemp(int amount)
        {
            if (amount <= 100 && amount >= 0)
            {
                Temp.sprite = TempSprites[(int)Mathf.Floor(amount * 6 / 101)];
            }
        }
        #endregion
        #region Water
        public Image Water;
        public Image WaterArrow;
        public Sprite[] WaterSprites;
        public void SetWater(int amount)
        {
            if (amount <= 100 && amount >= 0)
            {
                Water.sprite = WaterSprites[(int)Mathf.Floor(amount * 6 / 101)];
            }
        }
        #endregion
        #region Food
        public Image Food;
        public Image FoodArrow;
        public Sprite[] FoodSprites;
        public void SetFood(int amount)
        {
            if (amount <= 100 && amount >= 0)
            {
                Food.sprite = FoodSprites[(int)Mathf.Floor(amount * 6 / 101)];
            }
        }
        #endregion
        #region Blood
        public Image Blood;
        public Image BloodArrow;
        public Sprite[] BloodSprites;
        public void SetBlood(int amount)
        {
            if (amount <= 100 && amount >= 0)
            {
                Blood.sprite = BloodSprites[(int)Mathf.Floor(amount * 6 / 101)];
            }
        }
        #endregion
        #region Sickness
        public Image Sickness;
        public Sprite SicknessSprite;
        public void SetSickness(bool state)
        {
            if (state)
            {
                Sickness.sprite = SicknessSprite;
            }
            else
            {
                Sickness.sprite = EmptySprite;
            }
        }
        #endregion
        #region BrokenBone
        public Image BrokenBone;
        public Sprite BrokenBoneSprite;
        public void SetBrokenBone(bool state)
        {
            if (state)
            {
                Sickness.sprite = BrokenBoneSprite;
            }
            else
            {
                Sickness.sprite = EmptySprite;
            }
        }
        #endregion
    };
    #endregion

    #region Crafting Indicatiors
    [System.Serializable]
    public struct CraftingIndicators
    {

    }
    #endregion

    public class InventoryUI : MonoBehaviour
    {
        #region Singleton

        public static InventoryUI instance;

        private void Awake()
        {
            instance = this;
        }

        #endregion
                
        public StatusIndicators StatusIndicators;

        public GameObject PageGear;
        public GameObject PageCrafting;

        public GameObject GroundTabLeft;
        public GameObject GroundTabRight;

        public bool isOpen = false;

        private void Start()
        {
            StatusIndicators.Init();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Inventory"))
            {
                if (isOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
        }

        public void UIChangePage(int page)
        {
            switch (page)
            {
                case 0:
                    PageGear.SetActive(true);
                    PageCrafting.SetActive(false);
                    break;
                case 1:
                    PageGear.SetActive(false);
                    PageCrafting.SetActive(true);
                    break;
            }
        }

        public void UIChangeTab(bool side)
        {

        }


        public void Close()
        {
            isOpen = false;
        }

        public void Open()
        {
            isOpen = true;
        }
    }
}