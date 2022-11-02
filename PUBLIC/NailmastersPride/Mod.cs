using UnityEngine;
using System;
using System.Reflection;
using Modding;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using GlobalEnums;

namespace NailmastersPride
{
    class NailmastersPride : Mod
    {
        private static MethodInfo origHeroUpdate = typeof(HeroController).GetMethod("orig_Update", BindingFlags.NonPublic | BindingFlags.Instance);

        private ILHook ilOrigHeroUpdate;

        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.HeroController.CanAttack += OnCanAttack;
            On.HeroController.CanNailCharge += OnCanNailCharge;
            On.HeroController.CanNailArt += OnCanNailArt;
            ilOrigHeroUpdate = new ILHook(origHeroUpdate, NailmasterPrideHook);
            On.HutongGames.PlayMaker.Actions.BoolNoneTrue.OnEnter += OnBoolNoneTrueAction;
            On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPDBoolTestAction;
            On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += OnBoolTestAction;


            Log("Initialized");
        }
        private void OnBoolNoneTrueAction(On.HutongGames.PlayMaker.Actions.BoolNoneTrue.orig_OnEnter orig, BoolNoneTrue self)
        {
            if (self.Fsm.FsmComponent.gameObject.name == "Knight" && self.Fsm.FsmComponent.FsmName == "Nail Arts" && self.Fsm.FsmComponent.ActiveStateName == "Has Cyclone?")
            {
                self.sendEvent = PlayerData.instance.equippedCharm_26 ? FsmEvent.GetFsmEvent("FINISHED") : FsmEvent.GetFsmEvent("CANCEL");
            }

            if (self.Fsm.FsmComponent.gameObject.name == "Knight" && self.Fsm.FsmComponent.FsmName == "Nail Arts" && self.Fsm.FsmComponent.ActiveStateName == "Has G Slash?")
            {
                self.sendEvent = PlayerData.instance.equippedCharm_26 ? FsmEvent.GetFsmEvent("FINISHED") : FsmEvent.GetFsmEvent("CANCEL");
            }

            orig(self);
        }
        private void OnPDBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
        {
            if (self.Fsm.FsmComponent.gameObject.name == "Knight" && self.Fsm.FsmComponent.FsmName == "Nail Arts" && self.Fsm.FsmComponent.ActiveStateName == "Has Dash?")
            {
                self.isFalse = PlayerData.instance.equippedCharm_26 ? FsmEvent.GetFsmEvent("FINISHED") : FsmEvent.GetFsmEvent("CANCEL");
            }

            orig(self);
        }
        private void OnBoolTestAction(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, BoolTest self)
        {
            if (self.Fsm.FsmComponent.gameObject.name == "Knight" && self.Fsm.FsmComponent.FsmName == "Nail Arts" && self.Fsm.FsmComponent.ActiveStateName == "Has Cyclone?")
            {
                self.isFalse = PlayerData.instance.equippedCharm_26 ? FsmEvent.GetFsmEvent("FINISHED") : FsmEvent.GetFsmEvent("GREAT SLASH");
            }

            if (self.Fsm.FsmComponent.gameObject.name == "Knight" && self.Fsm.FsmComponent.FsmName == "Nail Arts" && self.Fsm.FsmComponent.ActiveStateName == "Has G Slash?")
            {
                self.isFalse = PlayerData.instance.equippedCharm_26 ? FsmEvent.GetFsmEvent("FINISHED") : FsmEvent.GetFsmEvent("CYCLONE");
            }

            orig(self);

        }

        // Prevents attacking while NMG is equipped
        private bool OnCanAttack(On.HeroController.orig_CanAttack orig, HeroController self)
        {
            if (PlayerData.instance.equippedCharm_26)
            {
                return false;
            }
            return orig(self);
        }

        //Prevents CanNailArt from resetting nailChargeTimer when NMG is equipped
        private bool OnCanNailArt(On.HeroController.orig_CanNailArt orig, HeroController self)
        {
            if (!PlayerData.instance.equippedCharm_26)
            {
                return orig(self);
            }
            if (!HeroController.instance.cState.transitioning && HeroController.instance.hero_state != ActorStates.no_input && !HeroController.instance.cState.attacking && !HeroController.instance.cState.hazardDeath && !HeroController.instance.cState.hazardRespawning && Modding.ReflectionHelper.GetField<HeroController, float>(HeroController.instance, "nailChargeTimer") >= Modding.ReflectionHelper.GetField<HeroController, float>(HeroController.instance, "nailChargeTime"))
            {
                Modding.ReflectionHelper.SetField<HeroController, float>(HeroController.instance, "nailChargeTimer", 0f);
                return true;
            }
            return false;
        }

        //Lets you charge Nail Arts with NMG equipped even if you don't know any
        private bool OnCanNailCharge(On.HeroController.orig_CanNailCharge orig, HeroController self)
        {
            if (PlayerData.instance.equippedCharm_26 && !HeroController.instance.cState.attacking && !HeroController.instance.controlReqlinquished && !HeroController.instance.cState.recoiling && !HeroController.instance.cState.recoilingLeft && !HeroController.instance.cState.recoilingRight)
            {
                return true;
            }
            return orig(self);
        }

        //Makes you no longer need to hold attack to charge Nail Arts if NMG is equipped
        private void NailmasterPrideHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            if (cursor.TryGotoNext(
                i => i.MatchLdfld<HeroActions>("attack"),
                i => i.MatchCallvirt<InControl.OneAxisInputControl>("get_IsPressed")
                ))
            {
                cursor.GotoNext();
                cursor.GotoNext();
                cursor.EmitDelegate<Func<bool, bool>>(NailmastersPrideCheck);
            }
        }
        private static bool NailmastersPrideCheck(bool attackIsPressed)
        {
            return attackIsPressed || PlayerData.instance.equippedCharm_26;
        }
    }
}
