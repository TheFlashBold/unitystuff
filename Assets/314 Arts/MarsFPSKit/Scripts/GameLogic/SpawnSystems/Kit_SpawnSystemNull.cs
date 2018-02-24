using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This spawn system will always return true
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Spawn Systems/Null Spawn System")]
    public class Kit_SpawnSystemNull : Kit_SpawnSystemBase
    {
        public override bool CheckSpawnPosition(Transform spawnPoint, PhotonPlayer spawningPlayer)
        {
            return true;
        }
    }
}