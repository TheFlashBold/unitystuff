using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

#if INTEGRATION_STEAM
using Steamworks;
#endif

namespace MarsFPSKit
{
    //Pause Menu state enum
    public enum PauseMenuState { teamSelection = -1, main = 0 }

    /// <summary>
    /// This class is used to store spawns for a game mode internally
    /// </summary>
    [System.Serializable]
    public class InternalSpawns
    {
        public List<Kit_PlayerSpawn> spawns = new List<Kit_PlayerSpawn>();
    }

    /// <summary>
    /// The Main script of the ingame logic (This is the heart of the game)
    /// </summary>
    /// //It is a PunBehaviour so we can have all the callbacks that we need
    public class Kit_IngameMain : Photon.PunBehaviour, IPunCallbacks
    {
        /// <summary>
        /// The root of all UI
        /// </summary>
        public GameObject ui_root;

        //The current state of the pause menu
        public PauseMenuState pauseMenuState = PauseMenuState.teamSelection;

        //This hols all game information
        #region Game Information
        [Header("Internal Game Information")]
        [Tooltip("This object contains all game information such as Maps, Game Modes and Weapons")]
        public Kit_GameInformation gameInformation;

        public GameObject playerPrefab; //The player prefab that we should use
        #endregion

        [Header("Map Settings")]
        /// <summary>
        /// If you are below this position on your y axis, you die
        /// </summary>
        public float mapDeathThreshold = -50f;

        //This contains all the game mode informations
        #region Game Mode Variables
        [Header("Game Mode Variables")]
        /// <summary>
        /// The game mode timer
        /// </summary>
        public float timer = 600f;
        /// <summary>
        /// A universal stage for game modes, since every game mode requires one like this
        /// </summary>
        public int gameModeStage;
        /// <summary>
        /// Used for the game mode stage changed callback (Called for everyone)
        /// </summary>
        private int lastGameModeStage;
        public int currentGameMode; //The game mode we are currently playing
        /// <summary>
        /// Here you can store runtime data for the game mode. Just make sure to sync it to everybody
        /// </summary>
        public object currentGameModeRuntimeData;

        [HideInInspector]
        public List<InternalSpawns> internalSpawns = new List<InternalSpawns>();
        #endregion

        //This contains everything needed for the team selection
        #region Team Selection
        [Header("Team Selection")]
        public GameObject ts_root;
        [Tooltip("Should we automatically try to spawn after we chose a team?")]
        public bool attemptSpawnAfterTeam = true; //Should we automatically try to spawn after we chose a team?
        /// <summary>
        /// Because this button controls both, changing teams and commiting suicide, we need to adjust its text
        /// </summary>
        public Text ts_changeTeamButtonText;
        /// <summary>
        /// The text which displays that we cannot join that team
        /// </summary>
        public Text ts_cantJoinTeamText;
        /// <summary>
        /// How long is the warning going to be displayed?
        /// </summary>
        public float ts_cantJoinTeamTime = 3f;
        /// <summary>
        /// Current alpha of the cant join message
        /// </summary>
        private float ts_cantJoinAlpha = 0f;
        #endregion

        //This contains everything needed for the Pause Menu
        #region Pause Menu
        [Header("Pause Menu, Use 'B' in the editor to open / close it")]
        public GameObject pm_root; //The root object of the pause menu
        public GameObject pm_main; //The main page of the pause menu
        public GameObject pm_options; //Options page of the pause menu
        /// <summary>
        /// Button for the loadout menu
        /// </summary>
        public GameObject pm_loadoutButton;
        #endregion

        //This contains everything needed for the Scoreboard
        #region Scoreboard
        [Header("Scoreboard")]
        public float sb_pingUpdateRate = 1f; //After how many seconds the ping in our Customproperties should be updated
        private float sb_lastPingUpdate; //When our ping was updated for the last time
        #endregion

        //This contains the local camera control
        #region Camera Control
        [Header("Camera Control")]
        public Camera mainCamera; //The main camera to use for the whole game
        //We recycle the same camera for the whole game, for easy setup of image effects
        //Be careful when changing near and far clip
        public Transform activeCameraTransform
        {
            get
            {
                if (mainCamera)
                {
                    return mainCamera.transform.parent;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                //We use one camera for the complete game
                //Set parent
                mainCamera.transform.parent = value;
                //If the parent is not null, reset position and rotation
                if (value)
                {
                    mainCamera.transform.localPosition = Vector3.zero;
                    mainCamera.transform.localRotation = Quaternion.identity;
                }
            }
        }
        public Transform spawnCameraPosition; //The spawn position for the camera
        #endregion

        [Header("Modules")]
        [Header("HUD")]
        //This contains the HUD reference
        #region HUD
        /// <summary>
        /// Use this to access the Player HUD
        /// </summary>
        public Kit_PlayerHUDBase hud;
        #endregion

        //This contains the Killfeed reference
        #region Killfeed
        [Header("Killfeed")]
        public Kit_KillFeedBase killFeed;
        #endregion

        #region Chat
        [Header("Chat")]
        public Kit_ChatBase chat;
        #endregion

        #region Impact Processor
        [Header("Impact Processor")]
        public Kit_ImpactParticleProcessor impactProcessor;
        #endregion

        #region Scoreboard
        [Header("Scoreboard")]
        public Scoreboard.Kit_ScoreboardBase scoreboard;
        #endregion

        #region PointsUI
        [Header("Points UI")]
        public Kit_PointsUIBase pointsUI;
        #endregion

        #region Victory Screen
        [Header("Victory Screen")]
        public Kit_VictoryScreenUI victoryScreenUI;
        #endregion

        #region MapVoting
        [Header("Map Voting")]
        public Kit_MapVotingUIBase mapVotingUI;
        #endregion

        #region Ping Limit
        [Header("Ping Limit")]
        public Kit_PingLimitBase pingLimitSystem;
        public Kit_PingLimitUIBase pingLimitUI;
        #endregion

        #region AFK Limit
        [Header("AFK Limit")]
        public Kit_AfkLimitBase afkLimitSystem;
        public Kit_AfkLimitUIBase afkLimitUI;
        #endregion

        #region Loadout
        [Header("Loadout")]
        public Kit_LoadoutBase loadoutMenu;
        #endregion

        #region Voting
        [Header("Voting")]
        public Kit_VotingUIBase votingMenu;
        [HideInInspector]
        public Kit_VotingBase currentVoting;
        #endregion

        #region Voice Chat
        [Header("Voice Chat")]
        public Kit_VoiceChatBase voiceChat;
        #endregion

        [Header("Instantiateables")]
        /// <summary>
        /// This contains the prefab for the victory screen. Once its setup it will sync to all other players.
        /// </summary>
        public GameObject victoryScreen;
        [HideInInspector]
        /// <summary>
        /// A reference to the victory screen so it can be destroyed when it's not needed anymore.
        /// </summary>
        public Kit_VictoryScreen currentVictoryScreen;
        /// <summary>
        /// This contains the prefab for the map voting. Once its setup it will sync to all other players
        /// </summary>
        public GameObject mapVoting;
        [HideInInspector]
        /// <summary>
        /// A reference to the map voting. Can be null
        /// </summary>
        public Kit_MapVotingBehaviour currentMapVoting;
        /// <summary>
        /// The prefab for the player initiated voting
        /// </summary>
        public GameObject playerStartedVoting;

        /// <summary>
        /// Prefab for the bot manager
        /// </summary>
        [Header("Bots")]
        public GameObject botManagerPrefab;
        [HideInInspector]
        /// <summary>
        /// If Bots are enabled, this is the bot manager
        /// </summary>
        public Kit_BotManager currentBotManager;
        [HideInInspector]
        /// <summary>
        /// All bot nav points
        /// </summary>
        public Transform[] botNavPoints;


        //This section contains internal variables used by the game
        #region Internal Variables
        [HideInInspector]
        public int assignedTeamID = 2;
        /// <summary>
        /// Our own player, returns null if we have not spawned
        /// </summary>
        [HideInInspector]
        public Kit_PlayerBehaviour myPlayer;
        [HideInInspector]
        public bool isPauseMenuOpen; //Is the pause menu currently opened?
        [HideInInspector]
        public Kit_GameModeBase currentGameModeBehaviour;
        /// <summary>
        /// Instance of current game mode HUD. Could be null.
        /// </summary>
        [HideInInspector]
        public Kit_GameModeHUDBase currentGameModeHUD;
        [HideInInspector]
        /// <summary>
        /// Is the ping limit system enabled by the user?
        /// </summary>
        public bool isPingLimiterEnabled = false;
        [HideInInspector]
        /// <summary>
        /// Is the afk limit system enabled by the user?
        /// </summary>
        public bool isAfkLimiterEnabled = false;
        [HideInInspector]
        /// <summary>
        /// Have we actually begun to play this game mode?
        /// </summary>
        public bool hasGameModeStarted = false;
        #endregion

        #region Unity Calls
        void Awake()
        {
            //Hide HUD initially
            hud.SetVisibility(false);
        }

        void OnEnable()
        {
            PhotonNetwork.OnEventCall += OnPhotonEvent;
        }

        void OnDisable()
        {
            PhotonNetwork.OnEventCall -= OnPhotonEvent;
        }

        void Start()
        {
            //Set initial states
            ts_root.SetActive(false);
            pm_root.SetActive(false);
            pm_main.SetActive(false);
            ui_root.SetActive(true);
            assignedTeamID = 2;

            //Make sure the main camera is child of the spawn camera position
            activeCameraTransform = spawnCameraPosition;

            if (gameInformation)
            {
                //Check if we're connected
                if (PhotonNetwork.inRoom)
                {
                    //Setup Game Mode based on Custom properties
                    int gameMode = (int)PhotonNetwork.room.CustomProperties["gameMode"];
                    gameInformation.allGameModes[gameMode].gameModeLogic.GamemodeSetup(this);
                    currentGameMode = gameMode;
                    currentGameModeBehaviour = gameInformation.allGameModes[gameMode].gameModeLogic;

                    //Check if we already have enough players to start playing
                    if (currentGameModeBehaviour.AreEnoughPlayersThere(this))
                    {
                        hasGameModeStarted = true;
                    }

                    //If we already have a game mode hud, destroy it
                    if (currentGameModeHUD)
                    {
                        Destroy(currentGameModeHUD.gameObject);
                    }

                    //Initialize Loadout Menu
                    if (loadoutMenu)
                    {
                        loadoutMenu.Initialize();
                        //Force it to be closed
                        loadoutMenu.ForceClose();
                    }

                    //Setup HUD
                    if (currentGameModeBehaviour.hudPrefab)
                    {
                        GameObject hudPrefab = Instantiate(currentGameModeBehaviour.hudPrefab, hud.transform, false);
                        //Move to the back
                        hudPrefab.transform.SetAsFirstSibling();
                        //Reset scale
                        hudPrefab.transform.localScale = Vector3.one;
                        //Get script
                        currentGameModeHUD = hudPrefab.GetComponent<Kit_GameModeHUDBase>();
                    }

                    //Is loadout supported?
                    if (currentGameModeBehaviour.LoadoutMenuSupported())
                    {
                        pm_loadoutButton.SetActive(true);
                    }
                    else
                    {
                        pm_loadoutButton.SetActive(false);
                    }

                    //Set timer
                    int duration = (int)PhotonNetwork.room.CustomProperties["duration"];
                    //Assign global game length
                    Kit_GameSettings.gameLength = duration;

                    //Get ping limit
                    int pingLimit = (int)PhotonNetwork.room.CustomProperties["ping"];

                    if (gameInformation.allPingLimits[pingLimit] > 0)
                    {
                        //Ping limit enabled
                        if (pingLimitSystem)
                        {
                            //Tell the system to start
                            pingLimitSystem.StartRelay(this, true, gameInformation.allPingLimits[pingLimit]);
                            isPingLimiterEnabled = true;
                        }
                    }
                    else
                    {
                        //Ping limit disablde
                        if (pingLimitSystem)
                        {
                            //Tell the system to not start
                            pingLimitSystem.StartRelay(this, false);
                            isPingLimiterEnabled = false;
                        }
                    }

                    //Get AFK limit
                    int afkLimit = (int)PhotonNetwork.room.CustomProperties["afk"];

                    if (gameInformation.allAfkLimits[afkLimit] > 0)
                    {
                        //AFK limit enabled
                        if (afkLimitSystem)
                        {
                            //Relay to the system
                            afkLimitSystem.StartRelay(this, true, gameInformation.allAfkLimits[afkLimit]);
                            isAfkLimiterEnabled = true;
                        }
                    }
                    else
                    {
                        //AFK limit disabled
                        if (afkLimitSystem)
                        {
                            afkLimitSystem.StartRelay(this, false);
                            isAfkLimiterEnabled = false;
                        }
                    }

                    //Setup Bots
                    if ((bool)PhotonNetwork.room.CustomProperties["bots"])
                    {
                        //Setup Nav Points
                        Kit_BotNavPoint[] navPoints = FindObjectsOfType<Kit_BotNavPoint>();

                        if (navPoints.Length == 0) throw new System.Exception("[Bots] No Nav Points have been found for this scene! You need to add some.");
                        List<Transform> tempNavPoints = new List<Transform>();
                        for (int i = 0; i < navPoints.Length; i++)
                        {
                            tempNavPoints.Add(navPoints[i].transform);
                        }
                        botNavPoints = tempNavPoints.ToArray();

                        if (PhotonNetwork.isMasterClient)
                        {
                            if (!currentBotManager)
                            {
                                GameObject go = PhotonNetwork.InstantiateSceneObject(botManagerPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
                                currentBotManager = go.GetComponent<Kit_BotManager>();
                                if (currentGameModeBehaviour.botManagerToUse)
                                {
                                    currentGameModeBehaviour.botManagerToUse.Inizialize(currentBotManager);
                                }
                            }
                        }
                        else
                        {
                            if (!currentBotManager)
                            {
                                currentBotManager = FindObjectOfType<Kit_BotManager>();
                            }
                        }
                    }

                    //Set initial Custom properties
                    Hashtable myLocalTable = new Hashtable();
                    //Set inital team
                    //2 = No Team
                    myLocalTable.Add("team", 2);
                    //Set inital stats
                    myLocalTable.Add("kills", 0);
                    myLocalTable.Add("deaths", 0);
                    myLocalTable.Add("assists", 0);
                    myLocalTable.Add("ping", PhotonNetwork.GetPing());
                    myLocalTable.Add("vote", -1); //For Map voting menu AND the player voting
                    //Assign to GameSettings
                    PhotonNetwork.player.SetCustomProperties(myLocalTable);

                    if (voiceChat)
                    {
                        //Setup Voice Chat
                        voiceChat.Setup(this);
                    }

                    if (!currentMapVoting && !currentVictoryScreen)
                    {
                        //Open Team Selection
                        ts_root.SetActive(true);
                        //Set Pause Menu state
                        pauseMenuState = PauseMenuState.teamSelection;
                    }

#if INTEGRATION_STEAM
                    //Set Steam Rich Presence
                    //Set connect
                    //Region@Room Name
                    SteamFriends.SetRichPresence("connect", PhotonNetwork.CloudRegion.ToString() + ":" + PhotonNetwork.room.Name);
                    //Set Status
                    SteamFriends.SetRichPresence("status", "Playing " + gameInformation.allGameModes[gameMode].gameModeName + " on " + gameInformation.allMaps[gameInformation.GetCurrentLevel()].mapName);
#endif

                    //Unlock the cursor
                    MarsScreen.lockCursor = false;
                }
                else
                {
                    //Go back to Main Menu
                    SceneManager.LoadScene(0);
                }
            }
            else
            {
                Debug.LogError("No Game Information assigned. Game will not work.");
            }
        }

        void Update()
        {
            //If we are in a room
            if (PhotonNetwork.inRoom)
            {
                //Host Logic
                if (PhotonNetwork.isMasterClient && hasGameModeStarted)
                {
                    #region Timer
                    //Decrease timer
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                        //Check if the timer has run out
                        if (timer <= 0)
                        {
                            //Call the game mode callback
                            gameInformation.allGameModes[currentGameMode].gameModeLogic.TimeRunOut(this);
                        }
                    }
                    #endregion
                }

                #region Scoreboard ping update
                //Check if we send a new update
                if (Time.time > sb_lastPingUpdate + sb_pingUpdateRate)
                {
                    //Set last update
                    sb_lastPingUpdate = Time.time;
                    //Update hashtable
                    Hashtable table = PhotonNetwork.player.CustomProperties;
                    table["ping"] = PhotonNetwork.GetPing();
                    //Update hashtable
                    PhotonNetwork.player.SetCustomProperties(table);
                }
                #endregion

                #region Pause Menu
                //Check if the pause menu is ready to be opened and closed and if nothing is blocking it
                if (pauseMenuState >= 0 && !currentVictoryScreen && !currentMapVoting && (!loadoutMenu || loadoutMenu && !loadoutMenu.isOpen))
                {
                    if (Input.GetKeyDown(KeyCode.Escape) && Application.platform != RuntimePlatform.WebGLPlayer || Input.GetKeyDown(KeyCode.B) && Application.isEditor || Input.GetKeyDown(KeyCode.M) && Application.platform == RuntimePlatform.WebGLPlayer) //Escape (for non WebGL), B (For the editor), M (For WebGL)
                    {
                        //Change state
                        isPauseMenuOpen = !isPauseMenuOpen;
                        //Set state
                        if (isPauseMenuOpen)
                        {
                            //Enable pause menu
                            pm_root.SetActive(true);
                            //Enable main page
                            pm_main.SetActive(true);
                            //Disable options menu
                            pm_options.SetActive(false);
                            //Unlock cursor
                            MarsScreen.lockCursor = false;
                            //Chat callback
                            chat.PauseMenuOpened();
                        }
                        else
                        {
                            //Disable pause menu
                            pm_root.SetActive(false);
                            //Lock cursor
                            MarsScreen.lockCursor = true;
                            //Chat callback
                            chat.PauseMenuClosed();
                        }
                    }
                }
                #endregion

                #region HUD Update
                if (currentGameModeHUD)
                {
                    //Relay update
                    currentGameModeHUD.HUDUpdate(this);
                }
                #endregion

                #region Game Mode
                if (PhotonNetwork.isMasterClient)
                {
                    if (currentGameModeBehaviour)
                    {
                        currentGameModeBehaviour.GameModeUpdate(this);
                    }
                }

                //Check if the game mode stage has changed
                if (lastGameModeStage != gameModeStage)
                {
                    //Call the callback
                    GameModeStageChanged(lastGameModeStage, gameModeStage);
                    //Set value
                    lastGameModeStage = gameModeStage;
                }
                #endregion

                #region Ping Limiter
                if (isPingLimiterEnabled && pingLimitSystem)
                {
                    pingLimitSystem.UpdateRelay(this);
                }
                #endregion

                #region AFK Limiter
                if (isAfkLimiterEnabled && afkLimitSystem)
                {
                    afkLimitSystem.UpdateRelay(this);
                }
                #endregion

                #region Waiting for Players
                //Check if the game mode should begin
                if (!hasGameModeStarted)
                {
                    if (PhotonNetwork.isMasterClient)
                    {
                        //Check if we now have enough players
                        if (currentGameModeBehaviour.AreEnoughPlayersThere(this))
                        {
                            hasGameModeStarted = true;
                            currentGameModeBehaviour.GameModeBeginMiddle(this);
                        }
                    }
                    //Show waiting on the HUD
                    hud.SetWaitingStatus(true);
                }
                else
                {
                    //Hide waiting on the HUD
                    hud.SetWaitingStatus(false);
                }
                #endregion

                #region Cannot Join Team
                if (ts_cantJoinAlpha > 0)
                {
                    //Decrease
                    ts_cantJoinAlpha -= Time.deltaTime;

                    //Set alpha
                    ts_cantJoinTeamText.color = new Color(ts_cantJoinTeamText.color.r, ts_cantJoinTeamText.color.g, ts_cantJoinTeamText.color.b, ts_cantJoinAlpha);

                    //Enable
                    ts_cantJoinTeamText.enabled = true;
                }
                else
                {
                    //Just disable
                    ts_cantJoinTeamText.enabled = false;
                }
                #endregion

                #region Team - Suicide Button
                if (myPlayer)
                {
                    ts_changeTeamButtonText.text = "Suicide";
                }
                else
                {
                    ts_changeTeamButtonText.text = "Change Team";
                }
                #endregion

                #region FOV
                if (!myPlayer)
                {
                    mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                }
                #endregion
            }
        }
        #endregion

        #region Photon Calls
        public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            //Someone left
            if (PhotonNetwork.player.IsMasterClient)
            {
                //We are the master client, clean up.
                Debug.Log("Clean up after player " + player);
                PhotonNetwork.DestroyPlayerObjects(player);
            }

            if (currentBotManager && currentGameModeBehaviour.botManagerToUse && PhotonNetwork.isMasterClient)
            {
                currentGameModeBehaviour.botManagerToUse.PlayerLeftTeam(currentBotManager);
            }

            //Inform chat
            chat.PlayerLeft(player);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            //Inform chat
            chat.PlayerJoined(newPlayer);
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            //Check if we are the new master client
            if (PhotonNetwork.isMasterClient || newMasterClient == PhotonNetwork.player)
            {
                Debug.Log("We are the new Master Client");
            }

            //Inform chat
            chat.MasterClientSwitched(newMasterClient);
        }

        public override void OnDisconnectedFromPhoton()
        {
            Debug.Log("Disconnected!");
            //We have disconnected from Photon, go to Main Menu
            SceneManager.LoadScene(0);
        }

        public override void OnLeftRoom()
        {
            Debug.Log("Left room!");
            SceneManager.LoadScene(0);
        }

        public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (currentMapVoting)
                {
                    //A player could have changed his vote. Recalculate.
                    currentMapVoting.RecalculateVotes();
                }
            }
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                //Synchronize timer
                stream.SendNext(timer);
                //Synchronize stage
                stream.SendNext(gameModeStage);
                //Synchronize playing stage
                stream.SendNext(hasGameModeStarted);
            }
            else
            {
                //Set timer
                timer = (float)stream.ReceiveNext();
                //Set stage
                gameModeStage = (int)stream.ReceiveNext();
                //Set playing stage
                hasGameModeStarted = (bool)stream.ReceiveNext();
            }
            //Relay to game mode
            if (currentGameModeBehaviour)
            {
                currentGameModeBehaviour.OnPhotonSerializeView(this, stream, info);
            }
        }
        #endregion

        #region Game Logic calls
        /// <summary>
        /// Tries to spawn a player
        /// <para>See also: <seealso cref="Kit_GameModeBase.CanSpawn(Kit_IngameMain, PhotonPlayer)"/></para>
        /// </summary>
        public void Spawn()
        {
            //We can only spawn if we do not have a player currently
            if (!myPlayer)
            {
                //Check if we can currently spawn
                if (!currentGameModeBehaviour.UsesCustomSpawn())
                {
                    if (gameInformation.allGameModes[currentGameMode].gameModeLogic.CanSpawn(this, PhotonNetwork.player))
                    {
                        //Get a spawn
                        Transform spawnLocation = gameInformation.allGameModes[currentGameMode].gameModeLogic.GetSpawn(this, PhotonNetwork.player);
                        if (spawnLocation)
                        {
                            //Create object array for photon use
                            object[] instData = new object[0];
                            //Assign the values
                            //0 = Team
                            //3 = Primary
                            //4 = Secondary
                            //5 = Length of primary attachments
                            //5 + length ... primary attachments
                            //5 + length pa + 1 = Length of secondary attachments
                            //5 + length pa + 2 ... secondary attachments
                            if (loadoutMenu)
                            {
                                //Get the current loadout
                                Loadout curLoadout = loadoutMenu.GetCurrentLoadout();
                                //Calculate length
                                //Base values + Primary Attachments + Secondary Attachments + Values for the length of the attachment arrays
                                int length = 5 + curLoadout.primaryAttachments.Length + curLoadout.secondaryAttachments.Length + 2;
                                instData = new object[length];
                                //Assign team
                                instData[0] = assignedTeamID;
                                //Tell the system its not a bot
                                instData[1] = false;
                                instData[2] = 0;
                                //Assign that
                                instData[3] = curLoadout.primaryWeapon;
                                instData[4] = curLoadout.secondaryWeapon;
                                instData[5] = curLoadout.primaryAttachments.Length;
                                for (int i = 0; i < curLoadout.primaryAttachments.Length; i++)
                                {
                                    instData[6 + i] = curLoadout.primaryAttachments[i];
                                }
                                instData[6 + curLoadout.primaryAttachments.Length] = curLoadout.secondaryAttachments.Length;
                                for (int i = 0; i < curLoadout.secondaryAttachments.Length; i++)
                                {
                                    instData[7 + curLoadout.primaryAttachments.Length + i] = curLoadout.secondaryAttachments[i];
                                }
                            }
                            else
                            {
                                throw new System.Exception("No Loadout menu assigned. This is not allowed.");
                            }
                            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
                            //Copy player
                            myPlayer = go.GetComponent<Kit_PlayerBehaviour>();
                            //Take control using the token
                            myPlayer.TakeControl();
                        }
                    }
                }
                else
                {
                    GameObject player = currentGameModeBehaviour.DoCustomSpawn(this);
                    if (player)
                    {
                        //Copy player
                        myPlayer = player.GetComponent<Kit_PlayerBehaviour>();
                        //Take control using the token
                        myPlayer.TakeControl();
                    }
                }
            }
        }

        private void InternalJoinTeam(int teamID)
        {
            Hashtable table = PhotonNetwork.player.CustomProperties;
            //Update our player's Hashtable
            table["team"] = teamID;
            PhotonNetwork.player.SetCustomProperties(table);
            //Assign local team ID
            assignedTeamID = teamID;
            //Tell all players that we switched teams
            if (PhotonNetwork.offlineMode)
            {
                PhotonNetwork.OnEventCall(5, null, 0);
            }
            else
            {
                PhotonNetwork.RaiseEvent(5, null, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
            }
            //Loadout callback
            if (loadoutMenu)
            {
                loadoutMenu.TeamChanged(assignedTeamID);
            }
            //Voice Chat Callback
            if (voiceChat)
            {
                voiceChat.JoinedTeam(teamID);
            }

            //Should we attempt to spawn?
            if (attemptSpawnAfterTeam)
            {
                //Proceed in the menu
                ts_root.SetActive(false); //Deactivate team selection
                pm_root.SetActive(false); //Keep the pause menu deactivated
                pm_main.SetActive(true); //Activate main page
                pauseMenuState = PauseMenuState.main;
                //Activate scoreboard
                scoreboard.Enable();
                //Try to spawn
                Spawn();
            }
            else
            {
                //Proceed in the menu
                ts_root.SetActive(false); //Deactivate team selection
                pm_root.SetActive(true); //Activate pause menu root
                //Enable main page
                pm_main.SetActive(true);
                //Disable options menu
                pm_options.SetActive(false);
                pauseMenuState = PauseMenuState.main;
                isPauseMenuOpen = true;
                //Activate scoreboard
                scoreboard.Enable();
            }
        }

        private void OnPhotonEvent(byte eventCode, object content, int senderId)
        {
            //Find sender
            PhotonPlayer sender = PhotonPlayer.Find(senderId);  // who sent this?
            //Player was killed
            if (eventCode == 0)
            {
                Hashtable deathInformation = (Hashtable)content;

                bool botShot = (bool)deathInformation[(byte)0];
                int killer = (int)deathInformation[(byte)1];
                bool botKilled = (bool)deathInformation[(byte)2];
                int killed = (int)deathInformation[(byte)3];
                int gun = (int)deathInformation[(byte)4];

                //Update death stat
                if (botKilled)
                {
                    if (PhotonNetwork.isMasterClient && currentBotManager)
                    {
                        Kit_Bot killedBot = currentBotManager.GetBotWithID(killed);
                        killedBot.deaths++;
                    }
                }
                else
                {
                    if (killed == PhotonNetwork.player.ID)
                    {
                        Hashtable myTable = PhotonNetwork.player.CustomProperties;
                        int deaths = (int)myTable["deaths"];
                        deaths++;
                        myTable["deaths"] = deaths;
                        PhotonNetwork.player.SetCustomProperties(myTable);
                    }
                }

                if (botShot)
                {
                    //Check if bot killed himself
                    if (!botKilled || botKilled && killer != killed)
                    {
                        if (PhotonNetwork.isMasterClient && currentBotManager)
                        {
                            Kit_Bot killerBot = currentBotManager.GetBotWithID(killer);
                            killerBot.kills++;

                            if (PhotonNetwork.isMasterClient)
                            {
                                //Call on game mode
                                currentGameModeBehaviour.MasterClientBotScoredKill(this, killerBot);
                            }
                        }
                    }
                }
                else
                {
                    if (killer == PhotonNetwork.player.ID && (botKilled || killed != PhotonNetwork.player.ID))
                    {
                        Hashtable myTable = PhotonNetwork.player.CustomProperties;
                        int kills = (int)myTable["kills"];
                        kills++;
                        myTable["kills"] = kills;
                        PhotonNetwork.player.SetCustomProperties(myTable);
                        //Display points
                        pointsUI.DisplayPoints(gameInformation.pointsPerKill, PointType.Kill);
                        //Call on game mode
                        currentGameModeBehaviour.LocalPlayerScoredKill(this);
                    }
                }

                if (PhotonNetwork.isMasterClient)
                {
                    //Game Mode callback
                    currentGameModeBehaviour.PlayerDied(this, botShot, killer, botKilled, killed);
                }

                //Display in the killfeed
                killFeed.Append(botShot, killer, botKilled, killed, gun);
            }
            //Request chat message
            else if (eventCode == 1)
            {
                Hashtable chatInformation = (Hashtable)content;
                //Get information out of the hashtable
                int type = (int)chatInformation[(byte)0];
                //Message sent from player
                if (type == 0)
                {
                    //Master client only message
                    if (PhotonNetwork.isMasterClient)
                    {
                        string message = (string)chatInformation[(byte)1];
                        int targets = (int)chatInformation[(byte)2];

                        if (!PhotonNetwork.offlineMode)
                        {
                            //Check game mode
                            if (currentGameModeBehaviour.isTeamGameMode && targets == 1)
                            {
                                Hashtable CustomProperties = sender.CustomProperties;
                                //Send the message to this player's team only
                                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                                {
                                    Hashtable CustomPropertiesReceiver = PhotonNetwork.playerList[i].CustomProperties;
                                    //Check if we are in the same team
                                    if (CustomProperties["team"] != null && CustomPropertiesReceiver["team"] != null && (int)CustomProperties["team"] == (int)CustomPropertiesReceiver["team"])
                                    {
                                        Hashtable chatMessage = new Hashtable(3);
                                        chatMessage[(byte)0] = message;
                                        chatMessage[(byte)1] = targets;
                                        chatMessage[(byte)2] = senderId;
                                        //Send it to this player
                                        PhotonNetwork.RaiseEvent(2, chatMessage, true, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.playerList[i].ID } });
                                    }
                                }
                            }
                            else
                            {
                                //Send the message to everyone
                                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                                {
                                    Hashtable chatMessage = new Hashtable(3);
                                    chatMessage[(byte)0] = message;
                                    chatMessage[(byte)1] = 0; //Default to zero, since it is a non team based game mode
                                    chatMessage[(byte)2] = senderId;
                                    //Send it to this player
                                    PhotonNetwork.RaiseEvent(2, chatMessage, true, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.playerList[i].ID } });
                                }
                            }
                        }
                        else
                        {
                            Hashtable chatMessage = new Hashtable(3);
                            chatMessage[(byte)0] = message;
                            chatMessage[(byte)1] = 0; //Default to zero, since it is a non team based game mode
                            chatMessage[(byte)2] = senderId;
                            PhotonNetwork.OnEventCall(2, chatMessage, 0);
                        }
                    }
                }
                //Message sent directly from bot
                else if (type == 1)
                {
                    string botSender = (string)chatInformation[(byte)1];
                    int messageType = (int)chatInformation[(byte)2];

                    if (messageType == 0)
                    {
                        chat.BotJoined(botSender);
                    }
                    else if (messageType == 1)
                    {
                        chat.BotLeft(botSender);
                    }
                }
            }
            //Chat message received
            else if (eventCode == 2)
            {
                Hashtable chatInformation = (Hashtable)content;
                //Get sender
                PhotonPlayer chatSender = PhotonPlayer.Find((int)chatInformation[(byte)2]);
                if (chatSender != null)
                {
                    //This is a final chat message, just display it.
                    chat.DisplayChatMessage(chatSender, (string)chatInformation[(byte)0], (int)chatInformation[(byte)1]);
                }
            }
            //Master Client asks us to reset ourselves.
            else if (eventCode == 3)
            {
                //Reset Stats
                //Set initial Custom properties
                Hashtable myLocalTable = PhotonNetwork.player.CustomProperties;
                //Set inital team
                //2 = No Team
                //Set inital stats
                myLocalTable["kills"] = 0;
                myLocalTable["deaths"] = 0;
                myLocalTable["assists"] = 0;
                myLocalTable["ping"] = PhotonNetwork.GetPing();
                myLocalTable["vote"] = -1; //For Map voting menu
                PhotonNetwork.player.SetCustomProperties(myLocalTable);
                //Kill our player and respawn
                if (myPlayer)
                {
                    PhotonNetwork.Destroy(myPlayer.photonView);
                }
                myPlayer = null;
                //Respawn
                Spawn();
            }
            //Start vote
            else if (eventCode == 4)
            {
                if (playerStartedVoting)
                {
                    //Check if vote can be started
                    if (currentGameModeBehaviour.CanStartVote(this))
                    {
                        //Check if there is not vote in progress
                        if (!currentVoting)
                        {
                            //Get data
                            Hashtable voteInformation = (Hashtable)content;
                            int type = (byte)voteInformation[(byte)0];
                            int id = (int)voteInformation[(byte)1];

                            object[] data = new object[3];
                            data[0] = type; //Which type to vote on
                            data[1] = id; //What to vote on
                            data[2] = sender.ID; //Starter

                            PhotonNetwork.Instantiate(playerStartedVoting.name, transform.position, transform.rotation, 0, data);
                        }
                    }
                }
            }
            //Player joined team
            else if (eventCode == 5)
            {
                if (currentBotManager && currentGameModeBehaviour.botManagerToUse && PhotonNetwork.isMasterClient)
                {
                    currentGameModeBehaviour.botManagerToUse.PlayerJoinedTeam(currentBotManager);
                }
            }
        }

        /// <summary>
        /// Ends the game with the supplied PhotonPlayer as winner
        /// </summary>
        /// <param name="winner">The Winner</param>
        public void EndGame(Kit_Player winner)
        {
            if (PhotonNetwork.isMasterClient)
            {
                object[] data = new object[3];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 0;
                data[1] = winner.isBot;
                data[2] = winner.id;
                PhotonNetwork.InstantiateSceneObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);
            }
        }

        /// <summary>
        /// Ends the game with the supplied team (or 2 for draw) as winner
        /// </summary>
        /// <param name="winner">The winning team. 2 means draw.</param>
        public void EndGame(int winner)
        {
            if (PhotonNetwork.isMasterClient)
            {
                object[] data = new object[2];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 1;
                data[1] = winner;
                PhotonNetwork.InstantiateSceneObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);
            }
        }

        /// <summary>
        /// Ends the game and displays scores for two team
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="scoreTeamOne"></param>
        /// <param name="scoreTeamTwo"></param>
        public void EndGame(int winner, int scoreTeamOne, int scoreTeamTwo)
        {
            if (PhotonNetwork.isMasterClient)
            {
                object[] data = new object[4];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 1;
                data[1] = winner;
                data[2] = scoreTeamOne;
                data[3] = scoreTeamTwo;
                PhotonNetwork.InstantiateSceneObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);
            }
        }

        /// <summary>
        /// Opens the voting menu if we are the master client
        /// </summary>
        public void OpenVotingMenu()
        {
            if (PhotonNetwork.isMasterClient)
            {
                List<MapGameModeCombo> usedCombos = new List<MapGameModeCombo>();

                //Get combos
                while (usedCombos.Count < mapVotingUI.amountOfAvailableVotes)
                {
                    //Get a new combo
                    usedCombos.Add(Kit_MapVotingBehaviour.GetMapGameModeCombo(gameInformation, usedCombos));
                }

                List<int> networkCombos = new List<int>();

                //Turn into an int list
                for (int i = 0; i < usedCombos.Count; i++)
                {
                    networkCombos.Add(usedCombos[i].gameMode);
                    networkCombos.Add(usedCombos[i].map);
                }

                object[] data = new object[mapVotingUI.amountOfAvailableVotes * 2];
                //Copy all combos
                for (int i = 0; i < networkCombos.Count; i++)
                {
                    data[i] = networkCombos[i];
                }

                PhotonNetwork.InstantiateSceneObject(mapVoting.name, Vector3.zero, Quaternion.identity, 0, data);
            }
        }

        /// <summary>
        /// Destroys all Players if we are the master client
        /// </summary>
        public void DeleteAllPlayers()
        {
            if (PhotonNetwork.isMasterClient)
            {
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.playerList[i]);
                }
                
                if (currentBotManager)
                {
                    for (int i = 0; i < currentBotManager.bots.Count; i++)
                    {
                        if (currentBotManager.IsBotAlive(currentBotManager.bots[i]))
                        {
                            PhotonNetwork.Destroy(currentBotManager.GetAliveBot(currentBotManager.bots[i]).photonView);
                        }
                    }
                    currentBotManager.enabled = false;
                }
            }
        }

        /// <summary>
        /// Called when the victory screen opened
        /// </summary>
        public void VictoryScreenOpened()
        {
            //Reset alpha
            ts_cantJoinAlpha = 0f;
            //Force close loadout menu
            if (loadoutMenu)
            {
                loadoutMenu.ForceClose();
            }
        }

        /// <summary>
        /// Called when the map voting screen opened
        /// </summary>
        public void MapVotingOpened()
        {
            ts_cantJoinAlpha = 0f;
            //Force close loadout menu
            if (loadoutMenu)
            {
                loadoutMenu.ForceClose();
            }
        }

        /// <summary>
        /// Switches the map to
        /// </summary>
        /// <param name="to"></param>
        public void SwitchMap(int to)
        {
            if (PhotonNetwork.isMasterClient)
            {
                //Get the hashtable
                Hashtable table = PhotonNetwork.room.CustomProperties;
                //Update table
                table["gameMode"] = currentGameMode;
                table["map"] = to;
                PhotonNetwork.room.SetCustomProperties(table);
                //Load the map
                Kit_SceneSyncer.instance.LoadScene(gameInformation.allMaps[to].sceneName);
            }
        }

        /// <summary>
        /// Switches the game mode to
        /// </summary>
        /// <param name="to"></param>
        public void SwitchGameMode(int to)
        {
            if (PhotonNetwork.isMasterClient)
            {
                //Get active map
                int map = gameInformation.GetCurrentLevel();
                //Get the hashtable
                Hashtable table = PhotonNetwork.room.CustomProperties;
                //Update table
                table["gameMode"] = to;
                table["map"] = map;
                PhotonNetwork.room.SetCustomProperties(table);
                //Load the map
                Kit_SceneSyncer.instance.LoadScene(gameInformation.allMaps[map].sceneName);
            }
        }
        #endregion

        public void DisplayMessage(string msg)
        {
            //Display message
            ts_cantJoinTeamText.text = msg;
            //Set alpha
            ts_cantJoinAlpha = ts_cantJoinTeamTime;
        }

        #region ButtonCalls
        /// <summary>
        /// Attempt to join the team with teamID
        /// </summary>
        /// <param name="teamID"></param>
        public void JoinTeam(int teamID)
        {
            //We can just do this if we are in a room
            if (PhotonNetwork.inRoom)
            {
                //We only allow to change teams if we have not spawned
                if (!myPlayer)
                {
                    //Clamp the team id to the available teams
                    teamID = Mathf.Clamp(teamID, 0, 1);
                    //Check if we can join this team OR if we are already in that team
                    if (gameInformation.allGameModes[currentGameMode].gameModeLogic.CanJoinTeam(this, PhotonNetwork.player, teamID) || teamID == assignedTeamID)
                    {
                        //Join the team
                        InternalJoinTeam(teamID);
                        //Hide message
                        ts_cantJoinAlpha = 0f;
                    }
                    else
                    {
                        //Display message
                        DisplayMessage("Could not join team");
                    }
                }
            }
        }

        public void ChangeTeam()
        {
            //We only allow to change teams if we have not spawned
            if (!myPlayer)
            {
                //Go back in the menu
                ts_root.SetActive(true); //Activate team selection
                pm_root.SetActive(false); //Deactivate pause menu root
                pm_main.SetActive(false); //Deactivate main page
                pauseMenuState = PauseMenuState.teamSelection;
            }
            else
            {
                //Commit suicide
                myPlayer.Suicide();
            }
        }

        /// <summary>
        /// Disconnect from the current room
        /// </summary>
        public void Disconnect()
        {
            //Disconnect
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// Press the resume button. Either locks cursor or tries to spawn
        /// </summary>
        public void ResumeButton()
        {
            //Check if we have spawned
            if (myPlayer)
            {
                //We have, just lock cursor
                //Close pause menu
                isPauseMenuOpen = false;
                pm_root.SetActive(false);
                //Lock Cursor
                MarsScreen.lockCursor = true;
            }
            else
            {
                //We haven't, try to spawn
                Spawn();
            }
        }

        /// <summary>
        /// Opens the loadout menu
        /// </summary>
        public void OpenLoadoutMenu()
        {
            //Check if something is blocking that
            if (!currentVictoryScreen && !currentMapVoting)
            {
                if (loadoutMenu)
                {
                    loadoutMenu.Open();
                }
            }
        }

        /// <summary>
        /// Opens the vote menu if no vote is in progress
        /// </summary>
        public void StartVote()
        {
            if (votingMenu)
            {
                votingMenu.OpenVotingMenu();
            }
        }
        #endregion

        #region Other Calls
        /// <summary>
        /// Opens or closes the pause menu
        /// </summary>
        /// <param name="open"></param>
        public void SetPauseMenuState(bool open, bool canLockCursor = true)
        {
            if (isPauseMenuOpen != open)
            {
                isPauseMenuOpen = open;
                //Set state
                if (isPauseMenuOpen)
                {
                    //Enable pause menu
                    pm_root.SetActive(true);
                    //Enable main page
                    pm_main.SetActive(true);
                    //Disable options menu
                    pm_options.SetActive(false);
                    //Unlock cursor
                    MarsScreen.lockCursor = false;
                    //Chat callback
                    chat.PauseMenuOpened();
                }
                else
                {
                    //Disable pause menu
                    pm_root.SetActive(false);
                    if (canLockCursor)
                    {
                        //Lock cursor
                        MarsScreen.lockCursor = true;
                        //Chat callback
                        chat.PauseMenuClosed();
                    }
                }
            }
        }

        /// <summary>
        /// Resets the player stats. Needs to be called at the end of the game. For everybody.
        /// </summary>
        public static void ResetStats()
        {
            //Set initial Custom properties
            Hashtable myLocalTable = new Hashtable();
            //Set inital team
            //2 = No Team
            myLocalTable.Add("team", 2);
            //Set inital stats
            myLocalTable.Add("kills", 0);
            myLocalTable.Add("deaths", 0);
            myLocalTable.Add("assists", 0);
            myLocalTable.Add("ping", PhotonNetwork.GetPing());
            myLocalTable.Add("vote", -1); //For Map voting menu
                                          //Assign to GameSettings
            PhotonNetwork.player.SetCustomProperties(myLocalTable);
        }

        /// <summary>
        /// Called when the game mode stage changes
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void GameModeStageChanged(int from, int to)
        {
            //If we have gone back to 0 we need to call Start again. It can happen when the same map is played twice in a row since Photon does for some reason not sync the scene.
            if (to == 0 && from != 0)
            {
                Start();
            }
        }
        #endregion
    }
}
