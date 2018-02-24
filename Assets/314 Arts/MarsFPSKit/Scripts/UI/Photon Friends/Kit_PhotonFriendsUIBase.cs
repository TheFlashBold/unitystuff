using UnityEngine;

namespace MarsFPSKit
{
    namespace PhotonFriends
    {
        public abstract class Kit_PhotonFriendsUIBase : MonoBehaviour
        {
            public Kit_MainMenu mainMenu;

            /// <summary>
            /// Redraws the Friends UI
            /// </summary>
            public abstract void Redraw();

            /// <summary>
            /// Adds a new friend. Call the base or <see cref="Kit_PhotonFriendsBase.AddFriend(Kit_MainMenu, string)"/> if you override it.
            /// </summary>
            /// <param name="friendToAdd"></param>
            public virtual void AddFriend(string friendToAdd)
            {
                if (mainMenu.photonFriends)
                {
                    mainMenu.photonFriends.AddFriend(mainMenu, friendToAdd);
                }
            }

            /// <summary>
            /// Attempts to remove a friend. Call the base or <see cref="Kit_PhotonFriendsBase.RemoveFriend(Kit_MainMenu, string)"/> if you override it.
            /// </summary>
            /// <param name="friendToRemove"></param>
            public virtual void RemoveFriend(string friendToRemove)
            {
                if (mainMenu.photonFriends)
                {
                    mainMenu.photonFriends.RemoveFriend(mainMenu, friendToRemove);
                }
            }
        }
    }
}