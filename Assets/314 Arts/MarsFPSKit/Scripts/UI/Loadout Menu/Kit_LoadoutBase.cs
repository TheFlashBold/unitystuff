using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Helper class that contains loadout info
    /// </summary>
    public class Loadout
    {
        public Loadout()
        {
            primaryAttachments = new int[0];
            secondaryAttachments = new int[0];
        }

        /// <summary>
        /// The primary weapon
        /// </summary>
        public int primaryWeapon;
        /// <summary>
        /// Selected attachments for the primary
        /// </summary>
        public int[] primaryAttachments = new int[0];
        /// <summary>
        /// The secondary weapon
        /// </summary>
        public int secondaryWeapon;
        /// <summary>
        /// Selected attachments for the primary
        /// </summary>
        public int[] secondaryAttachments = new int[0];
    }

    /// <summary>
    /// Use this to implement your own loadout menu
    /// </summary>
    public abstract class Kit_LoadoutBase : MonoBehaviour
    {
        /// <summary>
        /// Returns the currently selected loadout
        /// </summary>
        /// <returns></returns>
        public abstract Loadout GetCurrentLoadout();

        /// <summary>
        /// Called from <see cref="Kit_IngameMain"/> or from <see cref="Kit_MainMenu"/> to initialize the Loadout menu at the beginning
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when the Loadout menu should be opened
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// The Loadout menu should usually close itself. When the game needs it to close, this is called.
        /// </summary>
        public abstract void ForceClose();

        /// <summary>
        /// Is the loadout menu currently open?
        /// </summary>
        /// <returns></returns>
        public bool isOpen;

        /// <summary>
        /// Called when the team has changed
        /// </summary>
        /// <param name="newTeam"></param>
        public virtual void TeamChanged(int newTeam)
        {

        }
    }
}
