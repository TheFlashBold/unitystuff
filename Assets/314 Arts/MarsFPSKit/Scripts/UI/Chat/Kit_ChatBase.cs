using ExitGames.Client.Photon;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Use this class to impement your own chat. It also includes a function to send a chat message properly. The master player controls it
    /// </summary>
    public abstract class Kit_ChatBase : MonoBehaviour
    {
        /// <summary>
        /// Displays a chat message. No checks required, they are done before by the master client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <param name="type">0 = Everyone; 1 = Team only</param>
        public abstract void DisplayChatMessage(PhotonPlayer sender, string message, int type);

        /// <summary>
        /// A player left.
        /// </summary>
        /// <param name="player"></param>
        public abstract void PlayerLeft(PhotonPlayer player);

        /// <summary>
        /// A player joined
        /// </summary>
        /// <param name="player"></param>
        public abstract void PlayerJoined(PhotonPlayer player);

        /// <summary>
        /// A bot left.
        /// </summary>
        /// <param name="player"></param>
        public abstract void BotLeft(string botName);

        /// <summary>
        /// A bot joined
        /// </summary>
        /// <param name="player"></param>
        public abstract void BotJoined(string botName);

        /// <summary>
        /// The master client has switched
        /// </summary>
        /// <param name="player"></param>
        public abstract void MasterClientSwitched(PhotonPlayer player);
        
        /// <summary>
        /// Called when the pause menu was opened
        /// </summary>
        public virtual void PauseMenuOpened()
        {

        }

        /// <summary>
        /// Called when the pause menu was closed
        /// </summary>
        public virtual void PauseMenuClosed()
        {

        }

        /// <summary>
        /// Sends a chat message.
        /// </summary>
        /// <param name="content">What is the content of our chat message?</param>
        /// <param name="targets">0 = Everyone, 1 = Our team only (In team game modes)</param>
        public void SendChatMessage(string content, int targets)
        {
            //Check if we have a master player
            if (PhotonNetwork.masterClient != null)
            {
                //Create message content
                Hashtable messageContent = new Hashtable(3);
                //Set type
                messageContent[(byte)0] = 0;
                //Set our message (content)
                messageContent[(byte)1] = content;
                //Set who we want the message to see
                messageContent[(byte)2] = targets;

                if (PhotonNetwork.offlineMode)
                {
                    PhotonNetwork.OnEventCall(1, messageContent, 0);
                }
                else
                {
                    //Send it to the master client only. He decides who will get the actual message.
                    PhotonNetwork.RaiseEvent(1, messageContent, true, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.masterClient.ID } });
                }
            }
        }

        public void SendBotMessage(string botSender, int content)
        {
            //Create message content
            Hashtable messageContent = new Hashtable(3);
            //Set type
            messageContent[(byte)0] = 1;
            //Set sender
            messageContent[(byte)1] = botSender;
            //Set our message (content)
            messageContent[(byte)2] = content;

            if (PhotonNetwork.offlineMode)
            {
                PhotonNetwork.OnEventCall(1, messageContent, 0);
            }
            else
            {
                //Send it to everyone
                PhotonNetwork.RaiseEvent(1, messageContent, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
            }
        }
    }
}
