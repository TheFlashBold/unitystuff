using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MarsFPSKit
{
    /// <summary>
    /// An utils class to destroy an object after <see cref="destroyAfter"/>
    /// </summary>
    public class Kit_DestroyTimed : MonoBehaviour
    {
        public float destroyAfter = 5f;

        // Use this for initialization
        void Start()
        {
            //Just destroy after set seconds
            Destroy(gameObject, destroyAfter);
        }
    }
}
