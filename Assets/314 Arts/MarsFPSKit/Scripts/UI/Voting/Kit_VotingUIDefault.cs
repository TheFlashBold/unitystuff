using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class Kit_VotingUIDefault : Kit_VotingUIBase
    {
        public enum MenuState { Category, Player, Map, GameMode }
        private MenuState currentMenuState = MenuState.Category;

        [Header("Vote Start")]
        public GameObject voteMenuRoot;

        public GameObject voteMenuCategorySelection;

        public GameObject voteMenuSelection;
        /// <summary>
        /// The prefab for the selection menu (Players, Maps, Game Modes)
        /// </summary>
        public GameObject voteMenuSelectionPrefab;
        /// <summary>
        /// Where the prefab is going to be parented to
        /// </summary>
        public RectTransform voteMenuSelectionGO;
        /// <summary>
        /// Currently active entries
        /// </summary>
        public List<GameObject> voteMenuSelectionEntries = new List<GameObject>();
        /// <summary>
        /// The back button in the selection list
        /// </summary>
        public GameObject voteMenuSelectionBack;

        /// <summary>
        /// How many seconds need to pass until we can start another votE?
        /// </summary>
        public float votingCooldown = 60f;

        /// <summary>
        /// When have we started a vote for the last time?
        /// </summary>
        private float lastVote;

        [Header("Mid Round Vote")]
        public GameObject mrvRoot;
        /// <summary>
        /// Displays the username who started
        /// </summary>
        public Text voteStartedBy;
        /// <summary>
        /// Displays what is being voted on
        /// </summary>
        public Text voteDescription;
        /// <summary>
        /// Displays our own vote OR the controls
        /// </summary>
        public Text voteOwn;
        /// <summary>
        /// Displays the amount of yes votes
        /// </summary>
        public Text yesVotes;
        /// <summary>
        /// Displays the amount of no votes
        /// </summary>
        public Text noVotes;

        public override void OpenVotingMenu()
        {
            if (main.currentGameModeBehaviour.CanStartVote(main) && PhotonNetwork.playerList.Length > 1)
            {
                if (Time.time > lastVote)
                {
                    //Hide pause menu
                    main.SetPauseMenuState(false, false);
                    //Show menu
                    voteMenuRoot.SetActive(true);
                    //Default state
                    voteMenuCategorySelection.SetActive(true);
                    voteMenuSelection.SetActive(false);
                    //Set state
                    currentMenuState = MenuState.Category;
                }
                else
                {
                    main.DisplayMessage("You need to wait " + (lastVote - Time.time).ToString("F0") + " seconds before you can start another vote!");
                    BackToPauseMenu();
                }
            }
            else
            {
                main.DisplayMessage("A vote can currently not be started!");
                BackToPauseMenu();
            }
        }

        public override void CloseVotingMenu()
        {
            //Hide menu
            voteMenuRoot.SetActive(false);
        }

        public void KickPlayer()
        {
            if (PhotonNetwork.playerList.Length > 1)
            {
                //Clear list
                for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                {
                    Destroy(voteMenuSelectionEntries[i]);
                }
                voteMenuSelectionEntries = new List<GameObject>();

                //Loop through all players and list them
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    //Check if its not us
                    if (PhotonNetwork.playerList[i] != PhotonNetwork.player)
                    {
                        //Instantiate
                        GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                        //Get Entry
                        Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                        int current = i; //This is necessary, otherwise 'i' would change.
                                         //Set name
                        entry.text.text = PhotonNetwork.playerList[i].NickName;
                        //Add delegate
                        entry.btn.onClick.AddListener(delegate { StartVotePlayer(PhotonNetwork.playerList[current]); });

                        //Add to list
                        voteMenuSelectionEntries.Add(go);
                    }
                }

                //Move back button to the lower part
                voteMenuSelectionBack.transform.SetAsLastSibling();

                //Proceed
                voteMenuCategorySelection.SetActive(false);
                voteMenuSelection.SetActive(true);
                //Set menu state
                currentMenuState = MenuState.Player;
            }
            else
            {
                BackToCategory();
                main.DisplayMessage("Only you are in this room!");
            }
        }

        public void ChangeMap()
        {
            if (main.gameInformation.allMaps.Length > 1)
            {
                //Clear list
                for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                {
                    Destroy(voteMenuSelectionEntries[i]);
                }
                voteMenuSelectionEntries = new List<GameObject>();

                int currentMap = main.gameInformation.GetCurrentLevel();

                //Loop through all maps and list them
                for (int i = 0; i < main.gameInformation.allMaps.Length; i++)
                {
                    //Check if its not the current map
                    if (i != currentMap)
                    {
                        //Instantiate
                        GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                        //Get Entry
                        Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                        int current = i; //This is necessary, otherwise 'i' would change.
                                         //Set name
                        entry.text.text = main.gameInformation.allMaps[i].mapName;
                        //Add delegate
                        entry.btn.onClick.AddListener(delegate { StartVoteMap(current); });
                        //Add to list
                        voteMenuSelectionEntries.Add(go);
                    }
                }

                //Move back button to the lower part
                voteMenuSelectionBack.transform.SetAsLastSibling();

                //Proceed
                voteMenuCategorySelection.SetActive(false);
                voteMenuSelection.SetActive(true);
                //Set state
                currentMenuState = MenuState.Map;
            }
            else
            {
                main.DisplayMessage("Only one map is in this game!");
                BackToCategory();
            }
        }

        public void ChangeGameMode()
        {
            if (main.gameInformation.allGameModes.Length > 1)
            {
                //Clear list
                for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                {
                    Destroy(voteMenuSelectionEntries[i]);
                }
                voteMenuSelectionEntries = new List<GameObject>();

                //Loop through all game modes and list them
                for (int i = 0; i < main.gameInformation.allGameModes.Length; i++)
                {
                    //Check if its not the current map
                    if (i != main.currentGameMode)
                    {
                        //Instantiate
                        GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                        //Get Entry
                        Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                        int current = i; //This is necessary, otherwise 'i' would change.
                                         //Set name
                        entry.text.text = main.gameInformation.allGameModes[i].gameModeName;
                        //Add delegate
                        entry.btn.onClick.AddListener(delegate { StartVoteGameMode(current); });

                        //Add to list
                        voteMenuSelectionEntries.Add(go);
                    }
                }

                //Move back button to the lower part
                voteMenuSelectionBack.transform.SetAsLastSibling();

                //Proceed
                voteMenuCategorySelection.SetActive(false);
                voteMenuSelection.SetActive(true);
                //Set state
                currentMenuState = MenuState.GameMode;
            }
            else
            {
                main.DisplayMessage("This game only has one game mode!");
            }
        }

        public void StartVotePlayer(PhotonPlayer player)
        {
            //Send Event
            if (player != null)
            {
                //Set timer
                lastVote = Time.time + votingCooldown;
                //Tell master client we want to start a vote
                byte evCode = 4; //Event 4 = start vote
                //Create a table that holds our vote information
                Hashtable voteInformation = new Hashtable(2);
                //Type
                voteInformation[(byte)0] = (byte)0;
                //ID
                voteInformation[(byte)1] = player.ID;
                if (PhotonNetwork.offlineMode)
                {
                    PhotonNetwork.OnEventCall(evCode, voteInformation, 0);
                }
                else
                {
                    PhotonNetwork.RaiseEvent(evCode, voteInformation, true, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient });
                }
            }

            BackToPauseMenu();
        }

        public void StartVoteMap(int map)
        {
            //Set timer
            lastVote = Time.time + votingCooldown;
            //Tell master client we want to start a vote
            byte evCode = 4; //Event 4 = start vote
                             //Create a table that holds our vote information
            Hashtable voteInformation = new Hashtable(2);
            //Type
            voteInformation[(byte)0] = (byte)1;
            //ID
            voteInformation[(byte)1] = map;
            if (PhotonNetwork.offlineMode)
            {
                PhotonNetwork.OnEventCall(evCode, voteInformation, 0);
            }
            else
            {
                PhotonNetwork.RaiseEvent(evCode, voteInformation, true, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient });
            }
            BackToPauseMenu();
        }

        public void StartVoteGameMode(int gameMode)
        {
            //Set timer
            lastVote = Time.time + votingCooldown;
            //Tell master client we want to start a vote
            byte evCode = 4; //Event 4 = start vote
                             //Create a table that holds our vote information
            Hashtable voteInformation = new Hashtable(2);
            //Type
            voteInformation[(byte)0] = (byte)2;
            //ID
            voteInformation[(byte)1] = gameMode;
            if (PhotonNetwork.offlineMode)
            {
                PhotonNetwork.OnEventCall(evCode, voteInformation, 0);
            }
            else
            {
                PhotonNetwork.RaiseEvent(evCode, voteInformation, true, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient });
            }
            BackToPauseMenu();
        }

        public void BackToCategory()
        {
            //Default state
            voteMenuCategorySelection.SetActive(true);
            voteMenuSelection.SetActive(false);
            //Set state
            currentMenuState = MenuState.Category;
        }

        public void BackToPauseMenu()
        {
            //Shwo pause menu
            main.SetPauseMenuState(true);
            //Hide menu
            voteMenuRoot.SetActive(false);
        }

        public override void RedrawVotingUI(Kit_VotingBase voting)
        {
            if (voting)
            {
                if (!mrvRoot.activeSelf)
                    mrvRoot.SetActive(true);
                if (voteStartedBy && voting.voteStartedBy != null)
                {
                    //Set who started it
                    voteStartedBy.text = voting.voteStartedBy.NickName;
                }
                if (voteDescription)
                {
                    //Update description
                    if (voting.votingOn == Kit_VotingBase.VotingOn.Kick)
                    {
                        PhotonPlayer toKick = PhotonPlayer.Find(voting.argument);
                        if (toKick != null)
                        {
                            voteDescription.text = "Kick player: " + toKick.NickName;
                        }
                        else
                        {
                            voteDescription.text = "Kick player: ";
                        }
                    }
                    else if (voting.votingOn == Kit_VotingBase.VotingOn.Map)
                    {
                        voteDescription.text = "Switch map to: " + main.gameInformation.allMaps[voting.argument].mapName;
                    }
                    else if (voting.votingOn == Kit_VotingBase.VotingOn.GameMode)
                    {
                        voteDescription.text = "Switch Game Mode to: " + main.gameInformation.allGameModes[voting.argument].gameModeName;
                    }
                }
                if (yesVotes)
                {
                    //Yes Votes
                    yesVotes.text = voting.GetYesVotes().ToString();
                }
                if (noVotes)
                {
                    //No votes
                    noVotes.text = voting.GetNoVotes().ToString();
                }
                if (voteOwn)
                {
                    //Own vote
                    if (voting.myVote == -1)
                    {
                        voteOwn.text = "F1 <color=#00ff00ff>YES</color> F2 <color=#ff0000ff>NO</color>";
                    }
                    else if (voting.myVote == 0)
                    {
                        voteOwn.text = "You voted <color=#ff0000ff>NO</color>";
                    }
                    else if (voting.myVote == 1)
                    {
                        voteOwn.text = "You voted <color=#00ff00ff>YES</color>";
                    }
                }
            }
        }

        public override void VoteEnded(Kit_VotingBase voting)
        {
            //Hide the UI
            mrvRoot.SetActive(false);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            if (currentMenuState == MenuState.Player)
            {
                //Redraw
                KickPlayer();
            }
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if (currentMenuState == MenuState.Player)
            {
                if (PhotonNetwork.playerList.Length > 1)
                {
                    //Redraw
                    KickPlayer();
                }
                else
                {
                    BackToCategory();
                }
            }
        }
    }
}