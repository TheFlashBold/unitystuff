using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsFPSKit
{
    /// <summary>
    /// This Object contains the complete game information (Maps, GameModes, Weapons)
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Critical/Game Information")]
    public class Kit_GameInformation : ScriptableObject
    {
        [Tooltip("Make sure to change it when you publish a new udpate")]
        public string gameVersion = "1";
        [Tooltip("Keep it under 500: sendrate x max players")]
        public int sendRate = 40;
        [Tooltip("The default region to join")]
        public CloudRegionCode defaultRegion;
        public Kit_RegionInformation[] allRegions;
        [Tooltip("All Maps that are available")]
        public Kit_MapInformation[] allMaps;
        [Tooltip("All Game Modes that are avaialable")]
        public Kit_GameModeInformation[] allGameModes;
        [Tooltip("All Weapons that are available")]
        public Kit_WeaponInformation[] allWeapons;
        [Tooltip("Third Person Player Models for Team 1")]
        public Kit_PlayerModelInformation[] allTeamOnePlayerModels;
        [Tooltip("Third Person Player Models for Team 21")]
        public Kit_PlayerModelInformation[] allTeamTwoPlayerModels;

        [Tooltip("Player limit")]
        public byte[] allPlayerLimits = new byte[3] { 2, 5, 10 };
        [Tooltip("Game Duration in minutes")]
        public int[] allDurations = new int[3] { 5, 10, 20 };
        [Tooltip("Available Ping limits")]
        public int[] allPingLimits = new int[5] { 0, 50, 100, 200, 300 };
        [Tooltip("Available AFK limits")]
        public int[] allAfkLimits = new int[6] { 0, 60, 120, 180, 240, 300 };

        [Header("Points")]
        /// <summary>
        /// How many points do we receive per kill?
        /// </summary>
        public int pointsPerKill = 100;

        public int GetCurrentLevel()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            for (int i = 0; i < allMaps.Length; i++)
            {
                if (allMaps[i].sceneName == currentScene.name)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
