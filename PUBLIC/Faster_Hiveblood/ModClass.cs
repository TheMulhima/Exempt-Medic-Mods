using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;


namespace Faster_Hiveblood
{
    class Faster_Hiveblood : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            
            Log("Initialized");
        }
        private static void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Health" && self.FsmName == "Hive Health Regen")
            {
                ModifyFsm(self);
            }
        }
        private static void ModifyFsm(PlayMakerFSM fsm)
        {
            fsm.Fsm.Variables.FloatVariables[1].Value = 2.5f;
        }
    }
}
