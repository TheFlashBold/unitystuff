using UnityEngine;

namespace MarsFPSKit
{
    namespace PhotonFriends
    {
        /// <summary>
        /// Used to incorporate a Photon Friends system
        /// </summary>
        public abstract class Kit_PhotonFriendsBase : ScriptableObject
        {
            /// <summary>
            /// Called when we successfully logged in
            /// </summary>
            /// <param name="mainMenu"></param>
            public abstract void LoggedIn(Kit_MainMenu mainMenu);

            /// <summary>
            /// Called when the user wants to refresh the friends list
            /// </summary>
            /// <param name="mainMenu"></param>
            public abstract void RefreshFriendsList(Kit_MainMenu mainMenu);

            /// <summary>
            /// Called when the Photon Friends list was updated
            /// </summary>
            /// <param name="mainMenu"></param>
            public abstract void OnUpdatedFriendList(Kit_MainMenu mainMenu);

            /// <summary>
            /// Add a new friend
            /// </summary>
            /// <param name="mainMenu"></param>
            /// <param name="friendToAdd"></param>
            public abstract void AddFriend(Kit_MainMenu mainMenu, string friendToAdd);

            /// <summary>
            /// Attempts to remove given friend
            /// </summary>
            /// <param name="mainMenu"></param>
            /// <param name="friendToRemove"></param>
            public abstract void RemoveFriend(Kit_MainMenu mainMenu, string friendToRemove);
        }
    }
}
