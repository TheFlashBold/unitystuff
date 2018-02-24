using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class Kit_AttachmentRenderer : Kit_AttachmentBehaviour
        {
            [Tooltip("These renderers will be enabled if this attachment is selcted.")]
            public Renderer[] renderersToActivate;

            public override void Selected()
            {
                for (int i = 0; i < renderersToActivate.Length; i++)
                {
                    if (renderersToActivate[i])
                    {
                        renderersToActivate[i].enabled = true;
                    }
                    else
                    {
                        Debug.LogError(gameObject.name + ": Renderer at " + i + " is not assigned.");
                    }
                }
            }

            public override void Unselected()
            {
                for (int i = 0; i < renderersToActivate.Length; i++)
                {
                    if (renderersToActivate[i])
                    {
                        renderersToActivate[i].enabled = false;
                    }
                    else
                    {
                        Debug.LogError(gameObject.name + ": Renderer at " + i + " is not assigned.");
                    }
                }
            }
        }
    }
}
