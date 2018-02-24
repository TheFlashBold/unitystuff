using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public enum CameraAnimationType { Copy, LookAt }

        public class Kit_WeaponRenderer : MonoBehaviour
        {
            /// <summary>
            /// The weapon animator
            /// </summary>
            public Animator anim;

            /// <summary>
            /// The weapon renderers that are not of attachments
            /// </summary>
            [Tooltip("Asssign all weapon renderers here that are not of attachments")]
            public Renderer[] allWeaponRenderers;
            /// <summary>
            /// These rendererers will be disabled in the customization menu. E.g. arms
            /// </summary>
            public Renderer[] hideInCustomiazionMenu;

            [Header("Shell Ejection")]
            /// <summary>
            /// This is where the ejected shell will spawn, if assigned
            /// </summary>
            public Transform shellEjectTransform;

            [Header("Muzzle Flash")]
            /// <summary>
            /// The muzzle flash particle system to use
            /// </summary>
            public ParticleSystem muzzleFlash;

            [Header("Aiming")]
            /// <summary>
            /// Which position to move to when we are aiming
            /// </summary>
            public Vector3 aimingPos;
            /// <summary>
            /// Which rotation to rotate to when we are aming
            /// </summary>
            public Vector3 aimingRot;

            [Header("Run position / rotation")]
            /// <summary>
            /// Determines if the weapon should be moved when we are running
            /// </summary>
            public bool useRunPosRot;
            /// <summary>
            /// The run position to use
            /// </summary>
            public Vector3 runPos;
            /// <summary>
            /// The run rotation to use. Will be converted to Quaternion using <see cref="Quaternion.Euler(Vector3)"/>
            /// </summary>
            public Vector3 runRot;
            /// <summary>
            /// How fast is the weapon going to move / rotate towards the run pos / run rot?
            /// </summary>
            public float runSmooth = 3f;

            [Header("Camera Animation")]
            public bool cameraAnimationEnabled;
            /// <summary>
            /// If camera animation is enabled, which one should be used?
            /// </summary>
            public CameraAnimationType cameraAnimationType;
            /// <summary>
            /// The bone for the camera animation
            /// </summary>
            public Transform cameraAnimationBone;
            /// <summary>
            /// If the type is LookAt, this is the target
            /// </summary>
            public Transform cameraAnimationTarget;
            /// <summary>
            /// The reference rotation to add movemment to
            /// </summary>
            public Vector3 cameraAnimationReferenceRotation;

            [Header("Attachments")]
            public GameObject attachmentsRoot;

            public AttachmentSlot[] attachmentSlots;

            #region Cached values
            //This caches values from the attachments!
            /// <summary>
            /// Which position to move to when we are aiming
            /// </summary>
            [HideInInspector]
            public Vector3 cachedAimingPos;
            /// <summary>
            /// Which rotation to rotate to when we are aming
            /// </summary>
            [HideInInspector]
            public Vector3 cachedAimingRot;
            [HideInInspector]
            public bool cachedMuzzleFlashEnabled;

            /// <summary>
            /// Fire sound used for first person
            /// </summary>
            [HideInInspector]
            public AudioClip cachedFireSound;
            /// <summary>
            /// Fire sound used for third person
            /// </summary>
            [HideInInspector]
            public AudioClip cachedFireSoundThirdPerson;

            /// <summary>
            /// Max sound distance for third person fire
            /// </summary>
            [HideInInspector]
            public float cachedFireSoundThirdPersonMaxRange = 300f;
            /// <summary>
            /// Sound rolloff for third person fire
            /// </summary>
            [HideInInspector]
            public AnimationCurve cachedFireSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 300f, 0f);
            #endregion

            [Header("Loadout")]
            /// <summary>
            /// Use this to correct the position in the customization menu
            /// </summary>
            public Vector3 customizationMenuOffset;

#if UNITY_EDITOR
            //Test if everything is correctly assigned, but only in the editor.
            void OnEnable()
            {
                for (int i = 0; i < allWeaponRenderers.Length; i++)
                {
                    if (!allWeaponRenderers[i])
                    {
                        Debug.LogError("Weapon renderer from " + gameObject.name + " at index " + i + " not assigned.");
                    }
                }
            }
#endif

            /// <summary>
            /// Visibility state of the weapon
            /// </summary>
            public bool visible
            {
                get
                {
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        if (!allWeaponRenderers[i].enabled) return false;
                    }
                    return true;
                }
                set
                {
                    //Set renderers
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        allWeaponRenderers[i].enabled = value;
                    }
                    if (attachmentsRoot) //Hide | Show attachments!
                        attachmentsRoot.SetActive(value);
                }
            }

            /// <summary>
            /// Enables the given attachments.
            /// </summary>
            /// <param name="enabledAttachments"></param>
            public void SetAttachments(int[] enabledAttachments, Kit_ModernWeaponScript ws)
            {
                //Set default cached values
                cachedAimingPos = aimingPos;
                cachedAimingRot = aimingRot;
                cachedMuzzleFlashEnabled = true;
                cachedFireSound = ws.fireSound;
                cachedFireSoundThirdPerson = ws.fireSoundThirdPerson;
                cachedFireSoundThirdPersonMaxRange = ws.fireSoundThirdPersonMaxRange;
                cachedFireSoundThirdPersonRolloff = ws.fireSoundThirdPersonRolloff;

                //Loop through all slots
                for (int i = 0; i < enabledAttachments.Length; i++)
                {
                    if (i < attachmentSlots.Length)
                    {
                        //Loop through all attachments for that slot
                        for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                        {
                            //Check if this attachment is enabled
                            if (o == enabledAttachments[i])
                            {
                                //Tell the behaviours they are active!
                                for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                {
                                    attachmentSlots[i].attachments[o].attachmentBehaviours[p].Selected();
                                    //Check what it is
                                    if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentAimOverride))
                                    {
                                        Kit_AttachmentAimOverride aimOverride = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentAimOverride;
                                        //Override aim
                                        cachedAimingPos = aimOverride.aimPos;
                                        cachedAimingRot = aimOverride.aimRot;
                                    }
                                    else if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentDisableMuzzleFlash))
                                    {
                                        cachedMuzzleFlashEnabled = false;
                                    }
                                    else if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentOverrideSounds))
                                    {
                                        Kit_AttachmentOverrideSounds soundOverride = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentOverrideSounds;
                                        cachedFireSound = soundOverride.fireSound;
                                        cachedFireSoundThirdPerson = soundOverride.fireSoundThirdPerson;
                                        cachedFireSoundThirdPersonMaxRange = soundOverride.fireSoundThirdPersonMaxRange;
                                        cachedFireSoundThirdPersonRolloff = soundOverride.fireSoundThirdPersonRolloff;
                                    }
                                }
                            }
                            else
                            {
                                //Tell the behaviours they are not active!
                                for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                {
                                    attachmentSlots[i].attachments[o].attachmentBehaviours[p].Unselected();
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Something must have gone wrong with the attachments. Enabled attachments is longer than all slots.");
                    }
                }
            }
        }
    }
}