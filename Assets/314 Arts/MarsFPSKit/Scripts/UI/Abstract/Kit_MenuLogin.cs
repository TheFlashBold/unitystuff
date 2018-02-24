using UnityEngine;
using System.Collections;

namespace MarsFPSKit
{
    /// <summary>
    /// Abstract class for logging in (Simple, Login behaviour, Steamworks, etc..)
    /// </summary>
    public abstract class Kit_MenuLogin : MonoBehaviour
    {
        /// <summary>
        /// Initiates the login process
        /// </summary>
        public abstract void BeginLogin();
        /// <summary>
        /// Will be called when the login process is done
        /// </summary>
        public delegate void LoggedIn(string playerName, string userId);

        public LoggedIn OnLoggedIn;
    }
}
