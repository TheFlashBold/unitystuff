using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace MarsFPSKit
{
    public class Kit_LoadoutDefault : Kit_LoadoutBase
    {
        /// <summary>
        /// Reference to the main object
        /// </summary>
        public Kit_IngameMain mainIngame;

        public Kit_MainMenu mainMenu;

        /// <summary>
        /// The root object of the Loadout menu
        /// </summary>
        public GameObject root;

        /// <summary>
        /// Root where you select weapons and the class
        /// </summary>
        public GameObject weaponSelectionRoot;

        [Header("Weapon Selection")]
        /// <summary>
        /// Dropdown menu for all primary weapons
        /// </summary>
        public Dropdown primaryWeapons;
        /// <summary>
        /// Dropdown menu fro all secondary weapon
        /// </summary>
        public Dropdown secondaryWeapons;

        [Header("Weapon Stats")]
        /// <summary>
        /// Displays the name of the primary weapon
        /// </summary>
        public Text primaryName;
        /// <summary>
        /// The preview of the primary
        /// </summary>
        public Image primaryPreviewPicture;
        /// <summary>
        /// Used to display damage stats
        /// </summary>
        public Image primaryStatsDamage;
        /// <summary>
        /// Used to display fire rate stats
        /// </summary>
        public Image primaryStatsFireRate;
        /// <summary>
        /// Used to display recoil stats
        /// </summary>
        public Image primaryStatsRecoil;
        /// <summary>
        /// Used to display reach stats
        /// </summary>
        public Image primaryStatsReach;

        /// <summary>
        /// Displays the name of the secondary weapon
        /// </summary>
        public Text secondaryName;
        /// <summary>
        /// The preview of the secondary
        /// </summary>
        public Image secondaryPreviewPicture;
        /// <summary>
        /// Used to display damage stats
        /// </summary>
        public Image secondaryStatsDamage;
        /// <summary>
        /// Used to display fire rate stats
        /// </summary>
        public Image secondaryStatsFireRate;
        /// <summary>
        /// Used to display recoil stats
        /// </summary>
        public Image secondaryStatsRecoil;
        /// <summary>
        /// Used to display reach stats
        /// </summary>
        public Image secondaryStatsReach;

        [Header("Player Model")]
        /// <summary>
        /// Where are player models going to be positioned?
        /// </summary>
        public Transform playerModelGo;
        /// <summary>
        /// This list contains all elements instantiated by the player model
        /// </summary>
        private List<GameObject> playerModelObjects = new List<GameObject>();
        /// <summary>
        /// The currently displayed player model
        /// </summary>
        private Kit_ThirdPersonPlayerModel playerModelCurrent;
        /// <summary>
        /// Script to help with IK
        /// </summary>
        private Kit_LoadoutIKHelper playerModelIkHelper;

        /// <summary>
        /// This list contains all elements instantiated by the weapons
        /// </summary>
        private List<GameObject> weaponObjects = new List<GameObject>();


        [Header("Default values")]
        public int defaultPrimary = 0;
        public int defaultSecondary = 1;

        [Header("Weapon Customization")]
        /// <summary>
        /// Weapon customization uses a different canvas that is set to camera overlay
        /// </summary>
        public Canvas weaponCustomizationCanvas;
        /// <summary>
        /// Root where you customize the attachments
        /// </summary>
        public GameObject weaponCustomizationRoot;
        /// <summary>
        /// This is where the weapon prefab is going to be instantiated
        /// </summary>
        public Transform weaponCustomizationPrefabGo;
        /// <summary>
        /// This list contains all elements instantiated in the customization menu!
        /// </summary>
        private List<GameObject> weaponCustomizationObjects = new List<GameObject>();
        [HideInInspector]
        /// <summary>
        /// The renderer of the weapon we are currently customizing
        /// </summary>
        public Weapons.Kit_WeaponRenderer weaponCustomizationCurrentRenderer;
        /// <summary>
        /// The prefab for the drop down UI element that will be displayed for each attachment slot
        /// </summary>
        public GameObject weaponCustomizationDropdownPrefab;

        //RUNTIME DATA
        //[HideInInspector]
        public Loadout[] allLoadouts = new Loadout[5];
        [HideInInspector]
        public int currentLoadout;
        /// <summary>
        /// Which weapon is currently being displayed in the hands
        /// </summary>
        private int currentlyInHand;
        /// <summary>
        /// This list contains all IDs for the primaries
        /// </summary>
        private List<int> primaryWeaponsToID = new List<int>();
        /// <summary>
        /// This list contains all IDs for the secondaries
        /// </summary>
        private List<int> secondaryWeaponsToID = new List<int>();
        //END

        public override void ForceClose()
        {
            //Just normally Close
            Close();
        }

        public override Loadout GetCurrentLoadout()
        {
            //Just return the currently selected loadout
            return allLoadouts[currentLoadout];
        }

        public override void Initialize()
        {
            //Set initial values
            root.SetActive(false);
            isOpen = false;
            currentLoadout = 0;
            //Set Loadouts
            allLoadouts = new Loadout[5];
            for (int i = 0; i < allLoadouts.Length; i++)
            {
                currentLoadout = i;
                allLoadouts[i] = new Loadout();
                SetPrimaryWeapon(defaultPrimary);
                SetSecondaryWeapon(defaultSecondary);
            }

            //Reset to class 0
            currentLoadout = 0;

            List<Dropdown.OptionData> primaryDropdownData = new List<Dropdown.OptionData>();
            List<Dropdown.OptionData> secondaryDropdownData = new List<Dropdown.OptionData>();

            //Clear dropdown menus
            primaryWeapons.ClearOptions();
            secondaryWeapons.ClearOptions();

            if (mainIngame)
            {
                //Get all weapons
                for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                {
                    if (mainIngame.gameInformation.allWeapons[i].weaponType == WeaponType.Primary)
                    {
                        //Add to primary
                        primaryDropdownData.Add(new Dropdown.OptionData(mainIngame.gameInformation.allWeapons[i].weaponName));
                        int toAdd = i; //If we dont do this, all is would be the same
                        primaryWeaponsToID.Add(toAdd);
                    }
                    else if (mainIngame.gameInformation.allWeapons[i].weaponType == WeaponType.Secondary)
                    {
                        secondaryDropdownData.Add(new Dropdown.OptionData(mainIngame.gameInformation.allWeapons[i].weaponName));
                        int toAdd = i; //If we dont do this, all is would be the same
                        secondaryWeaponsToID.Add(toAdd);
                    }
                }
            }
            else if (mainMenu)
            {
                //Get all weapons
                for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                {
                    if (mainMenu.gameInformation.allWeapons[i].weaponType == WeaponType.Primary)
                    {
                        //Add to primary
                        primaryDropdownData.Add(new Dropdown.OptionData(mainMenu.gameInformation.allWeapons[i].weaponName));
                        int toAdd = i; //If we dont do this, all is would be the same
                        primaryWeaponsToID.Add(toAdd);
                    }
                    else if (mainMenu.gameInformation.allWeapons[i].weaponType == WeaponType.Secondary)
                    {
                        secondaryDropdownData.Add(new Dropdown.OptionData(mainMenu.gameInformation.allWeapons[i].weaponName));
                        int toAdd = i; //If we dont do this, all is would be the same
                        secondaryWeaponsToID.Add(toAdd);
                    }
                }
            }

            //Add options
            primaryWeapons.AddOptions(primaryDropdownData);
            secondaryWeapons.AddOptions(secondaryDropdownData);
            //Load
            Load();
        }

        public override void Open()
        {
            //Set bool
            isOpen = true;
            //Enable Loadout menu
            root.SetActive(true);
            weaponSelectionRoot.SetActive(true);
            weaponCustomizationRoot.SetActive(false);
            if (mainIngame)
            {
                //Disable normal UI
                mainIngame.ui_root.SetActive(false);
                //Disable camera
                mainIngame.mainCamera.enabled = false;
            }
            else if (mainMenu)
            {
                //Disable normal UI
                mainMenu.ui_Canvas.gameObject.SetActive(false);
                //Disable camera
                mainMenu.mainCamera.enabled = false;
            }
            //Make sure cursor is unlocked
            MarsScreen.lockCursor = false;
            if (mainIngame)
            {
                //Redraw, so it plays the correct animation!
                if (currentlyInHand == 0)
                {
                    //Primary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (currentlyInHand == 1)
                {
                    //Secondary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
            else if (mainMenu)
            {
                //Redraw, so it plays the correct animation!
                if (currentlyInHand == 0)
                {
                    //Primary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (currentlyInHand == 1)
                {
                    //Secondary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
        }

        /// <summary>
        /// Closes the menu
        /// </summary>
        public void Close()
        {
            //Set bool
            isOpen = false;
            //Disable loadout
            root.SetActive(false);
            if (mainIngame)
            {
                //Enable normal UI
                mainIngame.ui_root.SetActive(true);
                //Enable camera
                mainIngame.mainCamera.enabled = true;
            }
            else if (mainMenu)
            {
                //Enable normal UI
                mainMenu.ui_Canvas.gameObject.SetActive(true);
                //Enable camera
                mainMenu.mainCamera.enabled = true;
            }
            //Save
            Save();
        }

        /// <summary>
        /// Has the loadout been loaded once?
        /// </summary>
        bool loaded = false;

        /// <summary>
        /// Saves current loadout
        /// </summary>
        void Save()
        {
            if (loaded)
            {
                for (int i = 0; i < allLoadouts.Length; i++)
                {
                    PlayerPrefs.SetInt("loadoutClass_" + i + "_primaryWeapon", allLoadouts[i].primaryWeapon);
                    PlayerPrefs.SetInt("loadoutClass_" + i + "_secondaryWeapon", allLoadouts[i].secondaryWeapon);
                    PlayerPrefsExtended.SetIntArray("loadoutClass_" + i + "_primaryAttachments", allLoadouts[i].primaryAttachments);
                    PlayerPrefsExtended.SetIntArray("loadoutClass_" + i + "_secondaryAttachments", allLoadouts[i].secondaryAttachments);
                }
                Debug.Log("[Loadout] Saved!");
            }
        }

        /// <summary>
        /// Loads the saved loadouts
        /// </summary>
        void Load()
        {
            for (int i = 0; i < allLoadouts.Length; i++)
            {
                try
                {
                    allLoadouts[i].primaryWeapon = PlayerPrefs.GetInt("loadoutClass_" + i + "_primaryWeapon", defaultPrimary);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Loading the primary weapon threw exception: " + e + " reverting to default");
                    allLoadouts[i].primaryWeapon = defaultPrimary;
                }
                try
                {
                    allLoadouts[i].secondaryWeapon = PlayerPrefs.GetInt("loadoutClass_" + i + "_secondaryWeapon", defaultSecondary);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Loading the secondary weapon threw exception: " + e + " reverting to default");
                    allLoadouts[i].secondaryWeapon = defaultSecondary;
                }
                if (mainIngame)
                {
                    try
                    {
                        allLoadouts[i].primaryAttachments = PlayerPrefsExtended.GetIntArray("loadoutClass_" + i + "_primaryAttachments", 0, mainIngame.gameInformation.allWeapons[allLoadouts[i].primaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Loading the primary's attachments threw exception: " + e + " reverting to default");
                        allLoadouts[i].primaryAttachments = new int[mainIngame.gameInformation.allWeapons[allLoadouts[i].primaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length];
                    }
                    try
                    {
                        allLoadouts[i].secondaryAttachments = PlayerPrefsExtended.GetIntArray("loadoutClass_" + i + "_secondaryAttachments", 0, mainIngame.gameInformation.allWeapons[allLoadouts[i].secondaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Loading the secondary's attachments threw exception: " + e + " reverting to default");
                        allLoadouts[i].secondaryAttachments = new int[mainIngame.gameInformation.allWeapons[allLoadouts[i].secondaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length];
                    }
                }
                else if (mainMenu)
                {
                    try
                    {
                        allLoadouts[i].primaryAttachments = PlayerPrefsExtended.GetIntArray("loadoutClass_" + i + "_primaryAttachments", 0, mainMenu.gameInformation.allWeapons[allLoadouts[i].primaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Loading the primary's attachments threw exception: " + e + " reverting to default");
                        allLoadouts[i].primaryAttachments = new int[mainMenu.gameInformation.allWeapons[allLoadouts[i].primaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length];
                    }
                    try
                    {
                        allLoadouts[i].secondaryAttachments = PlayerPrefsExtended.GetIntArray("loadoutClass_" + i + "_secondaryAttachments", 0, mainMenu.gameInformation.allWeapons[allLoadouts[i].secondaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Loading the secondary's attachments threw exception: " + e + " reverting to default");
                        allLoadouts[i].secondaryAttachments = new int[mainMenu.gameInformation.allWeapons[allLoadouts[i].secondaryWeapon].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>().attachmentSlots.Length];
                    }
                }
            }
            Debug.Log("[Loadout] Loaded!");
            loaded = true;
        }

        public override void TeamChanged(int newTeam)
        {
            if (mainIngame)
            {
                //Redraw both
                if (newTeam == 0)
                {
                    Redraw(mainIngame.gameInformation.allTeamOnePlayerModels[Random.Range(0, mainIngame.gameInformation.allTeamOnePlayerModels.Length)]);
                }
                else if (newTeam == 1)
                {
                    Redraw(mainIngame.gameInformation.allTeamTwoPlayerModels[Random.Range(0, mainIngame.gameInformation.allTeamTwoPlayerModels.Length)]);
                }
            }
            else if (mainMenu)
            {
                //Redraw both
                if (newTeam == 0)
                {
                    Redraw(mainMenu.gameInformation.allTeamOnePlayerModels[Random.Range(0, mainMenu.gameInformation.allTeamOnePlayerModels.Length)]);
                }
                else if (newTeam == 1)
                {
                    Redraw(mainMenu.gameInformation.allTeamTwoPlayerModels[Random.Range(0, mainMenu.gameInformation.allTeamTwoPlayerModels.Length)]);
                }
            }
        }

        /// <summary>
        /// Redraws with given player model. Call before <see cref="Redraw(Kit_WeaponInformation)"/>
        /// </summary>
        /// <param name="pm"></param>
        void Redraw(Kit_PlayerModelInformation pm)
        {
            //First clean up
            for (int i = 0; i < playerModelObjects.Count; i++)
            {
                Destroy(playerModelObjects[i]);
            }
            //Create new list
            playerModelObjects = new List<GameObject>();
            //Because it erases weapon objects too, also reset that list
            weaponObjects = new List<GameObject>();
            //Instantiate new model
            GameObject newPrefab = Instantiate(pm.prefab, playerModelGo, false);
            //Reset scale
            newPrefab.transform.localScale = Vector3.one;
            //Get player model
            playerModelCurrent = newPrefab.GetComponent<Kit_ThirdPersonPlayerModel>();
            //Setup IK helper
            if (playerModelCurrent.anim)
            {
                playerModelIkHelper = playerModelCurrent.anim.gameObject.AddComponent<Kit_LoadoutIKHelper>();
                playerModelIkHelper.anim = playerModelCurrent.anim;
                playerModelIkHelper.applyIk = false;
            }
            //Set Layer
            newPrefab.transform.SetLayerRecursively(gameObject.layer);
            //Add to list
            playerModelObjects.Add(newPrefab);

            if (mainIngame)
            {
                //Redraw weapons
                if (currentlyInHand == 0)
                {
                    //Primary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (currentlyInHand == 1)
                {
                    //Secondary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
            else if (mainMenu)
            {
                //Redraw weapons
                if (currentlyInHand == 0)
                {
                    //Primary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (currentlyInHand == 1)
                {
                    //Secondary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
        }

        /// <summary>
        /// Redraws with given weapon
        /// </summary>
        /// <param name="wep"></param>
        void Redraw(Kit_WeaponInformation wep, int[] attachments)
        {
            //Clean up
            for (int i = 0; i < weaponObjects.Count; i++)
            {
                Destroy(weaponObjects[i]);
            }

            weaponObjects = new List<GameObject>();

            //Check if we have a player model
            if (playerModelCurrent)
            {
                GameObject newWep = Instantiate(wep.weaponBehaviour.thirdPersonPrefab, playerModelCurrent.weaponsInHandsGo);
                //Set layer
                newWep.transform.SetLayerRecursively(gameObject.layer);
                //Try
                if (newWep.GetComponent<Weapons.Kit_ThirdPersonWeaponRenderer>())
                {
                    Weapons.Kit_ThirdPersonWeaponRenderer tpw = newWep.GetComponent<Weapons.Kit_ThirdPersonWeaponRenderer>();
                    tpw.SetAttachments(attachments, wep.weaponBehaviour as Weapons.Kit_ModernWeaponScript);
                    //Show it
                    tpw.visible = true;
                    //Check if we have ik
                    playerModelIkHelper.leftHandGoal = tpw.leftHandIK;
                    if (playerModelIkHelper.leftHandGoal) playerModelIkHelper.applyIk = true;
                    else playerModelIkHelper.applyIk = false;
                }
                //Else dont apply IK
                else playerModelIkHelper.applyIk = false;
                //Add to the list
                weaponObjects.Add(newWep);
                //Redraw Stats
                RedrawStats();
                //Set model anim type
                if (playerModelCurrent.anim.gameObject.activeInHierarchy) //Only play if animator is active
                {
                    //Tell it to not use transitions
                    playerModelCurrent.anim.SetBool("animChangeTransition", false);
                    playerModelCurrent.SetAnimType(wep.weaponBehaviour.thirdPersonAnimType);
                }
            }
        }

        /// <summary>
        /// Redraws stats for both weapons (primary and secondary)
        /// </summary>
        void RedrawStats()
        {
            if (mainIngame)
            {
                //First redraw primary
                Weapons.WeaponStats stats = mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon].weaponBehaviour.GetStats();
                //Set name
                primaryName.text = mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon].weaponName;
                primaryPreviewPicture.sprite = mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon].weaponPicture;
                primaryStatsDamage.fillAmount = stats.damage / (float)GetHighestDamageStat();
                primaryStatsFireRate.fillAmount = stats.fireRate / (float)GetHighestFireRateStat();
                primaryStatsReach.fillAmount = stats.reach / (float)GetHighestReachStat();
                primaryStatsRecoil.fillAmount = stats.recoil / (float)GetHighestRecoilStat();
                //Then get secondary
                stats = mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon].weaponBehaviour.GetStats();
                //Set name
                secondaryName.text = mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon].weaponName;
                secondaryPreviewPicture.sprite = mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon].weaponPicture;
                secondaryStatsDamage.fillAmount = stats.damage / (float)GetHighestDamageStat();
                secondaryStatsFireRate.fillAmount = stats.fireRate / (float)GetHighestFireRateStat();
                secondaryStatsReach.fillAmount = stats.reach / (float)GetHighestReachStat();
                secondaryStatsRecoil.fillAmount = stats.recoil / (float)GetHighestRecoilStat();
            }
            else if (mainMenu)
            {
                //First redraw primary
                Weapons.WeaponStats stats = mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon].weaponBehaviour.GetStats();
                //Set name
                primaryName.text = mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon].weaponName;
                primaryPreviewPicture.sprite = mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon].weaponPicture;
                primaryStatsDamage.fillAmount = stats.damage / (float)GetHighestDamageStat();
                primaryStatsFireRate.fillAmount = stats.fireRate / (float)GetHighestFireRateStat();
                primaryStatsReach.fillAmount = stats.reach / (float)GetHighestReachStat();
                primaryStatsRecoil.fillAmount = stats.recoil / (float)GetHighestRecoilStat();
                //Then get secondary
                stats = mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon].weaponBehaviour.GetStats();
                //Set name
                secondaryName.text = mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon].weaponName;
                secondaryPreviewPicture.sprite = mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon].weaponPicture;
                secondaryStatsDamage.fillAmount = stats.damage / (float)GetHighestDamageStat();
                secondaryStatsFireRate.fillAmount = stats.fireRate / (float)GetHighestFireRateStat();
                secondaryStatsReach.fillAmount = stats.reach / (float)GetHighestReachStat();
                secondaryStatsRecoil.fillAmount = stats.recoil / (float)GetHighestRecoilStat();
            }
        }

        public void ChangeInHands(int newInHands)
        {
            //Set
            currentlyInHand = newInHands;
            if (mainIngame)
            {
                //Redraw weapon
                if (newInHands == 0)
                {
                    //Primary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (newInHands == 1)
                {
                    //Secondary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
            else if (mainMenu)
            {
                //Redraw weapon
                if (newInHands == 0)
                {
                    //Primary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (newInHands == 1)
                {
                    //Secondary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
        }

        public void ChangeClass(int newClass)
        {
            //Set Class
            currentLoadout = newClass;
            //Set Loadouts
            updateDropdowns = false; //If we dont do this, attachments would get reset as the weapon would be newly assigned.
            primaryWeapons.value = primaryWeaponsToID.IndexOf(allLoadouts[currentLoadout].primaryWeapon);
            secondaryWeapons.value = secondaryWeaponsToID.IndexOf(allLoadouts[currentLoadout].secondaryWeapon);
            updateDropdowns = true;
            if (mainIngame)
            {
                //Redraw
                if (currentlyInHand == 0)
                {
                    //Primary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (currentlyInHand == 1)
                {
                    //Secondary
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
            else if (mainMenu)
            {
                //Redraw
                if (currentlyInHand == 0)
                {
                    //Primary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (currentlyInHand == 1)
                {
                    //Secondary
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
        }

        /// <summary>
        /// Set to false if the callbacks should be ignored
        /// </summary>
        bool updateDropdowns = true;

        public void PrimaryDropdownChanged()
        {
            if (updateDropdowns)
            {
                //Set to primary
                currentlyInHand = 0;
                //Set primary weapon
                SetPrimaryWeapon(primaryWeaponsToID[primaryWeapons.value]);
                if (mainIngame)
                {
                    //Redraw
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
                else if (mainMenu)
                {
                    //Redraw
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
                }
            }
        }

        public void SecondaryDropdownChanged()
        {
            if (updateDropdowns)
            {
                //Set to secondary
                currentlyInHand = 1;
                //Set secondary weapon
                SetSecondaryWeapon(secondaryWeaponsToID[secondaryWeapons.value]);
                if (mainIngame)
                {
                    //Redraw
                    Redraw(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
                else if (mainMenu)
                {
                    //Redraw
                    Redraw(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
                }
            }
        }

        #region Helper functions
        /// <summary>
        /// Gets the highest damage of all weapons
        /// </summary>
        /// <returns></returns>
        public float GetHighestDamageStat()
        {
            float highest = 0.1f;
            if (mainIngame)
            {
                for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                {
                    if (mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().damage > highest)
                    {
                        highest = mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().damage;
                    }
                }
            }
            else if (mainMenu)
            {
                for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                {
                    if (mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().damage > highest)
                    {
                        highest = mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().damage;
                    }
                }
            }
            return highest;
        }

        /// <summary>
        /// Gets the highest fire rate of all weapons
        /// </summary>
        /// <returns></returns>
        public float GetHighestFireRateStat()
        {
            float highest = 0.1f;
            if (mainIngame)
            {
                for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                {
                    if (mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().fireRate > highest)
                    {
                        highest = mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().fireRate;
                    }
                }
            }
            else if (mainMenu)
            {
                for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                {
                    if (mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().fireRate > highest)
                    {
                        highest = mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().fireRate;
                    }
                }
            }
            return highest;
        }

        /// <summary>
        /// Gets the highest recoil of all weapons
        /// </summary>
        /// <returns></returns>
        public float GetHighestRecoilStat()
        {
            float highest = 0.1f;
            if (mainIngame)
            {
                for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                {
                    if (mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().recoil > highest)
                    {
                        highest = mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().recoil;
                    }
                }
            }
            else if (mainMenu)
            {
                for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                {
                    if (mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().recoil > highest)
                    {
                        highest = mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().recoil;
                    }
                }
            }
            return highest;
        }

        /// <summary>
        /// Gets the higehst reach of all weapons
        /// </summary>
        /// <returns></returns>
        public float GetHighestReachStat()
        {
            float highest = 0.1f;
            if (mainIngame)
            {
                for (int i = 0; i < mainIngame.gameInformation.allWeapons.Length; i++)
                {
                    if (mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().reach > highest)
                    {
                        highest = mainIngame.gameInformation.allWeapons[i].weaponBehaviour.GetStats().reach;
                    }
                }
            }
            else if (mainMenu)
            {
                for (int i = 0; i < mainMenu.gameInformation.allWeapons.Length; i++)
                {
                    if (mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().reach > highest)
                    {
                        highest = mainMenu.gameInformation.allWeapons[i].weaponBehaviour.GetStats().reach;
                    }
                }
            }
            return highest;
        }
        #endregion

        /// <summary>
        /// Sets the primary weapon and sets the attachments
        /// </summary>
        /// <param name="id"></param>
        void SetPrimaryWeapon(int id)
        {
            allLoadouts[currentLoadout].primaryWeapon = id;
            Weapons.Kit_WeaponRenderer renderer = null;
            if (mainIngame)
            {
                //Get Renderer
                renderer = mainIngame.gameInformation.allWeapons[id].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>();
            }
            else if (mainMenu)
            {
                //Get Renderer
                renderer = mainMenu.gameInformation.allWeapons[id].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>();
            }
            allLoadouts[currentLoadout].primaryAttachments = new int[renderer.attachmentSlots.Length];
        }

        /// <summary>
        /// Sets the secondary weapon and sets the attachments
        /// </summary>
        /// <param name="id"></param>
        void SetSecondaryWeapon(int id)
        {
            allLoadouts[currentLoadout].secondaryWeapon = id;
            Weapons.Kit_WeaponRenderer renderer = null;
            if (mainIngame)
            {
                renderer = mainIngame.gameInformation.allWeapons[id].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>();
            }
            else if (mainMenu)
            {
                renderer = mainMenu.gameInformation.allWeapons[id].weaponBehaviour.firstPersonPrefab.GetComponent<Weapons.Kit_WeaponRenderer>();
            }
            allLoadouts[currentLoadout].secondaryAttachments = new int[renderer.attachmentSlots.Length];
        }

        public void CustomizePrimary()
        {
            if (mainIngame)
            {
                //Setup customization menu
                SetupCustomization(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
            }
            else if (mainMenu)
            {
                //Setup customization menu
                SetupCustomization(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].primaryWeapon], allLoadouts[currentLoadout].primaryAttachments);
            }
            //Proceed
            ProceedToCustomization();
            //Set values
            currentlyInHand = 0;
        }

        public void CustomizeSecondary()
        {
            if (mainIngame)
            {
                //Setup customization menu
                SetupCustomization(mainIngame.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
            }
            else if (mainMenu)
            {
                //Setup customization menu
                SetupCustomization(mainMenu.gameInformation.allWeapons[allLoadouts[currentLoadout].secondaryWeapon], allLoadouts[currentLoadout].secondaryAttachments);
            }
            //Proceed
            ProceedToCustomization();
            //Set values
            currentlyInHand = 1;
        }

        void SetupCustomization(Kit_WeaponInformation inf, int[] attachments)
        {
            //Clean up
            for (int i = 0; i < weaponCustomizationObjects.Count; i++)
            {
                if (weaponCustomizationObjects[i]) Destroy(weaponCustomizationObjects[i]);
            }
            //Reset list
            weaponCustomizationObjects = new List<GameObject>();
            //Instantiate FP prefab
            GameObject prefab = Instantiate(inf.weaponBehaviour.firstPersonPrefab, weaponCustomizationPrefabGo, false);
            //Add to list
            weaponCustomizationObjects.Add(prefab);
            //Reset scale
            prefab.transform.localScale = Vector3.one;
            //Set Layer!
            prefab.transform.SetLayerRecursively(gameObject.layer);
            //Assign
            weaponCustomizationCurrentRenderer = prefab.GetComponent<Weapons.Kit_WeaponRenderer>();
            //Setup initial attachments
            weaponCustomizationCurrentRenderer.SetAttachments(attachments, inf.weaponBehaviour as Weapons.Kit_ModernWeaponScript);
            //Show
            weaponCustomizationCurrentRenderer.visible = true;
            //Hide renderers
            for (int i = 0; i < weaponCustomizationCurrentRenderer.hideInCustomiazionMenu.Length; i++)
            {
                weaponCustomizationCurrentRenderer.hideInCustomiazionMenu[i].enabled = false;
            }
            //Set offset
            weaponCustomizationCurrentRenderer.transform.localPosition = weaponCustomizationCurrentRenderer.customizationMenuOffset;
            //Setup drop down menus
            for (int i = 0; i < weaponCustomizationCurrentRenderer.attachmentSlots.Length; i++)
            {
                GameObject uiPrefab = Instantiate(weaponCustomizationDropdownPrefab, weaponCustomizationCanvas.transform, true);
                //Reparent
                uiPrefab.transform.SetParent(weaponCustomizationCurrentRenderer.attachmentSlots[i].uiPosition, false);
                //Reset rotation
                uiPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
                //Set scale
                uiPrefab.transform.localScale = Vector3.one;
                //Set position
                uiPrefab.transform.localPosition = Vector3.zero;
                //Reset to 1,1,1 world (!!!) scale!
                uiPrefab.transform.localScale = new Vector3 { x = uiPrefab.transform.localScale.x / weaponCustomizationPrefabGo.localScale.x, y = uiPrefab.transform.localScale.y / weaponCustomizationPrefabGo.localScale.y, z = uiPrefab.transform.localScale.z / weaponCustomizationPrefabGo.localScale.z };
                //Add
                weaponCustomizationObjects.Add(uiPrefab);

                //Setup dropdown
                Dropdown dd = uiPrefab.GetComponent<Dropdown>();
                //Clear
                dd.ClearOptions();
                List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();

                for (int o = 0; o < weaponCustomizationCurrentRenderer.attachmentSlots[i].attachments.Length; o++)
                {
                    Dropdown.OptionData data = new Dropdown.OptionData();
                    //Setup data
                    data.text = weaponCustomizationCurrentRenderer.attachmentSlots[i].attachments[o].name;
                    newOptions.Add(data);
                }

                //Set options
                dd.AddOptions(newOptions);

                //Set default value
                if (i < attachments.Length)
                {
                    dd.value = attachments[i];
                }
                else
                {
                    attachments = new int[weaponCustomizationCurrentRenderer.attachmentSlots.Length];
                    dd.value = attachments[i];
                }

                int current = i;

                //Set callback
                dd.onValueChanged.AddListener(delegate { attachments[current] = dd.value; UpdateCustomization(attachments, inf.weaponBehaviour as Weapons.Kit_ModernWeaponScript); });
            }
        }

        void UpdateCustomization(int[] attachments, Weapons.Kit_ModernWeaponScript wep)
        {
            if (weaponCustomizationCurrentRenderer)
            {
                weaponCustomizationCurrentRenderer.SetAttachments(attachments, wep);
            }
        }

        public void ProceedToCustomization()
        {
            //Enable Loadout menu
            root.SetActive(true);
            //Activate customization page
            weaponSelectionRoot.SetActive(false);
            weaponCustomizationRoot.SetActive(true);
        }

        public void BackToSelection()
        {
            //Enable Loadout menu
            root.SetActive(true);
            //Activate selection page
            weaponSelectionRoot.SetActive(true);
            weaponCustomizationRoot.SetActive(false);
            //Redraw team
            if (mainIngame)
            {
                //Redraw both
                if (mainIngame.assignedTeamID == 0)
                {
                    Redraw(mainIngame.gameInformation.allTeamOnePlayerModels[Random.Range(0, mainIngame.gameInformation.allTeamOnePlayerModels.Length)]);
                }
                else if (mainIngame.assignedTeamID == 1)
                {
                    Redraw(mainIngame.gameInformation.allTeamTwoPlayerModels[Random.Range(0, mainIngame.gameInformation.allTeamTwoPlayerModels.Length)]);
                }
            }
            else if (mainMenu)
            {
                Redraw(mainMenu.gameInformation.allTeamOnePlayerModels[Random.Range(0, mainMenu.gameInformation.allTeamOnePlayerModels.Length)]);
            }
        }
    }
}