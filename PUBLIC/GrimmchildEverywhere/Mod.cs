using Modding;
using UnityEngine;
using System;
using SFCore.Utils;

namespace GrimmchildEverywhere
{
    public class GrimmchildEverywhere : Mod
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

            if (self.gameObject.name == "Charm Effects" && self.FsmName == "Spawn Grimmchild")
            {
                self.ChangeFsmTransition("Dream?", "FINISHED", "Charms Allowed?");
            }
        }
    }
}
