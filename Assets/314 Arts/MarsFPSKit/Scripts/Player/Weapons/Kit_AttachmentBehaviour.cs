using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Contains information for attachment slots
        /// </summary>
        [System.Serializable]
        public class AttachmentSlot
        {
            /// <summary>
            /// Name of this slot
            /// </summary>
            public string name;

            /// <summary>
            /// The position for the UI dropdown for this slot
            /// </summary>
            public Transform uiPosition;

            /// <summary>
            /// All attachments in this slot
            /// </summary>
            public Attachment[] attachments;
        }

        /// <summary>
        /// An attachment that can be put in a slot
        /// </summary>
        [System.Serializable]
        public class Attachment
        {
            /// <summary>
            /// Name of this attachment
            /// </summary>
            public string name;

            public Kit_AttachmentBehaviour[] attachmentBehaviours;
        }

        public abstract class Kit_AttachmentBehaviour : MonoBehaviour
        {
            /// <summary>
            /// Called when this attachment is selected
            /// </summary>
            public abstract void Selected();

            /// <summary>
            /// Called when this attachment is not selected
            /// </summary>
            public abstract void Unselected();
        }
    }
}