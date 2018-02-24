using UnityEngine;

namespace MarsFPSKit
{
    public abstract class Kit_VitalsBase : ScriptableObject
    {
        /// <summary>
        /// Called to setup this system
        /// </summary>
        /// <param name=""></param>
        public abstract void Setup(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Apply fall damage!
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="dmg"></param>
        public abstract void ApplyFallDamage(Kit_PlayerBehaviour pb, float dmg);

        /// <summary>
        /// Commit suicide
        /// </summary>
        /// <param name="pb"></param>
        public abstract void Suicide(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Called to apply damage using this system
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="runtimeData"></param>
        public abstract void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, int gunID);

        /// <summary>
        /// Update callback
        /// </summary>
        /// <param name="pb"></param>
        public abstract void CustomUpdate(Kit_PlayerBehaviour pb);
    }
}
