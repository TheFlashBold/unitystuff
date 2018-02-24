using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MarsFPSKit
{
    namespace PhotonFriends
    {
        /// <summary>
        /// This implements Photon Friends using PlayerPrefs.
        /// </summary>
        [CreateAssetMenu(menuName = "MarsFPSKit/Photon Friends/Player Prefs")]
        public class Kit_PhotonFriendsPlayerPrefs : Kit_PhotonFriendsBase
        {
            public List<string> myFriends = new List<string>();

            public override void AddFriend(Kit_MainMenu mainMenu, string friendToAdd)
            {
                //Add to the list
                myFriends.Add(friendToAdd);
                //Refresh
                RefreshFriendsList(mainMenu);
                //Save
                Save();
            }

            public override void LoggedIn(Kit_MainMenu mainMenu)
            {
                //Reset List
                myFriends = new List<string>();
                //Load
                Load();
            }

            public override void OnUpdatedFriendList(Kit_MainMenu mainMenu)
            {
                //Redraw
                if (mainMenu.photonFriendsUI)
                {
                    mainMenu.photonFriendsUI.Redraw();
                }
            }

            public override void RefreshFriendsList(Kit_MainMenu mainMenu)
            {
                if (myFriends.Count > 0)
                {
                    //Request new friends
                    PhotonNetwork.FindFriends(myFriends.ToArray());
                }
            }

            public override void RemoveFriend(Kit_MainMenu mainMenu, string friendToRemove)
            {
                if (!myFriends.Remove(friendToRemove))
                {
                    Debug.LogWarning("Could not remove friend " + friendToRemove);
                }
                //Refresh friends list
                RefreshFriendsList(mainMenu);
                //Save
                Save();
            }

            /// <summary>
            /// Loads friends from the Player Prefs
            /// </summary>
            void Load()
            {
                myFriends = PlayerPrefsExtended.GetStringArray("photonFriends").ToList();
            }

            /// <summary>
            /// Saves friends to the Player Prefs
            /// </summary>
            void Save()
            {
                PlayerPrefsExtended.SetStringArray("photonFriends", myFriends.ToArray());
            }
        }
    }
}
