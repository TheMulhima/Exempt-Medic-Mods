using UnityEngine;
using System;
using System.Reflection;
using Modding;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using SFCore.Utils;
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
            On.PlayMakerFSM.OnEnable += OnFSMEnable;
            ilOrigHeroUpdate = new ILHook(origHeroUpdate, NailmasterPrideHook);
            On.HeroController.CanNailCharge += OnCanNailCharge;
            On.HeroController.CanNailArt += OnCanNailArt;

            Log("Initialized");
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

        //Allows Nail Arts to be used by pressing Attack and has NMG effectively ignore Nail Art requirements
        private void OnFSMEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.gameObject.name == "Knight" && self.FsmName == "Nail Arts")
            {
                self.AddFsmState("Dash NMG Check");
                self.AddFsmState("Cyclone NMG Check");
                self.AddFsmState("Great NMG Check");

                self.ChangeFsmTransition("Can Nail Art?", "DASH CHECK", "Dash NMG Check");
                self.ChangeFsmTransition("Move Choice", "CYCLONE", "Cyclone NMG Check");
                self.ChangeFsmTransition("Move Choice", "GREAT SLASH", "Great NMG Check");

                self.AddFsmTransition("Dash NMG Check", "EQUIPPED", "Dash Slash Ready");
                self.AddFsmTransition("Dash NMG Check", "NOT EQUIPPED", "Has Dash?");
                self.AddFsmAction("Dash NMG Check", new PlayerDataBoolTest()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    boolName = "equippedCharm_26",
                    isTrue = FsmEvent.GetFsmEvent("EQUIPPED"),
                    isFalse = FsmEvent.GetFsmEvent("NOT EQUIPPED")
                });

                self.AddFsmTransition("Cyclone NMG Check", "EQUIPPED", "Flash");
                self.AddFsmTransition("Cyclone NMG Check", "NOT EQUIPPED", "Has Cyclone?");
                self.AddFsmAction("Cyclone NMG Check", new PlayerDataBoolTest()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    boolName = "equippedCharm_26",
                    isTrue = FsmEvent.GetFsmEvent("EQUIPPED"),
                    isFalse = FsmEvent.GetFsmEvent("NOT EQUIPPED")
                });

                self.AddFsmTransition("Great NMG Check", "EQUIPPED", "Flash 2");
                self.AddFsmTransition("Great NMG Check", "NOT EQUIPPED", "Has G Slash?");
                self.AddFsmAction("Great NMG Check", new PlayerDataBoolTest()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    boolName = "equippedCharm_26",
                    isTrue = FsmEvent.GetFsmEvent("EQUIPPED"),
                    isFalse = FsmEvent.GetFsmEvent("NOT EQUIPPED")
                });

                self.GetFsmAction<ListenForAttack>("Inactive", 0).wasPressed = FsmEvent.GetFsmEvent("BUTTON UP");
            }
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
