using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class Kit_PlayerHUD : Kit_PlayerHUDBase
    {
        /// <summary>
        /// This is the root object of hideable HUD-elements
        /// </summary>
        public GameObject root;

        /// <summary>
        /// Reference to the main behaviour
        /// </summary>
        public Kit_IngameMain main;

        /// <summary>
        /// Reference to our canvas
        /// </summary>
        public Canvas canvas;

        /// <summary>
        /// The root of the HP display
        /// </summary>
        [Header("Health")]
        public GameObject healthRoot;
        /// <summary>
        /// How much HP do we have left?
        /// </summary>
        public Text healthText;

        /// <summary>
        /// How many bullets are left in the magazine?
        /// </summary>
        [Header("Ammo")]
        public Text bulletsLeft;
        /// <summary>
        /// How many bullets do we have left to reload?
        /// </summary>
        public Text bulletsLeftToReload; //It's a stylistic decision to split it up, you can do it in one text, if you like.

        [Header("Crosshair")]
        /// <summary>
        /// The root object of the crosshair, so that it can be hidden if needed.
        /// </summary>
        public GameObject crosshairRoot;
        /// <summary>
        /// The left part of the crosshair
        /// </summary>
        public Image crosshairLeft;
        /// <summary>
        /// The right part of the crosshair
        /// </summary>
        public Image crosshairRight;
        /// <summary>
        /// The upper part of the crosshair
        /// </summary>
        public Image crosshairUp;
        /// <summary>
        /// The lower part of the crosshair
        /// </summary>
        public Image crosshairDown;

        [Header("Bloody Screen")]
        /// <summary>
        /// The bloody screen effect when getting hit
        /// </summary>
        public Image bloodyScreen;

        [Header("Hitmarker")]
        public Image hitmarkerImage;
        /// <summary>
        /// How long is a hitmarker going to be displayed?
        /// </summary>
        public float hitmarkerTime;
        /// <summary>
        /// Sound that is going to be played when we hit someone
        /// </summary>
        public AudioClip hitmarkerSound;
        /// <summary>
        /// Audio source for <see cref="hitmarkerSound"/>
        /// </summary>
        public AudioSource hitmarkerAudioSource;
        /// <summary>
        /// At which <see cref="Time.time"/> is the hitmarker going to be completely invisible
        /// </summary>
        private float hitmarkerLastDisplay;
        /// <summary>
        /// Hitmarker color cache.
        /// </summary>
        private Color hitmarkerColor;

        [Header("Hitmarker Spawn Protected")]
        public Image hitmarkerSpawnProtectionImage;
        /// <summary>
        /// How long is a hitmarker going to be displayed?
        /// </summary>
        public float hitmarkerSpawnProtectionTime;
        /// <summary>
        /// Sound that is going to be played when we hit someone
        /// </summary>
        public AudioClip hitmarkerSpawnProtectionSound;
        /// <summary>
        /// Audio source for <see cref="hitmarkerSound"/>
        /// </summary>
        public AudioSource hitmarkerSpawnProtectionAudioSource;
        /// <summary>
        /// At which <see cref="Time.time"/> is the hitmarker going to be completely invisible
        /// </summary>
        private float hitmarkerSpawnProtectionLastDisplay;
        /// <summary>
        /// Hitmarker color cache.
        /// </summary>
        private Color hitmarkerSpawnProtectionColor;

        [Header("Damage Indicator")]
        /// <summary>
        /// The transform which is going to be rotated on the UI
        /// </summary>
        public RectTransform indicatorRotate;
        /// <summary>
        /// The image of the indicator to apply the alpha to
        /// </summary>
        public Image indicatorImage;
        /// <summary>
        /// An object which the player's position is going to be copied to. Parent of the helper.
        /// </summary>
        public Transform indicatorHelperRoot;
        /// <summary>
        /// A helper transform which looks at the last direction we were shot from
        /// </summary>
        public Transform indicatorHelper;
        /// <summary>
        /// How long is the damage indicator going to be visible?
        /// </summary>
        public float indicatorVisibleTime = 5f;
        /// <summary>
        /// Current alpha of the indicator
        /// </summary>
        private float indicatorAlpha;
        /// <summary>
        /// Current position we were shot from last time
        /// </summary>
        private Vector3 indicatorLastPos;

        [Header("Sniper Scope")]
        /// <summary>
        /// The root object of the sniper scope
        /// </summary>
        public GameObject sniperScopeRoot;
        /// <summary>
        /// A help boolean to only set the <see cref="sniperScopeRoot"/> active once
        /// </summary>
        private bool wasSniperScopeActive;

        [Header("Waiting for Players")]
        /// <summary>
        /// Root object of the 'Waiting for players'
        /// </summary>
        public GameObject waitingForPlayersRoot;

        [Header("Player Name Markers")]
        public List<Kit_PlayerMarker> allPlayerMarkers = new List<Kit_PlayerMarker>();
        /// <summary>
        /// Prefab for player markers
        /// </summary>
        public GameObject playerMarkerPrefab;
        /// <summary>
        /// Where do the player markers go?
        /// </summary>
        public RectTransform playerMarkerGo;
        /// <summary>
        /// Color used for friendly markers
        /// </summary>
        public Color friendlyMarkerColor = Color.white;
        /// <summary>
        /// Color used for enemy markers
        /// </summary>
        public Color enemyMarkerColor = Color.red;

        [Header("Spawn Protection")]
        /// <summary>
        /// The root object of the spawn protection
        /// </summary>
        public GameObject spRoot;
        /// <summary>
        /// This displays the time left of the spawn protection
        /// </summary>
        public Text spText;

        [Header("Weapon Pickup")]
        /// <summary>
        /// This displays the weapon pickup
        /// </summary>
        public GameObject weaponPickupRoot;
        /// <summary>
        /// This displays the weapon that is being picked up
        /// </summary>
        public Text weaponPickupText;

        #region Unity Calls
        void Awake()
        {
            //Cache color
            hitmarkerColor = hitmarkerImage.color;
            //SpawnProtection
            hitmarkerSpawnProtectionColor = hitmarkerSpawnProtectionImage.color;
        }

        void Update()
        {
            //Update hitmarker alpha
            hitmarkerColor.a = Mathf.Clamp01(hitmarkerLastDisplay - Time.time);
            //Set the color
            hitmarkerImage.color = hitmarkerColor;

            //Update hitmarker SP alpha
            hitmarkerSpawnProtectionColor.a = Mathf.Clamp01(hitmarkerSpawnProtectionLastDisplay - Time.time);
            //Set the color
            hitmarkerSpawnProtectionImage.color = hitmarkerSpawnProtectionColor;
        }
        #endregion

        #region Custom Calls
        /// <summary>
        /// Shows or hides the HUD. Some parts (such as the hitmarker) will always be visible.
        /// </summary>
        /// <param name="visible"></param>
        public override void SetVisibility(bool visible)
        {
            //Update the active state of root, but only if it doesn't have it already.
            if (root)
            {
                if (visible)
                {
                    if (!root.activeSelf) root.SetActive(true);
                }
                else
                {
                    if (root.activeSelf) root.SetActive(false);
                    //Hide spawn protection too
                    if (spRoot.activeSelf) spRoot.SetActive(false);
                }
            }
        }

        public override void SetWaitingStatus(bool isWaiting)
        {
            if (waitingForPlayersRoot.activeSelf != isWaiting)
            {
                //Set to the required state
                waitingForPlayersRoot.SetActive(isWaiting);
            }
        }

        public override void PlayerStart(Kit_PlayerBehaviour pb)
        {
            indicatorAlpha = 0f;
            //Update state
            wasSniperScopeActive = false;
            //Set state accordingly
            sniperScopeRoot.SetActive(false);
        }

        public override void PlayerUpdate(Kit_PlayerBehaviour pb)
        {
            //Position damage indicator
            indicatorHelperRoot.position = pb.transform.position;
            indicatorHelperRoot.rotation = pb.transform.rotation;
            //Look at
            indicatorHelper.LookAt(indicatorLastPos);
            //Decrease alpha
            if (indicatorAlpha > 0f) indicatorAlpha -= Time.deltaTime;
            //Set alpha
            indicatorImage.color = new Color(1f, 1f, 1f, indicatorAlpha);
            //Set rotation 
            indicatorRotate.localRotation = Quaternion.Euler(0f, 0f, -indicatorHelper.localEulerAngles.y);
        }

        /// <summary>
        /// Displays the hitmarker for <see cref="hitmarkerTime"/> seconds
        /// </summary>
        public override void DisplayHitmarker()
        {
            hitmarkerLastDisplay = Time.time + hitmarkerTime; //Set time of the hitmarker, which makes it visible automatically
            //Play sound
            if (hitmarkerSound)
            {
                hitmarkerAudioSource.clip = hitmarkerSound;
                hitmarkerAudioSource.PlayOneShot(hitmarkerSound);
            }
        }

        public override void DisplayHitmarkerSpawnProtected()
        {
            hitmarkerSpawnProtectionLastDisplay = Time.time + hitmarkerSpawnProtectionTime; //Set time of the hitmarker, which makes it visible automatically
            //Play sound
            if (hitmarkerSpawnProtectionSound)
            {
                hitmarkerSpawnProtectionAudioSource.clip = hitmarkerSpawnProtectionSound;
                hitmarkerSpawnProtectionAudioSource.PlayOneShot(hitmarkerSpawnProtectionSound);
            }
        }

        /// <summary>
        /// Display hit points in the HUD
        /// </summary>
        /// <param name="hp">Amount of hitpoints</param>
        public override void DisplayHealth(float hp)
        {
            if (hp >= 0f)
            {
                if (!healthRoot.activeSelf) healthRoot.SetActive(true);
                //Display the HP
                healthText.text = hp.ToString("F0"); //If you want decimals, change it to F1, F2, etc...
            }
            else
            {
                if (healthRoot.activeSelf) healthRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Display ammo count in the HUD
        /// </summary>
        /// <param name="bl">Bullets left (On the left side)</param>
        /// <param name="bltr">Bullets left to reload (On the right side)</param>
        public override void DisplayAmmo(int bl, int bltr)
        {
            //Set text for bullets left
            bulletsLeft.text = bl.ToString("F0");
            //Set text for bullets left to reload
            bulletsLeftToReload.text = bltr.ToString("F0");
        }

        public override void DisplayCrosshair(float size)
        {
            //For zero or smaller,
            if (size <= 0f)
            {
                //Hide it
                if (crosshairRoot.activeSelf)
                    crosshairRoot.SetActive(false);
            }
            else
            {
                //Show it
                if (!crosshairRoot.activeSelf)
                    crosshairRoot.SetActive(true);

                //Position all crosshair parts accordingly
                crosshairLeft.rectTransform.anchoredPosition = new Vector2 { x = size };
                crosshairRight.rectTransform.anchoredPosition = new Vector2 { x = -size };
                crosshairUp.rectTransform.anchoredPosition = new Vector2 { y = size };
                crosshairDown.rectTransform.anchoredPosition = new Vector2 { y = -size };
            }
        }

        public override void DisplayHurtState(float state)
        {
            //Update bloody screen
            bloodyScreen.color = new Color(1, 1, 1, state);
        }

        public override void DisplayShot(Vector3 from)
        {
            //Set pos
            indicatorLastPos = from;
            //Set alpha
            indicatorAlpha = indicatorVisibleTime;
        }

        public override void DisplaySniperScope(bool display)
        {
            //Check if the state changed
            if (display != wasSniperScopeActive)
            {
                //Update state
                wasSniperScopeActive = display;
                //Set state accordingly
                sniperScopeRoot.SetActive(display);
            }
        }

        public override void DisplayWeaponPickup(bool displayed, int weapon = -1)
        {
            if (displayed)
            {
                if (!weaponPickupRoot.activeSelf)
                    weaponPickupRoot.SetActive(true);
                if (weapon >= 0)
                {
                    //Set name
                    weaponPickupText.text = "Press [F] to pickup: " + main.gameInformation.allWeapons[weapon].weaponName;
                }
            }
            else
            {
                if (weaponPickupRoot.activeSelf)
                    weaponPickupRoot.SetActive(false);
            }
        }

        public override int GetUnusedPlayerMarker()
        {
            for (int i = 0; i < allPlayerMarkers.Count; i++)
            {
                //Check if its not used
                if (!allPlayerMarkers[i].used)
                {
                    //If its not, set it to used
                    allPlayerMarkers[i].used = true;
                    //Activate its root
                    allPlayerMarkers[i].markerRoot.gameObject.SetActive(true);
                    //And return its id
                    return i;
                }
            }
            //If not, add a new one and return that one
            GameObject newMarker = Instantiate(playerMarkerPrefab, playerMarkerGo, false);
            //Reset scale
            newMarker.transform.localScale = Vector3.one;
            //Add
            allPlayerMarkers.Add(newMarker.GetComponent<Kit_PlayerMarker>());
            allPlayerMarkers[allPlayerMarkers.Count - 1].used = true;
            allPlayerMarkers[allPlayerMarkers.Count - 1].markerRoot.gameObject.SetActive(true);
            return allPlayerMarkers.Count - 1;
        }

        public override void ReleasePlayerMarker(int id)
        {
            if (allPlayerMarkers[id].markerRoot)
            {
                //Deactivate its root
                allPlayerMarkers[id].markerRoot.gameObject.SetActive(false);
            }
            //And set it to unused
            allPlayerMarkers[id].used = false;
        }

        public override void UpdatePlayerMarker(int id, PlayerNameState state, Vector3 worldPos, string playerName)
        {
            //Get screen pos
            Vector3 canvasPos = canvas.WorldToCanvas(worldPos, main.mainCamera);
            //Set
            allPlayerMarkers[id].markerRoot.anchoredPosition3D = canvasPos;
            //Check if it is visible at all
            if (canvasPos.z > 0)
            {
                //Check the state
                if (state == PlayerNameState.friendlyClose)
                {
                    //Set name
                    allPlayerMarkers[id].markerText.text = playerName;
                    //Set color
                    allPlayerMarkers[id].markerText.color = friendlyMarkerColor;
                    //Display name
                    allPlayerMarkers[id].markerText.enabled = true;
                    //Dont display marker
                    allPlayerMarkers[id].markerArrow.enabled = false;
                }
                else if (state == PlayerNameState.friendlyFar)
                {
                    //Display marker
                    allPlayerMarkers[id].markerArrow.enabled = true;
                    //Dont display name
                    allPlayerMarkers[id].markerText.enabled = false;
                }
                else if (state == PlayerNameState.enemy)
                {
                    //Set name
                    allPlayerMarkers[id].markerText.text = playerName;
                    //Set color
                    allPlayerMarkers[id].markerText.color = enemyMarkerColor;
                    //Display name
                    allPlayerMarkers[id].markerText.enabled = true;
                    //Dont display marker
                    allPlayerMarkers[id].markerArrow.enabled = false;
                }
                else
                {
                    //Hide all
                    allPlayerMarkers[id].markerText.enabled = false;
                    allPlayerMarkers[id].markerArrow.enabled = false;
                }
            }
            //If its not...
            else
            {
                //...hide all
                allPlayerMarkers[id].markerText.enabled = false;
                allPlayerMarkers[id].markerArrow.enabled = false;
            }
        }

        public override void UpdateSpawnProtection(bool isActive, float timeLeft)
        {
            if (isActive)
            {
                //Activate root
                if (!spRoot.activeSelf) spRoot.SetActive(true);
                //Set time
                spText.text = timeLeft.ToString("F1");
            }
            else
            {
                //Deactivate root
                if (spRoot.activeSelf) spRoot.SetActive(false);
            }
        }
        #endregion
    }
}
