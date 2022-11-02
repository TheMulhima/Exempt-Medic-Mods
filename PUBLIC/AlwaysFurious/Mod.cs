using Modding;
using UnityEngine;
using System;
using SFCore.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace AlwaysFurious
{
    public class AlwaysFurious : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;

            Log("Initialized");
        }

        private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
        {
            if (self.Fsm.FsmComponent.FsmName == "Fury" && self.Fsm.FsmComponent.gameObject.name == "Charm Effects" && self.Fsm.FsmComponent.ActiveStateName == "Check HP")
            {
                self.isTrue = FsmEvent.GetFsmEvent("FURY");
            }

            orig(self);
        }

        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Charm Effects" && self.FsmName == "Fury")
            {
                self.AddFsmGlobalTransitions("CHARM EQUIP CHECK", "Check HP");
                self.ChangeFsmTransition("Init", "FINISHED", "Check HP");
                self.ChangeFsmTransition("Check HP", "CANCEL", "Deactivate");
                self.ChangeFsmTransition("Activate", "HERO HEALED FULL", "Recheck");
                self.ChangeFsmTransition("Stay Furied", "HERO HEALED FULL", "Recheck");
                self.ChangeFsmTransition("Recheck", "FINISHED", "Stay Furied");
            }
        }
    }
}
