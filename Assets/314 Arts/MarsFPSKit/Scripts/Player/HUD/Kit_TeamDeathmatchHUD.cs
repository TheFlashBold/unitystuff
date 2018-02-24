using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This is used for <see cref="Kit_GMB_TeamDeathmatch"/>
    /// </summary>
    public class Kit_TeamDeathmatchHUD : Kit_GameModeHUDBase
    {
        public Text timer;

        public Text teamOnePoints;
        public Text teamTwoPoints;

        private int roundedRestSeconds;
        private int displaySeconds;
        private int displayMinutes;

        public override void HUDUpdate(Kit_IngameMain main)
        {
            if (main.currentGameModeBehaviour.AreEnoughPlayersThere(main) || main.hasGameModeStarted)
            {
                roundedRestSeconds = Mathf.CeilToInt(main.timer);
                displaySeconds = roundedRestSeconds % 60; //Get seconds
                displayMinutes = roundedRestSeconds / 60; //Get minutes
                                                          //Update text
                timer.text = string.Format("{0:00} : {1:00}", displayMinutes, displaySeconds);
                timer.enabled = true;

                //Ensure we are using the correct runtime data
                if (main.currentGameModeRuntimeData != null && main.currentGameModeRuntimeData.GetType() == typeof(TeamDeathmatchRuntimeData))
                {
                    TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;
                    //Update texts
                    teamOnePoints.text = drd.teamOnePoints.ToString();
                    teamTwoPoints.text = drd.teamTwoPoints.ToString();
                }
                else
                {
                    //Reset points
                    teamOnePoints.text = "0";
                    teamTwoPoints.text = "0";
                }
                //Enable points
                teamOnePoints.enabled = true;
                teamTwoPoints.enabled = true;
            }
            else
            {
                //Hide all UI
                timer.enabled = false;
                teamOnePoints.enabled = false;
                teamTwoPoints.enabled = false;
            }
        }
    }
}
