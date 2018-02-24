using System;
using UnityEngine;

namespace MarsFPSKit
{
    public class BasicMouseLookRuntimeData
    {
        public float mouseX; //Rotation on Unity Y-Axis (Player Object)
        public float mouseY; //Rotation on Unity X-Axis (Camera/Weapons)

        public float recoilMouseX; //Recoil on x axis
        public float recoilMouseY; //Recoil on y axis

        public float finalMouseX; //Rotation on Unity Y-Axis with recoil applied
        public float finalMouseY; //Rotation on Unity X-Axis with recoil applied

        /// <summary>
        /// How are we currently leaning -1 (L) to 1 (R)
        /// </summary>
        public float leaningState;

        public Quaternion leaningSmoothState = Quaternion.identity;
    }

    public class BasicMouseLookOthersRuntimeData
    {
        public float mouseY; //Rotation on Unity X-Axis (Camera/Weapons)
        /// <summary>
        /// How are we currently leaning -1 (L) to 1 (R)
        /// </summary>
        public float leaningState;
    }

    /// <summary>
    /// Implements a basic Mouse Look on the X and Y axis
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Looking/Basic Mouse Look")]
    public class Kit_BasicMouseLook : Kit_MouseLookBase
    {
        //The basic sensitivity (Can not be changed by the user by default)
        [Header("Basic Sensitivity")]
        [Tooltip("Input multiplier for x rotation")]
        public float basicSensitivityX = 1f;
        [Tooltip("Input multiplier for y rotation")]
        public float basicSensitivityY = 1f;

        [Header("Limits")]
        public float minY = -85f; //Minimum for y looking
        public float maxY = 85f; //Maximum for y looking

        [Header("Leaning")]
        /// <summary>
        /// Is leaning enabled?
        /// </summary>
        public bool leaningEnabled;

        public Vector3 leftLeanPos;
        public Vector3 leftLeanRot;
        public Vector3 leftLeanWepPos;
        public Vector3 leftLeanWepRot;

        public Vector3 rightLeanPos;
        public Vector3 rightLeanRot;
        public Vector3 rightLeanWepPos;
        public Vector3 rightLeanWepRot;

        public float leaningSpeedMultiplier = 0.7f;

        public float leaningRotationSmoothSpeed = 15f;

        /// <summary>
        /// How fast is the leaning state going to thcange
        /// </summary>
        public float leaningSpeed = 2f;

        public override void CalculateLookUpdate(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData == null || pb.customMouseLookData.GetType() != typeof(BasicMouseLookRuntimeData))
            {
                pb.customMouseLookData = new BasicMouseLookRuntimeData();
            }

            //Get our custom data
            BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;

            //Get Input, if the cursor is locked
            if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer)
            {
                data.mouseY += pb.input.mouseY * basicSensitivityY * pb.weaponManager.CurrentSensitivity(pb);
                data.mouseX += pb.input.mouseX * basicSensitivityX * pb.weaponManager.CurrentSensitivity(pb);

                if (leaningEnabled)
                {
                    if (!pb.movement.IsRunning(pb))
                    {
                        if (pb.input.leanLeft)
                        {
                            data.leaningState = Mathf.MoveTowards(data.leaningState, -1f, Time.deltaTime * leaningSpeed);
                        }
                        else if (pb.input.leanRight)
                        {
                            data.leaningState = Mathf.MoveTowards(data.leaningState, 1f, Time.deltaTime * leaningSpeed);
                        }
                        else
                        {
                            data.leaningState = Mathf.MoveTowards(data.leaningState, 0f, Time.deltaTime * leaningSpeed);
                        }
                    }
                    else
                    {
                        data.leaningState = Mathf.MoveTowards(data.leaningState, 0f, Time.deltaTime * leaningSpeed);
                    }
                }
                else
                {
                    data.leaningState = 0f;
                }
            }

            //Calculate recoil
            if (pb.recoilApplyRotation.eulerAngles.x < 90)
            {
                data.recoilMouseY = pb.recoilApplyRotation.eulerAngles.x;
            }
            else
            {
                data.recoilMouseY = pb.recoilApplyRotation.eulerAngles.x - 360;
            }

            if (pb.recoilApplyRotation.eulerAngles.y < 90)
            {
                data.recoilMouseX = -pb.recoilApplyRotation.eulerAngles.y;
            }
            else
            {
                data.recoilMouseX = -(pb.recoilApplyRotation.eulerAngles.y - 360);
            }

            //Clamp y input
            data.mouseY = Mathf.Clamp(data.mouseY, minY, maxY);
            //Apply reocil
            data.finalMouseY = Mathf.Clamp(data.mouseY + data.recoilMouseY, minY, maxY);

            //Simplify x input
            data.mouseX %= 360;
            //Apply recoil
            data.finalMouseX = data.mouseX + data.recoilMouseX;

            if (!pb.isBot) //Bots cannot handle input like this, so they will need to assign it themselves.
            {
                //Apply rotation
                pb.transform.rotation = Quaternion.Euler(new Vector3(0, data.finalMouseX, 0f));
                //Smooth leaning
                data.leaningSmoothState = Quaternion.Slerp(data.leaningSmoothState, GetCameraRotationOffset(pb), Time.deltaTime * leaningRotationSmoothSpeed);
                pb.mouseLookObject.localRotation = Quaternion.Euler(-data.finalMouseY, 0, 0) * data.leaningSmoothState;
            }
            else
            {
                data.finalMouseY = -pb.mouseLookObject.localEulerAngles.x;
                if (data.finalMouseY < -180) data.finalMouseY += 360;
                data.mouseY = data.finalMouseY;
            }
        }

        public override bool ReachedYMax(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;

                //Check if we reached min or max value on Y
                if (Mathf.Approximately(data.finalMouseY, minY)) return true;
                else if (Mathf.Approximately(data.finalMouseY, maxY)) return true;
            }

            return false;
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    //Get our custom data
                    BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                    //Send y looking
                    stream.SendNext(data.finalMouseY);
                    //Send leaning
                    stream.SendNext(data.leaningState);
                }
                else
                {
                    //Send dummy values
                    stream.SendNext(0f);
                    stream.SendNext(0f);
                }
            }
            else
            {
                //Check if correct object is used by the player
                if (pb.customMouseLookData == null || pb.customMouseLookData.GetType() != typeof(BasicMouseLookOthersRuntimeData))
                {
                    pb.customMouseLookData = new BasicMouseLookOthersRuntimeData();
                }

                //Get our custom data
                BasicMouseLookOthersRuntimeData data = pb.customMouseLookData as BasicMouseLookOthersRuntimeData;
                //Receive look value
                data.mouseY = (float)stream.ReceiveNext();
                //Receive leaning
                data.leaningState = (float)stream.ReceiveNext();
            }
        }

        public override float GetSpeedMultiplier(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                return Mathf.Lerp(1f, leaningSpeedMultiplier, Mathf.Abs(data.leaningState));
            }
            return 1f;
        }

        public override Vector3 GetCameraOffset(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                if (data.leaningState > 0)
                {
                    return Vector3.Lerp(Vector3.zero, rightLeanPos, data.leaningState);
                }
                else
                {
                    return Vector3.Lerp(Vector3.zero, leftLeanPos, Mathf.Abs(data.leaningState));
                }
            }
            return base.GetCameraOffset(pb);
        }

        public override Quaternion GetCameraRotationOffset(Kit_PlayerBehaviour pb)
        {
            //Check if correct object is used by the player
            if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
            {
                //Get our custom data
                BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                if (data.leaningState > 0)
                {
                    return Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(rightLeanRot), data.leaningState);
                }
                else
                {
                    return Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(leftLeanRot), Mathf.Abs(data.leaningState));
                }
            }
            return base.GetCameraRotationOffset(pb);
        }

        public override Vector3 GetWeaponOffset(Kit_PlayerBehaviour pb)
        {
            if (pb.weaponManager.IsAiming(pb))
            {
                return Vector3.zero;
            }
            else
            {
                //Check if correct object is used by the player
                if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    //Get our custom data
                    BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                    if (data.leaningState > 0)
                    {
                        return Vector3.Lerp(Vector3.zero, rightLeanWepPos, data.leaningState);
                    }
                    else
                    {
                        return Vector3.Lerp(Vector3.zero, leftLeanWepPos, Mathf.Abs(data.leaningState));
                    }
                }
            }
            return base.GetWeaponOffset(pb);
        }

        public override Quaternion GetWeaponRotationOffset(Kit_PlayerBehaviour pb)
        {
            if (pb.weaponManager.IsAiming(pb))
            {
                return Quaternion.identity;
            }
            else
            {
                //Check if correct object is used by the player
                if (pb.customMouseLookData != null && pb.customMouseLookData.GetType() == typeof(BasicMouseLookRuntimeData))
                {
                    //Get our custom data
                    BasicMouseLookRuntimeData data = pb.customMouseLookData as BasicMouseLookRuntimeData;
                    if (data.leaningState > 0)
                    {
                        return Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(rightLeanWepRot), data.leaningState);
                    }
                    else
                    {
                        return Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(leftLeanWepRot), Mathf.Abs(data.leaningState));
                    }
                }
            }
            return base.GetWeaponRotationOffset(pb);
        }
    }
}
