using UnityEngine;
using Modding;
using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace NailmastersByNailArt
{
    class NailmastersByNailArt : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.HealthManager.Hit += OnHit;
            On.ExtraDamageable.ApplyExtraDamageToHealthManager += OnExtraDamage;
            On.SetHP.OnEnter += OnSetHP;
            On.SpellFluke.DoDamage += OnFlukenestDamage;

            Log("Initialized");
        }

        private void OnFlukenestDamage(On.SpellFluke.orig_DoDamage orig, SpellFluke self, GameObject obj, int upwardRecursionAmount, bool burst)
        {
            int damage = ReflectionHelper.GetField<SpellFluke, int>(self, "damage");

            if (obj.name == "Sheo Boss" || obj.name == "Oro" || obj.name == "Mato" || obj.name == "Sly Boss")
            {
                ReflectionHelper.SetField(self, "damage", 0);
            }

            orig(self, obj, upwardRecursionAmount, burst);

            ReflectionHelper.SetField(self, "damage", damage);
        }

        private void OnSetHP(On.SetHP.orig_OnEnter orig, SetHP self)
        {
            GameObject attacked = self.target.GetSafe(self);
            if (attacked.GetComponent<HealthManager>().hp > 0 && (attacked.name == "Sheo Boss" || attacked.name == "Oro" || attacked.name == "Mato" || attacked.name == "Sly Boss"))
            {
                self.hp.Value = attacked.GetComponent<HealthManager>().hp;
            }
            orig(self);
        }

        private void OnExtraDamage(On.ExtraDamageable.orig_ApplyExtraDamageToHealthManager orig, ExtraDamageable self, int damageAmount)
        {
            if (self.gameObject.name == "Sheo Boss" || self.gameObject.name == "Oro" || self.gameObject.name == "Mato" || self.gameObject.name == "Sly Boss")
            {
                damageAmount = 0;
            }
            orig(self, damageAmount);
        }

        private void OnHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.name == "Sheo Boss" && hitInstance.Source.gameObject.name != "Great Slash")
            {
                hitInstance.DamageDealt = 0;
            }
            if (self.gameObject.name == "Oro" && hitInstance.Source.gameObject.name != "Dash Slash")
            {
                hitInstance.DamageDealt = 0;
            }
            if (self.gameObject.name == "Mato" && hitInstance.Source.gameObject.transform.parent.gameObject.name != "Hits")
            {
                hitInstance.DamageDealt = 0;
            }
            if (self.gameObject.name == "Sly Boss" && hitInstance.Source.gameObject.name != "Great Slash" && hitInstance.Source.gameObject.name != "Dash Slash" && hitInstance.Source.gameObject.transform.parent.gameObject.name != "Hits")
            {
                hitInstance.DamageDealt = 0;
            }
            orig(self, hitInstance);
        }
    }
}
