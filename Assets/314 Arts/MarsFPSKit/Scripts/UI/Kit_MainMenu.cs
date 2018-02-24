using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

#if INTEGRATION_STEAM
using Steamworks;
#endif

namespace MarsFPSKit
{
    //Menu State enum
    public enum MenuState { closed = -1, main = 0, host = 2, browse = 3, quit = 4, selectRegion = 5 }

    /// <summary>
    /// This script is responsible for controlling the Main Menu
    /// </summary>
    //It is a PunBehaviour so we can have all the callbacks that we need
    public class Kit_MainMenu : Photon.PunBehaviour
    {
        //This holds the current menu states
        #region Menu States
        [Header("Menu State")]
        public MenuState currentMenuState = MenuState.closed;
        #endregion

        //This hols all game information
        #region Game Information
        [Header("Internal Game Information")]
        [Tooltip("This object contains all game information such as Maps, Game Modes and Weapons")]
        public Kit_GameInformation gameInformation;
        #endregion

        //This section includes all referneces needed for the script to work
        #region References
        [Header("References")]
        public EventSystem ui_EvntSystem;
        public Canvas ui_Canvas;
        public Camera mainCamera;
        #endregion

        //This section includes all different Menu Sections
        #region Menu Sections
        [Header("Menu Sections")]
        public GameObject section_main; //Main menu
        public GameObject section_hostGame; //Hosting Menu
        public GameObject section_browseGames; //Server Browser
        public GameObject section_region; //To choose the region that we want to use
        public GameObject section_quit; //Question before quitting
        #endregion

        //This section includes the Login function
        #region Login
        [Header("Login")]
        public Kit_MenuLogin login_System;

        [HideInInspector]
        public bool isLoggedIn;
        #endregion

        //This section includes everything needed for the Host Menu
        #region Host Menu
        [Header("Host Menu")]
        /// <summary>
        /// The Room Name
        /// </summary>
        public InputField hm_nameField;
        //All Labels
        /// <summary>
        /// Displays the current map
        /// </summary>
        public Text hm_curMapLabel;
        /// <summary>
        /// Displays the current game mode
        /// </summary>
        public Text hm_curGameModeLabel;
        /// <summary>
        /// Displays the current duration limit
        /// </summary>
        public Text hm_curDurationLabel;
        /// <summary>
        /// Displays the current player limit
        /// </summary>
        public Text hm_curPlayerLimitLabel;
        /// <summary>
        /// Displays the current ping limit
        /// </summary>
        public Text hm_curPingLimitLabel;
        /// <summary>
        /// Displays the current afk limit
        /// </summary>
        public Text hm_curAfkLimitLabel;
        /// <summary>
        /// Displays the current bot mode
        /// </summary>
        public Text hm_curBotModeLabel;
        /// <summary>
        /// Displays the current connectivity mode
        /// </summary>
        public Text hm_curOnlineModeLabel;
        //Ints to store the hosting information
        private int hm_currentMap;
        private int hm_currentGameMode;
        private int hm_currentDuration;
        private int hm_currentPlayerLimit;
        private int hm_currentPingLimit;
        private int hm_currentAfkLimit;
        private int hm_currentBotMode;
        private int hm_currentOnlineMode;
        #endregion

        //This section includes everything needed for the Region Menu
        #region Region Menu
        public RectTransform rm_EntriesGo; //The "Content" object of the Scroll view, where rm_EntriesPrefab will be instantiated
        public GameObject rm_EntriesPrefab; //The Region Menu Entry prefab
        #endregion

        //This section includes everything needed for the Server Browser
        #region Server Browser
        [Header("Server Browser")]
        public RectTransform sb_EntriesGo;//The "Content" object of the Scroll view, where sb_EntriesPrefab will be instantiated
        public GameObject sb_EntriesPrefab; //The Server Browser Entry prefab
        private List<GameObject> sb_ActiveEntries = new List<GameObject>(); //Currently active server browser entries - used for cleanup
        #endregion

        #region Photon Friends
        [Header("Photon Friends")]
        /// <summary>
        /// System used for Photon friends. Can also be null
        /// </summary>
        public PhotonFriends.Kit_PhotonFriendsBase photonFriends;
        /// <summary>
        /// UI used for the Photon Friends
        /// </summary>
        public PhotonFriends.Kit_PhotonFriendsUIBase photonFriendsUI;
        #endregion

        //This section includes everything needed for the error message window
        #region Error Message
        [Header("Error Message")]
        /// <summary>
        /// The root object of the error message.
        /// </summary>
        public GameObject em_root;
        /// <summary>
        /// The text object that will hold the error details
        /// </summary>
        public Text em_text;
        /// <summary>
        /// The "ok" button of the Error Mesesage.
        /// </summary>
        public Button em_button;
        #endregion

        #region Loadout
        [Header("Loadout")]
        /// <summary>
        /// Loadout menu
        /// </summary>
        public Kit_LoadoutBase loadout;
        [Range(0, 1)]
        public int loadoutTeamToDisplay;
        #endregion

        //This section includes Debug stuff
        #region Debug
        [Header("Debug")]
        public Text debug_PhotonState;
        #endregion

        //Internal variables
        #region Internal Variables
        private bool reconnectUponDisconnect; //Should we try to reconnect after we disconnected (When changing region for example)
        #endregion

        #region Unity Calls
        void Start()
        {
            PhotonNetwork.sendRate = gameInformation.sendRate;
            PhotonNetwork.sendRateOnSerialize = gameInformation.sendRate;

            //Firstly, reset our custom properties
            Hashtable myLocalTable = new Hashtable();
            //Set inital team
            //2 = No Team
            myLocalTable.Add("team", 2);
            //Set inital stats
            myLocalTable.Add("kills", 0);
            myLocalTable.Add("deaths", 0);
            myLocalTable.Add("assists", 0);
            myLocalTable.Add("ping", 0f);
            myLocalTable.Add("vote", -1); //For Map voting menu
            //Assign to GameSettings
            PhotonNetwork.player.SetCustomProperties(myLocalTable);

            //Start Login
            if (login_System)
            {
                //Assign Delgate
                login_System.OnLoggedIn = LoggedIn;
                //Begin Login
                login_System.BeginLogin();
            }
            else
            {
                Debug.LogError("No Login System assigned");
            }

            //Set initial states
            em_root.SetActive(false);

            //Generate random name for the Host Menu
            hm_nameField.text = "Room(" + Random.Range(1, 1000) + ")";

            //Set Photon Settings
            PhotonNetwork.autoJoinLobby = true;

            //The kit uses a custom scene sync script, so you want to make sure that this is set to false
            PhotonNetwork.automaticallySyncScene = false;

            //Set default region
            Kit_GameSettings.selectedRegion = (CloudRegionCode)PlayerPrefs.GetInt("region", (int)gameInformation.defaultRegion);

            //Setup Region Menu
            for (int i = 0; i < gameInformation.allRegions.Length; i++)
            {
                //Instantiate the rm_EntriesPrefab prefab
                GameObject go = Instantiate(rm_EntriesPrefab, rm_EntriesGo) as GameObject;
                //Set it up
                go.GetComponent<Kit_RegionEntry>().Setup(this, i);
            }

            if (loadout)
            {
                //Setup Loadout menu
                loadout.Initialize();
                //Force it to be closed
                loadout.ForceClose();
                loadout.TeamChanged(loadoutTeamToDisplay);
            }

            //Update Information (To make sure that everything is displayed correctly when the user first views the menus)
            UpdateAllDisplays();

#if INTEGRATION_STEAM
            //Set Steam Rich presence
            //Set connection
            SteamFriends.SetRichPresence("connect", null);
            //Set Menu
            SteamFriends.SetRichPresence("status", "Main Menu");
#endif

            //Unlock cursor
            MarsScreen.lockCursor = false;
        }

        void Update()
        {
            //If assigned
            if (debug_PhotonState)
            {
                //Display Photon Connection  state
                debug_PhotonState.text = PhotonNetwork.connectionState.ToString();
            }
        }
        #endregion

        #region Photon Calls
        //We just created a room
        public override void OnCreatedRoom()
        {
            //Our room is created and ready
            //Lets load the appropriate map
            //Get the hashtable
            Hashtable table = PhotonNetwork.room.CustomProperties;
            //Get the correct map
            int mapToLoad = (int)table["map"];
            //Deactivate all input
            ui_EvntSystem.enabled = false;
            //Load the map
            Kit_SceneSyncer.instance.LoadScene(gameInformation.allMaps[mapToLoad].sceneName);
        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            //We could not create a room
            DisplayErrorMessage("Could not create room");
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            DisplayErrorMessage("Could not join room");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room!");
        }

        public override void OnDisconnectedFromPhoton()
        {
            //Check if we should try to reconnect
            if (reconnectUponDisconnect)
            {
                //Connect
                PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion, gameInformation.gameVersion);
                //Set boolean
                reconnectUponDisconnect = false;
            }
        }

        public override void OnUpdatedFriendList()
        {
            if (photonFriends)
            {
                //Just relay to Photon Friends system
                photonFriends.OnUpdatedFriendList(this);
            }
        }
        #endregion

        #region Main Menu Management
        /// <summary>
        /// Changes the menu state
        /// </summary>
        public void ChangeMenuState(MenuState newState)
        {
            //Disable Everything
            section_main.SetActive(false);
            section_hostGame.SetActive(false);
            section_browseGames.SetActive(false);
            section_region.SetActive(false);
            section_quit.SetActive(false);

            //Activate correct objects
            if (newState == MenuState.main)
            {
                section_main.SetActive(true);
            }
            else if (newState == MenuState.host)
            {
                section_hostGame.SetActive(true);
            }
            else if (newState == MenuState.browse)
            {
                //Refresh List
                RefreshRoomList();
                section_browseGames.SetActive(true);
            }
            else if (newState == MenuState.quit)
            {
                section_quit.SetActive(true);
            }
            else if (newState == MenuState.selectRegion)
            {
                section_region.SetActive(true);
            }

            //Copy state
            currentMenuState = newState;
        }

        /// <summary>
        /// <see cref="ChangeMenuState(MenuState)"/>
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeMenuState(int newState)
        {
            ChangeMenuState((MenuState)newState);
        }

        /// <summary>
        /// Call this whenever you want to make sure that all information displayed is correct
        /// </summary>
        void UpdateAllDisplays()
        {
            #region Host Menu
            //Map
            hm_curMapLabel.text = gameInformation.allMaps[hm_currentMap].mapName;

            //Game Mode
            hm_curGameModeLabel.text = gameInformation.allGameModes[hm_currentGameMode].gameModeName;

            //Duration
            if (gameInformation.allDurations[hm_currentDuration] != 60)
                hm_curDurationLabel.text = (gameInformation.allDurations[hm_currentDuration] / 60).ToString() + " minutes";
            else
                hm_curDurationLabel.text = (gameInformation.allDurations[hm_currentDuration] / 60).ToString() + " minute";

            //Player Limit
            if (gameInformation.allPlayerLimits[hm_currentPlayerLimit] != 1)
                hm_curPlayerLimitLabel.text = gameInformation.allPlayerLimits[hm_currentPlayerLimit].ToString() + " players";
            else
                hm_curPlayerLimitLabel.text = gameInformation.allPlayerLimits[hm_currentPlayerLimit].ToString() + " player";

            //Ping Limit
            if (gameInformation.allPingLimits[hm_currentPingLimit] > 0)
                hm_curPingLimitLabel.text = gameInformation.allPingLimits[hm_currentPingLimit].ToString() + "ms";
            else
                hm_curPingLimitLabel.text = "Disabled";

            //AFK Limit
            if (gameInformation.allAfkLimits[hm_currentAfkLimit] > 0)
            {
                if (gameInformation.allAfkLimits[hm_currentAfkLimit] != 1)
                    hm_curAfkLimitLabel.text = gameInformation.allAfkLimits[hm_currentAfkLimit].ToString() + " seconds";
                else
                    hm_curAfkLimitLabel.text = gameInformation.allAfkLimits[hm_currentAfkLimit].ToString() + " second";
            }
            else
                hm_curAfkLimitLabel.text = "Disabled";

            if (hm_currentOnlineMode == 0)
            {
                hm_curOnlineModeLabel.text = "Online";
            }
            else
            {
                hm_curOnlineModeLabel.text = "Offline";
            }

            if (hm_currentBotMode == 0)
            {
                hm_curBotModeLabel.text = "Disabled";
            }
            else
            {
                hm_curBotModeLabel.text = "Enabled";
            }
            #endregion
        }

        public void Quit()
        {
            Application.Quit();
        }
        #endregion

        #region Login
        void LoggedIn(string userName, string userId)
        {
            //Set Logged in to true
            isLoggedIn = true;
            //Activate Menu
            ChangeMenuState(MenuState.main);
            //Store username
            Kit_GameSettings.userName = userName;
            //Assign username to Photon
            PhotonNetwork.player.NickName = userName;
            PhotonNetwork.player.UserId = userId;

            //Connect to the default region
            if (PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion, gameInformation.gameVersion))
            {

            }
            else
            {
                Debug.LogError("Could not connect to the default region");
            }

            if (photonFriends)
            {
                //Just relay to Photon Friends system
                photonFriends.LoggedIn(this);
            }
        }
        #endregion

        //This section contains all functions for the hosting menu
        #region HostMenu
        /// <summary>
        /// Starts a new Photon Session (Room)
        /// </summary>
        public void StartSession()
        {
            if (hm_currentOnlineMode == 0)
            {
                //Check if we are connected to the Photon Server
                if (PhotonNetwork.connected)
                {
                    //Check if the user entered a name
                    if (!hm_nameField.text.IsNullOrWhiteSpace())
                    {
                        //Create room options
                        RoomOptions options = new RoomOptions();
                        //Assign settings
                        //Player Limit
                        options.MaxPlayers = gameInformation.allPlayerLimits[hm_currentPlayerLimit];
                        //Create a new hashtable
                        options.CustomRoomProperties = new Hashtable();
                        //Map
                        options.CustomRoomProperties.Add("map", hm_currentMap);
                        //Game Mode
                        options.CustomRoomProperties.Add("gameMode", hm_currentGameMode);
                        //Duration
                        options.CustomRoomProperties.Add("duration", hm_currentDuration);
                        //Ping limit
                        options.CustomRoomProperties.Add("ping", hm_currentPingLimit);
                        //AFK limit
                        options.CustomRoomProperties.Add("afk", hm_currentAfkLimit);
                        //Bots
                        options.CustomRoomProperties.Add("bots", hm_currentBotMode == 1);
                        string[] customLobbyProperties = new string[4];
                        customLobbyProperties[0] = "map";
                        customLobbyProperties[1] = "gameMode";
                        customLobbyProperties[2] = "duration";
                        customLobbyProperties[3] = "bots";
                        options.CustomRoomPropertiesForLobby = customLobbyProperties;
                        PhotonNetwork.offlineMode = false;
                        //Try to create a new room
                        if (PhotonNetwork.CreateRoom(hm_nameField.text, options, null))
                        {
                            //TODO Display loading screen
                        }
                    }
                }
                else
                {
                    //Try to connect to the Photon Servers
                    PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion, gameInformation.gameVersion);
                }
            }
            else
            {
                //Check if the user entered a name
                if (!hm_nameField.text.IsNullOrWhiteSpace())
                {
                    PhotonNetwork.Disconnect();
                    PhotonNetwork.offlineMode = true;
                    PhotonNetwork.ConnectToRegion(CloudRegionCode.none, gameInformation.gameVersion);
                    //Create room options
                    RoomOptions options = new RoomOptions();
                    //Assign settings
                    //Player Limit
                    options.MaxPlayers = gameInformation.allPlayerLimits[hm_currentPlayerLimit];
                    //Create a new hashtable
                    options.CustomRoomProperties = new Hashtable();
                    //Map
                    options.CustomRoomProperties.Add("map", hm_currentMap);
                    //Game Mode
                    options.CustomRoomProperties.Add("gameMode", hm_currentGameMode);
                    //Duration
                    options.CustomRoomProperties.Add("duration", hm_currentDuration);
                    //Ping limit
                    options.CustomRoomProperties.Add("ping", hm_currentPingLimit);
                    //AFK limit
                    options.CustomRoomProperties.Add("afk", hm_currentAfkLimit);
                    //Bots
                    options.CustomRoomProperties.Add("bots", hm_currentBotMode == 1);
                    string[] customLobbyProperties = new string[4];
                    customLobbyProperties[0] = "map";
                    customLobbyProperties[1] = "gameMode";
                    customLobbyProperties[2] = "duration";
                    customLobbyProperties[3] = "bots";
                    options.CustomRoomPropertiesForLobby = customLobbyProperties;
                    //Try to create a new room
                    if (PhotonNetwork.CreateRoom(hm_nameField.text, options, null))
                    {
                        //TODO Display loading screen
                    }
                }
            }
        }

        /// <summary>
        /// Selects the next map
        /// </summary>
        public void NextMap()
        {
            //Increase number
            hm_currentMap++;
            //Check if we have that many
            if (hm_currentMap >= gameInformation.allMaps.Length) hm_currentMap = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous Map
        /// </summary>
        public void PreviousMap()
        {
            //Decrease number
            hm_currentMap--;
            //Check if we are below zero
            if (hm_currentMap < 0) hm_currentMap = gameInformation.allMaps.Length - 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next Game Mode
        /// </summary>
        public void NextGameMode()
        {
            //Increase number
            hm_currentGameMode++;
            //Check if we have that many
            if (hm_currentGameMode >= gameInformation.allGameModes.Length) hm_currentGameMode = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous Game Mode
        /// </summary>
        public void PreviousGameMode()
        {
            //Decrease number
            hm_currentGameMode--;
            //Check if we are below zero
            if (hm_currentGameMode < 0) hm_currentGameMode = gameInformation.allGameModes.Length - 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next Duration
        /// </summary>
        public void NextDuration()
        {
            //Increase number
            hm_currentDuration++;
            //Check if we have that many
            if (hm_currentDuration >= gameInformation.allDurations.Length) hm_currentDuration = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous Duration
        /// </summary>
        public void PreviousDuration()
        {
            //Decrease number
            hm_currentDuration--;
            //Check if we are below zero
            if (hm_currentDuration < 0) hm_currentDuration = gameInformation.allDurations.Length - 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next Player Limit
        /// </summary>
        public void NextPlayerLimit()
        {
            //Increase number
            hm_currentPlayerLimit++;
            //Check if we have that many
            if (hm_currentPlayerLimit >= gameInformation.allPlayerLimits.Length) hm_currentPlayerLimit = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous Player Limit
        /// </summary>
        public void PreviousPlayerLimit()
        {
            //Decrease number
            hm_currentPlayerLimit--;
            //Check if we are below zero
            if (hm_currentPlayerLimit < 0) hm_currentPlayerLimit = gameInformation.allPlayerLimits.Length - 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next Ping Limit
        /// </summary>
        public void NextPingLimit()
        {
            //Increase number
            hm_currentPingLimit++;
            //Check if we have that many
            if (hm_currentPingLimit >= gameInformation.allPingLimits.Length) hm_currentPingLimit = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous Ping Limit
        /// </summary>
        public void PreviousPingLimit()
        {
            //Decrease number
            hm_currentPingLimit--;
            //Check if we are below zero
            if (hm_currentPingLimit < 0) hm_currentPingLimit = gameInformation.allPingLimits.Length - 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next Afk Limit
        /// </summary>
        public void NextAFKLimit()
        {
            //Increase number
            hm_currentAfkLimit++;
            //Check if we have that many
            if (hm_currentAfkLimit >= gameInformation.allAfkLimits.Length) hm_currentAfkLimit = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous Afk Limit
        /// </summary>
        public void PreviousAFKLimit()
        {
            //Decrease number
            hm_currentAfkLimit--;
            //Check if we are below zero
            if (hm_currentAfkLimit < 0) hm_currentAfkLimit = gameInformation.allAfkLimits.Length - 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next online mode
        /// </summary>
        public void NextOnlineMode()
        {
            //Increase number
            hm_currentOnlineMode++;
            //Check if we have that many
            if (hm_currentOnlineMode >= 2) hm_currentOnlineMode = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous online mode
        /// </summary>
        public void PreviousOnlineMode()
        {
            //Decrease number
            hm_currentOnlineMode--;
            //Check if we are below zero
            if (hm_currentOnlineMode < 0) hm_currentOnlineMode = 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the next bot mode
        /// </summary>
        public void NextBotMode()
        {
            //Increase number
            hm_currentBotMode++;
            //Check if we have that many
            if (hm_currentBotMode >= 2) hm_currentBotMode = 0; //If not, reset

            //Update display
            UpdateAllDisplays();
        }

        /// <summary>
        /// Selects the previous bot mode
        /// </summary>
        public void PreviousBotMode()
        {
            //Decrease number
            hm_currentBotMode--;
            //Check if we are below zero
            if (hm_currentBotMode < 0) hm_currentBotMode = 1; //If so, set to end of the array

            //Update display
            UpdateAllDisplays();
        }
        #endregion

        //This section contains all functions for the Server Browser
        #region Server Browser
        public void RefreshRoomList()
        {
            //Check if we are connected
            if (PhotonNetwork.connected)
            {
                //Get Room List
                RoomInfo[] rooms = PhotonNetwork.GetRoomList();
                //Clean Up
                for (int i = 0; i < sb_ActiveEntries.Count; i++)
                {
                    //Destroy
                    Destroy(sb_ActiveEntries[i]);
                }
                //Reset list
                sb_ActiveEntries = new List<GameObject>();

                //Instantiate new List
                for (int i = 0; i < rooms.Length; i++)
                {
                    //Instantiate entry
                    GameObject go = Instantiate(sb_EntriesPrefab, sb_EntriesGo) as GameObject;
                    //Set it up
                    go.GetComponent<Kit_ServerBrowserEntry>().Setup(this, rooms[i]);
                    //Add it to our active list so it will get cleaned up next time
                    sb_ActiveEntries.Add(go);
                }
            }
            //If not, try to connect
            else
            {
                PhotonNetwork.ConnectToRegion(Kit_GameSettings.selectedRegion, gameInformation.gameVersion);
            }
        }
        #endregion

        #region Button Calls
        /// <summary>
        /// Changes the Photon region according to <see cref="Kit_GameInformation"/>
        /// <para> See also: <seealso cref="Kit_RegionInformation"/></para>
        /// </summary>
        /// <param name="id"></param>
        public void ChangeRegion(int id)
        {
            //Set reconnect boolean
            reconnectUponDisconnect = true;
            //Copy Region ID
            Kit_GameSettings.selectedRegion = gameInformation.allRegions[id].token;
            //Disconnect
            PhotonNetwork.Disconnect();
            //Go to Main Menu
            ChangeMenuState(MenuState.main);
            //Save
            PlayerPrefs.SetInt("region", (int)gameInformation.allRegions[id].token);
        }

        /// <summary>
        /// Attempts to join a room
        /// <para>See also: <seealso cref="Kit_ServerBrowserEntry"/></para>
        /// </summary>
        /// <param name="room"></param>
        public void JoinRoom(RoomInfo room)
        {
            if (PhotonNetwork.JoinRoom(room.Name))
            {

            }
        }

        /// <summary>
        /// Attempts to join a room
        /// </summary>
        /// <param name="room"></param>
        public void JoinRoom(string room)
        {
            if (PhotonNetwork.JoinRoom(room))
            {

            }
        }

        public void OpenLoadout()
        {
            if (loadout)
            {
                loadout.Open();
            }
        }
        #endregion

        #region Error Message
        public void DisplayErrorMessage(string content)
        {
            //Set text
            em_text.text = content;
            //Show
            em_root.SetActive(true);
            //Select button
            em_button.Select();
        }
        #endregion

        #region Photon Friends
        public void RefreshFriendsList()
        {
            if (photonFriends)
            {
                //Just relay to Photon Friends system
                photonFriends.RefreshFriendsList(this);
            }
        }
        #endregion
    }
}
