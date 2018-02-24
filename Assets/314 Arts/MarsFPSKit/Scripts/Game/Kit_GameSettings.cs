﻿namespace MarsFPSKit
{
    using ExitGames.Client.Photon;
    /// <summary>
    /// This class holds important game information, e.g. the username
    /// </summary>
    public static class Kit_GameSettings
    {
        public static string userName = "Unassigned"; //Our current username
        public static CloudRegionCode selectedRegion; //The current region that the user has selected

        /// <summary>
        /// Should aiming be hold or toggle?
        /// </summary>
        public static bool isAimingToggle = true;

        /// <summary>
        /// Game mode length, assigned from Kit_IngameMain
        /// </summary>
        public static int gameLength;

        /// <summary>
        /// The normal FoV
        /// </summary>
        public static float baseFov = 60f;

        /// <summary>
        /// Sensitivity for hip
        /// </summary>
        public static float hipSensitivity = 1f;

        /// <summary>
        /// Sensitivity for non fullscreen aiming
        /// </summary>
        public static float aimSensitivity = 0.8f;

        /// <summary>
        /// Sensitivity for fullscreen aiming
        /// </summary>
        public static float fullScreenAimSensitivity = 0.2f;
#if !UNITY_WEBGL
        //VOICE CHAT
        public static VoiceTransmitMode voiceChatTransmitMode;
        //END
#endif
    }
}
