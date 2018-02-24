using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MarsFPSKit
{
    namespace Scoreboard
    {
        /// <summary>
        /// This is a helper class to combine PhotonPlayer and Kit_Bot
        /// </summary>
        public class Kit_ScoreboardHelper
        {
            /// <summary>
            /// Name of this player
            /// </summary>
            public string name;
            /// <summary>
            /// Team of this player
            /// </summary>
            public int team;
            /// <summary>
            /// Kills of this player
            /// </summary>
            public int kills;
            /// <summary>
            /// Deaths of this player
            /// </summary>
            public int deaths;
            /// <summary>
            /// Ping of this player
            /// </summary>
            public int ping;
            /// <summary>
            /// To pool these, they need to have a used parameter
            /// </summary>
            public bool used;
        }

        public class Kit_ScoreboardMain : Kit_ScoreboardBase
        {
            public Kit_IngameMain main;

            /// <summary>
            /// The root object of the scoreboard
            /// </summary>
            public GameObject scoreboardRoot;

            [Header("Team Game Mode")]
            /// <summary>
            /// The root for the small, split (team) scoreboard
            /// </summary>
            public GameObject teamGameModeRoot;
            /// <summary>
            /// Entries for team one go here
            /// </summary>
            public RectTransform teamOneEntriesGo;
            /// <summary>
            /// Entries for team two go here
            /// </summary>
            public RectTransform teamTwoEntriesGo;
            /// <summary>
            /// The prefab for a team entry
            /// </summary>
            public GameObject teamPrefab;

            /// <summary>
            /// A list of active entries so we can pool them for team one
            /// </summary>
            private List<Kit_ScoreboardUIEntry> activeEntriesTeamOne = new List<Kit_ScoreboardUIEntry>();

            /// <summary>
            /// A list of active entries so we can pool them for team two
            /// </summary>
            private List<Kit_ScoreboardUIEntry> activeEntriesTeamTwo = new List<Kit_ScoreboardUIEntry>();

            [Header("Non Team Game Mode")]
            /// <summary>
            /// The root for the big (non team) scoreboard
            /// </summary>
            public GameObject nonTeamGameModeRoot;
            /// <summary>
            /// Where do the non team entries belong?
            /// </summary>
            public RectTransform entriesGo;
            /// <summary>
            /// The prefab for a non team entry entry
            /// </summary>
            public GameObject entryPrefab;

            /// <summary>
            /// A list of active entries so we can pool them
            /// </summary>
            private List<Kit_ScoreboardUIEntry> activeEntries = new List<Kit_ScoreboardUIEntry>();

            [Header("Settings")]
            /// <summary>
            /// After how many seconds should the scoreboard be redrawn, if it is open? So we can see updated values while the scoreboard is open.
            /// </summary>
            public float redrawFrequency = 1f;

            /// <summary>
            /// When was the scoreboard redrawn for the last time?
            /// </summary>
            private float lastRedraw;

            //[HideInInspector]
            public bool canUseScoreboard;

            #region Runtime
            private List<Kit_ScoreboardHelper> rt_ScoreboardEntries = new List<Kit_ScoreboardHelper>();
            #endregion

            void Update()
            {
                //Check if we can use the scoreboard
                if (canUseScoreboard)
                {
                    //Check for input
                    if (Input.GetKey(KeyCode.Tab))
                    {
                        //Check if its not already open
                        if (!isOpen)
                        {
                            isOpen = true;
                            //Redraw
                            Redraw();
                        }
                    }
                    else
                    {
                        //Check if it is open
                        if (isOpen)
                        {
                            isOpen = false;
                            //Redraw
                            Redraw();
                        }
                    }

                    if (isOpen)
                    {
                        if (Time.time > lastRedraw)
                        {
                            Redraw();
                        }
                    }
                }
            }

            public override void Disable()
            {
                //Disable use of scoreboard
                canUseScoreboard = false;
                //Force scoreboard to close
                isOpen = false;
                //Redraw
                Redraw();
            }

            public override void Enable()
            {
                //Enable use of scoreboard
                canUseScoreboard = true;
            }

            void Redraw()
            {
                //Set time
                lastRedraw = Time.time + redrawFrequency;

                //Set root based on state
                if (isOpen)
                {
                    scoreboardRoot.SetActive(true);
                }
                else
                {
                    scoreboardRoot.SetActive(false);
                }

                //Reset entries
                for (int o = 0; o < rt_ScoreboardEntries.Count; o++)
                {
                    rt_ScoreboardEntries[o].used = false;
                }

                //Convert PhotonPlayer to Scoreboard ready entries
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    //Check if entry is available
                    if (rt_ScoreboardEntries.Count > i)
                    {
                        //Update
                        //Set name
                        rt_ScoreboardEntries[i].name = PhotonNetwork.playerList[i].NickName;

                        //Check in which team that player is
                        if (PhotonNetwork.playerList[i].CustomProperties["team"] != null)
                        {
                            rt_ScoreboardEntries[i].team = (int)PhotonNetwork.playerList[i].CustomProperties["team"];
                        }
                        else
                        {
                            rt_ScoreboardEntries[i].team = 2;
                        }

                        if (PhotonNetwork.playerList[i].CustomProperties["kills"] != null)
                        {
                            rt_ScoreboardEntries[i].kills = (int)PhotonNetwork.playerList[i].CustomProperties["kills"];

                        }
                        else
                        {
                            rt_ScoreboardEntries[i].kills = 0;
                        }

                        //Check if he has deaths
                        if (PhotonNetwork.playerList[i].CustomProperties["deaths"] != null)
                        {
                            rt_ScoreboardEntries[i].deaths = (int)PhotonNetwork.playerList[i].CustomProperties["deaths"];
                        }
                        else
                        {
                            rt_ScoreboardEntries[i].deaths = 0;
                        }

                        //Check if he has ping
                        if (PhotonNetwork.playerList[i].CustomProperties["ping"] != null)
                        {
                            rt_ScoreboardEntries[i].ping = (int)PhotonNetwork.playerList[i].CustomProperties["ping"];
                        }
                        else
                        {
                            rt_ScoreboardEntries[i].ping = 0;
                        }

                        rt_ScoreboardEntries[i].used = true;
                    }
                    else
                    {
                        //Create new
                        Kit_ScoreboardHelper entry = new Kit_ScoreboardHelper();

                        //Set name
                        entry.name = PhotonNetwork.playerList[i].NickName;

                        //Check in which team that player is
                        if (PhotonNetwork.playerList[i].CustomProperties["team"] != null)
                        {
                            entry.team = (int)PhotonNetwork.playerList[i].CustomProperties["team"];
                        }
                        else
                        {
                            entry.team = 2;
                        }

                        if (PhotonNetwork.playerList[i].CustomProperties["kills"] != null)
                        {
                            entry.kills = (int)PhotonNetwork.playerList[i].CustomProperties["kills"];

                        }
                        else
                        {
                            entry.kills = 0;
                        }

                        //Check if he has deaths
                        if (PhotonNetwork.playerList[i].CustomProperties["deaths"] != null)
                        {
                            entry.deaths = (int)PhotonNetwork.playerList[i].CustomProperties["deaths"];
                        }
                        else
                        {
                            entry.deaths = 0;
                        }

                        //Check if he has ping
                        if (PhotonNetwork.playerList[i].CustomProperties["ping"] != null)
                        {
                            entry.ping = (int)PhotonNetwork.playerList[i].CustomProperties["ping"];
                        }
                        else
                        {
                            entry.ping = 0;
                        }

                        //Set to used
                        entry.used = true;

                        //Add
                        rt_ScoreboardEntries.Add(entry);
                    }
                }

                //Convert Bots to List
                if (main.currentBotManager)
                {
                    for (int i = 0; i < main.currentBotManager.bots.Count; i++)
                    {
                        //Check if entry is available
                        if (rt_ScoreboardEntries.Count > i + PhotonNetwork.playerList.Length)
                        {
                            //Update
                            //Set name
                            rt_ScoreboardEntries[i + PhotonNetwork.playerList.Length].name = main.currentBotManager.bots[i].name;
                            //Copy team
                            rt_ScoreboardEntries[i + PhotonNetwork.playerList.Length].team = main.currentBotManager.bots[i].team;
                            //Copy kills
                            rt_ScoreboardEntries[i + PhotonNetwork.playerList.Length].kills = main.currentBotManager.bots[i].kills;
                            //Copy Deaths
                            rt_ScoreboardEntries[i + PhotonNetwork.playerList.Length].deaths = main.currentBotManager.bots[i].deaths;
                            //Set ping to 0
                            rt_ScoreboardEntries[i + PhotonNetwork.playerList.Length].ping = 0;
                            //Set to used
                            rt_ScoreboardEntries[i + PhotonNetwork.playerList.Length].used = true;
                        }
                        else
                        {
                            //Create new
                            Kit_ScoreboardHelper entry = new Kit_ScoreboardHelper();

                            //Set name
                            entry.name = main.currentBotManager.bots[i].name;
                            //Copy team
                            entry.team = main.currentBotManager.bots[i].team;
                            //Copy kills
                            entry.kills = main.currentBotManager.bots[i].kills;
                            //Copy deaths
                            entry.deaths = main.currentBotManager.bots[i].deaths;
                            //Set ping to 0
                            entry.ping = 0;

                            //Set to used
                            entry.used = true;

                            //Add
                            rt_ScoreboardEntries.Add(entry);
                        }
                    }
                }

                //Sort List
               rt_ScoreboardEntries =  rt_ScoreboardEntries.OrderBy(x => x.kills).Reverse().ToList();

                //Different Scoreboard for team and non team game modes
                //Team Game Mode
                if (main.currentGameModeBehaviour.isTeamGameMode)
                {
                    //Use correct scoreaboard
                    if (!teamGameModeRoot.activeSelf) teamGameModeRoot.SetActive(true);
                    if (nonTeamGameModeRoot.activeSelf) nonTeamGameModeRoot.SetActive(false);

                    //Set all to unused
                    for (int o = 0; o < activeEntriesTeamOne.Count; o++)
                    {
                        activeEntriesTeamOne[o].used = false;
                    }
                    for (int o = 0; o < activeEntriesTeamTwo.Count; o++)
                    {
                        activeEntriesTeamTwo[o].used = false;
                    }

                    //Current index for team one
                    int currentIndexTeamOne = 0;
                    //Current index for team two
                    int currentIndexTeamTwo = 0;

                    //Redraw
                    for (int i = 0; i < rt_ScoreboardEntries.Count; i++)
                    {
                        //Check in which team that player is
                        if ((rt_ScoreboardEntries[i].team == 0 || rt_ScoreboardEntries[i].team == 1) && rt_ScoreboardEntries[i].used)
                        {
                            int team = rt_ScoreboardEntries[i].team;
                            if (team == 0)
                            {
                                //Check if we have an active entry for this player
                                if (activeEntriesTeamOne.Count <= currentIndexTeamOne)
                                {
                                    //We don't have one, create one
                                    GameObject go = Instantiate(teamPrefab, teamOneEntriesGo, false);
                                    //Reset scale
                                    go.transform.localScale = Vector3.one;
                                    //Add to list
                                    activeEntriesTeamOne.Add(go.GetComponent<Kit_ScoreboardUIEntry>());
                                }

                                //Calculate total score
                                int totalScore = 0;

                                //Redraw
                                activeEntriesTeamOne[currentIndexTeamOne].used = true; //Set to true
                                if (!activeEntriesTeamOne[currentIndexTeamOne].gameObject.activeSelf) activeEntriesTeamOne[currentIndexTeamOne].gameObject.SetActive(true);
                                activeEntriesTeamOne[currentIndexTeamOne].nameText.text = rt_ScoreboardEntries[i].name; //Update nickname

                                //Kills
                                activeEntriesTeamOne[currentIndexTeamOne].kills.text = rt_ScoreboardEntries[i].kills.ToString();
                                //Add to score
                                totalScore += rt_ScoreboardEntries[i].kills * main.gameInformation.pointsPerKill;


                                //Check if he has deaths
                                activeEntriesTeamOne[currentIndexTeamOne].deaths.text = rt_ScoreboardEntries[i].deaths.ToString();


                                //Check if he has ping
                                activeEntriesTeamOne[currentIndexTeamOne].ping.text = rt_ScoreboardEntries[i].ping.ToString();


                                //Update score
                                activeEntriesTeamOne[currentIndexTeamOne].score.text = totalScore.ToString();

                                //Increase index
                                currentIndexTeamOne++;
                            }
                            else if (team == 1)
                            {
                                //Check if we have an active entry for this player
                                if (activeEntriesTeamTwo.Count <= currentIndexTeamTwo)
                                {
                                    //We don't have one, create one
                                    GameObject go = Instantiate(teamPrefab, teamTwoEntriesGo, false);
                                    //Reset scale
                                    go.transform.localScale = Vector3.one;
                                    //Add to list
                                    activeEntriesTeamTwo.Add(go.GetComponent<Kit_ScoreboardUIEntry>());
                                }

                                //Calculate total score
                                int totalScore = 0;

                                //Redraw
                                activeEntriesTeamTwo[currentIndexTeamTwo].used = true; //Set to true
                                if (!activeEntriesTeamTwo[currentIndexTeamTwo].gameObject.activeSelf) activeEntriesTeamTwo[currentIndexTeamTwo].gameObject.SetActive(true);
                                activeEntriesTeamTwo[currentIndexTeamTwo].nameText.text = rt_ScoreboardEntries[i].name; //Update nickname

                                //Kills
                                activeEntriesTeamTwo[currentIndexTeamTwo].kills.text = rt_ScoreboardEntries[i].kills.ToString();
                                //Add to score
                                totalScore += rt_ScoreboardEntries[i].kills * main.gameInformation.pointsPerKill;

                                //Deaths
                                activeEntriesTeamTwo[currentIndexTeamTwo].deaths.text = rt_ScoreboardEntries[i].deaths.ToString();

                                //Ping
                                activeEntriesTeamTwo[currentIndexTeamTwo].ping.text = rt_ScoreboardEntries[i].ping.ToString();

                                //Update score
                                activeEntriesTeamTwo[currentIndexTeamTwo].score.text = totalScore.ToString();

                                //Increase index
                                currentIndexTeamTwo++;
                            }
                        }
                    }

                    //Disable unused ones
                    for (int p = 0; p < activeEntriesTeamOne.Count; p++)
                    {
                        if (!activeEntriesTeamOne[p].used)
                        {
                            activeEntriesTeamOne[p].gameObject.SetActive(false);
                        }
                    }
                    for (int p = 0; p < activeEntriesTeamTwo.Count; p++)
                    {
                        if (!activeEntriesTeamTwo[p].used)
                        {
                            activeEntriesTeamTwo[p].gameObject.SetActive(false);
                        }
                    }
                }
                //Non Team Game Mode
                else
                {
                    //Use correct scoreaboard
                    if (teamGameModeRoot.activeSelf) teamGameModeRoot.SetActive(false);
                    if (!nonTeamGameModeRoot.activeSelf) nonTeamGameModeRoot.SetActive(true);

                    //Set all to unused
                    for (int o = 0; o < activeEntries.Count; o++)
                    {
                        activeEntries[o].used = false;
                    }

                    int activeIndex = 0;

                    //Redraw
                    for (int i = 0; i < rt_ScoreboardEntries.Count; i++)
                    {
                        if (rt_ScoreboardEntries[i].used)
                        {
                            //Check if we have an active entry for this player
                            if (activeEntries.Count <= activeIndex)
                            {
                                //We don't have one, create one
                                GameObject go = Instantiate(entryPrefab, entriesGo, false);
                                //Reset scale
                                go.transform.localScale = Vector3.one;
                                //Add to list
                                activeEntries.Add(go.GetComponent<Kit_ScoreboardUIEntry>());
                            }

                            //Calculate total score
                            int totalScore = 0;

                            //Redraw
                            activeEntries[i].used = true; //Set to true
                            if (!activeEntries[i].gameObject.activeSelf) activeEntries[i].gameObject.SetActive(true);
                            activeEntries[i].nameText.text = rt_ScoreboardEntries[i].name; //Update nickname

                            //Kills
                            activeEntries[i].kills.text = rt_ScoreboardEntries[i].kills.ToString();
                            //Add to score
                            totalScore += rt_ScoreboardEntries[i].kills * main.gameInformation.pointsPerKill;

                            //Check if he has deaths
                            activeEntries[i].deaths.text = rt_ScoreboardEntries[i].deaths.ToString();

                            //Check if he has ping
                            activeEntries[i].ping.text = rt_ScoreboardEntries[i].ping.ToString();

                            //Update score
                            activeEntries[i].score.text = totalScore.ToString();

                            activeIndex++;
                        }
                    }

                    //Disable unused ones
                    for (int p = 0; p < activeEntries.Count; p++)
                    {
                        if (!activeEntries[p].used)
                        {
                            activeEntries[p].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
