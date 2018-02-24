using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class Kit_PlayerMarker : MonoBehaviour
    {
        /// <summary>
        /// The root object of this marker that positions it
        /// </summary>
        public RectTransform markerRoot;
        /// <summary>
        /// The text that will display the name
        /// </summary>
        public Text markerText;
        /// <summary>
        /// The sprite that will be displayed for friendlies if they are further away
        /// </summary>
        public Image markerArrow;
        /// <summary>
        /// Is this marker used?
        /// </summary>
        public bool used;
    }
}
