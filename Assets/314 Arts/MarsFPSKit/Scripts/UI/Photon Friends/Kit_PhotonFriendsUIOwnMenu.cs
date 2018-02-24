using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace PhotonFriends
    {
        /// <summary>
        /// Implements a friends menu that uses its own sub menu
        /// </summary>
        public class Kit_PhotonFriendsUIOwnMenu : Kit_PhotonFriendsUIBase
        {
            /// <summary>
            /// Root of the Friends Menu
            /// </summary>
            public GameObject root;

            [Header("Friends List")]
            /// <summary>
            /// Active list entries are kept here
            /// </summary>
            private List<Kit_PhotonFriendsUIEntry> activeFriendEntries = new List<Kit_PhotonFriendsUIEntry>();

            /// <summary>
            /// The entry prefab for the friends list
            /// </summary>
            public GameObject friendEntryPrefab;

            /// <summary>
            /// Where the prefab <see cref="friendEntryPrefab"/> goes
            /// </summary>
            public RectTransform friendEntryGo;

            [Header("Add Friends")]
            public InputField addFriendInputField;

            void Start()
            {
                //Hide menu
                root.SetActive(false);
            }

            public override void Redraw()
            {
                //Delete all active entries
                for (int i = 0; i < activeFriendEntries.Count; i++)
                {
                    if (activeFriendEntries[i])
                    {
                        Destroy(activeFriendEntries[i].gameObject);
                    }
                }
                //Reset the list
                activeFriendEntries = new List<Kit_PhotonFriendsUIEntry>();

                //Get the friend List
                List<FriendInfo> friends = PhotonNetwork.Friends;
                //Check if it exists
                if (friends != null)
                {
                    //Redraw
                    for (int i = 0; i < friends.Count; i++)
                    {
                        Kit_PhotonFriendsUIEntry entry = Instantiate(friendEntryPrefab, friendEntryGo, false).GetComponent<Kit_PhotonFriendsUIEntry>();
                        //Reset scale
                        entry.transform.localScale = Vector3.one;
                        //Set up the name
                        entry.friendName.text = friends[i].UserId;
                        //Set up join button
                        if (friends[i].IsInRoom)
                        {
                            entry.friendJoin.gameObject.SetActive(true);
                        }
                        else
                        {
                            entry.friendJoin.gameObject.SetActive(false);
                        }
                        //Set Color
                        if (friends[i].IsOnline)
                        {
                            if (friends[i].IsInRoom)
                            {
                                entry.friendStateImage.color = Color.green;
                            }
                            else
                            {
                                entry.friendStateImage.color = Color.blue;
                            }
                        }
                        else
                        {
                            entry.friendStateImage.color = Color.red;
                        }
                        //Set friend value
                        entry.myInfo = friends[i];
                        //Set Main Menu
                        entry.mainMenu = mainMenu;
                        //Add to the list
                        activeFriendEntries.Add(entry);
                    }
                }
            }

            public void OpenMenu()
            {
                //Refresh
                Refresh();
                //Go to Main page on the main menu
                mainMenu.ChangeMenuState(MenuState.main);
                //Disable main menu
                mainMenu.section_main.SetActive(false);
                //Redraw
                Redraw();
                //Enable Friends Menu
                root.SetActive(true);
            }

            public void CloseMenu()
            {
                //Disable Friends menu
                root.SetActive(false);
                //Go to Main page on the main menu
                mainMenu.ChangeMenuState(MenuState.main);
            }

            /// <summary>
            /// Called from the Menu Button to add a friend whose name is inside the input field
            /// </summary>
            public void AddFriendButtonPress()
            {
                //Check if we have entered a name
                if (!addFriendInputField.text.IsNullOrWhiteSpace())
                {
                    //Add that friend
                    AddFriend(addFriendInputField.text);
                    //Reset text
                    addFriendInputField.text = "";
                }
            }

            /// <summary>
            /// Refreshes the friends list
            /// </summary>
            public void Refresh()
            {
                if (mainMenu.photonFriends)
                {
                    mainMenu.photonFriends.RefreshFriendsList(mainMenu);
                }
            }
        }
    }
}
