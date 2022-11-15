using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using Modding;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using SFCore.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using GlobalEnums;

namespace CharmOverhaul
{
    class CharmOverhaul : Mod
    {

        private bool shield = false;

        private bool kinematic = false;

        private static MethodInfo origCharmUpdate = typeof(HeroController).GetMethod("orig_CharmUpdate", BindingFlags.Public | BindingFlags.Instance);

        private ILHook ilOrigCharmUpdate;

        public override string GetVersion() => "1.0";
        public override void Initialize()
        {
            Log("Initializing");

            On.HeroController.Awake += OnHCAwake;

            On.PlayMakerFSM.OnEnable += OnFSMEnable;

            On.HeroController.ShouldHardLand += OnShouldHardLand;

            On.HeroController.CharmUpdate += OnCharmUpdate;

            On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += OnSetPlayerDataIntAction;

            On.HeroController.TakeDamage += OnHCTakeDamage;
            
            ModHooks.GetPlayerIntHook += OnGetPlayerIntHook;

            On.HeroController.AddGeo += OnAddGeo;

            On.GeoControl.OnEnable += OnGeoEnable;

            On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += OnSendMessageAction;

            On.HealthManager.TakeDamage += OnHMTakeDamage;

            On.HeroController.CancelDash += OnCancelDash;

            On.NailSlash.SetFury += OnSetFury;

            On.HutongGames.PlayMaker.Actions.SendMessageV2.DoSendMessage += OnSendMessageV2Action;

            On.SpellFluke.OnEnable += OnSpellFluke;

            On.ExtraDamageable.GetDamageOfType += OnExtraDamageGetType;

            ilOrigCharmUpdate = new ILHook(origCharmUpdate, CharmUpdateHook);

            Log("Initialized");
        }

        private int OnExtraDamageGetType(On.ExtraDamageable.orig_GetDamageOfType orig, ExtraDamageTypes extraDamageTypes)
        {
            if (HeroController.instance.gameObject.transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury").ActiveStateName == "Activate" || HeroController.instance.gameObject.transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury").ActiveStateName == "Stay Furied")
            {
                return 3;
            }

            return orig(extraDamageTypes);
        }

        private void OnSpellFluke(On.SpellFluke.orig_OnEnable orig, SpellFluke self)
        {
            orig(self);

            ReflectionHelper.SetField<SpellFluke, int>(self, "damage", PlayerData.instance.GetBool("equippedCharm_19") ? 7 : 5);
        }

        private void OnSendMessageV2Action(On.HutongGames.PlayMaker.Actions.SendMessageV2.orig_DoSendMessage orig, SendMessageV2 self)
        {
            if (self.Fsm.FsmComponent.FsmName == "White Charm" && self.Fsm.FsmComponent.gameObject.name == "Charm Effects" && self.Fsm.FsmComponent.ActiveStateName == "Soul UP")
            {
                self.functionCall.IntParameter = 4 + (PlayerData.instance.GetBool("equippedCharm_20") ? PlayerData.instance.GetBool("equippedCharm_21") ? 3 : 1 : PlayerData.instance.GetBool("equippedCharm_21") ? 2 : 0);
            }

            orig(self);
        }

        //Fury of the Fallen + Sprintmaster Speed Boost
        private void OnSetFury(On.NailSlash.orig_SetFury orig, NailSlash self, bool set)
        {
            if (PlayerData.instance.GetBool("equippedCharm_37"))
            {
                HeroController.instance.RUN_SPEED_CH += set ? 2f : -2f;
                HeroController.instance.RUN_SPEED_CH_COMBO += set ? 2f : -2f;
            }

            orig(self, set);
        }

        //Sharp Shadow + Sprintmaster i-frames
        private void OnCancelDash(On.HeroController.orig_CancelDash orig, HeroController self)
        {
            if (self.cState.shadowDashing && PlayerData.instance.GetBool("equippedCharm_16") && PlayerData.instance.GetBool("equippedCharm_37"))
            {
                HeroController.instance.StartCoroutine("Invulnerable", 0.25f);
            }

            orig(self);
        }

        //Sharp Shadow + Voidheart SOUL gain
        private void OnHMTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (hitInstance.AttackType == AttackTypes.SharpShadow && PlayerData.instance.GetBool("equippedCharm_36") && PlayerData.instance.GetInt("royalCharmState") == 4)
            {
                HeroController.instance.AddMPCharge(8);
            }

            //Carefree Melody + Fury of the Fallen Crits
            if (hitInstance.AttackType == AttackTypes.Nail && PlayerData.instance.GetBool("equippedCharm_6") && HeroController.instance.carefreeShieldEquipped && !(HeroController.instance.gameObject.transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury").ActiveStateName == "Activate") && !(HeroController.instance.gameObject.transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury").ActiveStateName == "Stay Furied"))
            {
                hitInstance.DamageDealt = (int)(hitInstance.DamageDealt * ((UnityEngine.Random.Range(0, 100) < ((2 * (PlayerData.instance.GetInt("maxHealth") - PlayerData.instance.GetInt("health"))) + ((PlayerData.instance.GetInt("maxHealth") == PlayerData.instance.GetInt("health")) ? 1 : 0))) ? 1.25f : 1.0f));
            }

            orig(self, hitInstance);
        }

        private void OnSendMessageAction(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, SendMessage self)
        {
            //Voidheart + Spell Twister reduced costs
            if (self.Fsm.FsmComponent.FsmName == "Spell Control" && self.Fsm.FsmComponent.gameObject.name == "Knight")
            {
                if (self.Fsm.FsmComponent.ActiveStateName == "Fireball 2" && self.functionCall.FunctionName == "TakeMP")
                {
                    self.Fsm.FsmComponent.GetFsmIntVariable("MP Cost").Value = (PlayerData.instance.GetBool("equippedCharm_33") && PlayerData.instance.GetBool("equippedCharm_36") && PlayerData.instance.GetInt("royalCharmState") == 4) ? 22 : PlayerData.instance.GetBool("equippedCharm_33") ? 24 : 33;
                }

                else if (self.Fsm.FsmComponent.ActiveStateName == "Scream Burst 2" && self.functionCall.FunctionName == "TakeMP")
                {
                    self.Fsm.FsmComponent.GetFsmIntVariable("MP Cost").Value = (PlayerData.instance.GetBool("equippedCharm_33") && PlayerData.instance.GetBool("equippedCharm_36") && PlayerData.instance.GetInt("royalCharmState") == 4) ? 22 : PlayerData.instance.GetBool("equippedCharm_33") ? 24 : 33;
                }

                else if (self.Fsm.FsmComponent.ActiveStateName == "Level Check 2" && self.functionCall.FunctionName == "TakeMP")
                {
                    self.Fsm.FsmComponent.GetFsmIntVariable("MP Cost").Value = (PlayerData.instance.GetBool("equippedCharm_33") && PlayerData.instance.GetBool("equippedCharm_36") && PlayerData.instance.GetInt("royalCharmState") == 4 && self.Fsm.FsmComponent.GetFsmIntVariable("Spell Level").Value == 2) ? 22 : PlayerData.instance.GetBool("equippedCharm_33") ? 24 : 33;
                }
            }
            else if (self.Fsm.FsmComponent.FsmName == "Hive Health Regen" && self.Fsm.FsmComponent.gameObject.name == "Health")
            {
                if (self.Fsm.FsmComponent.ActiveStateName == "SOUL 1")
                {
                    self.functionCall.IntParameter.Value = PlayerData.instance.GetBool("equippedCharm_3") ? (PlayerData.instance.GetBool("equippedCharm_35") ? 10 : 5) : 0;
                }

                else if (self.Fsm.FsmComponent.ActiveStateName == "SOUL 2")
                {
                    self.functionCall.IntParameter.Value = PlayerData.instance.GetBool("equippedCharm_3") ? (PlayerData.instance.GetBool("equippedCharm_35") ? 15 : 10) : 0;
                }
            }

            orig(self);
        }

        //Greed + Gathering Swarm instant money
        private void OnGeoEnable(On.GeoControl.orig_OnEnable orig, GeoControl self)
        {
            orig(self);

            if (PlayerData.instance.GetBool("equippedCharm_1") && PlayerData.instance.GetBool("equippedCharm_24") && !PlayerData.instance.GetBool("brokenCharm_24"))
            {
                HeroController.instance.AddGeo((int)Math.Pow(5, self.type));
                self.Disable(0f);
            }
        }

        //Soul Eater + Greed granting SOUL
        private void OnAddGeo(On.HeroController.orig_AddGeo orig, HeroController self, int amount)
        {
            if (PlayerData.instance.GetBool("equippedCharm_21") && PlayerData.instance.GetBool("equippedCharm_24") && !PlayerData.instance.GetBool("brokenCharm_24"))
            {
                self.AddMPCharge(amount);
            }

            orig(self, amount);
        }

        private int OnGetPlayerIntHook(string name, int orig)
        {
            //Heavy Blow increases Nail damage
            if (name == "nailDamage")
            {
                orig = (int)(orig * (PlayerData.instance.GetBool("equippedCharm_15") ? 1.15f : 1.0f));
            }

            return orig;
        }

        private void OnHCTakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            //Stalwart Shell + Defender's Crest Damage
            if (PlayerData.instance.GetBool("equippedCharm_4") && PlayerData.instance.GetBool("equippedCharm_10") && self.gameObject.GetComponent<Rigidbody2D>().velocity.y <= 0 - self.MAX_FALL_VELOCITY && !self.cState.spellQuake && !self.cState.shadowDashing)
            {
                damageAmount = 0;
                if (!kinematic)
                {
                    go.GetComponent<HealthManager>().ApplyExtraDamage(20);
                    kinematic = true;
                    GameManager.instance.StartCoroutine(Kinematic());
                }
            }

            //Joni's Blessing + Carefree Melody
            if (ReflectionHelper.GetField<HeroController, int>(self, "hitsSinceShielded") != 0 && hazardType == 1 && PlayerData.instance.GetBool("equippedCharm_27") && HeroController.instance.carefreeShieldEquipped)
            {
                shield = true;
            }

            orig(self, go, damageSide, damageAmount, hazardType);

            //Joni's Blessing + Carefree Melody
            if (ReflectionHelper.GetField<HeroController, int>(self, "hitsSinceShielded") == 0 && shield)
            {
                EventRegister.SendEvent("ADD BLUE HEALTH");
                if (PlayerData.instance.GetBool("equippedCharm_23") && !PlayerData.instance.GetBool("brokenCharm_23") && PlayerData.instance.GetBool("gotCharm_9"))
                {
                    EventRegister.SendEvent("ADD BLUE HEALTH");
                }

                shield = false;
            }

            //Joni's Blessing + Grubberfly's Elegy
            if (PlayerData.instance.GetBool("equippedCharm_27") && PlayerData.instance.GetBool("equippedCharm_35"))
            {
                ReflectionHelper.SetField<HeroController, bool>(self, "joniBeam", true);
            }
        }

        private void OnSetPlayerDataIntAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetPlayerDataInt self)
        {
            //Changes cost of Carefree Melody
            if (self.Fsm.FsmComponent.FsmName == "Conversation Control" && self.Fsm.FsmComponent.gameObject.name == "Nymm NPC" && self.Fsm.FsmComponent.ActiveStateName == "Get Charm" && self.intName.Value == "charmCost_40")
            {
                self.value.Value = 2;
            }

            //Changes cost of Kingsoul
            if (self.Fsm.FsmComponent.FsmName == "Msg Control" && self.Fsm.FsmComponent.gameObject.name == "UI Msg Get WhiteCharm" && self.Fsm.FsmComponent.ActiveStateName == "Set Full" && self.intName.Value == "charmCost_36")
            {
                self.value.Value = 4;
            }

            //Increases Crystal Heart damage when Nail damage is changed
            if (self.intName.Value == "nailDamage")
            {
                HeroController.instance.transform.Find("SuperDash Damage").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("damageDealt").Value = (PlayerData.instance.GetBool("equippedCharm_34") ? 2 : 1) * (13 + (PlayerData.instance.GetInt("nailSmithUpgrades") * 4));
                HeroController.instance.transform.Find("Effects/SD Burst").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("damageDealt").Value = (PlayerData.instance.GetBool("equippedCharm_34") ? 2 : 1) * (13 + (PlayerData.instance.GetInt("nailSmithUpgrades") * 4));
            }

            orig(self);
        }

        private void OnCharmUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
        {
            orig(self);

            //Increase Walking Speed if Sprintmaster is equipped
            self.WALK_SPEED = 6f + (PlayerData.instance.GetBool("equippedCharm_37") ? 0.85f : 0);

            //Changes charge time of Crystal Heart based on Deep Focus & Quick Focus
            self.gameObject.LocateMyFSM("Superdash").GetFsmFloatVariable("Charge Time").Value = 0.8f + (PlayerData.instance.GetBool("equippedCharm_34") ? 0.2f : 0) - (PlayerData.instance.GetBool("equippedCharm_7") ? 0.2f : 0);

            //Increases Crystal Heart damage (including Nail upgrades and Deep Focus scaling)
            self.gameObject.transform.Find("SuperDash Damage").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("damageDealt").Value = (PlayerData.instance.GetBool("equippedCharm_34") ? 2 : 1) * (13 + (PlayerData.instance.GetInt("nailSmithUpgrades") * 4));
            self.gameObject.transform.Find("Effects/SD Burst").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("damageDealt").Value = (PlayerData.instance.GetBool("equippedCharm_34") ? 2 : 1) * (13 + (PlayerData.instance.GetInt("nailSmithUpgrades") * 4));

            //Stalwart Shell & Baldur Shell increase i-frames
            self.INVUL_TIME_STAL = PlayerData.instance.GetBool("equippedCharm_5") ? 2.05f : 1.75f;

            //Heavy Blow + Nailmaster's Glory Spell Damage
            self.gameObject.transform.Find("Attacks/Great Slash").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("attackType").Value = (PlayerData.instance.GetBool("equippedCharm_15") && PlayerData.instance.GetBool("equippedCharm_26")) ? 3 : 0;
            self.gameObject.transform.Find("Attacks/Dash Slash").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("attackType").Value = (PlayerData.instance.GetBool("equippedCharm_15") && PlayerData.instance.GetBool("equippedCharm_26")) ? 3 : 0;

            //Fury of the Fallen Overcharming
            if (PlayerData.instance.GetBool("equippedCharm_6") && PlayerData.instance.GetBool("overcharmed"))
            {
                PlayerData.instance.SetInt("maxHealth", 1);
                PlayerData.instance.SetInt("health", 1);
                HeroController.instance.gameObject.transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury").SendEvent("HERO DAMAGED");
            }            
        }

        private bool OnShouldHardLand(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision)
        {
            //Makes Steady Body prevent hard landings
            if (PlayerData.instance.GetBool("equippedCharm_14"))
            {
                return false;
            }

            //Failsafe to reset Kinematic Shell bool in case of StopAllCoroutines
            kinematic = false;

            return orig(self, collision);
        }

        //Changes Joni's Blessing to use 1.5 multiplier
        private void CharmUpdateHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.GotoNext(i => i.MatchLdcR4(1.4f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(x => 1.5f);
        }

        private void OnFSMEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            //Hiveblood + Grubsong Interaction
            if (self.FsmName == "Hive Health Regen" && self.gameObject.name == "Health")
            {
                self.AddFsmState("SOUL 1");
                self.AddFsmState("SOUL 2");
                self.ChangeFsmTransition("Recover 1", "DAMAGE TAKEN", "SOUL 1");
                self.ChangeFsmTransition("Recover 2", "DAMAGE TAKEN", "SOUL 2");
                self.AddFsmTransition("SOUL 1", "FINISHED", "Start Recovery");
                self.AddFsmTransition("SOUL 2", "FINISHED", "Start Recovery");

                self.AddFsmAction("SOUL 1", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = HeroController.instance.gameObject
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "AddMPCharge",
                        ParameterType = "int",
                        IntParameter = 0
                    }
                });

                self.AddFsmAction("SOUL 2", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = HeroController.instance.gameObject
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "AddMPCharge",
                        ParameterType = "int",
                        IntParameter = 0
                    }
                });
            }

            //Baldur Shell + Hiveblood Regeneration
            if (self.FsmName == "Control" && self.gameObject.name == "Blocker Shield")
            {
                self.AddFsmFloatVariable("Timer");
                self.AddFsmState("Block Checker");
                self.AddFsmState("3 Blocks");
                self.AddFsmState("2 Blocks");
                self.AddFsmState("1 Blocks");
                self.AddFsmState("0 Blocks");
                self.AddFsmState("Restore");

                self.AddFsmTransition("Equipped", "HIVEBLOOD", "Block Checker");
                self.AddFsmTransition("Block Checker", "FOCUS START", "Hits Left?");
                self.AddFsmTransition("Block Checker", "3", "3 Blocks");
                self.AddFsmTransition("Block Checker", "2", "2 Blocks");
                self.AddFsmTransition("Block Checker", "1", "1 Blocks");
                self.AddFsmTransition("Block Checker", "0", "0 Blocks");
                self.AddFsmTransition("3 Blocks", "FOCUS START", "Hits Left?");
                self.AddFsmTransition("2 Blocks", "FOCUS START", "Hits Left?");
                self.AddFsmTransition("1 Blocks", "FOCUS START", "Hits Left?");
                self.AddFsmTransition("0 Blocks", "FOCUS START", "Hits Left?");
                self.AddFsmTransition("3 Blocks", "RESTORE", "Restore");
                self.AddFsmTransition("2 Blocks", "RESTORE", "Restore");
                self.AddFsmTransition("1 Blocks", "RESTORE", "Restore");
                self.AddFsmTransition("0 Blocks", "RESTORE", "Restore");
                self.AddFsmTransition("Restore", "FINISHED", "Equipped");

                self.AddFsmAction("Equipped", new PlayerDataBoolTest()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject,
                    },
                    boolName = "equippedCharm_29",
                    isTrue = FsmEvent.GetFsmEvent("HIVEBLOOD"),
                    isFalse = null
                });

                self.AddFsmAction("Block Checker", new GetPlayerDataInt()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    intName = "blockerHits",
                    storeValue = self.GetFsmIntVariable("Blocks")
                });

                self.AddFsmAction("Block Checker", new IntCompare()
                {
                    integer1 = self.GetFsmIntVariable("Blocks"),
                    integer2 = 1,
                    equal = FsmEvent.GetFsmEvent("1"),
                    lessThan = FsmEvent.GetFsmEvent("0"),
                    greaterThan = null,
                    everyFrame = false
                });

                self.AddFsmAction("Block Checker", new IntCompare()
                {
                    integer1 = self.GetFsmIntVariable("Blocks"),
                    integer2 = 3,
                    equal = FsmEvent.GetFsmEvent("3"),
                    lessThan = FsmEvent.GetFsmEvent("2"),
                    greaterThan = null,
                    everyFrame = false
                });

                self.AddFsmAction("3 Blocks", new SetFloatValue()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    floatValue = 0f,
                    everyFrame = false
                });
                self.AddFsmAction("3 Blocks", new FloatAdd()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    add = 1f,
                    everyFrame = true,
                    perSecond = true
                });
                self.AddFsmAction("3 Blocks", new FloatCompare()
                {
                    float1 = self.GetFsmFloatVariable("Timer"),
                    float2 = 20f,
                    tolerance = 0f,
                    equal = FsmEvent.GetFsmEvent("RESTORE"),
                    lessThan = null,
                    greaterThan = FsmEvent.GetFsmEvent("RESTORE"),
                    everyFrame = true
                });


                self.AddFsmAction("2 Blocks", new SetFloatValue()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    floatValue = 0f,
                    everyFrame = false
                });
                self.AddFsmAction("2 Blocks", new FloatAdd()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    add = 1f,
                    everyFrame = true,
                    perSecond = true
                });
                self.AddFsmAction("2 Blocks", new FloatCompare()
                {
                    float1 = self.GetFsmFloatVariable("Timer"),
                    float2 = 24f,
                    tolerance = 0f,
                    equal = FsmEvent.GetFsmEvent("RESTORE"),
                    lessThan = null,
                    greaterThan = FsmEvent.GetFsmEvent("RESTORE"),
                    everyFrame = true
                });


                self.AddFsmAction("1 Blocks", new SetFloatValue()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    floatValue = 0f,
                    everyFrame = false
                });
                self.AddFsmAction("1 Blocks", new FloatAdd()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    add = 1f,
                    everyFrame = true,
                    perSecond = true
                });
                self.AddFsmAction("1 Blocks", new FloatCompare()
                {
                    float1 = self.GetFsmFloatVariable("Timer"),
                    float2 = 28f,
                    tolerance = 0f,
                    equal = FsmEvent.GetFsmEvent("RESTORE"),
                    lessThan = null,
                    greaterThan = FsmEvent.GetFsmEvent("RESTORE"),
                    everyFrame = true
                });

                self.AddFsmAction("0 Blocks", new SetFloatValue()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    floatValue = 0f,
                    everyFrame = false
                });
                self.AddFsmAction("0 Blocks", new FloatAdd()
                {
                    floatVariable = self.GetFsmFloatVariable("Timer"),
                    add = 1f,
                    everyFrame = true,
                    perSecond = true
                });
                self.AddFsmAction("0 Blocks", new FloatCompare()
                {
                    float1 = self.GetFsmFloatVariable("Timer"),
                    float2 = 32f,
                    tolerance = 0f,
                    equal = FsmEvent.GetFsmEvent("RESTORE"),
                    lessThan = null,
                    greaterThan = FsmEvent.GetFsmEvent("RESTORE"),
                    everyFrame = true
                });


                self.AddFsmAction("Restore", new PlayerDataIntAdd()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = GameManager.instance.gameObject
                    },
                    intName = "blockerHits",
                    amount = 1
                });
            }
        }

        //Sets Dashmaster, Hiveblood, Deep Focus, Flukenest, and Carefree Melody costs.
        private void OnHCAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            
            PlayerData.instance.SetInt("charmCost_31", 1);
            PlayerData.instance.SetInt("charmCost_29", 3);
            PlayerData.instance.SetInt("charmCost_34", 3);
            PlayerData.instance.SetInt("charmCost_11", 2);
            PlayerData.instance.SetInt("charmCost_40", 2);
        }

        private IEnumerator Kinematic()
        {
            yield return new WaitForSeconds(0.5f);
            kinematic = false;
        }
    }
}
