using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;

namespace Game
{
    #region Health Speeds
    public enum HealthSpeed
    {
        Slow = 0,
        Medium = 1,
        Fast = 2
    }
    #endregion
    #region Health Attribute
    [System.Serializable]
    public struct HealthAttribute
    {
        public float value;
        public float target;
        public float max;
        public float[] steps;

        public HealthSpeed speed;
        public bool UIdone;

        public HealthAttribute Init()
        {
            speed = HealthSpeed.Medium;
            UIdone = false;
            target = value;
            return this;
        }

        public void Set(float value, HealthSpeed speed)
        {
            if(value >= max)
            {
                target = max;
            }
            else
            {
                target = value;
            }
        }

        public delegate void OnValueChanged(float value);
        public OnValueChanged onValueChangedCallback;

        public delegate void OnArrowChanged(int arrow);
        public OnArrowChanged onArrowChangedCallback;

        public void Tick()
        {
            if (value == target)
            {
                if (onValueChangedCallback != null && !UIdone)
                {
                    onValueChangedCallback.Invoke(value);
                    UIdone = true;
                }
                return;
            }

            if ((value + steps[(int)speed]) <= (target - steps[(int)speed])) // Fillup if >= step
            {
                value += steps[(int)speed];
                if (onValueChangedCallback != null)
                {
                    onValueChangedCallback.Invoke(value);
                }
                if (onArrowChangedCallback != null)
                {
                    onArrowChangedCallback.Invoke((int)UIArrows.RisingMedium);
                }
            }
            else if (value + steps[(int)speed] > (target - steps[(int)speed]) && value < target) // Fillup if < step
            {
                value = target;
                if (onValueChangedCallback != null)
                {
                    onValueChangedCallback.Invoke(value);
                }
                if(onArrowChangedCallback != null)
                {
                    onArrowChangedCallback.Invoke((int)UIArrows.None);
                }
            }
            else if(value >= (target + steps[(int)speed])) // Reduce >= step
            { 
                value -= steps[(int)speed];
                if (onValueChangedCallback != null)
                {
                    onValueChangedCallback.Invoke(value);
                }
                if (onArrowChangedCallback != null)
                {
                    onArrowChangedCallback.Invoke((int)UIArrows.FallingMedium);
                }
            }
            else if (value > (target + steps[(int)speed])) // Reduce < step
            {
                value = target;
                if (onValueChangedCallback != null)
                {
                    onValueChangedCallback.Invoke(value);
                }
                if (onArrowChangedCallback != null)
                {
                    onArrowChangedCallback.Invoke((int)UIArrows.FallingMedium);
                }
            }
        }
    }
    #endregion

    public class Health : MonoBehaviour
    {
        #region Singleton
        public static Health instance;
        InventoryUI UI;

        private void Awake()
        {
            instance = this;
            UI = InventoryUI.instance;
        }
        #endregion

        public HealthAttribute temp = new HealthAttribute() { value = 37, max = 40 }.Init();
        public HealthAttribute water = new HealthAttribute() { value = 100, max = 100 }.Init();
        public HealthAttribute food = new HealthAttribute() { value = 100, max = 100 }.Init();
        public HealthAttribute blood = new HealthAttribute() { value = 12000, max = 12000 }.Init();

        private void Update()
        {

        }

        private void FixedUpdate()
        {
            temp.Tick();
            water.Tick();
            food.Tick();
            blood.Tick();
        }

    }

}