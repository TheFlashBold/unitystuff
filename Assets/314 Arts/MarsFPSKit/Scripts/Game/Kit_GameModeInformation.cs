using UnityEngine;
using System.Collections;

namespace MarsFPSKit
{
    /// <summary>
    /// This Object contains the complete game information (Maps, GameModes, Weapons)
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Critical/Game Mode Information")]
    public class Kit_GameModeInformation : ScriptableObject
    {
        public string gameModeName;
        public Kit_GameModeBase gameModeLogic;
    }
}
