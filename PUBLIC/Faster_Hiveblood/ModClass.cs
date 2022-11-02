using Modding;
using UnityEngine;
using System;
using SFCore.Utils;

namespace FasterHiveblood
{
    public class FasterHiveblood : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFsmEnable;

            Log("Initialized");
        }

        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Health" && self.FsmName == "Hive Health Regen")
            {
                self.GetFsmFloatVariable("Recover Time").Value = 2.5f;
            }
        }
    }
}
