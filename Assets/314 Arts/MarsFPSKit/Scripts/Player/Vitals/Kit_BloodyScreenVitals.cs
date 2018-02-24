using System;
using UnityEngine;

namespace MarsFPSKit
{
    public class BloodyScreenVitalsRuntimeData
    {
        public float hitPoints;
        public float lastHit;
    }

    /// <summary>
    /// Implements a CoD style health regeneration type of health system
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Vitals/Bloody Screen")]
    public class Kit_BloodyScreenVitals : Kit_VitalsBase
    {
        /// <summary>
        /// How many seconds do we wait (since the last hit) until we regenerate our health?
        /// </summary>
        public float timeUntilHealthIsRegenerated = 5f;
        /// <summary>
        /// How fast do we recover? HP/S
        /// </summary>
        public float healthRegenerationSpeed = 25f;

        public override void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, int gunID)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Check if we can take damage
                if (!pb.spawnProtection || pb.spawnProtection.CanTakeDamage(pb))
                {
                    //Check for hitpoints
                    if (vrd.hitPoints > 0)
                    {
                        //Set time
                        vrd.lastHit = Time.time;
                        //Apply damage
                        vrd.hitPoints -= dmg;
                        //Check for death
                        if (vrd.hitPoints <= 0)
                        {
                            //Call the die function on pb
                            pb.Die(botShot, idWhoShot, gunID);
                        }
                    }
                }
            }
        }

        public override void ApplyFallDamage(Kit_PlayerBehaviour pb, float dmg)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Check for hitpoints
                if (vrd.hitPoints > 0)
                {
                    if (!pb.isBot)
                    {
                        //Set time
                        vrd.lastHit = Time.time;
                    }
                    //Apply damage
                    vrd.hitPoints -= dmg;
                    //Check for death
                    if (vrd.hitPoints <= 0)
                    {
                        //Reset player force
                        pb.ragdollForce = 0f;
                        //Call the die function on pb
                        pb.Die(-2);
                    }
                }
            }
        }

        public override void Suicide(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                //Reset player force
                pb.ragdollForce = 0f;
                //Call the die function on pb
                pb.Die(-3);
            }
        }

        public override void CustomUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(BloodyScreenVitalsRuntimeData))
            {
                BloodyScreenVitalsRuntimeData vrd = pb.customVitalsData as BloodyScreenVitalsRuntimeData;
                //Clamp
                vrd.hitPoints = Mathf.Clamp(vrd.hitPoints, 0f, 100f);
                if (!pb.isBot)
                {
                    //Update hud with negative values, so its hidden
                    pb.main.hud.DisplayHealth(-1);
                    //Negative values should hide it
                    pb.main.hud.DisplayHurtState(1 - vrd.hitPoints / 100f);
                }

                if (vrd.hitPoints < 100f)
                {
                    //Check for hp regeneration
                    if (Time.time > vrd.lastHit + timeUntilHealthIsRegenerated)
                    {
                        vrd.hitPoints += Time.deltaTime * healthRegenerationSpeed;
                    }
                }

                //Check if we are lower than death threshold
                if (pb.transform.position.y <= pb.main.mapDeathThreshold)
                {
                    pb.Die(-1);
                }
            }
        }

        public override void Setup(Kit_PlayerBehaviour pb)
        {
            //Create runtime data
            BloodyScreenVitalsRuntimeData vrd = new BloodyScreenVitalsRuntimeData();
            //Set standard values
            vrd.hitPoints = 100f;
            //Assign it
            pb.customVitalsData = vrd;
        }
    }
}
