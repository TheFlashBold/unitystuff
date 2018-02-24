using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This class marks a spawn in the scene
    /// </summary>
    public class Kit_PlayerSpawn : MonoBehaviour
    {
        /// <summary>
        /// The Spawn Group ID of this spawn, used by <see cref="Kit_GameModeBase"/>
        /// </summary>
        public int spawnGroupID = 0;
        /// <summary>
        /// The game modes this spawn can be used for
        /// </summary>
        [Tooltip("Drag all game modes into this array that this spawn should be used for")]
        public Kit_GameModeBase[] gameModes;

        void OnDrawGizmos()
        {
            //Color the spawn based on the group id
            if (spawnGroupID == 0)
                Gizmos.color = Color.red;
            else if (spawnGroupID == 1)
                Gizmos.color = Color.green;
            else if (spawnGroupID == 2)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.black;

            //Draw a cube to indicate
            Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
        }
    }
}
