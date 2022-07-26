using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;

namespace Always_Furious
{
    class Always_Furious : Mod
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

            if (self.gameObject.name == "Charm Effects" && self.FsmName == "Fury")
            {
                self.InsertFsmAction("Check HP", new SendEvent()
                {
                    eventTarget = new FsmEventTarget()
                    {
                        target = FsmEventTarget.EventTarget.GameObject,
                        gameObject = new FsmOwnerDefault()
                        {
                            OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                            GameObject = self.gameObject
                        }
                    },
                    sendEvent = FsmEvent.GetFsmEvent("FURY"),
                    delay = 0f,
                    everyFrame = false
                }, 1);
                self.AddFsmAction("Idle", new SendEvent()
                {
                    eventTarget = new FsmEventTarget()
                    {
                        target = FsmEventTarget.EventTarget.GameObject,
                        gameObject = new FsmOwnerDefault()
                        {
                            OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                            GameObject = self.gameObject
                        }
                    },
                    sendEvent = FsmEvent.GetFsmEvent("HERO DAMAGED"),
                    delay = 0f,
                    everyFrame = false
                });
                self.ChangeFsmTransition("Recheck", "FINISHED", "Stay Furied");
                self.ChangeFsmTransition("Activate", "HERO HEALED FULL", "Stay Furied");
                self.ChangeFsmTransition("Stay Furied", "HERO HEALED FULL", "Stay Furied");
            }
        }
    }
}
