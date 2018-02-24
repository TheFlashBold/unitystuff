using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "MarsFPSKit/Bots/Game Mode Behaviour/Simple Fillup")]
    /// <summary>
    /// This script fills up bots to the player limit, supports team based game modes too
    /// </summary>
    public class Kit_BotGameModeFillup : Kit_BotGameModeManagerBase
    {
        public override void Inizialize(Kit_BotManager manager)
        {
            if (manager.main.currentGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;
                if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 0;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(0);
                        tries++;
                    }
                }
                //Reset tries
                tries = 0;

                if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 1;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(1);
                        tries++;
                    }
                }
            }
            else
            {
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.room.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.room.MaxPlayers)
                    {
                        manager.AddNewBot();
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.room.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.room.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                    }
                }
            }
        }

        public override void PlayerJoinedTeam(Kit_BotManager manager)
        {
            if (manager.main.currentGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;
                if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 0;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(0);
                        tries++;
                    }
                }
                //Reset tries
                tries = 0;

                if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 1;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(1);
                        tries++;
                    }
                }
            }
            else
            {
                int tries = 0;
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.room.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.room.MaxPlayers && tries <= 20)
                    {
                        manager.AddNewBot();
                        tries++;
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.room.MaxPlayers && tries <= 20)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.room.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                        tries++;
                    }
                }
            }
        }

        public override void PlayerLeftTeam(Kit_BotManager manager)
        {
            if (manager.main.currentGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;
                if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() < PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 0;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamOne() + manager.GetBotsInTeamOne() > PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(0);
                        tries++;
                    }
                }
                //Reset tries
                tries = 0;

                if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() < PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        Kit_Bot bot = manager.AddNewBot();
                        bot.team = 1;
                        tries++;
                    }
                }
                else if (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.room.MaxPlayers / 2)
                {
                    while (manager.GetPlayersInTeamTwo() + manager.GetBotsInTeamTwo() > PhotonNetwork.room.MaxPlayers / 2 && tries <= 20)
                    {
                        manager.RemoveBotInTeam(1);
                        tries++;
                    }
                }
            }
            else
            {
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.room.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.room.MaxPlayers)
                    {
                        manager.AddNewBot();
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.room.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.room.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                    }
                }
            }
        }
    }
}
