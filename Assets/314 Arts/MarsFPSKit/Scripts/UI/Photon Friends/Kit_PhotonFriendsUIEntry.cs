using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace PhotonFriends
    {
        /// <summary>
        /// This class holds all information for a friend entry
        /// </summary>
        public class Kit_PhotonFriendsUIEntry : MonoBehaviour
        {
            /// <summary>
            /// The text for displaying this friend's name
            /// </summary>
            public Text friendName;
            /// <summary>
            /// The button for joining this friend's room
            /// </summary>
            public Button friendJoin;
            /// <summary>
            /// The button for deleting this friend from the friends list
            /// </summary>
            public Button friendDelete;
            /// <summary>
            /// The image that displays this friend's state
            /// </summary>
            public Image friendStateImage;

            /// <summary>
            /// The friend this entry belongs to
            /// </summary>
            public FriendInfo myInfo;

            [HideInInspector]
            /// <summary>
            /// Reference to the main menu
            /// </summary>
            public Kit_MainMenu mainMenu;

            /// <summary>
            /// Called from <see cref="friendJoin"/>
            /// </summary>
            public void Join()
            {
                if (myInfo.IsInRoom)
                {
                    mainMenu.JoinRoom(myInfo.Room);
                }
            }

            /// <summary>
            /// Called from <see cref="friendDelete"/>
            /// </summary>
            public void Remove()
            {
                if (mainMenu.photonFriendsUI)
                {
                    //Call it on the UI
                    mainMenu.photonFriendsUI.RemoveFriend(myInfo.UserId);
                    //Directly destroy this game object
                    Destroy(gameObject);
                }
            }
        }
    }
}
