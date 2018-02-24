using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This is used for <see cref="Kit_GMB_GunGame"/>
    /// </summary>
    public class Kit_GunGameHUD : Kit_GameModeHUDBase
    {
        public Text timer;

        public Text nextWeaponDisplay;

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

                //Only display next weapon if the player is spawned
                if (main.myPlayer)
                {
                    Kit_GMB_GunGame gameMode = main.currentGameModeBehaviour as Kit_GMB_GunGame;
                    //Get Game Mode data
                    GunGameRuntimeData ggrd = main.currentGameModeRuntimeData as GunGameRuntimeData;
                    if (ggrd != null)
                    {
                        if (ggrd.currentGun + 1 < gameMode.weaponOrders[ggrd.currentGunOrder].weapons.Length)
                        {
                            nextWeaponDisplay.text = "Next Weapon: " + main.gameInformation.allWeapons[gameMode.weaponOrders[ggrd.currentGunOrder].weapons[ggrd.currentGun + 1]].name;
                        }
                        else
                        {
                            nextWeaponDisplay.text = "Final level reached";
                        }
                    }
                    nextWeaponDisplay.enabled = true;
                }
                else
                {
                    nextWeaponDisplay.enabled = false;
                }
            }
            else
            {
                timer.enabled = false;
                //Disable next weapon
                nextWeaponDisplay.enabled = false;
            }
        }
    }
}
