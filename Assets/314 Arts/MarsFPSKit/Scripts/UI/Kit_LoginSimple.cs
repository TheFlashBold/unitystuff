using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// Provides a simple login where you can freely set your name
    /// </summary>
    public class Kit_LoginSimple : Kit_MenuLogin
    {
        //This section contains everything UI realted
        #region UI
        [Header("UI")]
        public GameObject loginRoot;
        public InputField userNameField;
        #endregion

        //This section contains everything that has to be assigned manually in order for this script to work
        #region References
        [Header("References")]
        public Kit_MainMenu mainMenu; //The main menu
        #endregion

        /// <summary>
        /// Set to true once we are logged in
        /// </summary>
        private static bool alreadyLoggedIn;
        /// <summary>
        /// Saved name for the new log in
        /// </summary>
        private string alreadyLoggedInName;

        //The Login process is initiated
        public override void BeginLogin()
        {
            if (!alreadyLoggedIn)
            {
                //Disable Menu
                mainMenu.ChangeMenuState(MenuState.closed);
                //Enable the name set window
                loginRoot.SetActive(true);
                //Generate a guest name
                userNameField.text = "Guest(" + Random.Range(1, 1000) + ")";
            }
            else
            {
                userNameField.text = alreadyLoggedInName;
                Debug.Log("Already logged in");
                //Disable Login window
                loginRoot.SetActive(false);
                //Sucess, continue
                OnLoggedIn(userNameField.text, userNameField.text);
            }
        }

        public void RequestLogin()
        {
            //Check if the user has set a name
            if (!userNameField.text.IsNullOrWhiteSpace())
            {
                Debug.Log("Successfully logged in");
                //Disable Login window
                loginRoot.SetActive(false);
                //Set name
                alreadyLoggedInName = userNameField.text;
                alreadyLoggedIn = true;
                //Sucess, continue
                OnLoggedIn(userNameField.text, userNameField.text);
            }
        }
    }
}
