using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class Kit_KillFeedManager : Kit_KillFeedBase
    {
        [Header("References")]
        public Kit_IngameMain main;

        /// <summary>
        /// The root transform of the entries
        /// </summary>
        public RectTransform killFeedGo;
        /// <summary>
        /// The entry prefab
        /// </summary>
        public GameObject killFeedPrefab;

        [Header("Settings")]
        /// <summary>
        /// The color of team 1 in the killfeed
        /// </summary>
        public Color team1Color;
        /// <summary>
        /// The color of team 2 in the killfeed
        /// </summary>
        public Color team2Color;

        /// <summary>
        /// Add an entry to the killfeed
        /// </summary>
        /// <param name="killer">Who shot</param>
        /// <param name="killed">Who was killed</param>
        /// <param name="gun">Which weapon was used to kill</param>
        public override void Append(bool botKiller, int killer, bool botKilled, int killed, int gun)
        {
            GameObject go = Instantiate(killFeedPrefab, killFeedGo, false);
            //Reset scale
            go.transform.localScale = Vector3.one;
            //Set it up
            go.GetComponent<Kit_KillFeedEntry>().SetUp(botKiller, killer, botKilled, killed, gun, this);
        }
    }
}
