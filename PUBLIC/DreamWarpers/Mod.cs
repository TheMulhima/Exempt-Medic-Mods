using UnityEngine;
using System;
using Modding;
using SFCore.Utils;

namespace DisableEnemyGeo
{
    class DisableEnemyGeo : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFSMEnable;

            Log("Initialized");
        }

        private void OnFSMEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Ghost Warrior Slug" && self.FsmName == "Movement")
            {
                self.ChangeFsmTransition("Hover", "TOOK DAMAGE", "Set Warp");
            }
            else if (self.gameObject.name == "Ghost Warrior No Eyes" && self.FsmName == "Damage Response")
            {
                self.ChangeFsmTransition("Idle", "TOOK DAMAGE", "Send");
            }
            else if (self.gameObject.name == "Ghost Warrior Hu" && self.FsmName == "Movement")
            {
                self.ChangeFsmTransition("Hover", "TOOK DAMAGE", "Set Warp");
            }
        }
    }
}
