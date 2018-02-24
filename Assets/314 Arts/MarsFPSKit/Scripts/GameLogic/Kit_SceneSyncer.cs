using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This custom scene manager makes sure that scenes are properly synced. In contrast to the default manager from Photon, reloading a scene is possible.
    /// </summary>
    public class Kit_SceneSyncer : MonoBehaviour
    {
        public static Kit_SceneSyncer instance;
        /// <summary>
        /// The canvas that displays the loading screen
        /// </summary>
        public GameObject loadingCanvas;
        /// <summary>
        /// The image that displays the progress
        /// </summary>
        public Image loadingBar;

        void Awake()
        {
            if (!instance)
            {
                //Setup callbacks
                SceneManager.activeSceneChanged += ActiveSceneChanged;
                PhotonNetwork.OnEventCall += OnPhotonEvent;
                //Assign instance
                instance = this;
                //Disable default manager
                PhotonNetwork.automaticallySyncScene = false;
                //Enable message queue by default so we can connect to photon
                PhotonNetwork.isMessageQueueRunning = true;
                //Make sure it doens't get destroyed
                DontDestroyOnLoad(this);
                //Hide canvas
                loadingCanvas.SetActive(false);
            }
            else
            {
                //Only one of this instance may be active at a time
                Destroy(gameObject);
            }
        }

        private void ActiveSceneChanged(Scene arg0, Scene arg1)
        {
            //We finished loading. Let Photon handle the things again
            PhotonNetwork.isMessageQueueRunning = true;
            Debug.Log("[Scene Sync] Activating network queue!");
        }

        private void OnPhotonEvent(byte eventCode, object content, int senderId)
        {
            //Last code is reserved for the scene sync
            if (eventCode == (byte)199)
            {
                PhotonNetwork.isMessageQueueRunning = false;
                Debug.Log("[Scene Sync] Deactivating network queue!");
                string scene = (string)content;
                StartCoroutine(LoadSceneAsync(scene));
            }
        }

        IEnumerator LoadSceneAsync(string scene)
        {
            //Reset progress
            loadingBar.fillAmount = 0f;
            //Show canvas
            loadingCanvas.SetActive(true);
            AsyncOperation loading = SceneManager.LoadSceneAsync(scene);
            while (!loading.isDone)
            {
                loadingBar.fillAmount = loading.progress;
                yield return null;
            }
            //Hide canvas again
            loadingCanvas.SetActive(false);
        }

        /// <summary>
        /// Network loads a scene
        /// </summary>
        /// <param name="scene"></param>
        public void LoadScene(string scene)
        {
            if (PhotonNetwork.isMasterClient)
            {
                //First clean up!
                RaiseEventOptions options = new RaiseEventOptions();
                options.CachingOption = EventCaching.RemoveFromRoomCache;
                options.Receivers = ReceiverGroup.All;
                if (PhotonNetwork.offlineMode)
                {
                    PhotonNetwork.OnEventCall(199, scene, 0);
                }
                else
                {
                    PhotonNetwork.RaiseEvent(199, scene, true, options);
                }
                Debug.Log("[Scene Sync] Cleaning up room cache!");
                //Send event to load the new scene
                RaiseEventOptions optionsNew = new RaiseEventOptions();
                //Make sure it is in the GLOBAL cache so it will be there when the master client leaves
                optionsNew.CachingOption = EventCaching.AddToRoomCacheGlobal;
                optionsNew.Receivers = ReceiverGroup.All;
                PhotonNetwork.RaiseEvent(199, scene, true, optionsNew);
                Debug.Log("[Scene Sync] Sending scene load event!");
            }
        }
    }
}
