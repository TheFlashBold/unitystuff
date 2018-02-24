using MarsFPSKit.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// A helper class to use coroutines from scriptable objects. The coroutines need to do checks if the instances supplied still exist though otherwise it might throw errors
    /// </summary>
    [DisallowMultipleComponent]
    public class Kit_ScriptableObjectCoroutineHelper : MonoBehaviour
    {
        public static Kit_ScriptableObjectCoroutineHelper instance;

        void Awake()
        {
            //The object should only exist once. Assign the instance
            instance = this;
        }

        public IEnumerator Kick(Transform trans, Vector3 target, float time)
        {
            Quaternion startRotation = trans.localRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(target);
            float rate = 1.0f / time;
            float t = 0.0f;
            while (trans && t < 1.0f)
            {
                //Advance
                t += Time.deltaTime * rate;
                //Slerp to it 
                trans.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }
        }

        public IEnumerator WeaponApplyRecoil(Kit_ModernWeaponScript behaviour, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb, Vector2 target, float time)
        {
            Quaternion startRotation = pb.recoilApplyRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(target.y, -target.x, 0f);
            float rate = 1.0f / time;
            float t = 0.0f;
            while (pb && behaviour && data != null && t < 1.0f)
            {
                //Advance
                t += Time.deltaTime * rate;
                //Slerp to it 
                pb.recoilApplyRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }
        }

        public IEnumerator WeaponBurstFire(Kit_ModernWeaponScript values, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < values.burstBulletsPerShot && data.bulletsLeft > 0)
            {
                //Fire
                bulletsFired++;

                //Subtract bullets
                data.bulletsLeft--;

                //Play sound
                data.soundFire.PlayOneShot(data.weaponRenderer.cachedFireSound);

                //Apply recoil using coroutine helper
                instance.StartCoroutine(instance.WeaponApplyRecoil(values, data, pb, RandomExtensions.RandomBetweenVector2(values.recoilPerShotMin, values.recoilPerShotMax), values.recoilApplyTime));

                //Set firerate
                data.lastFire = Time.time;
                //Play fire animation
                if (data.bulletsLeft == 1)
                {
                    //Last fire
                    data.weaponRenderer.anim.Play("Fire Last", 0, 0f);
                }
                else
                {
                    if (data.isAiming)
                    {
                        //Normal fire (in aiming mode)
                        data.weaponRenderer.anim.Play("Fire Aim", 0, 0f);
                    }
                    else
                    {
                        //Normal fire
                        data.weaponRenderer.anim.Play("Fire", 0, 0f);
                    }
                }

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                //Set shell ejection
                if (values.shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + values.shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.weaponRenderer.muzzleFlash && data.weaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.weaponRenderer.muzzleFlash.Play(true);
                }

                //Simple fire
                if (values.fireTypeMode == FireTypeMode.Simple)
                {
                    values.FireRaycast(pb, data);
                }
                //Pellet fire
                else if (values.fireTypeMode == FireTypeMode.Pellets)
                {
                    //Count how many have been shot
                    int pelletsShot = 0;
                    while (pelletsShot < values.amountOfPellets)
                    {
                        //Increase amount of shot ones
                        pelletsShot++;
                        //Fire
                        values.FireRaycast(pb, data);
                    }
                }

                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }

        public IEnumerator WeaponBurstFireOthers(Kit_ModernWeaponScript values, WeaponControllerOthersRuntimeData data, Kit_PlayerBehaviour pb, int burstLength)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < burstLength)
            {
                //Fire
                bulletsFired++;

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (values.shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + values.shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }

                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }

        public IEnumerator WeaponBurstFireOthers(Kit_ModernWeaponScript values, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb, int burstLength)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < burstLength)
            {
                //Fire
                bulletsFired++;

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                //Set firerate
                data.lastFire = Time.time;

                //Set shell ejection
                if (values.shellEjectionPrefab)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + values.shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }

                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }


        public IEnumerator WeaponBurstFireBot(Kit_ModernWeaponScript values, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb, int burstLength)
        {
            int bulletsFired = 0;

            while (pb && values && data != null && bulletsFired < burstLength)
            {
                //Fire
                bulletsFired++;

                //Set clip
                pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                //Update range
                pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                //Update sound rolloff
                pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                //Play
                pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(values.thirdPersonAnimType);

                //Play Muzzle Flash Particle System, if assigned
                if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                {
                    data.tpWeaponRenderer.muzzleFlash.Play(true);
                }

                yield return new WaitForSeconds(values.burstTimeBetweenShots);
            }
        }

        public IEnumerator NetworkReplaceWeaponWait(Kit_PlayerBehaviour pb, int slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
        {
            while (!pb || pb.customWeaponManagerData == null) yield return null;

            //Get runtime data
            if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerOthersRuntimeData))
            {
                WeaponManagerControllerOthersRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerOthersRuntimeData;
                while (runtimeData.weaponsInUse[slot].runtimeData == null) yield return null;
                //Get old data
                WeaponControllerOthersRuntimeData oldWcrd = runtimeData.weaponsInUse[slot].runtimeData as WeaponControllerOthersRuntimeData;
                //Clean Up
                for (int i = 0; i < oldWcrd.instantiatedObjects.Count; i++)
                {
                    Destroy(oldWcrd.instantiatedObjects[i]);
                }
                //Hide crosshair
                pb.main.hud.DisplayCrosshair(0f);
                //Setup both weapons
                Kit_WeaponInformation newWeaponInfo = pb.gameInformation.allWeapons[weapon];
                //Get their behaviour modules
                Kit_WeaponBase newWeaponBehaviour = newWeaponInfo.weaponBehaviour;
                //Setup new
                newWeaponBehaviour.SetupValues(weapon); //This sets up values in the object itself, nothing else
                object newRuntimeData = newWeaponBehaviour.SetupThirdPersonOthers(pb, newWeaponBehaviour as Kit_ModernWeaponScript, attachments); //This creates the first person objects
                runtimeData.weaponsInUse[slot] = new WeaponReference();
                runtimeData.weaponsInUse[slot].behaviour = newWeaponBehaviour;
                runtimeData.weaponsInUse[slot].runtimeData = newRuntimeData;
                runtimeData.weaponsInUse[slot].attachments = attachments;
                //Select current weapon
                newWeaponBehaviour.DrawWeaponOthers(pb, newRuntimeData);
                //Set current weapon
                runtimeData.currentWeapon = slot;
            }
        }
    }
}
