using System;
using UnityEngine;

namespace MarsFPSKit
{
    public class VitalsRuntimeData
    {
        public float hitPoints;

        /// <summary>
        /// For displaying the bloody screen
        /// </summary>
        public float hitAlpha;
    }

    /// <summary>
    /// Implements a basic vitals
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Vitals/Simple")]
    public class Kit_SimpleVitals : Kit_VitalsBase
    {
        public float bloodyScreenTime = 3f;

        public override void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, int gunID)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Check if we can take damage
                if (!pb.spawnProtection || pb.spawnProtection.CanTakeDamage(pb))
                {
                    //Check for hitpoints
                    if (vrd.hitPoints > 0)
                    {
                        //Apply damage
                        vrd.hitPoints -= dmg;
                        if (!pb.isBot)
                        {
                            //Set damage effect
                            vrd.hitAlpha = 2f;
                        }
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
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Check for hitpoints
                if (vrd.hitPoints > 0)
                {
                    //Apply damage
                    vrd.hitPoints -= dmg;
                    //Set damage effect
                    vrd.hitAlpha = 2f;
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
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                //Reset player force
                pb.ragdollForce = 0f;
                //Call the die function on pb
                pb.Die(-3);
            }
        }

        public override void CustomUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Clamp
                vrd.hitPoints = Mathf.Clamp(vrd.hitPoints, 0f, 100f);
                //Decrease hit alpha
                if (vrd.hitAlpha > 0)
                {
                    vrd.hitAlpha -= (Time.deltaTime * 2) / bloodyScreenTime;
                }
                //Update hud
                pb.main.hud.DisplayHealth(vrd.hitPoints);
                pb.main.hud.DisplayHurtState(vrd.hitAlpha);

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
            VitalsRuntimeData vrd = new VitalsRuntimeData();
            //Set standard values
            vrd.hitPoints = 100f;
            //Assign it
            pb.customVitalsData = vrd;
        }
    }
}
