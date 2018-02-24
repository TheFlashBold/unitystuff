using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Used by <see cref="Kit_PlayerBehaviour"/> to calculate the movement. Drag into <see cref="Kit_PlayerBehaviour.movement"/> after you created a .asset file of it
    /// </summary>
    public abstract class Kit_MovementBase : ScriptableObject
    {
        /// <summary>
        /// Calculate the movement (Update)
        /// </summary>
        /// <param name="pb"></param>
        public abstract void CalculateMovementUpdate(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Calculate the movement (Late Update)
        /// </summary>
        /// <param name="pb"></param>
        public virtual void CalculateMovementLateUpdate(Kit_PlayerBehaviour pb) //This is optional
        {

        }

        /// <summary>
        /// Callback for OnControllerColliderHit
        /// </summary>
        /// <param name="hit"></param>
        public virtual void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit) //This is optional
        {

        }

        /// <summary>
        /// Returns a bool that determines if we can fire our weapons
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract bool CanFire(Kit_PlayerBehaviour pb);

        /// <summary>
        /// If running is true, this is true
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract bool IsRunning(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Returns the animation that should be currently played. 0 = Idle; 1 = Walk; 2 = Run;
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract int GetCurrentWeaponMoveAnimation(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Returns the speed that should be used for the walking animation
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract float GetCurrentWalkAnimationSpeed(Kit_PlayerBehaviour pb);
        
        /// <summary>
        /// Retrieves the current (local!) movement direction
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract Vector3 GetMovementDirection(Kit_PlayerBehaviour pb);

        /// <summary>
        /// This is called for everyone in update
        /// </summary>
        /// <param name="pb"></param>
        public abstract void CalculateFootstepsUpdate(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Callback for photon serialization
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);

        /// <summary>
        /// Returns the velocity, either local or across the network.
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract Vector3 GetVelocity(Kit_PlayerBehaviour pb);
    }
}
