using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This class holds information for a voting entry
    /// </summary>
    public class Kit_MapVotingEntry : MonoBehaviour
    {
        /// <summary>
        /// Text to display the map with
        /// </summary>
        public Image mapImage;
        /// <summary>
        /// Text to display the map's name with
        /// </summary>
        public Text mapName;
        /// <summary>
        /// Text to display the game mode name with
        /// </summary>
        public Text gameModeName;

        /// <summary>
        /// Percentage filled background image
        /// </summary>
        public Image votePercentageImage;
        /// <summary>
        /// Text that displays voting percentage
        /// </summary>
        public Text votePercentageText;

        /// <summary>
        /// Which combo is this one voting for?
        /// </summary>
        public int myVote;

        /// <summary>
        /// Updates our CustomProperties so that we vote for this combo
        /// </summary>
        public void VoteForThis()
        {
            Hashtable table = PhotonNetwork.player.CustomProperties;
            table["vote"] = myVote;
            PhotonNetwork.player.SetCustomProperties(table);
        }
    }
}
