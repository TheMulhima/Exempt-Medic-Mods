using System;
using Modding;
using UnityEngine;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using System.Collections;
using System.Runtime.CompilerServices;
using GlobalEnums;
using Satchel;
using Satchel.Reflected;

namespace FOCUS_CLAW
{
    public class FOCUS_CLAW : Mod
    {
        public override string GetVersion()
        {
            return "1.0";
        }
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFSM;
            On.HeroController.CanFocus += OnFocus;

            Log("Initialized");
        }
        private bool OnFocus(On.HeroController.orig_CanFocus orig, HeroController self)
        {
            return !GameManager.instance.isPaused && HeroController.instance.hero_state != ActorStates.no_input && !HeroController.instance.cState.dashing && !HeroController.instance.cState.backDashing && (!HeroController.instance.cState.attacking || HeroControllerR.attack_time >= HeroController.instance.ATTACK_RECOVERY_TIME) && !HeroController.instance.cState.recoiling && !HeroController.instance.cState.transitioning && !HeroController.instance.cState.recoilFrozen && !HeroController.instance.cState.hazardDeath && !HeroController.instance.cState.hazardRespawning && HeroController.instance.CanInput() && (HeroController.instance.cState.onGround || HeroController.instance.cState.wallSliding || (PlayerData.instance.hasWalljump && HeroController.instance.cState.touchingWall && !HeroController.instance.cState.touchingNonSlider));
        }
        private void OnFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Knight" && self.FsmName == "Spell Control")
            {
                self.RemoveTransition("Focus Start", "LEFT GROUND");
                self.RemoveTransition("Focus", "LEFT GROUND");
                self.AddAction("Focus Start", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.gameObject
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "AffectedByGravity",
                        ParameterType = "bool",
                        BoolParameter = new FsmBool(false)
                    }
                });
                self.AddAction("Regain Control", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.gameObject
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "AffectedByGravity",
                        ParameterType = "bool",
                        BoolParameter = new FsmBool(true)
                    }
                });
            }
        }
    }
}
