using System;
using UnityEngine;

using Random = UnityEngine.Random;

namespace MarsFPSKit
{
    /// <summary>
    /// Creates random valid loadouts for bots
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Bots/Loadout/Random")]
    public class Kit_BotRandomLoadoutManager : Kit_BotLoadoutManager
    {
        public override Loadout GetBotLoadout(Kit_IngameMain main)
        {
            Loadout toReturn = new Loadout();
            //Find a primary
            Kit_WeaponInformation[] primaries = Array.FindAll(main.gameInformation.allWeapons, x => x.weaponType == WeaponType.Primary);
            Kit_WeaponInformation primary = primaries[Random.Range(0, primaries.Length)];
            int primaryIndex = Array.IndexOf(main.gameInformation.allWeapons, primary);
            //Find a secondary
            Kit_WeaponInformation[] secondaries = Array.FindAll(main.gameInformation.allWeapons, x => x.weaponType == WeaponType.Secondary);
            Kit_WeaponInformation secondary = secondaries[Random.Range(0, secondaries.Length)];
            int secondaryIndex = Array.IndexOf(main.gameInformation.allWeapons, secondary);

            //Assign
            toReturn.primaryWeapon = primaryIndex;
            toReturn.primaryAttachments = new int[primary.weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length];
            toReturn.secondaryWeapon = secondaryIndex;
            toReturn.secondaryAttachments = new int[secondary.weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length];
            return toReturn;
        }
    }
}
