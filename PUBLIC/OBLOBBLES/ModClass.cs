using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using Satchel.Reflected;

namespace OBLOBBLES
{
    class OBLOBBLES : Mod
    {
        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnFSM;

            Log("Initialized");
        }

        private void OnFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if ((self.gameObject.name == "Mega Fat Bee" || self.gameObject.name == "Mega Fat Bee (1)") && self.FsmName == "Fatty Fly Attack")
            {
                self.AddFsmState("Loop Check");
                self.AddFsmState("Raging?");
                self.AddFsmState("Raging");
                self.AddFsmState("Not Raging");
                self.ChangeFsmTransition("Attack Check", "END", "Loop Check");
                self.AddFsmTransition("Loop Check", "END", "CD");
                self.AddFsmTransition("Loop Check", "RAGE CHECK", "Raging?");
                self.AddFsmTransition("Raging?", "RAGING", "Raging");
                self.AddFsmTransition("Raging?", "NOT RAGING", "Not Raging");
                self.AddFsmTransition("Raging", "FINISHED", "Attack");
                self.AddFsmTransition("Not Raging", "FINISHED", "Attack");
                self.GetFsmAction<Wait>("Attack", 21).Enabled = false;
                self.GetFsmAction<Wait>("Attack 2", 21).Enabled = false;
                self.GetFsmIntVariable("Loops").Value = 4;
                self.AddFsmAction("Loop Check", new IntOperator()
                {
                    integer1 = self.GetFsmIntVariable("Loops"),
                    integer2 = 1,
                    operation = IntOperator.Operation.Subtract,
                    storeResult = self.GetFsmIntVariable("Loops"),
                    everyFrame = false
                });
                self.AddFsmAction("Loop Check", new IntCompare()
                {
                    integer1 = self.GetFsmIntVariable("Loops"),
                    integer2 = 0,
                    equal = FsmEvent.GetFsmEvent("END"),
                    lessThan = FsmEvent.GetFsmEvent("END"),
                    greaterThan = null
                });
                self.AddFsmAction("Loop Check", new Wait()
                {
                    time = self.GetFsmFloatVariable("Shot Pause"),
                    finishEvent = FsmEvent.GetFsmEvent("RAGE CHECK"),
                    realTime = false
                });
                self.AddFsmAction("Raging?", new BoolTest()
                {
                    boolVariable = self.GetFsmBoolVariable("Rage"),
                    isTrue = FsmEvent.GetFsmEvent("RAGING"),
                    isFalse = FsmEvent.GetFsmEvent("NOT RAGING"),
                    everyFrame = false
                });
                self.AddFsmAction("Raging", new SetIntValue()
                {
                    intVariable = self.GetFsmIntVariable("Attacks"),
                    intValue = 4,
                    everyFrame = false
                });
                self.AddFsmAction("Not Raging", new SetIntValue()
                {
                    intVariable = self.GetFsmIntVariable("Attacks"),
                    intValue = 3,
                    everyFrame = false
                });
                self.AddFsmAction("CD", new SetIntValue()
                {
                    intVariable = self.GetFsmIntVariable("Loops"),
                    intValue = 4,
                    everyFrame = false
                });
            }
        }
    }
}
