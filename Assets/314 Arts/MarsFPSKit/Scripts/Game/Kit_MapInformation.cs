using UnityEngine;
using System.Collections;

namespace MarsFPSKit
{
    /// <summary>
    /// This Object contains the complete game information (Maps, GameModes, Weapons)
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Critical/Map Information")]
    public class Kit_MapInformation : ScriptableObject
    {
        public string mapName = "Assign a map name"; //The name of this map
        public string sceneName = "Scene here"; //The scene of this map
        public Sprite mapPicture;
    }
}
