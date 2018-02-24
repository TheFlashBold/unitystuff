using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This holds the voice chat information and controls the display state
    /// </summary>
    public class Kit_PhotonVoiceBehaviour : MonoBehaviour
    {
#if !UNITY_WEBGL
        public PhotonView photonView;
        public PhotonVoiceSpeaker speaker;
        public PhotonVoiceRecorder recorder;

        //UI
        /// <summary>
        /// Reference to the voice chat
        /// </summary>
        private Kit_PhotonVoiceChat photonVoiceChat;
        /// <summary>
        /// Assigned UI ID
        /// </summary>
        private int myUi = -1;
        //END

        void Start()
        {
            photonVoiceChat = FindObjectOfType<Kit_PhotonVoiceChat>();
            if (photonVoiceChat)
            {
                //Get UI
                myUi = photonVoiceChat.GetUnusedUI();
                //Set name
                photonVoiceChat.activeUis[myUi].playerName.text = photonView.owner.NickName;
                //Set different color for myself
                if (photonView.isMine)
                {
                    photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiOwnColor;
                }
            }
        }

        void Update()
        {
            if (myUi >= 0)
            {
                if (photonView.isMine)
                {
                    if (recorder.IsTransmitting)
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(true);
                    }
                    else
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(false);
                    }
                }
                else
                {
                    if (speaker.IsPlaying)
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(true);
                    }
                    else
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(false);
                    }

                    //Set color based on Team
                    if (recorder.AudioGroup == 1)
                    {
                        photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiTeamOneColor;
                    }
                    else if (recorder.AudioGroup == 2)
                    {
                        photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiTeamTwoColor;
                    }
                    else
                    {
                        photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiAllColor;
                    }
                }
            }
        }

        void OnDestroy()
        {
            //If voice chat is active and we have an UI assigned, release it
            if (photonVoiceChat && myUi >= 0)
            {
                photonVoiceChat.ReleaseUI(myUi);
            }
        }
#endif
    }
}
