using Modding;
using UnityEngine;
using System;
using SFCore.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace EternalGrubs
{
    public class EternalGrubs : Mod
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

            if (self.gameObject.name == "Grub King" && self.FsmName == "King Control")
            {
                self.GetFsmAction<SetPlayerDataBool>("Final Reward?", 1).Enabled = false;
            }
        }
    }
}
