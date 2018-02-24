using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

namespace MarsFPSKit
{
    //Runtime data class
    public class TeamDeathmatchRuntimeData
    {
        /// <summary>
        /// Points scored by team 1
        /// </summary>
        public int teamOnePoints;
        /// <summary>
        /// Points scored by team 2
        /// </summary>
        public int teamTwoPoints;
    }

    [CreateAssetMenu(menuName = ("MarsFPSKit/Gamemodes/Team Deathmatch Logic"))]
    public class Kit_GMB_TeamDeathmatch : Kit_GameModeBase
    {
        /// <summary>
        /// How many kills does a team need to win the match?
        /// </summary>
        public int killLimit = 75;

        [Tooltip("The maximum amount of difference the teams can have in player count")]
        /// <summary>
        /// The maximum amount of difference the teams can have in player count
        /// </summary>
        public int maxTeamDifference = 2;

        /// <summary>
        /// How many seconds need to be left in order to be able to start a vote?
        /// </summary>
        public float votingThreshold = 30f;

        [Header("Times")]
        /// <summary>
        /// How many seconds until we can start playing? This is the first countdown during which players cannot move or do anything other than spawn or chat.
        /// </summary>
        public float preGameTime = 20f;

        /// <summary>
        /// How many seconds until the map/gamemode voting menu is opened
        /// </summary>
        public float endGameTime = 10f;

        /// <summary>
        /// How many seconds do we have to vote on the next map and game mode?
        /// </summary>
        public float mapVotingTime = 20f;

        public override bool CanJoinTeam(Kit_IngameMain main, PhotonPlayer player, int team)
        {
            int amountOfPlayers = PhotonNetwork.room.MaxPlayers;
            //Check if the team has met its limits
            if (team == 0 && playersInTeamOne >= (amountOfPlayers / 2))
            {
                return false;
            }
            else if (team == 1 && playersInTeamTwo >= (amountOfPlayers / 2))
            {
                return false;
            }

            //Check if the difference is too big
            if (team == 0)
            {
                if (playersInTeamOne - playersInTeamTwo > maxTeamDifference) return false;
            }
            else if (team == 1)
            {
                if (playersInTeamTwo - playersInTeamOne > maxTeamDifference) return false;
            }

            //If none of the excluding factors were met, return true
            return true;
        }

        public override void GamemodeSetup(Kit_IngameMain main)
        {
            //Get all spawns
            Kit_PlayerSpawn[] allSpawns = FindObjectsOfType<Kit_PlayerSpawn>();
            //Are there any spawns at all?
            if (allSpawns.Length <= 0) throw new Exception("This scene has no spawns.");
            //Filter all spawns that are appropriate for this game mode
            List<Kit_PlayerSpawn> filteredSpawns = new List<Kit_PlayerSpawn>();
            for (int i = 0; i < allSpawns.Length; i++)
            {
                //Check if that spawn is useable for this game mode logic
                if (allSpawns[i].gameModes.Contains(this))
                {
                    //Add it to the list
                    filteredSpawns.Add(allSpawns[i]);
                }
            }

            //This game mode doesn't use different game mode for teams, so just keep it one layered
            main.internalSpawns = new List<InternalSpawns>();
            //Create a new InternalSpawns instance
            InternalSpawns dmSpawns = new InternalSpawns();
            dmSpawns.spawns = filteredSpawns;
            main.internalSpawns.Add(dmSpawns);

            //Set stage and timer
            main.gameModeStage = 0;
            main.timer = preGameTime;
        }

        public override void GameModeUpdate(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Checks all players in <see cref="PhotonNetwork.playerList"/> if they reached the kill limit, if the game is not over already
        /// </summary>
        void CheckForWinner(Kit_IngameMain main)
        {
            //Check if someone can still win
            if (main.gameModeStage < 2)
            {
                //Ensure we are using the correct runtime data
                if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
                {
                    main.currentGameModeRuntimeData = new TeamDeathmatchRuntimeData();
                }
                TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;

                if (drd.teamOnePoints >= killLimit)
                {
                    //End Game
                    main.EndGame(0, drd.teamOnePoints, drd.teamTwoPoints);
                    //Set game stage
                    main.timer = endGameTime;
                    main.gameModeStage = 2;
                }
                else if (drd.teamTwoPoints >= killLimit)
                {
                    //End Game
                    main.EndGame(1, drd.teamOnePoints, drd.teamTwoPoints);
                    //Set game stage
                    main.timer = endGameTime;
                    main.gameModeStage = 2;
                }
            }
        }

        /// <summary>
        /// How many players are in team one?
        /// </summary>
        int playersInTeamOne
        {
            get
            {
                int toReturn = 0;

                //Loop through all players
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    //Check if that player joined a team
                    if (PhotonNetwork.playerList[i].CustomProperties["team"] != null)
                    {
                        //Check if he is in team one
                        if ((int)PhotonNetwork.playerList[i].CustomProperties["team"] == 0) toReturn++;
                    }
                }

                //Return
                return toReturn;
            }
        }

        /// <summary>
        /// How many players are in team two?
        /// </summary>
        int playersInTeamTwo
        {
            get
            {
                int toReturn = 0;

                //Loop through all players
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    //Check if that player joined a team
                    if (PhotonNetwork.playerList[i].CustomProperties["team"] != null)
                    {
                        //Check if he is in team two
                        if ((int)PhotonNetwork.playerList[i].CustomProperties["team"] == 1) toReturn++;
                    }
                }

                //Return
                return toReturn;
            }
        }

        public override Transform GetSpawn(Kit_IngameMain main, PhotonPlayer player)
        {
            //Define spawn tries
            int tries = 0;
            Transform spawnToReturn = null;
            //Try to get a spawn
            while (!spawnToReturn)
            {
                //To prevent an unlimited loop, only do it ten times
                if (tries >= 10)
                {
                    break;
                }
                //Team deathmatch has no fixed spawns in this behaviour. Only use one layer
                Transform spawnToTest = main.internalSpawns[0].spawns[UnityEngine.Random.Range(0, main.internalSpawns[0].spawns.Count)].transform;
                //Test the spawn
                if (spawnToTest)
                {
                    if (spawnSystemToUse.CheckSpawnPosition(spawnToTest, player))
                    {
                        //Assign spawn
                        spawnToReturn = spawnToTest;
                        //Break the while loop
                        break;
                    }
                }
                tries++;
            }

            return spawnToReturn;
        }

        public override void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
            {
                main.currentGameModeRuntimeData = new TeamDeathmatchRuntimeData();
            }
            TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;
            if (botKiller)
            {
                if (main.currentBotManager)
                {
                    //Check if he killed himself
                    if (!botKilled || killed != killer)
                    {
                        //Get bot
                        Kit_Bot killerBot = main.currentBotManager.GetBotWithID(killer);
                        if (killerBot != null)
                        {
                            //Check in which team the killer is
                            int killerTeam = killerBot.team;
                            if (killerTeam == 0)
                            {
                                //Increase points
                                drd.teamOnePoints++;
                            }
                            else if (killerTeam == 1)
                            {
                                //Increase points
                                drd.teamTwoPoints++;
                            }
                        }
                    }
                }
            }
            else
            {
                //Check if he killed himself
                if (botKilled || killed != killer)
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
                        //Check in which team the killer is
                        int killerTeam = (int)playerKiller.CustomProperties["team"];
                        if (killerTeam == 0)
                        {
                            //Increase points
                            drd.teamOnePoints++;
                        }
                        else if (killerTeam == 1)
                        {
                            //Increase points
                            drd.teamTwoPoints++;
                        }
                    }
                }
            }
            //Check if a team has won
            CheckForWinner(main);
        }

        public override void TimeRunOut(Kit_IngameMain main)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
            {
                main.currentGameModeRuntimeData = new TeamDeathmatchRuntimeData();
            }
            TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;

            //Check stage
            if (main.gameModeStage == 0)
            {
                //Pre game time to main game
                main.timer = main.gameInformation.allDurations[Kit_GameSettings.gameLength];
                main.gameModeStage = 1;
            }
            //Time run out, determine winner
            else if (main.gameModeStage == 1)
            {

                main.timer = endGameTime;
                main.gameModeStage = 2;

                //Check if one team has more points
                if (drd.teamOnePoints > drd.teamTwoPoints)
                {
                    //Team One won
                    main.EndGame(0, drd.teamOnePoints, drd.teamTwoPoints);
                }
                else if (drd.teamTwoPoints > drd.teamOnePoints)
                {
                    //Team Two won
                    main.EndGame(1, drd.teamOnePoints, drd.teamTwoPoints);
                }
                else
                {
                    //No team has more points. It's a draw
                    main.EndGame(2, drd.teamOnePoints, drd.teamTwoPoints);
                }
            }
            //Victory screen is over. Proceed to map voting.
            else if (main.gameModeStage == 2)
            {
                //Destroy victory screen
                if (main.currentVictoryScreen)
                {
                    PhotonNetwork.Destroy(main.currentVictoryScreen.photonView);
                }
                //Set time and stage
                main.timer = mapVotingTime;
                main.gameModeStage = 3;
                //Open the voting menu
                main.OpenVotingMenu();
                //Delete all players
                main.DeleteAllPlayers();
            }
            //End countdown is over, start new game
            else if (main.gameModeStage == 3)
            {
                //TODO: Load new map / game mode
                main.gameModeStage = 4;

                //Lets load the appropriate map
                //Get the hashtable
                Hashtable table = PhotonNetwork.room.CustomProperties;

                //Get combo
                MapGameModeCombo nextCombo = main.currentMapVoting.GetComboWithMostVotes();
                //Delete map voting
                PhotonNetwork.Destroy(main.currentMapVoting.gameObject);
                //Update table
                table["gameMode"] = nextCombo.gameMode;
                table["map"] = nextCombo.map;
                PhotonNetwork.room.SetCustomProperties(table);

                //Load the map
                Kit_SceneSyncer.instance.LoadScene(main.gameInformation.allMaps[nextCombo.map].sceneName);
            }
        }

        public override bool CanSpawn(Kit_IngameMain main, PhotonPlayer player)
        {
            //Check if game stage allows spawning
            if (main.gameModeStage < 2)
            {
                //Check if the player has joined a team and updated his Custom properties
                if (player.CustomProperties["team"] != null)
                {
                    if (player.CustomProperties["team"].GetType() == typeof(int))
                    {
                        //Check if it is a valid team
                        if ((int)player.CustomProperties["team"] == 0 || (int)player.CustomProperties["team"] == 1) return true;
                    }
                }
            }
            return false;
        }

        public override bool CanControlPlayer(Kit_IngameMain main)
        {
            //While we are waiting for enough players, we can move!
            if (!AreEnoughPlayersThere(main) && !main.hasGameModeStarted) return true;
            //We can only control our player if we are in the main phase
            return main.gameModeStage == 1;
        }

        public override bool AreEnoughPlayersThere(Kit_IngameMain main)
        {
            //If there are bots ...
            if (main && main.currentBotManager && main.currentBotManager.bots.Count > 0)
            {
                return true;
            }
            else
            {
                //If there are 2 or more players, we can play!
                if (PhotonNetwork.playerList.Length >= 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void GameModeBeginMiddle(Kit_IngameMain main)
        {
            if (PhotonNetwork.offlineMode)
            {
                PhotonNetwork.OnEventCall(3, null, 0);
            }
            else
            {
                //Ask players to reset themselves
                PhotonNetwork.RaiseEvent(3, null, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
            }
        }

        public override void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
            {
                main.currentGameModeRuntimeData = new TeamDeathmatchRuntimeData();
            }
            TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;
            if (stream.isWriting)
            {
                //Send team one points
                stream.SendNext(drd.teamOnePoints);
                //Send team two points
                stream.SendNext(drd.teamTwoPoints);
            }
            else
            {
                //Get team one points
                drd.teamOnePoints = (int)stream.ReceiveNext();
                //Get team two points
                drd.teamTwoPoints = (int)stream.ReceiveNext();
            }
        }

        public override bool ArePlayersEnemies(Kit_PlayerBehaviour playerOne, Kit_PlayerBehaviour playerTwo)
        {
            if (playerOne.myTeam != playerTwo.myTeam) return true;
            return false;
        }

        public override bool AreWeEnemies(Kit_IngameMain main, bool botEnemy, int enemyId)
        {
            int enemyTeam = -1;

            if (botEnemy)
            {
                Kit_Bot bot = main.currentBotManager.GetBotWithID(enemyId);
                enemyTeam = bot.team;
            }
            else
            {
                PhotonPlayer player = PhotonPlayer.Find(enemyId);
                enemyTeam = (int)player.CustomProperties["team"];
            }

            if (main.assignedTeamID != enemyTeam) return true;

            return false;
        }

        public override bool CanStartVote(Kit_IngameMain main)
        {
            //While we are waiting for enough players, we can vote!
            if (!AreEnoughPlayersThere(main) && !main.hasGameModeStarted) return true;
            //We can only vote during the main phase and if enough time is left
            return main.gameModeStage == 1 && main.timer > votingThreshold;
        }
    }
}
