using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace MarsFPSKit
{
    //The runtime data. It will be stored as an object in the player. Never store runtime data in a scriptable object.
    public class BootsOnGroundRuntimeData
    {
        public Vector3 moveDirection = Vector3.zero;

        /// <summary>
        /// The local movement direction
        /// </summary>
        public Vector3 localMoveDirection = Vector3.zero;

        /// <summary>
        /// Are we grounded?
        /// </summary>
        public bool isGrounded;
        /// <summary>
        /// The character state.
        /// <para>0 = Standing</para>
        /// <para>1 = Crouching</para>
        /// </summary>
        public int state;
        /// <summary>
        /// Are we currently sprinting?
        /// </summary>
        public bool isSprinting;
        /// <summary>
        /// Material type for footsteps
        /// </summary>
        public int currentMaterial;
        /// <summary>
        /// When can we make our next footstep?
        /// </summary>
        public float nextFootstep;

        /// <summary>
        /// Last state of the crouch input
        /// </summary>
        public bool lastCrouch = false;
        /// <summary>
        /// Last state of the jump input
        /// </summary>
        public bool lastJump = false;

        /// <summary>
        /// Should we play slow (aiming) walk animation?
        /// </summary>
        public bool playSlowWalkAnimation;

        #region Fall Damage
        /// <summary>
        /// Are we falling?
        /// </summary>
        public bool falling;
        /// <summary>
        /// How far have we fallen?
        /// </summary>
        public float fallDistance;
        /// <summary>
        /// From where did we fall?
        /// </summary>
        public float fallHighestPoint;
        #endregion
    }

    public class BootsOnGroundSyncRuntimeData
    {
        public Vector3 velocity;
        /// <summary>
        /// Are we grounded?
        /// </summary>
        public bool isGrounded;
        /// <summary>
        /// The character state.
        /// <para>0 = Standing</para>
        /// <para>1 = Crouching</para>
        /// </summary>
        public int state;
        /// <summary>
        /// Are we currently sprinting?
        /// </summary>
        public bool isSprinting;
        /// <summary>
        /// Material type for footsteps
        /// </summary>
        public int currentMaterial;
        /// <summary>
        /// When can we make our next footstep?
        /// </summary>
        public float nextFootstep;
        /// <summary>
        /// Should we play slow (aiming) walk animation?
        /// </summary>
        public bool playSlowWalkAnimation;
    }

    /// <summary>
    /// Helper class for Footsteps
    /// </summary>
    [System.Serializable]
    public class Footstep
    {
        /// <summary>
        /// Sounds for this footstep material
        /// </summary>
        public AudioClip[] clips;
        /// <summary>
        /// Max audio distance for this footstep material
        /// </summary>
        public float maxDistance = 20f;
        /// <summary>
        /// Audio rolloff for this footstep material
        /// </summary>
        public AnimationCurve rollOff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Movement/Boots on ground")]
    public class Kit_Movement_BootsOnGround : Kit_MovementBase
    {
        [Header("Stats")]
        [Tooltip("Sprinting speed")]
        public float sprintSpeed = 6f;
        [Tooltip("Normal walk speed")]
        public float walkSpeed = 6f;
        [Tooltip("Crouch walk speed")]
        public float crouchSpeed = 6f;

        [Tooltip("Multiplier that is applied to Physics.gravity for the character controller")]
        /// <summary>
        /// Multiplier for Physics.gravity
        /// </summary>
        public float gravityMultiplier = 1f;

        [Tooltip("The speed that is applied when you try to jump")]
        public float jumpSpeed = 8f;

        [Header("Character Heights")]
        public float standHeight = 1.8f; //State 0 height
        public float crouchHeight = 1.2f; //State 1 height

        [Header("Camera Positions")]
        public float camPosSmoothSpeed = 6f; //The lerp speed of changing the camera position
        public Vector3 camPosStand = new Vector3(0, 1.65f, 0f);
        public Vector3 camPosCrouch = new Vector3(0, 1.05f, 0f);

        [Header("Fall Damage")]
        /// <summary>
        /// After how many units of falling will we take damage?
        /// </summary>
        public float fallDamageThreshold = 10;
        /// <summary>
        /// With which value will the fall damage be multiplied with?
        /// </summary>
        public float fallDamageMultiplier = 5f;

        [Header("Others")]
        [Tooltip("How many units should we be moved down by default? To be able to walk down stairs properly")]
        public float defaultYmove = -2f; //How many units should we be moved down by default? To be able to walk down stairs properly

        [Header("Footsteps")]
        public float footstepsRunTime = 0.25f; //Time between footsteps when we're running
        public float footstepsWalkTime = 0.4f; //Time between footsteps when we're standing
        public float footstepsCrouchTime = 0.7f; //Time between footsteps when we're crouching

        public float footstepsRunVolume = 0.8f; //Volume for footsteps when we're running
        public float footstepsWalkVolume = 0.4f; //Volume for footsteps when we're walking
        public float footstepsCrouchVolume = 0.1f; //Volume for footsteps when we're crouching

        public Footstep[] allFootsteps; //All footstep materials

        [Header("Fall Down effect")]
        public float fallDownAmount = 10.0f;
        public float fallDownMinOffset = -6.0f;
        public float fallDownMaxoffset = 6.0f;
        public float fallDownTime = 0.3f;
        public float fallDownReturnSpeed = 1f;

        public override void CalculateMovementUpdate(Kit_PlayerBehaviour pb)
        {
            //Check if the object is correct
            if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundRuntimeData))
            {
                pb.customMovementData = new BootsOnGroundRuntimeData();
            }

            BootsOnGroundRuntimeData data = pb.customMovementData as BootsOnGroundRuntimeData;

            //Move transform back
            pb.playerCameraFallDownTransform.localRotation = Quaternion.Slerp(pb.playerCameraFallDownTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

            if (pb.cc.isGrounded)
            {
                #region Fall Damage
                if (data.falling)
                {
                    //Calculate distance we have fallen
                    data.fallDistance = data.fallHighestPoint - pb.transform.position.y;
                    data.falling = false;
                    if (data.fallDistance > fallDamageThreshold)
                    {
                        //Apply Fall distance multiplied with the multiplier (=Fall Damage)
                        pb.ApplyFallDamage(data.fallDistance * fallDamageMultiplier);
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraFallDownTransform, new Vector3(fallDownAmount, Random.Range(fallDownMinOffset, fallDownMaxoffset), 0), fallDownTime));
                        //Tell weapon manager
                        pb.weaponManager.FallDownEffect(pb, true);
                    }
                    else if (data.fallDistance > 0.1f)
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraFallDownTransform, new Vector3(fallDownAmount / 3, Random.Range(fallDownMinOffset, fallDownMaxoffset) / 2, 0), fallDownTime));
                        //Tell weapon manager
                        pb.weaponManager.FallDownEffect(pb, false);
                    }
                }
                #endregion

                #region Main Input
                //Only get input if the cursor is locked
                if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                {
                    //Calculate move direction based on input
                    data.moveDirection.x = pb.input.hor;
                    data.moveDirection.y = 0f;
                    data.moveDirection.z = pb.input.ver;

                    //Check if we want to move
                    if (data.isGrounded && data.moveDirection.sqrMagnitude > 0.005f)
                    {
                        //Call for spawn protection
                        if (pb.spawnProtection)
                        {
                            pb.spawnProtection.PlayerMoved(pb);
                        }
                    }

                    data.moveDirection.y = defaultYmove;

                    //Correct strafe
                    data.moveDirection = Vector3.ClampMagnitude(data.moveDirection, 1f);

                    if (data.lastCrouch != pb.input.crouch)
                    {
                        data.lastCrouch = pb.input.crouch;
                        //Get crouch input
                        if (pb.input.crouch)
                        {
                            //Change state
                            if (data.state == 0)
                            {
                                //We are standing, crouch
                                data.state = 1;
                            }
                            else if (data.state == 1)
                            {
                                //We are crouching, stand up
                                data.state = 0;
                            }
                        }
                    }

                    //Get sprinting input
                    if (pb.input.sprint && data.moveDirection.z > 0.3f && pb.weaponManager.CanRun(pb))
                    {
                        //Check if we can sprint
                        if (data.state == 0)
                        {
                            data.isSprinting = true;
                        }
                        //We cannot sprint
                        else
                        {
                            data.isSprinting = false;
                        }
                    }
                    else
                    {
                        //We are not sprinting
                        data.isSprinting = false;
                    }
                }
                //If not, don't move
                else
                {
                    //Reset move direction
                    data.moveDirection = new Vector3(0f, defaultYmove, 0f);
                    //Reset sprinting
                    data.isSprinting = false;
                }
                #endregion

                #region Character Height
                //Change character height based on the state
                //Standing
                if (data.state == 0)
                {
                    pb.cc.height = standHeight; //Set height
                    pb.cc.center = new Vector3(0f, standHeight / 2, 0f); //Set center
                }
                //Crouch
                else if (data.state == 1)
                {
                    pb.cc.height = crouchHeight; //Set height
                    pb.cc.center = new Vector3(0f, crouchHeight / 2, 0f); //Set center
                }
                #endregion

                //Take rotation in consideration (local to world)
                data.moveDirection = pb.transform.TransformDirection(data.moveDirection);
                //Apply speed based on state
                //Standing
                if (data.state == 0)
                {
                    //Sprinting
                    if (data.isSprinting)
                    {
                        data.moveDirection *= sprintSpeed;
                    }
                    //Not sprinting
                    else
                    {
                        data.moveDirection *= walkSpeed;
                    }
                }
                //Crouching
                else if (data.state == 1)
                {
                    data.moveDirection *= crouchSpeed;
                }

                //Mouse Look multiplier
                data.moveDirection *= pb.looking.GetSpeedMultiplier(pb);
                //Weapon multiplier
                data.moveDirection *= pb.weaponManager.CurrentMovementMultiplier(pb); //Retrive from weapon manager
                //Should play slow animation?
                data.playSlowWalkAnimation = pb.weaponManager.IsAiming(pb); //Retrive from weapon manager

                #region Jump
                if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                {
                    if (data.lastJump != pb.input.jump)
                    {
                        data.lastJump = pb.input.jump;
                        //Get Jump input
                        if (pb.input.jump)
                        {
                            //Check if we can jump
                            if (data.state == 0)
                            {
                                data.moveDirection.y = jumpSpeed;

                                //Call for spawn protection
                                if (pb.spawnProtection)
                                {
                                    pb.spawnProtection.PlayerMoved(pb);
                                }
                            }
                            //If we try to jump and we try to jump, stand up
                            else if (data.state == 1)
                            {
                                data.state = 1;
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                //Save initial falling point
                if (!data.falling)
                {
                    data.fallHighestPoint = pb.transform.position.y;
                    data.falling = true;
                }
                //Check if we moved higher for some reason
                if (pb.transform.position.y > data.fallHighestPoint)
                {
                    data.fallHighestPoint = pb.transform.position.y;
                }
            }

            //Apply gravity
            data.moveDirection += Physics.gravity * Time.deltaTime * gravityMultiplier;
            //Move
            pb.cc.Move(data.moveDirection * Time.deltaTime);
            //Get local movement direction
            data.localMoveDirection = pb.transform.InverseTransformDirection(pb.cc.velocity);
            //Check grounded
            data.isGrounded = pb.cc.isGrounded;

            //Move the camer to the correct position
            #region CameraMove
            //Standing
            if (data.state == 0)
            {
                //Smoothly lerp to the correct state
                pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosStand + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
            }
            //Crouching
            else if (data.state == 1)
            {
                //Smoothly lerp to the correct state
                pb.mouseLookObject.localPosition = Vector3.Lerp(pb.mouseLookObject.localPosition, camPosCrouch + pb.looking.GetCameraOffset(pb), Time.deltaTime * camPosSmoothSpeed);
            }
            #endregion
        }

        public override int GetCurrentWeaponMoveAnimation(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.5f)
                    {
                        //Check if we're sprinting, if return 2
                        if (bogrd.isSprinting) return 2;
                        //If not return 1
                        else
                            return 1;
                    }
                }
            }
            return 0;
        }

        public override float GetCurrentWalkAnimationSpeed(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check if we're grounded
                if (bogrd.isGrounded)
                {
                    //Check if we're moving
                    if (pb.cc.velocity.sqrMagnitude > 0.1f)
                    {
                        //Check if we're sprinting, if return speed divided by sprintSpeed
                        if (bogrd.isSprinting) return pb.cc.velocity.magnitude / sprintSpeed;
                        //If not return speed divided by normal walking speed
                        else
                            return pb.cc.velocity.magnitude / walkSpeed;
                    }
                }
            }
            return 1f;
        }

        public override Vector3 GetMovementDirection(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                return bogrd.localMoveDirection.normalized;
            }
            return Vector3.zero;
        }

        public override bool CanFire(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Just based on our spriting value, if we are sprinting we cannot fire
                if (bogrd.isSprinting && bogrd.isGrounded) return false;
                else return true;
            }
            return false;
        }

        public override bool IsRunning(Kit_PlayerBehaviour pb)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Just based on our spriting value, if we are sprinting we cannot fire
                if (bogrd.isSprinting) return true;
                else return false;
            }
            return false;
        }

        public override void CalculateFootstepsUpdate(Kit_PlayerBehaviour pb)
        {
            //Since controlling and non controlling players use different runtime datas, we have to differentiate
            if (pb.isController)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    //Check if we're grounded
                    if (bogrd.isGrounded)
                    {
                        //Get velMag
                        if (pb.cc.velocity.magnitude > 0.5f)
                        {
                            //We're moving
                            //Check if enough time has passed since the last footstep
                            if (Time.time >= bogrd.nextFootstep)
                            {
                                //Set next footstep sound
                                pb.footstepSource.clip = allFootsteps[bogrd.currentMaterial].clips[Random.Range(0, allFootsteps[bogrd.currentMaterial].clips.Length)];
                                //Set footstep source rolloff and distance
                                pb.footstepSource.maxDistance = allFootsteps[bogrd.currentMaterial].maxDistance;
                                pb.footstepSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, allFootsteps[bogrd.currentMaterial].rollOff);
                                //Set volume and time
                                if (bogrd.state == 0)
                                {
                                    if (bogrd.isSprinting) //Sprinting
                                    {
                                        pb.footstepSource.volume = footstepsRunVolume;
                                        bogrd.nextFootstep = Time.time + footstepsRunTime;
                                    }
                                    else //Normal walking
                                    {
                                        pb.footstepSource.volume = footstepsWalkVolume;
                                        bogrd.nextFootstep = Time.time + footstepsWalkTime;
                                    }
                                }
                                else if (bogrd.state == 1) //Crouching
                                {
                                    pb.footstepSource.volume = footstepsCrouchVolume;
                                    bogrd.nextFootstep = Time.time + footstepsCrouchTime;
                                }
                                //Play
                                pb.footstepSource.Play();
                            }
                        }
                    }
                }
            }
            else
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                {
                    BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                    //Check if we're grounded
                    if (bogsrd.isGrounded)
                    {
                        //Get velMag
                        if (bogsrd.velocity.magnitude > 0.5f)
                        {
                            //We're moving
                            //Check if enough time has passed since our last footstep
                            if (Time.time >= bogsrd.nextFootstep)
                            {
                                //Set next footstep sound
                                pb.footstepSource.clip = allFootsteps[bogsrd.currentMaterial].clips[Random.Range(0, allFootsteps[bogsrd.currentMaterial].clips.Length)];
                                //Set footstep source rolloff and distance
                                pb.footstepSource.maxDistance = allFootsteps[bogsrd.currentMaterial].maxDistance;
                                pb.footstepSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, allFootsteps[bogsrd.currentMaterial].rollOff);
                                //Set volume and time
                                if (bogsrd.state == 0)
                                {
                                    if (bogsrd.isSprinting) //Sprinting
                                    {
                                        pb.footstepSource.volume = footstepsRunVolume;
                                        bogsrd.nextFootstep = Time.time + footstepsRunTime;
                                    }
                                    else //Normal walking
                                    {
                                        pb.footstepSource.volume = footstepsWalkVolume;
                                        bogsrd.nextFootstep = Time.time + footstepsWalkTime;
                                    }
                                }
                                else if (bogsrd.state == 1) //Crouching
                                {
                                    pb.footstepSource.volume = footstepsCrouchVolume;
                                    bogsrd.nextFootstep = Time.time + footstepsCrouchTime;
                                }
                                //Play
                                pb.footstepSource.Play();
                            }
                        }
                    }
                }
            }
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
                {
                    BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                    //Send velocity
                    stream.SendNext(pb.cc.velocity);
                    //Send grounded
                    stream.SendNext(bogrd.isGrounded);
                    //Send state
                    stream.SendNext(bogrd.state);
                    //Send sprinting
                    stream.SendNext(bogrd.isSprinting);
                    //Send material type
                    stream.SendNext(bogrd.currentMaterial);
                    //Send slow walk animation
                    stream.SendNext(bogrd.playSlowWalkAnimation);
                }
                else
                {
                    //Send dummies
                    //Send velocity
                    stream.SendNext(Vector3.zero);
                    //Send grounded
                    stream.SendNext(true);
                    //Send state
                    stream.SendNext(0);
                    //Send sprinting
                    stream.SendNext(false);
                    //Send material type
                    stream.SendNext(0);
                    //Send slow walk animation
                    stream.SendNext(false);
                }
            }
            else if (stream.isReading) //To avoid errors before data arrives
            {
                //Check if the object is correct
                if (pb.customMovementData == null || pb.customMovementData.GetType() != typeof(BootsOnGroundSyncRuntimeData))
                {
                    pb.customMovementData = new BootsOnGroundSyncRuntimeData();
                }
                BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                //Read velocity
                bogsrd.velocity = (Vector3)stream.ReceiveNext();
                //Read grounded
                bogsrd.isGrounded = (bool)stream.ReceiveNext();
                //Read state
                bogsrd.state = (int)stream.ReceiveNext();
                //Read isSprinting
                bogsrd.isSprinting = (bool)stream.ReceiveNext();
                //Read material type
                bogsrd.currentMaterial = (int)stream.ReceiveNext();
                //Read slow animation
                bogsrd.playSlowWalkAnimation = (bool)stream.ReceiveNext();
            }
        }

        public override void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
        {
            if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundRuntimeData))
            {
                BootsOnGroundRuntimeData bogrd = (BootsOnGroundRuntimeData)pb.customMovementData;
                //Check materials
                if (hit.collider.CompareTag("Dirt")) //Check for dirt
                {
                    bogrd.currentMaterial = 1;
                }
                else if (hit.collider.CompareTag("Metal")) //Check for metal
                {
                    bogrd.currentMaterial = 2;
                }
                else if (hit.collider.CompareTag("Wood")) //Check for wood
                {
                    bogrd.currentMaterial = 3;
                }
                else //Else use concrete
                {
                    bogrd.currentMaterial = 0;
                }
            }
        }

        public override Vector3 GetVelocity(Kit_PlayerBehaviour pb)
        {
            if (pb.isController)
            {
                //If we are the controller, just get velocity from the character controller
                return pb.cc.velocity;
            }
            else
            {
                //If we are not, get it from the sync data
                if (pb.customMovementData != null && pb.customMovementData.GetType() == typeof(BootsOnGroundSyncRuntimeData))
                {
                    BootsOnGroundSyncRuntimeData bogsrd = (BootsOnGroundSyncRuntimeData)pb.customMovementData;
                    return bogsrd.velocity;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }
    }
}
