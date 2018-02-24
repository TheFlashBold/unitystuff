using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    [System.Serializable]
    public class BulletMarkMaterials
    {
        public Material[] materials;
    }

    /// <summary>
    /// This is a simple impact processor. It only instantiates bullet marks / particles. A more advanced one could use object pooling.
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Impact Processors/Simple")]
    public class Kit_SimpleImpactProcessor : Kit_ImpactParticleProcessor
    {
        [Header("Material impacts")]
        /// <summary>
        /// Impact particles array
        /// </summary>
        public GameObject[] impactParticles;

        /// <summary>
        /// The Prefab for the bullet markss. Needs to have <see cref="Kit_BulletMarks"/> on it 
        /// </summary>
        public GameObject bulletMarksPrefab;

        /// <summary>
        /// Bullet marks array
        /// </summary>
        public BulletMarkMaterials[] bulletMarksMaterials;

        /// <summary>
        /// How much is the bullet marks object moved into the normal's direction?
        /// </summary>
        public float bulletMarksNormalOffset;

        /// <summary>
        /// Bullet mark lifetime in seconds
        /// </summary>
        public float bulletMarksLifetime = 60f;

        [Header("Enemy impacts")]
        public GameObject[] enemyImpactParticles;

        public override void ProcessImpact(Vector3 pos, Vector3 normal, int materialType, Transform parent = null)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            GameObject go = Instantiate(impactParticles[materialType], pos, rot); //Instantiate appropriate particle
            if (parent) go.transform.parent = parent; //Set parent if we have one
                                                      //The instantiated GO should destroy itself

            //Bullet marks
            GameObject bm = Instantiate(bulletMarksPrefab, pos + normal * bulletMarksNormalOffset, rot);
            //The instantiated GO should destroy itself
            //Set material
            bm.GetComponent<Kit_BulletMarks>().SetMaterial(bulletMarksMaterials[materialType].materials[Random.Range(0, bulletMarksMaterials[materialType].materials.Length)], bulletMarksLifetime);
            if (parent) bm.transform.parent = parent; //Set parent if we have one
        }

        public override void ProcessEnemyImpact(Vector3 pos, Vector3 normal)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            Instantiate(enemyImpactParticles[Random.Range(0, enemyImpactParticles.Length)], pos, rot); //Instantiate appropriate particle
                                                                                                       //The instantiated GO should destroy itself
        }
    }
}
