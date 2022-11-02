using Modding;
using UnityEngine;
using System;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;

namespace SilentDreamshield
{
    public class SilentDreamshield : Mod
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

            if (self.gameObject.name == "Shield" && self.FsmName == "Shield Hit")
            {
                self.GetFsmAction<AudioPlayerOneShotSingle>("Slash Anim", 1).Enabled = false;
            }
        }
    }
}
