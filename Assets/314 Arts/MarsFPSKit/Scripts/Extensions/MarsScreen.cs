using UnityEngine;
using System.Collections;

namespace MarsFPSKit
{
    /// <summary>
    /// This class acts as a replacement for the old Screen.lockCursor
    /// </summary>
    public class MarsScreen
    {
        /// <summary>
        /// Lock / unlock the cursor or retrieve its state
        /// </summary>
        public static bool lockCursor
        {
            get
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }
}
