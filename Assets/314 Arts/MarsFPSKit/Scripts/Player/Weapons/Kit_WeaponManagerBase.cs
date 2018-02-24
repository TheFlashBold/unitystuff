using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Expand upon this class to create a custom Weapon Manager. It should be using <see cref="Kit_WeaponBase"/> and <see cref="Kit_WeaponInformation"/> to be compatible with all setups
        /// </summary>
        public abstract class Kit_WeaponManagerBase : ScriptableObject
        {
            /// <summary>
            /// Called when the manager should be setup (Only called for the local player)
            /// </summary>
            /// <param name="pb"></param>
            public abstract void SetupManager(Kit_PlayerBehaviour pb, object[] instantiationData);

            /// <summary>
            /// Called when the manager should be setup (called for everyone if this is a bot)
            /// </summary>
            /// <param name="pb"></param>
            public abstract void SetupManagerBot(Kit_PlayerBehaviour pb, object[] instantiationData);

            /// <summary>
            /// Called when the manager should be setup (Only called for other players that are not controlling this player)
            /// </summary>
            /// <param name="pb"></param>
            public abstract void SetupManagerOthers(Kit_PlayerBehaviour pb, object[] instantiationData);

            #region Unity Callback
            /// <summary>
            /// Called in update for the local (controlling) player
            /// </summary>
            public abstract void CustomUpdate(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Called in update for the other (not controlling) players
            /// </summary>
            public abstract void CustomUpdateOthers(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Called just before the player dies
            /// </summary>
            /// <param name="pb"></param>
            public virtual void PlayerDead(Kit_PlayerBehaviour pb) { }

            /// <summary>
            /// OnControllerColliderHit relay
            /// </summary>
            /// <param name="hit"></param>
            public abstract void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit);

            /// <summary>
            /// OnAnimatorIK relay
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="anim"></param>
            public abstract void OnAnimatorIKCallback(Kit_PlayerBehaviour pb, Animator anim);

            /// <summary>
            /// When the fall down effect should be played, this is called
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            /// <param name="wasFallDamageApplied"></param>
            public abstract void FallDownEffect(Kit_PlayerBehaviour pb, bool wasFallDamageApplied);
            #endregion

            #region Photon stuff
            /// <summary>
            /// Photonview Serialize callback
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="info"></param>
            public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);
            #endregion

            #region Values for other systems
            /// <summary>
            /// Get current weapon state
            /// 0 = loaded
            /// 1 = empty
            /// 2 = completely empty
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract int WeaponState(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Get current weapon type
            /// 0 = Full Auto
            /// 1 = Semi Auto
            /// 2 = Close up only semi
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract int WeaponType(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Retrives whether we are currently aiming or not
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract bool IsAiming(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Retrives the current movement multiplier
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract float CurrentMovementMultiplier(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Returns the current sensitivty for the mouse look
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract float CurrentSensitivity(Kit_PlayerBehaviour pb);

            /// <summary>
            /// Does the weapon manager currently allow us to run?
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public abstract bool CanRun(Kit_PlayerBehaviour pb);
            #endregion

            #region Weapon Network Relays
            /// <summary>
            /// We received a semi shot RPC
            /// </summary>
            public abstract void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb);

            /// <summary>
            /// We received a bolt action RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="state">The state of this bolt action</param>
            public abstract void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, int state);

            /// <summary>
            /// We received a burst RPC
            /// </summary>
            /// <param name="pb"></param>
            public abstract void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, int burstLength);

            /// <summary>
            /// We received a reload RPC
            /// </summary>
            /// <param name="isEmpty"></param>
            public abstract void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty);

            /// <summary>
            /// We received a procedural reload RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="stage"></param>
            public abstract void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage);

            /// <summary>
            /// We received a weapon replace RPC
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="slot"></param>
            /// <param name="weapon"></param>
            /// <param name="bulletsLeft"></param>
            /// <param name="bulletsLeftToReload"></param>
            /// <param name="attachments"></param>
            public abstract void NetworkReplaceWeapon(Kit_PlayerBehaviour pb, int slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments);
            #endregion
        }
    }
}