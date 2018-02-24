using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This ragdoll behaviour will set the camera to a first person mode if we died
    /// </summary>
    public class Kit_FirstPersonRagdoll : Kit_RagdollObject
    {
        public Transform cameraPos;

        public override void OurRagdoll()
        {
            //Find main
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
            if (main)
            {
                //Set camera
                main.activeCameraTransform = cameraPos;
            }
        }

        void OnDestroy()
        {
            //Find main
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
            if (main)
            {
                //Set camera
                if (main.activeCameraTransform == cameraPos)
                {
                    main.activeCameraTransform = main.spawnCameraPosition;
                }
            }
        }
    }
}
