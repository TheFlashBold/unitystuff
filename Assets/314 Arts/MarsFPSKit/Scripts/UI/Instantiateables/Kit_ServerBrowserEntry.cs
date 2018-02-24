using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This class contains references for the server browser and acts as a sender  
    /// </summary>
    public class Kit_ServerBrowserEntry : MonoBehaviour
    {
        public Text serverName; //The name of this room
        public Text mapName; //The map that is currently played in this room
        public Text gameModeName; //The game mode that is currently played in this room
        public Text players; //How many players are in this room
        public Text ping; //The ping of this room - The cloud

        private Kit_MainMenu mm; //The current Main Menu
        private RoomInfo myRoom;

        /// <summary>
        /// Called from Main Menu to properly set this entry up
        /// </summary>
        public void Setup(Kit_MainMenu curMm, RoomInfo curRoom)
        {
            mm = curMm;
            myRoom = curRoom;

            if (myRoom != null)
            {
                //Set Info
                serverName.text = myRoom.Name;
                //Map
                mapName.text = mm.gameInformation.allMaps[(int)myRoom.CustomProperties["map"]].mapName;
                //Game Mode
                gameModeName.text = mm.gameInformation.allGameModes[(int)myRoom.CustomProperties["gameMode"]].gameModeName;
                bool bots = (bool)myRoom.CustomProperties["bots"];
                if (bots)
                {
                    //Players
                    players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers + " (bots)";
                }
                else
                {
                    //Players
                    players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers;
                }
                //Ping
                ping.text = PhotonNetwork.GetPing().ToString();
            }

            //Reset scale (Otherwise it will be offset)
            transform.localScale = Vector3.one;
        }

        //Called from the button that is on this prefab, to join this room (attempt)
        public void OnClick()
        {
            //Check if this button is ready
            if (mm)
            {
                if (myRoom != null)
                {
                    //Attempt to join
                    mm.JoinRoom(myRoom);
                }
            }
        }
    }
}