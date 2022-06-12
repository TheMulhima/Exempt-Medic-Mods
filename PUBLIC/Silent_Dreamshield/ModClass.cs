using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;


namespace Silent_Dreamshield
{
    class Silent_Dreamshield : Mod
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

            if (self.gameObject.name == "Shield" && self.FsmName == "Shield Hit")
            {
                FsmStateAction[] origActions = self.Fsm.GetState("Slash Anim").Actions;
                FsmStateAction[] actions = new FsmStateAction[1];
                actions[0] = origActions[0];
                self.Fsm.GetState("Slash Anim").Actions = actions;
            }
        }
    }
}
