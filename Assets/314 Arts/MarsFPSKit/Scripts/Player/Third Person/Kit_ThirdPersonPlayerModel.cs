using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This script is the base for a third person model behaviour
    /// </summary>
    public abstract class Kit_ThirdPersonPlayerModel : MonoBehaviour
    {
        /// <summary>
        /// Our animator that is used to animate this model
        /// </summary>
        public Animator anim;

        [Header("Weapon Placement")]
        //Every model needs to have transform objects for the weapons
        /// <summary>
        /// The transform for weapons that are in the player's hands
        /// </summary>
        public Transform weaponsInHandsGo;

        [Header("Sounds")]
        /// <summary>
        /// Audio source used for fire sounds
        /// </summary>
        public AudioSource soundFire;

        /// <summary>
        /// Audio Source used for reload sounds
        /// </summary>
        public AudioSource soundReload;

        /// <summary>
        /// Audio Source used for other sounds
        /// </summary>
        public AudioSource soundOther;

        [Header("Name Above Head")]
        /// <summary>
        /// the collider which triggers our name to be displayed for enemies (if they are aiming at us)
        /// </summary>
        public Collider enemyNameAboveHeadTrigger;
        /// <summary>
        /// Where is our name going to be displayed?
        /// </summary>
        public Transform enemyNameAboveHeadPos;


        /// <summary>
        /// The initial setup for this model, (for example setting up the animator)
        /// </summary>
        public abstract void SetupModel(Kit_PlayerBehaviour kpb);
        /// <summary>
        /// Called when the model should be set up for first person usage (Things like shadows only for example)
        /// </summary>
        public abstract void FirstPerson();
        /// <summary>
        /// Called when the model should be set up for third person usage
        /// </summary>
        public abstract void ThirdPerson();

        /// <summary>
        /// Uses given animset
        /// </summary>
        /// <param name="animType"></param>
        public abstract void SetAnimType(Weapons.ThirdPersonAnimType animType);

        /// <summary>
        /// Play a fire animation for given anim type
        /// </summary>
        /// <param name="animType"></param>
        public abstract void PlayWeaponFireAnimation(Weapons.ThirdPersonAnimType animType);

        /// <summary>
        /// Play a reload animation for given anim type
        /// </summary>
        /// <param name="animType"></param>
        public abstract void PlayWeaponReloadAnimation(Weapons.ThirdPersonAnimType animType);

        /// <summary>
        /// Stop all weapon animations
        /// </summary>
        public abstract void AbortWeaponAnimations();

        /// <summary>
        /// Called when we die (for everyone). Create ragdoll.
        /// </summary>
        public abstract void CreateRagdoll();
    }
}
