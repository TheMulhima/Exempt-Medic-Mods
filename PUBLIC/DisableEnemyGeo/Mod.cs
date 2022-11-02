using UnityEngine;
using System;
using System.Reflection;
using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace DisableEnemyGeo
{
    class DisableEnemyGeo : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFSMEnable;
            On.HealthManager.Die += OnDie;

            Log("Initialized");
        }

        //Disables Gruz Mother Geo Drop
        private void OnFSMEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Corpse Big Fly Burster(Clone)" && self.FsmName == "burster")
            {
                self.GetFsmAction<FlingObjectsFromGlobalPool>("Geo", 0).Enabled = false;
            }
        }

        //Disables all other enemy Geo drops
        private void OnDie(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
        {
            self.SetGeoSmall(0);
            self.SetGeoMedium(0);
            self.SetGeoLarge(0);
            orig(self, attackDirection, attackType, ignoreEvasion);
        }
    }
}
