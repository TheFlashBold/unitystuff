using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MarsFPSKit
{
    /// <summary>
    /// This object holds ragdoll information
    /// </summary>
    public class Kit_RagdollObject : Photon.MonoBehaviour
    {
        /// <summary>
        /// After how many seconds is this ragdoll going to be destroyed?
        /// </summary>
        public float lifeTime = 30f;
        /// <summary>
        /// The colliders to copy the ragdolls to
        /// </summary>
        public Collider[] ragdollColliderCopy;

        public Rigidbody[] ragdollRigidbodies;

        void Start()
        {
            object[] data = photonView.instantiationData;
            //Get Velocity
            Vector3 velocity = (Vector3)data[0];
            //Forward
            Vector3 forward = (Vector3)data[1];
            //Force
            float force = (float)data[2];
            //Point
            Vector3 point = (Vector3)data[3];
            //ID
            int id = (int)data[4];
            bool isBot = (bool)data[5];
            //The current collider being copied
            int currentRagdollId = 0;
            for (int i = 0; i < ragdollColliderCopy.Length * 2; i++)
            {
                if (i % 2 == 0)
                {
                    //Since each one consists of two pairs, we only read every second
                    ragdollColliderCopy[currentRagdollId].transform.position = (Vector3)data[6 + i];
                    ragdollColliderCopy[currentRagdollId].transform.rotation = (Quaternion)data[7 + i];
                    currentRagdollId++;
                }
            }
            //Copy velocity
            for (int i = 0; i < ragdollRigidbodies.Length; i++)
            {
                ragdollRigidbodies[i].isKinematic = false;
                ragdollRigidbodies[i].velocity = velocity;
            }
            //Apply force
            ragdollRigidbodies[id].AddForceAtPosition(forward * force, point);
            //Destroy
            if (photonView.isMine)
            {
                if (!isBot)
                {
                    OurRagdoll();
                }
                StartCoroutine(DestroyTimed());
            }
        }

        IEnumerator DestroyTimed()
        {
            yield return new WaitForSeconds(lifeTime);
            PhotonNetwork.Destroy(gameObject);
        }

        /// <summary>
        /// Called if this is the ragdoll of our local player. You might switch the camera to first person here or do something else
        /// </summary>
        public virtual void OurRagdoll()
        {

        }
    }
}
