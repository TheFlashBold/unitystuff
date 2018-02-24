using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_BulletMarks : MonoBehaviour
    {
        public Renderer bulletMarksRenderer;

        /// <summary>
        /// Update the material
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="lifeTime"></param>
        public void SetMaterial(Material mat, float lifeTime)
        {
            //Rotate around y axis
            transform.Rotate(Vector3.up, Random.Range(0, 360f));
            //Set material
            bulletMarksRenderer.sharedMaterial = mat; //Set shared material so it can be batched
            //Reset scale
            transform.localScale = Vector3.one;
            //Destroy
            Destroy(gameObject, lifeTime);
        }
    }
}
