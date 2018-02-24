using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MarsFPSKit
{
    /// <summary>
    /// All input for the player (e.g. LMB, W,A,S,D, etc) should be stored here, so that bots may use the same scripts.
    /// </summary>
    public class Kit_PlayerInput
    {
        public float hor;
        public float ver;
        public bool crouch;
        public bool sprint;
        public bool jump;
        public bool weaponOne;
        public bool weaponTwo;
        public bool dropWeapon;
        public bool lmb;
        public bool rmb;
        public bool reload;
        public float mouseX;
        public float mouseY;
        public bool leanLeft;
        public bool leanRight;
    }

    public class Kit_PlayerBehaviour : Photon.MonoBehaviour, IPunObservable
    {
        #region Game Information
        [Header("Internal Game Information")]
        [Tooltip("This object contains all game information such as Maps, Game Modes and Weapons")]
        public Kit_GameInformation gameInformation;
        #endregion

        //This section contains everything for the local camera control
        #region Camera Control
        [Header("Camera Control")]
        public Transform playerCameraTransform;
        /// <summary>
        /// Transform that should be used for camera animations from weapons
        /// </summary>
        public Transform playerCameraAnimationTransform;
        /// <summary>
        /// Fall effects should be applied here
        /// </summary>
        public Transform playerCameraFallDownTransform;
        #endregion

        //This section contains everything for the movement
        #region Movement
        [Header("Movement")]
        public Kit_MovementBase movement; //The system used for movement
        //Object used to store custom movement data
        [HideInInspector]
        public object customMovementData;

        public CharacterController cc; //Our Character Controller, assign it here
        public AudioSource footstepSource; //Our footstep audio source
        #endregion

        //This section contains everything for the Mouse Look
        #region Looking
        [Header("Mouse Look")]
        public Kit_MouseLookBase looking; //The system used for looking
        public Transform mouseLookObject; //The transform used for looking around
        [HideInInspector]
        /// <summary>
        /// This is used by the mouse looking script to apply the recoil and by the weapon script to set the recoil
        /// </summary>
        public Quaternion recoilApplyRotation;
        [HideInInspector]
        public object customMouseLookData; //Used to store custom mouse look data
        #endregion

        //This section contains everything for the weapons
        #region Weapons
        [Header("Weapons")]
        public Weapons.Kit_WeaponManagerBase weaponManager; //The system used for weapon management
        public Transform weaponsGo;
        [HideInInspector]
        public object customWeaponManagerData; //Used to store custom weapon manager data

        /// <summary>
        /// Layermask for use with weapon Raycasts
        /// </summary>
        [Tooltip("These layers will be hit by Raycasts that weapons use")]
        public LayerMask weaponHitLayers;
        #endregion

        #region Player Vitals
        [Header("Player Vitals")]
        public Kit_VitalsBase vitalsManager;
        [HideInInspector]
        public object customVitalsData;
        #endregion

        #region Player Name UI
        [Header("Player Name UI")]
        public Kit_PlayerNameUIBase nameManager;
        public object customNameData;
        #endregion

        #region Spawn Protection
        [Header("Spawn Protection")]
        public Kit_SpawnProtectionBase spawnProtection;
        public object customSpawnProtectionData;
        #endregion

        #region Bots
        [Header("Bot Controls")]
        /// <summary>
        /// This module will control the behaviour of the bot
        /// </summary>
        public Kit_PlayerBotControlBase botControls;
        /// <summary>
        /// Use this to store runtime data for bot control
        /// </summary>
        public object botControlsRuntimeData;
        #endregion

        //This section contains internal variables
        #region Internal Variables
        //Team
        public int myTeam = -1;
        /// <summary>
        /// Returns true if this is our player
        /// </summary>
        [HideInInspector]
        public bool isController = false; //Are we the player that is controlling this Player object?

        /// <summary>
        /// Input for the player, only assigned if we are controlling this player or we are the master client
        /// </summary>
        public Kit_PlayerInput input;

        /// <summary>
        /// Is this player being controlled by AI?
        /// </summary>
        [HideInInspector]
        public bool isBot;
        /// <summary>
        /// If this player is a bot, this is its ID
        /// </summary>
        [HideInInspector]
        public int botId;
        [HideInInspector]
        public Kit_IngameMain main; //The main object of this scene, dynamically assigned at runtime

        //Position and rotation are synced by photon transform view
        private bool syncSetup;

        //We cache this value to avoid to calculate it many times
        [HideInInspector]
        public bool canControlPlayer = true;

        //Third Person Model
        [HideInInspector]
        public Kit_ThirdPersonPlayerModel thirdPersonPlayerModel;
        [HideInInspector]
        /// <summary>
        /// Last forward vector from where we were shot
        /// </summary>
        public Vector3 ragdollForward;
        [HideInInspector]
        /// <summary>
        /// Last force which we were shot with
        /// </summary>
        public float ragdollForce;
        [HideInInspector]
        /// <summary>
        /// Last point from where we were shot
        /// </summary>
        public Vector3 ragdollPoint;
        [HideInInspector]
        /// <summary>
        /// Which collider should the force be applied to?
        /// </summary>
        public int ragdollId;
        #endregion

        /// <summary>
        /// Sets up local player for controls, if owned by the local player
        /// </summary>
        public void TakeControl()
        {
            if (photonView.isMine)
            {
                //Assign input
                input = new Kit_PlayerInput();
                //Start coroutine to take control after player is setup.
                StartCoroutine(TakeControlWait());
            }
        }

        /// <summary>
        /// Because it can take a moment to set everything up, here we wait for it, then proceed.
        /// </summary>
        /// <returns></returns>
        IEnumerator TakeControlWait()
        {
            if (photonView.isMine)
            {
                while (!thirdPersonPlayerModel) yield return null;

                isController = true;
                //Move camera to the right position
                main.activeCameraTransform = playerCameraTransform;
                //Setup third person model
                thirdPersonPlayerModel.FirstPerson();
                //Setup weapon manager
                weaponManager.SetupManager(this, photonView.instantiationData);
                //Setup Vitals
                vitalsManager.Setup(this);
                //Show HUD
                main.hud.SetVisibility(true);
                //Lock the cursor
                MarsScreen.lockCursor = true;
                //Close pause menu
                main.isPauseMenuOpen = false;
                main.pm_root.SetActive(false);
            }
        }

        #region Unity Calls
        void Start()
        {
            //0 = Team
            //1 = Primary
            //2 = Secondary
            object[] instObjects = photonView.instantiationData;
            //Copy team
            myTeam = (int)instObjects[0];
            //Assign input if this is a bot
            isBot = (bool)instObjects[1];
            if (isBot)
            {
                input = new Kit_PlayerInput();
                botId = (int)instObjects[2];
                Kit_BotManager manager = FindObjectOfType<Kit_BotManager>();
                manager.AddActiveBot(this);
                //Initialize bot input
                botControls.InitializeControls(this);
            }
            //Set up player model
            if (myTeam == 0) //Team 1
            {
                //Instantiate one random player model for team 1
                GameObject go = Instantiate(main.gameInformation.allTeamOnePlayerModels[Random.Range(0, main.gameInformation.allTeamOnePlayerModels.Length)].prefab, transform, false);
                //Reset scale
                go.transform.localScale = Vector3.one;
                //Assign
                thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                //Setup
                thirdPersonPlayerModel.SetupModel(this);
                //Make it third person initially
                thirdPersonPlayerModel.ThirdPerson();
            }
            else //Team 2
            {
                //Instantiate one random player model for team 2
                GameObject go = Instantiate(main.gameInformation.allTeamTwoPlayerModels[Random.Range(0, main.gameInformation.allTeamTwoPlayerModels.Length)].prefab, transform, false);
                //Reset scale
                go.transform.localScale = Vector3.one;
                //Assign
                thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                //Setup
                thirdPersonPlayerModel.SetupModel(this);
                //Make it third person initially
                thirdPersonPlayerModel.ThirdPerson();
            }

            //Start Spawn Protection
            if (spawnProtection)
            {
                spawnProtection.CustomStart(this);
            }

            if (isBot)
            {
                weaponManager.SetupManagerBot(this, photonView.instantiationData);

                //Setup Vitals
                vitalsManager.Setup(this);

                //Setup done
                syncSetup = true;

                //Setup marker
                if (nameManager)
                {
                    nameManager.StartRelay(this);
                }
            }
            else
            {
                //Setup weapon manager for the others
                if (!photonView.isMine)
                {
                    weaponManager.SetupManagerOthers(this, photonView.instantiationData);

                    //Setup done
                    syncSetup = true;

                    //Setup marker
                    if (nameManager)
                    {
                        nameManager.StartRelay(this);
                    }
                }
                else
                {
                    main.hud.PlayerStart(this);
                    //Disable our own name hitbox
                    thirdPersonPlayerModel.enemyNameAboveHeadTrigger.enabled = false;
                }
            }
        }

        void OnEnable()
        {
            //Find main reference
            main = FindObjectOfType<Kit_IngameMain>();
        }

        bool isShuttingDown = false;

        void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        void OnDestroy()
        {
            if (!isShuttingDown)
            {
                //Hide HUD if we were killed
                if (isController && !isBot)
                {
                    main.hud.SetVisibility(false);
                }
                if (!photonView.isMine || !photonView.isOwnerActive || isBot)
                {
                    //Release marker
                    if (nameManager)
                    {
                        nameManager.OnDestroyRelay(this);
                    }
                }
                //Make sure the camera never gets destroyed
                if (main.activeCameraTransform == playerCameraTransform && !isBot)
                {
                    main.activeCameraTransform = main.spawnCameraPosition;
                    //Set Fov
                    main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                }

                //Unparent sounds
                thirdPersonPlayerModel.soundFire.transform.parent = null;
                if (thirdPersonPlayerModel.soundFire.clip)
                {
                    Destroy(thirdPersonPlayerModel.soundFire.gameObject, thirdPersonPlayerModel.soundFire.clip.length);
                }
                else
                {
                    Destroy(thirdPersonPlayerModel.soundFire.gameObject, 1f);
                }
            }
        }

        void Update()
        {
            if (photonView)
            {
                //If we are not the owner of the photonView, we need to update position and rotation
                if (!photonView.isMine)
                {
                    if (syncSetup)
                    {
                        //Weapon manager update for others
                        weaponManager.CustomUpdateOthers(this);
                    }

                    if (isBot)
                    {
                        if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateEnemy(this);
                            }
                        }
                        else
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateFriendly(this);
                            }
                        }
                    }
                    else
                    {
                        if (main.currentGameModeBehaviour.AreWeEnemies(main, false, photonView.owner.ID))
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateEnemy(this);
                            }
                        }
                        else
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateFriendly(this);
                            }
                        }
                    }
                }
                else if (isBot)
                {
                    if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }

                if (photonView.isMine)
                {
                    if (!isBot)
                    {
                        //Get all input
                        input.hor = Input.GetAxis("Horizontal");
                        input.ver = Input.GetAxis("Vertical");
                        input.crouch = Input.GetButton("Crouch");
                        input.sprint = Input.GetButton("Sprint");
                        input.jump = Input.GetKey(KeyCode.Space);
                        input.weaponOne = Input.GetKey(KeyCode.Alpha1);
                        input.weaponTwo = Input.GetKey(KeyCode.Alpha2);
                        input.dropWeapon = Input.GetKey(KeyCode.F);
                        input.lmb = Input.GetMouseButton(0);
                        input.rmb = Input.GetKey(KeyCode.Mouse1);
                        input.reload = Input.GetKey(KeyCode.R);
                        input.mouseX = Input.GetAxisRaw("Mouse X");
                        input.mouseY = Input.GetAxisRaw("Mouse Y");
                        input.leanLeft = Input.GetButton("Lean Left");
                        input.leanRight = Input.GetButton("Lean Right");
                    }
                    else
                    {
                        //Get Bot Input
                        botControls.WriteToPlayerInput(this);
                    }

                    //Update control value
                    canControlPlayer = main.currentGameModeBehaviour.CanControlPlayer(main);

                    //If we are the controller, update everything
                    if (isController || isBot && PhotonNetwork.isMasterClient)
                    {
                        movement.CalculateMovementUpdate(this);
                        looking.CalculateLookUpdate(this);
                        weaponManager.CustomUpdate(this);
                        vitalsManager.CustomUpdate(this);
                        //Update spawn protection
                        if (spawnProtection)
                        {
                            spawnProtection.CustomUpdate(this);
                        }

                        //Update hud
                        if (main && main.hud && !isBot)
                        {
                            main.hud.PlayerUpdate(this);
                        }
                    }
                }
                //Footstep callback
                movement.CalculateFootstepsUpdate(this);
            }
        }

        void LateUpdate()
        {
            //If we are the controller, update everything
            if (isController || isBot && PhotonNetwork.isMasterClient)
            {
                movement.CalculateMovementLateUpdate(this);
                looking.CalculateLookLateUpdate(this);
            }

            //If we are not the owner of the photonView, we need to update position and rotation
            if (!photonView.isMine)
            {
                if (isBot)
                {
                    if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }
                else
                {
                    if (main.currentGameModeBehaviour.AreWeEnemies(main, false, photonView.owner.ID))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }
            }
            else if (isBot)
            {
                if (main.currentGameModeBehaviour.AreWeEnemies(main, true, botId))
                {
                    if (nameManager)
                    {
                        nameManager.UpdateEnemy(this);
                    }
                }
                else
                {
                    if (nameManager)
                    {
                        nameManager.UpdateFriendly(this);
                    }
                }
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Relay to movement script
            movement.OnControllerColliderHitRelay(this, hit);
            //Relay to mouse look script
            looking.OnControllerColliderHitRelay(this, hit);
            //Relay to weapon manager
            weaponManager.OnControllerColliderHitRelay(this, hit);
        }
        #endregion

        #region Photon Calls
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //Movement
            movement.OnPhotonSerializeView(this, stream, info);
            //Mouse Look
            looking.OnPhotonSerializeView(this, stream, info);
            //Spawn Protection
            if (spawnProtection)
            {
                spawnProtection.OnPhotonSerializeView(this, stream, info);
            }
            //Weapon manager
            weaponManager.OnPhotonSerializeView(this, stream, info);
            //Relay
            if (isBot)
            {
                //Bot Controls
                botControls.OnPhotonSerializeView(this, stream, info);
            }
        }
        #endregion

        #region Custom Calls
        public void LocalDamage(float dmg, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id, bool botShot, int idWhoShot)
        {
            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", PhotonTargets.MasterClient, dmg, botShot, idWhoShot, gunID, shotPos, forward, force, hitPos, id);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", photonView.owner, dmg, botShot, idWhoShot, gunID, shotPos, forward, force, hitPos, id);
                }
            }
        }

        public void ApplyFallDamage(float dmg)
        {
            if (isController && photonView)
            {
                vitalsManager.ApplyFallDamage(this, dmg);
            }
        }

        public void Suicide()
        {
            if (isController && photonView)
            {
                vitalsManager.Suicide(this);
            }
        }

        /// <summary>
        /// Kill the player by cause.
        /// </summary>
        /// <param name="cause"></param>
        public void Die(int cause)
        {
            if (photonView)
            {
                if (photonView.isMine)
                {
                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = 0; //Event 0 = player dead
                                     //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(5);
                    if (isBot)
                    {
                        deathInformation[(byte)0] = true;
                        //Who killed us?
                        deathInformation[(byte)1] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)0] = false;
                        //Who killed us?
                        deathInformation[(byte)1] = photonView.owner.ID;
                    }
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.owner.ID;
                    }
                    deathInformation[(byte)4] = cause;
                    //With which weapon were we killed?
                    if (PhotonNetwork.offlineMode)
                    {
                        PhotonNetwork.OnEventCall(evCode, deathInformation, 0);
                    }
                    else
                    {
                        PhotonNetwork.RaiseEvent(evCode, deathInformation, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
                    }
                    //Instantiate Ragdoll
                    thirdPersonPlayerModel.CreateRagdoll();
                    //Destroy the player
                    PhotonNetwork.Destroy(photonView);
                }
            }
        }

        public void Die(bool botShot, int killer, int gunID)
        {
            if (photonView)
            {
                if (photonView.isMine)
                {
                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = 0; //Event 0 = player dead
                    //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(5);
                    deathInformation[(byte)0] = botShot;
                    //Who killed us?
                    deathInformation[(byte)1] = killer;
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.owner.ID;
                    }
                    //With which weapon were we killed?
                    deathInformation[(byte)4] = gunID;
                    if (PhotonNetwork.offlineMode)
                    {
                        PhotonNetwork.OnEventCall(evCode, deathInformation, 0);
                    }
                    else
                    {
                        PhotonNetwork.RaiseEvent(evCode, deathInformation, true, new RaiseEventOptions { Receivers = ReceiverGroup.All });
                    }
                    //Instantiate Ragdoll
                    thirdPersonPlayerModel.CreateRagdoll();
                    //Destroy the player
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void ApplyDamageNetwork(float dmg, bool botShot, int idWhoShot, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id)
        {
            if (isController || isBot && PhotonNetwork.isMasterClient)
            {
                ragdollForce = force;
                ragdollForward = forward;
                ragdollPoint = hitPos;
                ragdollId = id;
                //Relay to the assigned manager
                vitalsManager.ApplyDamage(this, dmg, botShot, idWhoShot, gunID);
                if (!isBot)
                {
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        //If we fire using a semi auto weapon, this is called
        [PunRPC]
        public void WeaponSemiFireNetwork()
        {
            //Relay to weapon manager
            weaponManager.NetworkSemiRPCReceived(this);
        }

        //If we fire using a bolt action weapon, this is called
        [PunRPC]
        public void WeaponBoltActionFireNetwork(int state)
        {
            //Relay to weapon manager
            weaponManager.NetworkBoltActionRPCReceived(this, state);
        }

        [PunRPC]
        public void WeaponBurstFireNetwork(int burstLength)
        {
            //Relay to weapon manager
            weaponManager.NetworkBurstRPCReceived(this, burstLength);
        }

        //When we reload, this is called
        [PunRPC]
        public void WeaponReloadNetwork(bool empty)
        {
            //Reload to weapon manager
            weaponManager.NetworkReloadRPCReceived(this, empty);
        }

        //When a procedural reload occurs, this will be called with the correct stage
        [PunRPC]
        public void WeaponProceduralReloadNetwork(int stage)
        {
            //Relay to weapon manager
            weaponManager.NetworkProceduralReloadRPCReceived(this, stage);
        }

        [PunRPC]
        public void WeaponRaycastHit(Vector3 pos, Vector3 normal, int material)
        {
            if (material == -1)
            {
                //Relay to impact processor
                main.impactProcessor.ProcessEnemyImpact(pos, normal);
            }
            else
            {
                //Relay to impact processor
                main.impactProcessor.ProcessImpact(pos, normal, material);
            }
        }

        [PunRPC]
        public void ReplaceWeapon(int slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
        {
            //Relay to weapon manager
            weaponManager.NetworkReplaceWeapon(this, slot, weapon, bulletsLeft, bulletsLeftToReload, attachments);
        }
        #endregion
    }
}
