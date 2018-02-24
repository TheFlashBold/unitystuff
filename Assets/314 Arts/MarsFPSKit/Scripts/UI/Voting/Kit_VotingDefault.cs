using ExitGames.Client.Photon;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_VotingDefault : Kit_VotingBase, IPunObservable
    {
        /// <summary>
        /// Reference to main
        /// </summary>
        public Kit_IngameMain main;

        public float timer = 30f;

        void Start()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
            //Check if enough time is left
            if (main.timer > timer)
            {
                //Assign it
                main.currentVoting = this;
                //Reset our own vote
                Hashtable table = PhotonNetwork.player.CustomProperties;
                if (table["vote"] != null)
                {
                    table["vote"] = -1;
                    PhotonNetwork.player.SetCustomProperties(table);
                }
                myVote = -1;

                //Get setup properties
                object[] voteProperties = photonView.instantiationData;
                //Get vote
                int voting = (int)voteProperties[0];
                votingOn = (VotingOn)voting;
                int arg = (int)voteProperties[1];
                argument = arg;
                int started = (int)voteProperties[2];
                //Assign starting player
                voteStartedBy = PhotonPlayer.Find(started);
                //If we started this vote, automatically vote yes.
                if (voteStartedBy == PhotonNetwork.player)
                {
                    VoteYes();
                }

                //Redraw
                if (main.votingMenu)
                {
                    main.votingMenu.RedrawVotingUI(this);
                }
            }
            else
            {
                VoteFailed();
            }
        }

        void Update()
        {
            if (PhotonNetwork.isMasterClient)
            {
                //Decrease the timer
                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        TimeRanOut();
                    }
                }
            }

            //Check if we did not vote yet!
            if (myVote == -1)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    VoteYes();
                }
                else if (Input.GetKeyDown(KeyCode.F2))
                {
                    VoteNo();
                }
            }
        }

        void OnDestroy()
        {
            //Reset our own vote
            Hashtable table = PhotonNetwork.player.CustomProperties;
            if (table["vote"] != null)
            {
                table["vote"] = -1;
                PhotonNetwork.player.SetCustomProperties(table);
            }

            //Tell the voting behaviour
            if (main.votingMenu)
            {
                main.votingMenu.VoteEnded(this);
            }
        }

        public override int GetYesVotes()
        {
            int yesVotes = 0;
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                Hashtable cp = PhotonNetwork.playerList[i].CustomProperties;
                //Check if vote is set
                if (cp["vote"] != null)
                {
                    //Get the vote
                    int vote = (int)cp["vote"];
                    //Check if it is a yes vote
                    if (vote == 1)
                    {
                        yesVotes++;
                    }
                }
            }

            return yesVotes;
        }

        public override int GetNoVotes()
        {
            int noVotes = 0;
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                Hashtable cp = PhotonNetwork.playerList[i].CustomProperties;
                //Check if vote is set
                if (cp["vote"] != null)
                {
                    //Get the vote
                    int vote = (int)cp["vote"];
                    //Check if it is a no vote
                    if (vote == 0)
                    {
                        noVotes++;
                    }
                }
            }

            return noVotes;
        }

        public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            //A player's vote has possibly changed! Redraw!
            if (main && main.votingMenu)
            {
                main.votingMenu.RedrawVotingUI(this);
            }
            if (PhotonNetwork.isMasterClient)
            {
                RecalculateVotes();
            }
        }

        //Check if enough votes were gained
        void RecalculateVotes()
        {
            //Have all players voted?
            if ((GetYesVotes() + GetNoVotes()) >= PhotonNetwork.playerList.Length)
            {
                //More yes than no votes?
                if (GetYesVotes() > GetNoVotes())
                {
                    VoteSucceeded();
                }
                else
                {
                    VoteFailed();
                }
            }
        }

        void TimeRanOut()
        {
            //More than 50% of all players need to vote and more yes votes than no votes!
            int totalVotes = GetYesVotes() + GetNoVotes();
            if (totalVotes > (PhotonNetwork.playerList.Length / 2))
            {
                if (GetYesVotes() > GetNoVotes())
                {
                    VoteSucceeded();
                }
                else
                {
                    VoteFailed();
                }
            }
            else
            {
                VoteFailed();
            }
        }

        void VoteSucceeded()
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (votingOn == VotingOn.Kick)
                {
                    //Get that player
                    PhotonPlayer toKick = PhotonPlayer.Find(argument);
                    //Kick that player
                    PhotonNetwork.CloseConnection(toKick);
                }
                else if (votingOn == VotingOn.Map)
                {
                    //Switch map
                    main.SwitchMap(argument);
                }
                else if (votingOn == VotingOn.GameMode)
                {
                    //Switch game mode
                    main.SwitchGameMode(argument);
                }

                //Destroy this
                PhotonNetwork.Destroy(gameObject);
            }
        }

        void VoteFailed()
        {
            //Destroy this
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                //Synchronize timer
                stream.SendNext(timer);
            }
            else
            {
                //Set timer
                timer = (float)stream.ReceiveNext();
            }
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            //If the player who started the vote left, abort!
            if (otherPlayer == voteStartedBy)
            {
                VoteFailed();
            }
            //If we are voting on this player, also abort
            if (votingOn == VotingOn.Kick && otherPlayer.ID == argument)
            {
                VoteFailed();
            }
        }
    }
}