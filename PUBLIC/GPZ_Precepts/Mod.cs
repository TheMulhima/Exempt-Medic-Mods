using Modding;
using UnityEngine;
using System;
using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;

namespace GPZ_Precepts
{
    public class GPZ_Precepts : Mod
    {
        
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.HealthManager.Hit += OnHit;
            On.HealthManager.TakeDamage += OnTakeDamage;
            On.HutongGames.PlayMaker.Actions.StringCompare.OnEnter += OnStringCompareAction;
            On.HutongGames.PlayMaker.Actions.ConvertIntToString.OnEnter += OnConvertIntToStringAction;

            Log("Initialized");
        }
        private void OnHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.name == "Grey Prince")
            {
                if (GameObject.Find("Dream Msg").LocateMyFSM("Display").ActiveStateName == "Idle")
                {
                    self.IsInvincible = false;
                }
            }
            orig(self, hitInstance);
        }

        private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.name == "Grey Prince")
            {
                self.IsInvincible = true;
                PlayMakerFSM DreamFSM = GameObject.Find("Dream Msg").LocateMyFSM("Display");
                if (DreamFSM.ActiveStateName == "Idle")
                {
                    DreamFSM.GetFsmIntVariable("Convo Amount").Value = 57;
                    DreamFSM.GetFsmStringVariable("Convo Title").Value = "PRECEPT";
                    PlayMakerFSM.BroadcastEvent("DISPLAY ENEMY DREAM");
                }
            }
            orig(self, hitInstance);
        }
        private void OnStringCompareAction(On.HutongGames.PlayMaker.Actions.StringCompare.orig_OnEnter orig, StringCompare self)
        {
            if (self.Fsm.FsmComponent.FsmName == "Display" && self.Fsm.FsmComponent.gameObject.name == "Dream Msg" && self.Fsm.FsmComponent.ActiveStateName == "Check Convo")
            {
                self.Fsm.FsmComponent.GetFsmAction<GetLanguageString>("Set Convo", 3).sheetName.Value = (self.Fsm.FsmComponent.GetFsmStringVariable("Convo Title").Value == "PRECEPT") ? "Zote" : "Enemy Dreams";
            }
            orig(self);
        }
        private void OnConvertIntToStringAction(On.HutongGames.PlayMaker.Actions.ConvertIntToString.orig_OnEnter orig, ConvertIntToString self)
        {
            orig(self);

            if (self.Fsm.FsmComponent.FsmName == "Display" && self.Fsm.FsmComponent.gameObject.name == "Dream Msg" && self.Fsm.FsmComponent.GetFsmStringVariable("Convo Title").Value == "PRECEPT" && self.Fsm.FsmComponent.ActiveStateName == "Set Convo" && self.intVariable.Value == 1)
            {
                self.Fsm.FsmComponent.GetFsmStringVariable("Convo Num Str").Value = "1_R";
            }
        }
    }
}
