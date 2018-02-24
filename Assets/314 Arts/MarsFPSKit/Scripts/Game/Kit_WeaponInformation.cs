using UnityEngine;

namespace MarsFPSKit
{
    public enum WeaponType { Primary, Secondary }

    /// <summary>
    /// This Object contains the weapon information
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Weapons/Weapon Information")]
    public class Kit_WeaponInformation : ScriptableObject
    {
        /// <summary>
        /// The name of this weapon as it will be displayed in the loadout menu / Killfeed
        /// </summary>
        public string weaponName;

        /// <summary>
        /// The sprite of this weapon
        /// </summary>
        public Sprite weaponPicture;

        /// <summary>
        /// The type of this weapon
        /// </summary>
        public WeaponType weaponType = WeaponType.Primary;

        /// <summary>
        /// The script that will control this weapon at runtime
        /// </summary>
        public Weapons.Kit_WeaponBase weaponBehaviour;
    }
}
