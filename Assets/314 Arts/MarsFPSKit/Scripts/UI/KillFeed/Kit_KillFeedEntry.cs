using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class Kit_KillFeedEntry : MonoBehaviour
    {
        /// <summary>
        /// How long is this entry going to be visible?
        /// </summary>
        public float lifeTime = 4f;
        /// <summary>
        ///  How long is the fade out going to take?
        /// </summary>
        public float fadeOutTime = 1f;
        /// <summary>
        /// When was this entry created?
        /// </summary>
        private float timeOfAppearance;

        /// <summary>
        /// The UI text reference
        /// </summary>
        public Text txt;
        /// <summary>
        /// CanvasGroup for alpha change, so the text in <see cref="txt"/> can be colorful
        /// </summary>
        public CanvasGroup cg;

        public void SetUp(bool botKiller, int killer, bool botKilled, int killed, int gun, Kit_KillFeedManager kfm)
        {
            timeOfAppearance = Time.time;

            string killerName = "";
            string killedName = "";

            //Determine their teams
            int killerTeam = -1;
            int killedTeam = -1;

            if (botKiller)
            {
                if (kfm.main.currentBotManager)
                {
                    Kit_Bot killerBot = kfm.main.currentBotManager.GetBotWithID(killer);
                    if (killerBot != null)
                    {
                        killerName = killerBot.name;
                        killerTeam = killerBot.team;
                    }
                }
            }
            else
            {
                PhotonPlayer playerKiller = null;
                //Get player
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    if (PhotonNetwork.playerList[i].ID == killer)
                    {
                        playerKiller = PhotonNetwork.playerList[i];
                        break;
                    }
                }
                if (playerKiller != null)
                {
                    killerName = playerKiller.NickName;
                    killerTeam = (int)playerKiller.CustomProperties["team"];
                }
            }

            if (botKilled)
            {
                if (kfm.main.currentBotManager)
                {
                    Kit_Bot killedBot = kfm.main.currentBotManager.GetBotWithID(killed);
                    killedName = killedBot.name;
                    killedTeam = killedBot.team;
                }
            }
            else
            {
                PhotonPlayer playerKilled = null;
                //Get player
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    if (PhotonNetwork.playerList[i].ID == killed)
                    {
                        playerKilled = PhotonNetwork.playerList[i];
                        break;
                    }
                }
                if (playerKilled != null)
                {
                    killedName = playerKilled.NickName;
                    killedTeam = (int)playerKilled.CustomProperties["team"];
                }
            }

            //Determine the correct color
            Color killerColor = (killerTeam == 0) ? kfm.team1Color : kfm.team2Color;
            Color killedColor = (killedTeam == 0) ? kfm.team1Color : kfm.team2Color;

            string weaponName = "";

            //If we were killed with a normal weapon, get it from the game information
            if (gun >= 0)
            {
                weaponName = kfm.main.gameInformation.allWeapons[gun].weaponName;
            }
            else
            {
                //Else check what caused it
                if (gun == -1)
                {
                    weaponName = "Environment";
                }
                else if (gun == -2)
                {
                    weaponName = "Fall Damage";
                }
                else if (gun == -3)
                {
                    weaponName = "Suicide";
                }
            }

            //Create string
            txt.text = "<color=#" + ColorUtility.ToHtmlStringRGB(killerColor) + ">" + killerName + "</color>" + " [" + weaponName + "] " + "<color=#" + ColorUtility.ToHtmlStringRGB(killedColor) + ">" + killedName + "</color>";
            enabled = true;
        }

        void Update()
        {
            if (Time.time > timeOfAppearance + lifeTime)
            {
                cg.alpha = Mathf.Clamp01(timeOfAppearance + lifeTime + fadeOutTime - Time.time);

                if (Time.time > timeOfAppearance + lifeTime + fadeOutTime)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                cg.alpha = 1f;
            }
        }
    }
}
