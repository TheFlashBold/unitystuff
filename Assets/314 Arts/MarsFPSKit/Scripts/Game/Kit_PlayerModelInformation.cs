using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This object contains information for a Third Person Player model
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Player Models/New")]
    public class Kit_PlayerModelInformation : ScriptableObject
    {
        public GameObject prefab; //The prefab for this player model
    }
}
