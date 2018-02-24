using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Used for game mode specific HUD elements
    /// </summary>
    public abstract class Kit_GameModeHUDBase : MonoBehaviour
    {
        /// <summary>
        /// Calculate the hud
        /// </summary>
        /// <param name="main"></param>
        public abstract void HUDUpdate(Kit_IngameMain main);
    }
}
