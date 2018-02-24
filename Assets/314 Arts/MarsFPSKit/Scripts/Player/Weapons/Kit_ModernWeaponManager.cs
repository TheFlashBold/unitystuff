using System;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// This will store runtime data for the controlling player
        /// </summary>
        public class WeaponManagerControllerRuntimeData
        {
            /// <summary>
            /// Our currently selected weapon
            /// </summary>
            public int currentWeapon;

            /// <summary>
            /// The weapon we want to select
            /// </summary>
            public int desiredWeapon;

            /// <summary>
            /// If secondary weapon is -1 this will be set to false
            /// </summary>
            public bool canUseSecondaryWeapon = true;

            /// <summary>
            /// The data of our two weapons that are in use. None of these should ever be null.
            /// </summary>
            public WeaponReference[] weaponsInUse = new WeaponReference[2];

            /// <summary>
            /// Last state for the weapon one input
            /// </summary>
            public bool lastWeaponOne;
            /// <summary>
            /// Last state for the weapon two input
            /// </summary>
            public bool lastWeaponTwo;
            /// <summary>
            /// Last state for the drop weapon
            /// </summary>
            public bool lastDropWeapon;

            /// <summary>
            /// Are we currently switching weapons?
            /// </summary>
            public bool switchInProgress;
            /// <summary>
            /// When is the next switching phase over?
            /// </summary>
            public float switchNextEnd; //This is only so we don't have to use a coroutine
            /// <summary>
            /// The current phase of switching
            /// </summary>
            public int switchPhase;
            /// <summary>
            /// Raycast hit for the pickup process
            /// </summary>
            public RaycastHit hit;

            #region IK
            /// <summary>
            /// Weight of the left hand IK
            /// </summary>
            public float leftHandIKWeight;
            #endregion
        }

        /// <summary>
        /// This contains the reference to a generic weapon.
        /// </summary>
        public class WeaponReference
        {
            public Kit_WeaponBase behaviour;
            public object runtimeData;
            public int[] attachments;
        }

        /// <summary>
        /// Other players will keep this runtime data, to replicate the behaviour based on what this player tells them
        /// </summary>
        public class WeaponManagerControllerOthersRuntimeData
        {
            //Our currently selected weapon
            public int currentWeapon;

            //The weapon we want to select
            public int desiredWeapon;

            //The data of our two weapons that are in use. None of these should ever be null.
            public WeaponReference[] weaponsInUse = new WeaponReference[2];

            /// <summary>
            /// Are we currently switching weapons?
            /// </summary>
            public bool switchInProgress;
            /// <summary>
            /// When is the next switching phase over?
            /// </summary>
            public float switchNextEnd; //This is only so we don't have to use a coroutine
            /// <summary>
            /// The current phase of switching
            /// </summary>
            public int switchPhase;

            #region IK
            /// <summary>
            /// Weight of the left hand IK
            /// </summary>
            public float leftHandIKWeight;
            #endregion
        }

        public enum DeadDrop { None, Selected, Both }

        /// <summary>
        /// This is a modern, generic weapon manager. Weapons are put away and then taken out, like in COD or Battlefield. Supports 2 weapons.
        /// </summary>
        [CreateAssetMenu(menuName = ("MarsFPSKit/Weapons/Modern Weapon Manager"))]
        public class Kit_ModernWeaponManager : Kit_WeaponManagerBase
        {
            /// <summary>
            /// Main drop prefab!
            /// </summary>
            public GameObject dropPrefab;
            /// <summary>
            /// Layers that will be hit by the pickup raycast
            /// </summary>
            public LayerMask pickupLayers;
            /// <summary>
            /// Distance for the pickup raycast
            /// </summary>
            public float pickupDistance = 3f;
            /// <summary>
            /// Which weapons should be dropped upon death?
            /// </summary>
            public DeadDrop uponDeathDrop;
            /// <summary>
            /// How fast does the weapon position change?
            /// </summary>
            public float weaponPositionChangeSpeed = 5f;
            /// <summary>
            /// Can we switch while we are running?
            /// </summary>
            public bool allowSwitchingWhileRunning;

            public override void SetupManager(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerRuntimeData runtimeData = new WeaponManagerControllerRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                //Hide crosshair
                pb.main.hud.DisplayCrosshair(0f);

                Debug.Log("[Weapon Manager] Manager Setup");
                int primaryWeapon = (int)instantiationData[3];
                int secondaryWeapon = (int)instantiationData[4];

                int primaryAttachmentsLength = (int)instantiationData[5];
                int[] primaryAttachments = new int[primaryAttachmentsLength];
                for (int i = 0; i < primaryAttachments.Length; i++)
                {
                    primaryAttachments[i] = (int)instantiationData[6 + i];
                }

                int secondaryAttachmentsLength = (int)instantiationData[6 + primaryAttachmentsLength];
                int[] secondaryAttachments = new int[secondaryAttachmentsLength];
                for (int i = 0; i < secondaryAttachments.Length; i++)
                {
                    secondaryAttachments[i] = (int)instantiationData[7 + primaryAttachmentsLength + i];
                }

                //Setup both weapons
                Kit_WeaponInformation primaryWeaponInfo = pb.gameInformation.allWeapons[primaryWeapon];

                //Get their behaviour modules
                Kit_WeaponBase primaryWeaponBehaviour = primaryWeaponInfo.weaponBehaviour;

                //Setup weapons in use array
                if (secondaryWeapon >= 0)
                {
                    //Two weapons are in use
                    runtimeData.weaponsInUse = new WeaponReference[2];
                }
                else
                {
                    //Only one weapon is in use...
                    runtimeData.weaponsInUse = new WeaponReference[1];
                }

                //Setup primary
                primaryWeaponBehaviour.SetupValues(primaryWeapon); //This sets up values in the object itself, nothing else
                object primaryRuntimeData = primaryWeaponBehaviour.SetupFirstPerson(pb, primaryAttachments); //This creates the first person objects
                //Set data
                runtimeData.weaponsInUse[0] = new WeaponReference();
                runtimeData.weaponsInUse[0].behaviour = primaryWeaponBehaviour;
                runtimeData.weaponsInUse[0].runtimeData = primaryRuntimeData;
                runtimeData.weaponsInUse[0].attachments = primaryAttachments;
                //Setup third person
                primaryWeaponBehaviour.SetupThirdPerson(pb, primaryWeaponBehaviour as Kit_ModernWeaponScript, primaryRuntimeData, primaryAttachments);

                if (secondaryWeapon >= 0)
                {
                    Kit_WeaponInformation secondaryWeaponInfo = pb.gameInformation.allWeapons[secondaryWeapon];
                    Kit_WeaponBase secondaryWeaponBehaviour = secondaryWeaponInfo.weaponBehaviour;
                    runtimeData.canUseSecondaryWeapon = true;
                    //Setup secondary
                    secondaryWeaponBehaviour.SetupValues(secondaryWeapon); //This sets up values in the object itself, nothing else
                    object secondaryRuntimeData = secondaryWeaponBehaviour.SetupFirstPerson(pb, secondaryAttachments); //This creates the first person objects
                                                                                                                       //Set data
                    runtimeData.weaponsInUse[1] = new WeaponReference();
                    runtimeData.weaponsInUse[1].behaviour = secondaryWeaponBehaviour;
                    runtimeData.weaponsInUse[1].runtimeData = secondaryRuntimeData;
                    runtimeData.weaponsInUse[1].attachments = secondaryAttachments;
                    //Setup third person
                    secondaryWeaponBehaviour.SetupThirdPerson(pb, secondaryWeaponBehaviour as Kit_ModernWeaponScript, secondaryRuntimeData, secondaryAttachments);
                }
                else
                {
                    //No secondary weapon specified, disable it
                    runtimeData.canUseSecondaryWeapon = false;
                }

                //Select current weapon
                primaryWeaponBehaviour.DrawWeapon(pb, primaryRuntimeData);
                //Set current weapon
                runtimeData.currentWeapon = 0;
                //Set time
                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.drawTime;
                //Set phase
                runtimeData.switchPhase = 1;
                //Set switching
                runtimeData.switchInProgress = true;
            }

            public override void SetupManagerBot(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerRuntimeData runtimeData = new WeaponManagerControllerRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                Debug.Log("[Weapon Manager] Manager Setup Bots");
                int primaryWeapon = (int)instantiationData[3];
                int secondaryWeapon = (int)instantiationData[4];

                int primaryAttachmentsLength = (int)instantiationData[5];
                int[] primaryAttachments = new int[primaryAttachmentsLength];
                for (int i = 0; i < primaryAttachments.Length; i++)
                {
                    primaryAttachments[i] = (int)instantiationData[6 + i];
                }

                int secondaryAttachmentsLength = (int)instantiationData[6 + primaryAttachmentsLength];
                int[] secondaryAttachments = new int[secondaryAttachmentsLength];
                for (int i = 0; i < secondaryAttachments.Length; i++)
                {
                    secondaryAttachments[i] = (int)instantiationData[7 + primaryAttachmentsLength + i];
                }

                //Setup both weapons
                Kit_WeaponInformation primaryWeaponInfo = pb.gameInformation.allWeapons[primaryWeapon];

                //Get their behaviour modules
                Kit_WeaponBase primaryWeaponBehaviour = primaryWeaponInfo.weaponBehaviour;

                //Setup weapons in use array
                if (secondaryWeapon >= 0)
                {
                    //Two weapons are in use
                    runtimeData.weaponsInUse = new WeaponReference[2];
                }
                else
                {
                    //Only one weapon is in use...
                    runtimeData.weaponsInUse = new WeaponReference[1];
                }

                //Setup primary
                primaryWeaponBehaviour.SetupValues(primaryWeapon); //This sets up values in the object itself, nothing else
                object primaryRuntimeData = primaryWeaponBehaviour.SetupFirstPerson(pb, primaryAttachments); //This creates the first person objects
                //Set data
                runtimeData.weaponsInUse[0] = new WeaponReference();
                runtimeData.weaponsInUse[0].behaviour = primaryWeaponBehaviour;
                runtimeData.weaponsInUse[0].runtimeData = primaryRuntimeData;
                runtimeData.weaponsInUse[0].attachments = primaryAttachments;
                //Setup third person
                primaryWeaponBehaviour.SetupThirdPerson(pb, primaryWeaponBehaviour as Kit_ModernWeaponScript, primaryRuntimeData, primaryAttachments);

                if (secondaryWeapon >= 0)
                {
                    Kit_WeaponInformation secondaryWeaponInfo = pb.gameInformation.allWeapons[secondaryWeapon];
                    Kit_WeaponBase secondaryWeaponBehaviour = secondaryWeaponInfo.weaponBehaviour;
                    runtimeData.canUseSecondaryWeapon = true;
                    //Setup secondary
                    secondaryWeaponBehaviour.SetupValues(secondaryWeapon); //This sets up values in the object itself, nothing else
                    object secondaryRuntimeData = secondaryWeaponBehaviour.SetupFirstPerson(pb, secondaryAttachments); //This creates the first person objects
                                                                                                                       //Set data
                    runtimeData.weaponsInUse[1] = new WeaponReference();
                    runtimeData.weaponsInUse[1].behaviour = secondaryWeaponBehaviour;
                    runtimeData.weaponsInUse[1].runtimeData = secondaryRuntimeData;
                    runtimeData.weaponsInUse[1].attachments = secondaryAttachments;
                    //Setup third person
                    secondaryWeaponBehaviour.SetupThirdPerson(pb, secondaryWeaponBehaviour as Kit_ModernWeaponScript, secondaryRuntimeData, secondaryAttachments);
                }
                else
                {
                    //No secondary weapon specified, disable it
                    runtimeData.canUseSecondaryWeapon = false;
                }

                //Select current weapon
                primaryWeaponBehaviour.DrawWeapon(pb, primaryRuntimeData);
                //Set current weapon
                runtimeData.currentWeapon = 0;
                //Set time
                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.drawTime;
                //Set phase
                runtimeData.switchPhase = 1;
                //Set switching
                runtimeData.switchInProgress = true;
            }

            public override void SetupManagerOthers(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerOthersRuntimeData runtimeData = new WeaponManagerControllerOthersRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                Debug.Log("[Weapon Manager] Manager Setup Others");
                int primaryWeapon = (int)instantiationData[3];
                int secondaryWeapon = (int)instantiationData[4];

                int primaryAttachmentsLength = (int)instantiationData[5];
                int[] primaryAttachments = new int[primaryAttachmentsLength];
                for (int i = 0; i < primaryAttachments.Length; i++)
                {
                    primaryAttachments[i] = (int)instantiationData[6 + i];
                }

                int secondaryAttachmentsLength = (int)instantiationData[6 + primaryAttachmentsLength];
                int[] secondaryAttachments = new int[secondaryAttachmentsLength];
                for (int i = 0; i < secondaryAttachments.Length; i++)
                {
                    secondaryAttachments[i] = (int)instantiationData[7 + primaryAttachmentsLength + i];
                }

                //Setup weapons in use array
                if (secondaryWeapon >= 0)
                {
                    //Two weapons are in use
                    runtimeData.weaponsInUse = new WeaponReference[2];
                }
                else
                {
                    //Only one weapon is in use...
                    runtimeData.weaponsInUse = new WeaponReference[1];
                }

                //Setup both weapons
                Kit_WeaponInformation primaryWeaponInfo = pb.gameInformation.allWeapons[primaryWeapon];

                //Get their behaviour modules
                Kit_WeaponBase primaryWeaponBehaviour = primaryWeaponInfo.weaponBehaviour;

                //Setup primary
                primaryWeaponBehaviour.SetupValues(primaryWeapon); //This sets up values in the object itself, nothing else
                object primaryRuntimeData = primaryWeaponBehaviour.SetupThirdPersonOthers(pb, primaryWeaponBehaviour as Kit_ModernWeaponScript, primaryAttachments); //This creates the third person objects AND setups the runtime data
                //Set data
                runtimeData.weaponsInUse[0] = new WeaponReference();
                runtimeData.weaponsInUse[0].behaviour = primaryWeaponBehaviour;
                runtimeData.weaponsInUse[0].runtimeData = primaryRuntimeData;
                runtimeData.weaponsInUse[0].attachments = primaryAttachments;

                //Only set it up if its needed
                if (secondaryWeapon >= 0)
                {
                    //Setup both weapons
                    Kit_WeaponInformation secondaryWeaponInfo = pb.gameInformation.allWeapons[secondaryWeapon];
                    //Get their behaviour modules
                    Kit_WeaponBase secondaryWeaponBehaviour = secondaryWeaponInfo.weaponBehaviour;

                    //Setup secondary
                    secondaryWeaponBehaviour.SetupValues(secondaryWeapon); //This sets up values in the object itself, nothing else
                    object secondaryRuntimeData = secondaryWeaponBehaviour.SetupThirdPersonOthers(pb, secondaryWeaponBehaviour as Kit_ModernWeaponScript, secondaryAttachments); //This creates the third person objects AND setups the runtime data
                                                                                                                                                                                 //Set data
                    runtimeData.weaponsInUse[1] = new WeaponReference();
                    runtimeData.weaponsInUse[1].behaviour = secondaryWeaponBehaviour;
                    runtimeData.weaponsInUse[1].runtimeData = secondaryRuntimeData;
                    runtimeData.weaponsInUse[1].attachments = secondaryAttachments;
                }

                //Select current weapon
                primaryWeaponBehaviour.DrawWeaponOthers(pb, primaryRuntimeData);
                //Set current weapon
                runtimeData.currentWeapon = 0;
            }

            public override void CustomUpdate(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
                    {
                        if (runtimeData.lastWeaponOne != pb.input.weaponOne)
                        {
                            runtimeData.lastWeaponOne = pb.input.weaponOne;
                            //Check for input
                            if (pb.input.weaponOne && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                            {
                                runtimeData.desiredWeapon = 0;
                            }
                        }

                        //If weapon is not disable
                        if (runtimeData.canUseSecondaryWeapon)
                        {
                            if (runtimeData.lastWeaponTwo != pb.input.weaponTwo)
                            {
                                runtimeData.lastWeaponTwo = pb.input.weaponTwo;
                                if (pb.input.weaponTwo && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                                {
                                    runtimeData.desiredWeapon = 1;
                                }
                            }
                        }

                        if (Physics.Raycast(pb.playerCameraTransform.position, pb.playerCameraTransform.forward, out runtimeData.hit, pickupDistance, pickupLayers.value))
                        {
                            if (runtimeData.hit.transform.root.GetComponent<Kit_DropBehaviour>())
                            {
                                Kit_DropBehaviour drop = runtimeData.hit.transform.root.GetComponent<Kit_DropBehaviour>();
                                if (!pb.isBot)
                                {
                                    pb.main.hud.DisplayWeaponPickup(true, drop.weaponID);
                                }
                                if (runtimeData.lastDropWeapon != pb.input.dropWeapon)
                                {
                                    runtimeData.lastDropWeapon = pb.input.dropWeapon;
                                    if (pb.input.dropWeapon && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                                    {
                                        //First drop our weapon
                                        DropWeapon(pb, runtimeData.currentWeapon, drop.transform);
                                        //Pickup new weapon
                                        pb.photonView.RPC("ReplaceWeapon", PhotonTargets.AllBuffered, runtimeData.currentWeapon, drop.weaponID, drop.bulletsLeft, drop.bulletsLeftToReload, drop.attachments);
                                        //First hide
                                        drop.rendererRoot.SetActive(false);
                                        //Delete object
                                        drop.photonView.RPC("PickedUp", drop.photonView.owner);
                                    }
                                }
                            }
                            else if (!pb.isBot)
                            {
                                pb.main.hud.DisplayWeaponPickup(false);
                            }
                        }
                        else if (!pb.isBot)
                        {
                            pb.main.hud.DisplayWeaponPickup(false);
                        }
                    }

                    //Update weapon animation
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.AnimateWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, pb.movement.GetCurrentWeaponMoveAnimation(pb), pb.movement.GetCurrentWalkAnimationSpeed(pb));

                    //Move weapons transform
                    pb.weaponsGo.localPosition = Vector3.Lerp(pb.weaponsGo.localPosition, Vector3.zero + pb.looking.GetWeaponOffset(pb), Time.deltaTime * weaponPositionChangeSpeed);

                    //Move weapons transform
                    pb.weaponsGo.localRotation = Quaternion.Slerp(pb.weaponsGo.localRotation, pb.looking.GetWeaponRotationOffset(pb), Time.deltaTime * weaponPositionChangeSpeed);

                    if (!runtimeData.switchInProgress)
                    {
                        //If we aren't switching weapons, update weapon behaviour
                        runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.CalculateWeaponUpdate(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);

                        //Check if we want to select a different weapon
                        if (runtimeData.desiredWeapon != runtimeData.currentWeapon)
                        {
                            //If not, start to switch
                            runtimeData.switchInProgress = true;
                            //Set time (Because here we cannot use a coroutine)
                            runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.putawayTime;
                            //Set phase
                            runtimeData.switchPhase = 0;
                            //Start putaway
                            runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.PutawayWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                            if (!pb.isBot)
                            {
                                //Hide crosshair
                                pb.main.hud.DisplayCrosshair(0f);
                            }
                        }
                    }
                    else
                    {
                        //Switching, courtine less
                        #region Switching
                        //Check for time
                        if (Time.time >= runtimeData.switchNextEnd)
                        {
                            //Time is over, check which phase is next
                            if (runtimeData.switchPhase == 0)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.PutawayWeaponHide(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                                //Set weapon
                                runtimeData.currentWeapon = runtimeData.desiredWeapon;
                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                                //Set phase
                                runtimeData.switchPhase = 1;
                                //Set time
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.switchPhase == 1)
                            {
                                //Switching is over
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                        #endregion
                    }
                }
            }

            public override void CustomUpdateOthers(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    if (!runtimeData.switchInProgress)
                    {
                        //If we aren't switching weapons, update weapon behaviour
                        runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.CalculateWeaponUpdateOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);

                        //Check if we want to select a different weapon
                        if (runtimeData.desiredWeapon != runtimeData.currentWeapon)
                        {
                            //If not, start to switchz
                            runtimeData.switchInProgress = true;
                            //Set time (Because here we cannot use a coroutine)
                            runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.putawayTime;
                            //Set phase
                            runtimeData.switchPhase = 0;
                            //Start putaway
                            runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.PutawayWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                        }
                    }
                    else
                    {
                        //Switching, courtine less
                        #region Switching
                        //Check for time
                        if (Time.time >= runtimeData.switchNextEnd)
                        {
                            //Time is over, check which phase is next
                            if (runtimeData.switchPhase == 0)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.PutawayWeaponHideOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                                //Set weapon
                                runtimeData.currentWeapon = runtimeData.desiredWeapon;
                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.DrawWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                                //Set phase
                                runtimeData.switchPhase = 1;
                                //Set time
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.switchPhase == 1)
                            {
                                //Switching is over
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                        #endregion
                    }
                }
                else //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (!runtimeData.switchInProgress)
                    {
                        //If we aren't switching weapons, update weapon behaviour
                        runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.CalculateWeaponUpdateOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);

                        //Check if we want to select a different weapon
                        if (runtimeData.desiredWeapon != runtimeData.currentWeapon)
                        {
                            //If not, start to switchz
                            runtimeData.switchInProgress = true;
                            //Set time (Because here we cannot use a coroutine)
                            runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.putawayTime;
                            //Set phase
                            runtimeData.switchPhase = 0;
                            //Start putaway
                            runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.PutawayWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                        }
                    }
                    else
                    {
                        //Switching, courtine less
                        #region Switching
                        //Check for time
                        if (Time.time >= runtimeData.switchNextEnd)
                        {
                            //Time is over, check which phase is next
                            if (runtimeData.switchPhase == 0)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.PutawayWeaponHideOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                                //Set weapon
                                runtimeData.currentWeapon = runtimeData.desiredWeapon;
                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.DrawWeaponOthers(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                                //Set phase
                                runtimeData.switchPhase = 1;
                                //Set time
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.switchPhase == 1)
                            {
                                //Switching is over
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                        #endregion
                    }
                }
            }

            public override void PlayerDead(Kit_PlayerBehaviour pb)
            {
                if (pb.main.currentGameModeBehaviour.CanDropWeapons(pb.main))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (uponDeathDrop == DeadDrop.Selected)
                        {

                            DropWeaponDead(pb, runtimeData.currentWeapon);
                        }
                        else if (uponDeathDrop == DeadDrop.Both)
                        {
                            DropWeaponDead(pb, 0);
                            if (runtimeData.canUseSecondaryWeapon)
                                DropWeaponDead(pb, 1);
                        }
                    }
                }
            }

            public override void OnAnimatorIKCallback(Kit_PlayerBehaviour pb, Animator anim)
            {
                //Get runtime data
                if (pb.isController)
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                            if (ikv.leftHandIK)
                            {
                                anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                            }
                            if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                            }
                            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                        }
                    }
                }
                else
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                    {
                        WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                            if (ikv.leftHandIK)
                            {
                                anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                            }
                            if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                            }
                            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                        }
                    }
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, bool wasFallDamageApplied)
            {
                if (pb.isBot) return;
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.FallDownEffect(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, wasFallDamageApplied);
                }
            }

            public override void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.OnControllerColliderHitCallback(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, hit);
                }
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
            {
                if (stream.isWriting)
                {
                    //Get runtime data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Send runtime data
                        stream.SendNext(runtimeData.desiredWeapon);

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                    }
                    //Send dummy data
                    else
                    {
                        stream.SendNext(0);
                    }
                }
                else
                {
                    //Read data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                    {
                        WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                        runtimeData.desiredWeapon = (int)stream.ReceiveNext();

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                    }
                    else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        runtimeData.desiredWeapon = (int)stream.ReceiveNext();

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                    }
                    else
                    {
                        //Dummy reading
                        stream.ReceiveNext();
                    }
                }
            }

            public override void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkSemiRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkSemiRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
            }

            public override void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, int state)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkBoltActionRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, state);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkBoltActionRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, state);
                }
            }

            public override void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, int burstLength)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkBurstRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, burstLength);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkBurstRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData, burstLength);
                }
            }

            public override void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkReloadRPCReceived(pb, isEmpty, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkReloadRPCReceived(pb, isEmpty, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
            }

            public override void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
                {
                    WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkProceduralReloadRPCReceived(pb, stage, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
                else if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.NetworkProceduralReloadRPCReceived(pb, stage, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
            }

            public override bool IsAiming(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.IsWeaponAiming(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData); //Relay to weapon script
                }
                return false;
            }

            public override bool CanRun(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (allowSwitchingWhileRunning) return true;
                    else return !runtimeData.switchInProgress;
                }
                return true;
            }

            public override float CurrentMovementMultiplier(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.SpeedMultiplier(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData); //Relay to weapon script
                }
                return 1f;
            }

            public override float CurrentSensitivity(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.Sensitivity(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData); //Relay to weapon script
                }
                return 1f;
            }

            public override void NetworkReplaceWeapon(Kit_PlayerBehaviour pb, int slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
            {
                if (pb.photonView.isMine)
                {
                    //Get runtime data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Get old data
                        WeaponControllerRuntimeData oldWcrd = runtimeData.weaponsInUse[slot].runtimeData as WeaponControllerRuntimeData;
                        //Clean Up
                        for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                        {
                            Destroy(oldWcrd.instantiatedObjects[i]);
                        }
                        if (!pb.isBot)
                        {
                            //Hide crosshair
                            pb.main.hud.DisplayCrosshair(0f);
                        }
                        //Setup both weapons
                        Kit_WeaponInformation newWeaponInfo = pb.gameInformation.allWeapons[weapon];
                        //Get their behaviour modules
                        Kit_WeaponBase newWeaponBehaviour = newWeaponInfo.weaponBehaviour;
                        //Setup new
                        newWeaponBehaviour.SetupValues(weapon); //This sets up values in the object itself, nothing else
                        object newRuntimeData = newWeaponBehaviour.SetupFirstPerson(pb, attachments); //This creates the first person objects
                        //Set data
                        WeaponControllerRuntimeData wcrd = newRuntimeData as WeaponControllerRuntimeData;
                        //Set data
                        wcrd.bulletsLeft = bulletsLeft;
                        wcrd.bulletsLeftToReload = bulletsLeftToReload;
                        runtimeData.weaponsInUse[slot] = new WeaponReference();
                        runtimeData.weaponsInUse[slot].behaviour = newWeaponBehaviour;
                        runtimeData.weaponsInUse[slot].runtimeData = newRuntimeData;
                        runtimeData.weaponsInUse[slot].attachments = attachments;
                        //Setup third person
                        newWeaponBehaviour.SetupThirdPerson(pb, newWeaponBehaviour as Kit_ModernWeaponScript, newRuntimeData, attachments);
                        //Select current weapon
                        newWeaponBehaviour.DrawWeapon(pb, newRuntimeData);
                        //Set current weapon
                        runtimeData.currentWeapon = slot;
                        //Set time
                        runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.drawTime;
                        //Set phase
                        runtimeData.switchPhase = 1;
                        //Set switching
                        runtimeData.switchInProgress = true;
                    }
                }
                else
                {
                    Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.NetworkReplaceWeaponWait(pb, slot, weapon, bulletsLeft, bulletsLeftToReload, attachments));
                }
            }

            public void DropWeapon(Kit_PlayerBehaviour pb, int slot)
            {
                if (pb.main.currentGameModeBehaviour.CanDropWeapons(pb.main))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        //Get the manager's runtime data
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].runtimeData != null && runtimeData.weaponsInUse[slot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].runtimeData as WeaponControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].behaviour as Kit_ModernWeaponScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.bulletsLeft;
                            //Bullets Left To Reload
                            instData[2] = wepData.bulletsLeftToReload;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                    }
                }
            }

            /// <summary>
            /// Drops a weapon and applies the ragdoll force!
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="slot"></param>
            public void DropWeaponDead(Kit_PlayerBehaviour pb, int slot)
            {
                if (pb.main.currentGameModeBehaviour.CanDropWeapons(pb.main))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        //Get the manager's runtime data
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].runtimeData != null && runtimeData.weaponsInUse[slot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].runtimeData as WeaponControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].behaviour as Kit_ModernWeaponScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.bulletsLeft;
                            //Bullets Left To Reload
                            instData[2] = wepData.bulletsLeftToReload;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].attachments[i];
                            }
                            //Instantiate
                            GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                            Rigidbody body = go.GetComponent<Rigidbody>();
                            body.velocity = pb.movement.GetVelocity(pb);
                            body.AddForce(pb.ragdollForward * pb.ragdollForce);
                        }
                    }
                }
            }

            public void DropWeapon(Kit_PlayerBehaviour pb, int slot, Transform replace)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Setup instantiation data
                    object[] instData = new object[4 + runtimeData.weaponsInUse[slot].attachments.Length];
                    if (runtimeData.weaponsInUse[slot].runtimeData != null && runtimeData.weaponsInUse[slot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        //Get the weapon's runtime data
                        WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].runtimeData as WeaponControllerRuntimeData;
                        //Get the Scriptable object
                        Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].behaviour as Kit_ModernWeaponScript;
                        //ID
                        instData[0] = wepScript.gameGunID;
                        //Bullets left
                        instData[1] = wepData.bulletsLeft;
                        //Bullets Left To Reload
                        instData[2] = wepData.bulletsLeftToReload;
                        //Attachments length
                        instData[3] = runtimeData.weaponsInUse[slot].attachments.Length;
                        for (int i = 0; i < runtimeData.weaponsInUse[slot].attachments.Length; i++)
                        {
                            instData[4 + i] = runtimeData.weaponsInUse[slot].attachments[i];
                        }
                        //Instantiate
                        PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                    }
                }
            }

            public override int WeaponState(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.WeaponState(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
                return 0;
            }

            public override int WeaponType(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon].behaviour.WeaponType(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon].runtimeData);
                }
                return 0;
            }
        }
    }
}
