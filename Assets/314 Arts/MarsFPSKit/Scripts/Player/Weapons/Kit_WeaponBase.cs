using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class WeaponIKValues
        {
            public Transform leftHandIK;
            public bool canUseIK;
        }

        /// <summary>
        /// Helper class for the loadout menu which retrieves the weapon's stats
        /// </summary>
        public class WeaponStats
        {
            /// <summary>
            /// How high is this weapon's damage?
            /// </summary>
            public float damage;
            /// <summary>
            /// How high is this weapon's fire rate?
            /// </summary>
            public float fireRate;
            /// <summary>
            /// How much recoil does this weapon have?
            /// </summary>
            public float recoil;
            /// <summary>
            /// How far can this weapon shoot?
            /// </summary>
            public float reach;
        }

        public enum ThirdPersonAnimType { Rifle, Pistol }

        /// <summary>
        /// This script is executed when this weapon is active
        /// </summary>
        public abstract class Kit_WeaponBase : ScriptableObject
        {
            #region Prefabs
            [Header("Prefabs")]
            public GameObject firstPersonPrefab; //The prefab to use for first person
            public GameObject thirdPersonPrefab; //The prefab to use for third person
            public GameObject dropPrefab; //The prefab to use for drop
            #endregion

            /// <summary>
            /// Which third person animset to use
            /// </summary>
            public ThirdPersonAnimType thirdPersonAnimType;
            public float drawTime = 0.5f; //Time it takes to take this weapon out
            public float putawayTime = 0.5f; //Time it takes to put this weapon away

            /// <summary>
            /// Calculate the weapon (Update)
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public abstract void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Calculate the weapon (Late Update)
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public virtual void CalculateWeaponLateUpdate(Kit_PlayerBehaviour pb, object runtimeData) //This is optional
            {

            }

            /// <summary>
            /// Calculate the weapon, if we are not the controller (Update)
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public abstract void CalculateWeaponUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Calculate the weapon, if we are not the controller (Late Update)
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public virtual void CalculateWeaponLateUpdateOthers(Kit_PlayerBehaviour pb, object runtimeData) //This is optional
            {

            }

            /// <summary>
            /// Tells the weapon to play idle, walk or run animation
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <param name="id">The animation that should be played</param>
            /// <param name="speed">The speed at which the animation should be played</param>
            public abstract void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed);

            /// <summary>
            /// When the fall down effect should be played, this is called
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <param name="wasFallDamageApplied"></param>
            public abstract void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied);

            /// <summary>
            /// Callback for OnControllerColliderHit
            /// </summary>
            /// <param name="hit"></param>
            /// /// <param name="runtimeData"></param>
            public virtual void OnControllerColliderHitCallback(Kit_PlayerBehaviour pb, object runtimeData, ControllerColliderHit hit) //This is optional
            {

            }

            /// <summary>
            /// Called when this weapon should be taken out
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public abstract void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Called when this weapon should be put away (not hidden)
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public abstract void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Called when the weapon should be hidden after putaway sequence is done
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public abstract void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Called when this weapon should be taken out for other (non controlling) players
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public abstract void DrawWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Called when this weapon should be put away (not hidden) for other (non controlling) players
            /// </summary>
            /// <param name="pb"></param>
            /// /// <param name="runtimeData"></param>
            public abstract void PutawayWeaponOthers(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Called when the weapon should be hidden after putaway sequence is done for other (non controlling) players
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public abstract void PutawayWeaponHideOthers(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Called when this weapon is setup for the first time. Will be called every time it is setup.
            /// </summary>
            /// <param name="info"></param>
            public abstract void SetupValues(int id);

            /// <summary>
            /// Create First Person for this weapon
            /// </summary>
            /// <param name="pb"></param>
            public abstract object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments);

            /// <summary>
            /// Create Third Person for this weapon
            /// </summary>
            /// <param name="pb"></param>
            public abstract void SetupThirdPerson(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, object runtimeData, int[] attachments);

            /// <summary>
            /// Create Third Person for this weapon (for others, which needs to create the runtime data)
            /// </summary>
            /// <param name="pb"></param>
            public abstract object SetupThirdPersonOthers(Kit_PlayerBehaviour pb, Kit_ModernWeaponScript ws, int[] attachments);

            /// <summary>
            /// Photonview Serialize callback
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="info"></param>
            public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData);

            #region Weapon Network Relays
            /// <summary>
            /// We received a semi shot RPC
            /// </summary>
            public abstract void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// We received a bolt action shot RPC
            /// </summary>
            public abstract void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int state);

            /// <summary>
            /// We received a burst fire RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            public abstract void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int burstLength);

            /// <summary>
            /// We received a reload RPC
            /// </summary>
            /// <param name="isEmpty"></param>
            public abstract void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty, object runtimeData);

            /// <summary>
            /// We received a procedural reload RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="stage"></param>
            /// <param name="runtimeData"></param>
            public abstract void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage, object runtimeData);
            #endregion

            #region For Other Scripts
            /// <summary>
            /// Retrives if the weapon is currently aiming or not
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <returns></returns>
            public abstract bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Retrieves the movement speed multiplier for this weapon
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <returns></returns>
            public abstract float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Returns the sensitivity for the weapon
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <returns></returns>
            public abstract float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Returns the IK data for this weapon
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <returns></returns>
            public abstract WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData);

            /// <summary>
            /// Retrieve this weapon's stats
            /// </summary>
            /// <returns></returns>
            public abstract WeaponStats GetStats();

            /// <summary>
            /// Get current weapon state
            /// 0 = loaded
            /// 1 = empty
            /// 2 = completely empty
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract int WeaponState(Kit_PlayerBehaviour pb, object runtimeData);

            /// <summary>
            /// Get current weapon type
            /// 0 = Full Auto
            /// 1 = Semi Auto
            /// 2 = Close up only semi
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract int WeaponType(Kit_PlayerBehaviour pb, object runtimeData);
            #endregion
        }
    }
}