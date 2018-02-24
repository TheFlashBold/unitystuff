using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class Kit_ThirdPersonWeaponRenderer : MonoBehaviour
        {
            /// <summary>
            /// The weapon renderers that are not of attachments
            /// </summary>
            [Tooltip("Asssign all weapon renderers here that are not of attachments")]
            public Renderer[] allWeaponRenderers;

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

            [Header("Inverse Kinematics")]
            public Transform leftHandIK;

            [Header("Attachments")]
            public GameObject attachmentsRoot;

            [Tooltip("Make sure they MATCH the first person attachment slots!")]
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

#if UNITY_EDITOR
            //Test if everything is correctly assigned, but only in the editor.
            void OnEnable()
            {
                for (int i = 0; i < allWeaponRenderers.Length; i++)
                {
                    if (!allWeaponRenderers[i])
                    {
                        Debug.LogError("Third person weapon renderer from " + gameObject.name + " at index " + i + " not assigned.");
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
            /// Is this weapon set to shadows only?
            /// </summary>
            public bool shadowsOnly
            {
                get
                {
                    for (int i = 0; i < allWeaponRenderers.Length; i++)
                    {
                        if (allWeaponRenderers[i].shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) return false;
                    }
                    return true;
                }
                set
                {
                    if (value)
                    {
                        //Set renderers
                        for (int i = 0; i < allWeaponRenderers.Length; i++)
                        {
                            allWeaponRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                        }
                        //Attachment renderers
                        for (int i = 0; i < attachmentSlots.Length; i++)
                        {
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                {
                                    if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentRenderer))
                                    {
                                        Kit_AttachmentRenderer ar = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentRenderer;
                                        for (int a = 0; a < ar.renderersToActivate.Length; a++)
                                        {
                                            ar.renderersToActivate[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Set renderers
                        for (int i = 0; i < allWeaponRenderers.Length; i++)
                        {
                            allWeaponRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        }
                        //Attachment renderers
                        for (int i = 0; i < attachmentSlots.Length; i++)
                        {
                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                {
                                    if (attachmentSlots[i].attachments[o].attachmentBehaviours[p].GetType() == typeof(Kit_AttachmentRenderer))
                                    {
                                        Kit_AttachmentRenderer ar = attachmentSlots[i].attachments[o].attachmentBehaviours[p] as Kit_AttachmentRenderer;
                                        for (int a = 0; a < ar.renderersToActivate.Length; a++)
                                        {
                                            ar.renderersToActivate[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Enables the given attachments.
            /// </summary>
            /// <param name="enabledAttachments"></param>
            public void SetAttachments(int[] enabledAttachments, Kit_ModernWeaponScript ws)
            {
                //Set default cached values
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