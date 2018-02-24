using UnityEngine;
using System.Collections;
using System;

namespace MarsFPSKit
{
    /// <summary>
    /// Helper class to combine players & bots
    /// </summary>
    public class Kit_Player
    {
        /// <summary>
        /// Is this a reference to a bot or a player?
        /// </summary>
        public bool isBot;
        /// <summary>
        /// Either photon ID or bot ID
        /// </summary>
        public int id;
    }

    public abstract class Kit_GameModeBase : ScriptableObject
    {
        /// <summary>
        /// Which HUD prefab should be used for this game mode? Can be null.
        /// </summary>
        public GameObject hudPrefab;

        public Kit_SpawnSystemBase spawnSystemToUse;

        public Kit_BotGameModeManagerBase botManagerToUse;

        /// <summary>
        /// If this is enabled, you can send team only chat messages.
        /// </summary>
        public bool isTeamGameMode;

        /// <summary>
        /// Started upon starting playing with this game mode
        /// </summary>
        /// <param name="main"></param>
        public abstract void GamemodeSetup(Kit_IngameMain main);

        /// <summary>
        /// Called when the game mode starts when enough players are connected. Not called if there are enough players when the game mode initially began.
        /// </summary>
        /// <param name="main"></param>
        public abstract void GameModeBeginMiddle(Kit_IngameMain main);

        /// <summary>
        /// Called every frame as long as this game mode is active
        /// </summary>
        /// <param name="main"></param>
        public abstract void GameModeUpdate(Kit_IngameMain main);

        /// <summary>
        /// Called every time a player dies
        /// </summary>
        /// <param name="main"></param>
        public abstract void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed);

        /// <summary>
        /// Called when the local player has gained a kill
        /// </summary>
        /// <param name="main"></param>
        public virtual void LocalPlayerScoredKill(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called for the master client when a bot has gained a kill
        /// </summary>
        /// <param name="main"></param>
        /// <param name="bot"></param>
        public virtual void MasterClientBotScoredKill(Kit_IngameMain main, Kit_Bot bot)
        {

        }

        /// <summary>
        /// Called when the timer reaches zero
        /// </summary>
        /// <param name="main"></param>
        public abstract void TimeRunOut(Kit_IngameMain main);

        /// <summary>
        /// Returns a spawnpoint for the associated player
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract Transform GetSpawn(Kit_IngameMain main, PhotonPlayer player);

        /// <summary>
        /// Can we currently spawn?
        /// </summary>
        /// <param name=""></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanSpawn(Kit_IngameMain main, PhotonPlayer player);

        /// <summary>
        /// Does this game mode have a custom spawn method?
        /// </summary>
        /// <returns></returns>
        public virtual bool UsesCustomSpawn()
        {
            return false;
        }

        public virtual GameObject DoCustomSpawn(Kit_IngameMain main)
        {
            throw new NotImplementedException("Game mode " + this.name + " uses custom spawn, but it has not been implemented [players]!");
        }

        public virtual Loadout DoCustomSpawnBot(Kit_IngameMain main, Kit_Bot bot)
        {
            throw new NotImplementedException("Game mode " + this.name + " uses custom spawn, but it has not been implemented [bots]!");
        }

        /// <summary>
        /// Can we join this specific team?
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanJoinTeam(Kit_IngameMain main, PhotonPlayer player, int team);

        /// <summary>
        /// Can the player be controlled at this stage of this game mode?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public abstract bool CanControlPlayer(Kit_IngameMain main);

        /// <summary>
        /// Called when the player properties have been changed
        /// </summary>
        /// <param name="playerAndUpdatedProps"></param>
        public virtual void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {

        }

        /// <summary>
        /// Relay for serialization to sync data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        /// <summary>
        /// Are there enough players currently to play this game mode?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public abstract bool AreEnoughPlayersThere(Kit_IngameMain main);

        /// <summary>
        /// Can weapons be dropped in this game mode?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public virtual bool CanDropWeapons(Kit_IngameMain main)
        {
            return true;
        }

        /// <summary>
        /// Is our local player enemies with this player?
        /// </summary>
        /// <param name="with"></param>
        /// <returns></returns>
        public abstract bool AreWeEnemies(Kit_IngameMain main, bool botEnemy, int enemyId);

        /// <summary>
        /// Are these two players enemies?
        /// </summary>
        /// <param name="playerOne"></param>
        /// <param name="playerTwo"></param>
        /// <returns></returns>
        public abstract bool ArePlayersEnemies(Kit_PlayerBehaviour playerOne, Kit_PlayerBehaviour playerTwo);

        /// <summary>
        /// Can a vote be started currently?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public abstract bool CanStartVote(Kit_IngameMain main);

        /// <summary>
        /// Does this game mode support the loadout menu?
        /// </summary>
        /// <returns></returns>
        public virtual bool LoadoutMenuSupported()
        {
            return true;
        }
    }
}
