using System;
using System.Collections.Generic;
using MarsFPSKit.Weapons;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarsFPSKit
{
    /// <summary>
    /// A modern animating player model, which calculates velocity locally. It will read data from <see cref="Kit_Movement_BootsOnGround"/>
    /// </summary>
    public class Kit_ThirdPersonModernPlayerModel : Kit_ThirdPersonPlayerModel
    {
        /// <summary>
        /// These are all renderers that will be set to "shadows only" when using first person
        /// </summary>
        public Renderer[] fpShadowOnlyRenderers;
        /// <summary>
        /// These are colliders for hit detection
        /// </summary>
        public Collider[] raycastColliders;

        [Header("Ragdoll")]
        /// <summary>
        /// The prefab for the ragdoll
        /// </summary>
        public GameObject ragdollPrefab;
        /// <summary>
        /// To copy the ragdoll position, we use these colliders
        /// </summary>
        public Collider[] ragdollColliderCopy;
        /// <summary>
        /// The root object for the ragdoll instantiations
        /// </summary>
        public Transform ragdollGo;

        /// <summary>
        /// This is the reference to our player
        /// </summary>
        private Kit_PlayerBehaviour kpb;
        //Cache movement transform
        private Transform movementTransform;
        //To calculate movement
        private Vector3 position;
        private Vector3 oldPosition;

        //Speed
        public float smoothedSpeed;
        private float rawSpeed;
        public Vector3 direction;
        public Vector3 localDirection;

        public override void FirstPerson()
        {
            //Set all renderers to shadow only
            for (int i = 0; i < fpShadowOnlyRenderers.Length; i++)
            {
                fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        public override void SetupModel(Kit_PlayerBehaviour newKpb)
        {
            //Store the reference to our player
            kpb = newKpb;
            //Cache movement transform
            movementTransform = kpb.transform;
            //Store position
            position = movementTransform.position;
            oldPosition = movementTransform.position;

            enabled = true;
        }

        public override void ThirdPerson()
        {
            //Set all renderers to normal shadows
            for (int i = 0; i < fpShadowOnlyRenderers.Length; i++)
            {
                fpShadowOnlyRenderers[i].shadowCastingMode = ShadowCastingMode.On;
            }
        }

        public override void SetAnimType(ThirdPersonAnimType animType)
        {
            if (animType == ThirdPersonAnimType.Pistol)
            {
                //Set anim type
                anim.SetInteger("animType", 1);
                //Tell the animator it changed
                anim.SetTrigger("animTypeChanged");
            }
            else if (animType == ThirdPersonAnimType.Rifle)
            {
                //Set anim type
                anim.SetInteger("animType", 0);
                //Tell the animator it changed
                anim.SetTrigger("animTypeChanged");
            }
        }

        public override void PlayWeaponFireAnimation(ThirdPersonAnimType animType)
        {
            if (anim)
            {
                if (animType == ThirdPersonAnimType.Rifle)
                {
                    anim.Play("Fire Rifle", 2, 0f);
                }
                else
                {
                    anim.Play("Fire Pistol", 2, 0f);
                }
            }
        }

        public override void PlayWeaponReloadAnimation(ThirdPersonAnimType animType)
        {
            if (anim)
            {
                if (animType == ThirdPersonAnimType.Rifle)
                {
                    anim.CrossFade("Reload Rifle", 0.1f, 1, 0f);
                }
                else
                {
                    anim.CrossFade("Reload Pistol", 0.1f, 1, 0f);
                }
            }
        }

        public override void AbortWeaponAnimations()
        {
            if (anim)
            {
                anim.CrossFade("Null", 0.05f, 1, 0f); //Reload layer
                anim.CrossFade("Null", 0.05f, 2, 0f); //Fire layer
            }
            //Also stop reload sounds
            soundReload.Stop();
        }

        public override void CreateRagdoll()
        {
            //Create data pool with needed information
            object[] ragdollData = new object[6 + (ragdollColliderCopy.Length * 2)];
            //0 = Velocity
            ragdollData[0] = kpb.movement.GetVelocity(kpb);
            //Shot force
            ragdollData[1] = kpb.ragdollForward;
            ragdollData[2] = kpb.ragdollForce;
            ragdollData[3] = kpb.ragdollPoint;
            ragdollData[4] = kpb.ragdollId;
            ragdollData[5] = kpb.isBot;
            List<object> ragdollInfo = new List<object>();

            //Ragdoll pos and rot
            for (int i = 0; i < ragdollColliderCopy.Length; i++)
            {
                ragdollInfo.Add(ragdollColliderCopy[i].transform.position);
                ragdollInfo.Add(ragdollColliderCopy[i].transform.rotation);
            }

            //Add to the array
            for (int i = 0; i < ragdollInfo.Count; i++)
            {
                ragdollData[6 + i] = ragdollInfo[i];
            }

            //Instantiate Ragdoll
            PhotonNetwork.Instantiate(ragdollPrefab.name, ragdollGo.position, ragdollGo.rotation, 0, ragdollData);
        }

        void Update()
        {
            //Smooth speed
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Time.deltaTime * 10f);

            #region Animator Update
            //Update the animator with our locally calculated values
            //Speed
            anim.SetFloat("walkSpeed", smoothedSpeed, 0.1f, Time.deltaTime);
            //Direction
            anim.SetFloat("walkX", localDirection.x, 0.1f, Time.deltaTime);
            anim.SetFloat("walkZ", localDirection.z, 0.1f, Time.deltaTime);
            #endregion

            #region Movement Data
            if (kpb)
            {
                if (kpb.customMovementData != null)
                {
                    //Local data
                    if (kpb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                    {
                        BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)kpb.customMovementData;
                        //Update state
                        anim.SetInteger("state", bogrd.state);
                        //Update blend
                        anim.SetFloat("stateBlend", (float)bogrd.state, 0.1f, Time.deltaTime);
                        //Update aiming
                        anim.SetBool("aiming", bogrd.playSlowWalkAnimation);
                    }
                    //Synced data
                    else if (kpb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                    {
                        BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)kpb.customMovementData;
                        //Update state
                        anim.SetInteger("state", bogsrd.state);
                        //Update blend
                        anim.SetFloat("stateBlend", (float)bogsrd.state, 0.1f, Time.deltaTime);
                        //Update aiming
                        anim.SetBool("aiming", bogsrd.playSlowWalkAnimation);
                    }
                }
            }
            #endregion

            #region Looking Data
            if (kpb.customMouseLookData != null)
            {
                if (kpb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    BasicMouseLookRuntimeData bmlrd = (BasicMouseLookRuntimeData)kpb.customMouseLookData;
                    //Smoothly update looking value
                    anim.SetFloat("lookY", bmlrd.finalMouseY, 0.1f, Time.deltaTime);
                    //Smoothly update leaning value
                    anim.SetFloat("leaning", bmlrd.leaningState, 0.1f, Time.deltaTime);
                }
                else if (kpb.customMouseLookData.GetType() == typeof(BasicMouseLookOthersRuntimeData))
                {
                    BasicMouseLookOthersRuntimeData bmlord = (BasicMouseLookOthersRuntimeData)kpb.customMouseLookData;
                    //Smoothly update looking value
                    anim.SetFloat("lookY", bmlord.mouseY, 0.1f, Time.deltaTime);
                    //Smoothly update leaning value
                    anim.SetFloat("leaning", bmlord.leaningState, 0.1f, Time.deltaTime);
                }
            }
            #endregion
        }

        void LateUpdate()
        {
            //Pause Error
            if (Time.deltaTime <= float.Epsilon)
            {
                smoothedSpeed = 0f;
                rawSpeed = 0f;
                return;
            }

            //Update position
            position = movementTransform.position;

            //Calculate speed
            rawSpeed = (position - oldPosition).magnitude / Time.deltaTime;

            //Update direction
            if (position != oldPosition)
            {
                direction = Vector3.Normalize(position - oldPosition);
            }
            else
            {
                direction = Vector3.zero;
            }

            //Update local direction
            localDirection = movementTransform.InverseTransformDirection(direction);

            //Update old position
            oldPosition = position;
        }

        public void OnAnimatorIKRelay()
        {
            if (enabled && kpb && kpb.weaponManager)
            {
                kpb.weaponManager.OnAnimatorIKCallback(kpb, anim);
            }
        }
    }
}
