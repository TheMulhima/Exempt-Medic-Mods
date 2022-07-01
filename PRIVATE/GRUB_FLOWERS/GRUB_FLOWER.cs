        private void OnFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.scene.name == "Crossroads_31" && self.gameObject.name == "Dream Dialogue" && self.FsmName == "npc_dream_dialogue")
            {
                self.AddFsmAction("Box Up", new SetStringValue()
                {
                    stringVariable = self.GetFsmStringVariable("Convo Name"),
                    stringValue = "ELDERBUG_DREAM_FLOWER",
                    everyFrame = false
                });
                self.AddFsmAction("Box Up", new SetStringValue()
                {
                    stringVariable = self.GetFsmStringVariable("Sheet Name"),
                    stringValue = "Elderbug",
                    everyFrame = false
                });
            }
        }
        
                private void OnFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.scene.name == "Crossroads_31" && self.gameObject.name == "Dream Dialogue" && self.FsmName == "npc_dream_dialogue")
            {
                self.GetFsmStringVariable("Convo Name").Value = "ELDERBUG_DREAM_FLOWER";
                self.GetFsmStringVariable("Sheet Name").Value = "Elderbug";
            }
        }
