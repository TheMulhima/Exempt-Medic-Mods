using System;
using System.Reflection;
using Modding;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

namespace ShadeSpawnLog
{
    public class ShadeSpawnLog : Mod
    {
        internal static ShadeSpawnLog Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private List<string> _doneGos = new List<string>();

        private Dictionary<string, string> _pdToNameMap = new Dictionary<string, string>();

        public override List<ValueTuple<string, string>> GetPreloadNames()
        {
            var dict = new List<ValueTuple<string, string>>();
            int max = 499;
            //max = 3;
            for (int i = 0; i < max; i++)
            {
                switch (i)
                {
                    case 0:
                    case 1:
                    case 2:
                        continue;
                }
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                dict.Add((Path.GetFileNameWithoutExtension(scenePath), "_SceneManager"));
            }
            return dict;
        }

        public ShadeSpawnLog() : base("Shade Spawn Log")
        {
            On.SceneManager.Start += OnSceneManagerStart;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneManagerStart(On.SceneManager.orig_Start orig, SceneManager self)
        {
            orig(self);
            Check();
        }

        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode lsm)
        {
            Log($"Scene '{loadedScene.name}' loaded!");
            Check();
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
        }

        private void Check()
        {
            foreach (var fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
            {
                CheckFsm(fsm);
            }
        }

        private void CheckFsm(PlayMakerFSM self)
        {
            try
            {
                if (self.FsmName == "FSM" && self.gameObject.name.Contains("Shade Marker"))
                {
                    Log(self.gameObject.scene.name);
                    Log("FOUND ONE");
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
