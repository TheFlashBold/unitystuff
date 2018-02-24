using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    [System.Serializable]
    public class Kit_Bot
    {
        /// <summary>
        /// The ID of this bot
        /// </summary>
        public int id;
        /// <summary>
        /// The name of this bot
        /// </summary>
        public string name;
        /// <summary>
        /// Team of this bot
        /// </summary>
        public int team;
        /// <summary>
        /// Kills of this bot
        /// </summary>
        public int kills;
        /// <summary>
        /// Deaths of this bot
        /// </summary>
        public int deaths;
    }

    public class Kit_BotManager : Photon.PunBehaviour
    {
        public List<Kit_Bot> bots = new List<Kit_Bot>();
        private List<Kit_PlayerBehaviour> activeBots = new List<Kit_PlayerBehaviour>();

        /// <summary>
        /// Bot name manager to use
        /// </summary>
        public Kit_BotNameManager nameManager;

        public Kit_BotLoadoutManager loadoutManager;

        [HideInInspector]
        /// <summary>
        /// Reference to ingame main
        /// </summary>
        public Kit_IngameMain main;

        public float spawnFrequency = 1f;
        private float lastSpawn;
        private int lastId;

        void Awake()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
            //Assign
            main.currentBotManager = this;
        }

        void Update()
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (Time.time > lastSpawn)
                {
                    lastSpawn = Time.time + spawnFrequency;
                    SpawnBots();
                }
            }
        }

        void SpawnBots()
        {
            for (int i = 0; i < bots.Count; i++)
            {
                if (!IsBotAlive(bots[i]))
                {
                    SpawnBot(bots[i]);
                    break;
                }
            }
        }

        void SpawnBot(Kit_Bot bot)
        {
            //Get a spawn
            Transform spawnLocation = main.gameInformation.allGameModes[main.currentGameMode].gameModeLogic.GetSpawn(main, PhotonNetwork.player);
            if (spawnLocation)
            {
                //Create object array for photon use
                object[] instData = new object[0];
                //Assign the values
                if (!main.currentGameModeBehaviour.UsesCustomSpawn())
                {
                    //Get the current loadout
                    Loadout curLoadout = loadoutManager.GetBotLoadout(main);
                    //Calculate length
                    //Base values + Primary Attachments + Secondary Attachments + Values for the length of the attachment arrays
                    int length = 5 + curLoadout.primaryAttachments.Length + curLoadout.secondaryAttachments.Length + 2;
                    instData = new object[length];
                    //Assign team
                    instData[0] = bot.team;
                    //Tell the system its a bot
                    instData[1] = true;
                    instData[2] = bot.id;
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
                    //Game mode is not loadout driven
                    //Get the current loadout
                    Loadout curLoadout = main.currentGameModeBehaviour.DoCustomSpawnBot(main, bot);
                    //Calculate length
                    //Base values + Primary Attachments + Secondary Attachments + Values for the length of the attachment arrays
                    int length = 5 + curLoadout.primaryAttachments.Length + curLoadout.secondaryAttachments.Length + 2;
                    instData = new object[length];
                    //Assign team
                    instData[0] = bot.team;
                    //Tell the system its a bot
                    instData[1] = true;
                    instData[2] = bot.id;
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
                PhotonNetwork.InstantiateSceneObject(main.playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                //Send last id
                stream.SendNext(lastId);
                //Send Count
                stream.SendNext(bots.Count);
                //Send contents
                for (int i = 0; i < bots.Count; i++)
                {
                    stream.SendNext(bots[i].id);
                    stream.SendNext(bots[i].name);
                    stream.SendNext(bots[i].team);
                    stream.SendNext(bots[i].kills);
                    stream.SendNext(bots[i].deaths);
                }
            }
            else
            {
                //Get last id
                lastId = (int)stream.ReceiveNext();
                //Get Count
                int count = (int)stream.ReceiveNext();
                //Adjust length
                if (bots.Count != count)
                {
                    while (bots.Count > count) bots.RemoveAt(bots.Count - 1);
                    while (bots.Count < count) bots.Add(new Kit_Bot());
                }
                //Get contents
                for (int i = 0; i < count; i++)
                {
                    bots[i].id = (int)stream.ReceiveNext();
                    bots[i].name = (string)stream.ReceiveNext();
                    bots[i].team = (int)stream.ReceiveNext();
                    bots[i].kills = (int)stream.ReceiveNext();
                    bots[i].deaths = (int)stream.ReceiveNext();
                }
            }
        }

        /// <summary>
        /// Adds a new, empty bot
        /// </summary>
        /// <returns></returns>
        public Kit_Bot AddNewBot()
        {
            Kit_Bot newBot = new Kit_Bot();
            newBot.id = lastId;
            lastId++;
            //Get a new name
            newBot.name = nameManager.GetRandomName(this);
            //Send chat message
            main.chat.SendBotMessage(newBot.name, 0);
            bots.Add(newBot);
            return newBot;
        }

        /// <summary>
        /// Removes a random bot
        /// </summary>
        public void RemoveRandomBot()
        {
            Kit_Bot toRemove = GetBotWithID(Random.Range(0, bots.Count));
            //Send chat message
            main.chat.SendBotMessage(toRemove.name, 1);
            if (IsBotAlive(toRemove))
            {
                PhotonNetwork.Destroy(GetAliveBot(toRemove).photonView);
            }
            bots.Remove(toRemove);
        }

        /// <summary>
        /// Removes a bot that is in this team
        /// </summary>
        /// <param name="team"></param>
        public void RemoveBotInTeam(int team)
        {
            Kit_Bot toRemove = bots.Find(x => x.team == team);
            if (toRemove != null)
            {
                //Send chat message
                main.chat.SendBotMessage(toRemove.name, 1);
                if (IsBotAlive(toRemove))
                {
                    PhotonNetwork.Destroy(GetAliveBot(toRemove).photonView);
                }
                bots.Remove(toRemove);
            }
        }

        public Kit_Bot GetBotWithID(int id)
        {
            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].id == id)
                {
                    return bots[i];
                }
            }
            return null;
        }

        public bool IsBotAlive(Kit_Bot bot)
        {
            for (int i = 0; i < activeBots.Count; i++)
            {
                if (activeBots[i] && activeBots[i].isBot && activeBots[i].botId == bot.id)
                {
                    return true;
                }
            }
            return false;
        }

        public Kit_PlayerBehaviour GetAliveBot(Kit_Bot bot)
        {
            for (int i = 0; i < activeBots.Count; i++)
            {
                if (activeBots[i] && activeBots[i].isBot && activeBots[i].botId == bot.id)
                {
                    return activeBots[i];
                }
            }
            return null;
        }

        public void AddActiveBot(Kit_PlayerBehaviour bot)
        {
            activeBots.Add(bot);
        }

        /// <summary>
        /// How many bots are there?
        /// </summary>
        public int GetAmountOfBots()
        {
            return bots.Count;
        }

        /// <summary>
        /// How many bots are in team one?
        /// </summary>
        /// <returns></returns>
        public int GetBotsInTeamOne()
        {
            int toReturn = 0;
            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].team == 0) toReturn++;
            }
            return toReturn;
        }

        /// <summary>
        /// How many bots are in team two?
        /// </summary>
        /// <returns></returns>
        public int GetBotsInTeamTwo()
        {
            int toReturn = 0;
            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].team == 1) toReturn++;
            }
            return toReturn;
        }

        /// <summary>
        /// How many players are there?
        /// </summary>
        /// <returns></returns>
        public int GetAmountOfPlayers()
        {
            return PhotonNetwork.playerList.Length;
        }

        /// <summary>
        /// How many players are in team one?
        /// </summary>
        /// <returns></returns>
        public int GetPlayersInTeamOne()
        {
            int toReturn = 0;
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                if (PhotonNetwork.playerList[i].CustomProperties["team"] != null)
                {
                    int team = (int)PhotonNetwork.playerList[i].CustomProperties["team"];
                    if (team == 0) toReturn++;
                }
            }
            return toReturn;
        }

        /// <summary>
        /// How many players are in team two?
        /// </summary>
        /// <returns></returns>
        public int GetPlayersInTeamTwo()
        {
            int toReturn = 0;
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                if (PhotonNetwork.playerList[i].CustomProperties["team"] != null)
                {
                    int team = (int)PhotonNetwork.playerList[i].CustomProperties["team"];
                    if (team == 1) toReturn++;
                }
            }
            return toReturn;
        }
    }
}