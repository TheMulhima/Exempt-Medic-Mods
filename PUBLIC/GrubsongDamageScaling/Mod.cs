using Modding;
using UnityEngine;
using System;

namespace GrubsongDamageScaling
{
    public class GrubsongDamageScaling : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            ModHooks.TakeDamageHook += OnTakeDamage;

            Log("Initialized");
        }

        private int OnTakeDamage(ref int hazardType, int damage)
        {
            if (GameManager.instance.playerData.GetBool("overcharmed"))
            {
                damage *= 2;
            }
            if (BossSceneController.IsBossScene && BossSceneController.Instance.BossLevel == 1)
            {
                damage *= 2;
            }

            HeroController.instance.GRUB_SOUL_MP = 15;
            HeroController.instance.GRUB_SOUL_MP_COMBO = 25;
            HeroController.instance.GRUB_SOUL_MP *= damage;
            HeroController.instance.GRUB_SOUL_MP_COMBO *= damage;

            if (GameManager.instance.playerData.GetBool("overcharmed"))
            {
                damage /= 2;
            }
            if (BossSceneController.IsBossScene && BossSceneController.Instance.BossLevel == 1)
            {
                damage /= 2;
            }
            return damage;
        }
    }
}
