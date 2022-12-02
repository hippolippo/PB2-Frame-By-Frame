using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;
using System.Reflection;
using PolyTechFramework;
using PolyPhysics;
namespace FrameManip {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInDependency(PolyTechMain.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    
    
    public class FrameManip : PolyTechMod {
        
        
        public const string pluginGuid = "polytech.FrameManip";
        public const string pluginName = "FrameManip";
        public const string pluginVersion = "1.0.0";
        public const string configHeader = "Frames may be inconsistent at high speeds";

        public static Panel_TopBar topbar;
        public static bool pauseNextFrame;
        
        public static ConfigEntry<bool> mEnabled;
        public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> _nextFrameBind;
        public static ConfigEntry<int> _autoPauseFrame;
        public static ConfigEntry<bool> _showFrameNumber;

        static bool Enabled{
            get{
                return mEnabled.Value;
            }
        }
        static BepInEx.Configuration.KeyboardShortcut nextFrameBind{
            get{
                return _nextFrameBind.Value;
            }
        }
        static int autoPauseFrame{
            get{
                return _autoPauseFrame.Value;
            }
        }
        static bool showFrameNumber{
            get{
                return _showFrameNumber.Value;
            }
        }
        
        public ConfigDefinition mEnabledDef = new ConfigDefinition(configHeader, "Enable/Disable Mod");
        public ConfigDefinition nextFrameBindDef = new ConfigDefinition(configHeader, "Unpause for one frame");
        public ConfigDefinition autoPauseFrameDef = new ConfigDefinition(configHeader, "Autopause frame (-1 to disable)");
        public ConfigDefinition showFrameNumberDef = new ConfigDefinition(configHeader, "Show frame numbers");
        
        
        
        public static bool nextFrameBindJustPressed = false;
        public static bool nextFrameBindDown = false;
        
        public override void enableMod(){
            mEnabled.Value = true;
            this.isEnabled = true;
        }
        public override void disableMod(){
            mEnabled.Value = false;
            this.isEnabled = false;
        }
        public override string getSettings(){
            return "";
        }
        public override void setSettings(string settings){
            
        }
        public FrameManip(){
            
            mEnabled = Config.Bind(mEnabledDef, false, new ConfigDescription("Controls if the mod should be enabled or disabled", null, new ConfigurationManagerAttributes {Order = 0}));
            _nextFrameBind = Config.Bind(nextFrameBindDef, new BepInEx.Configuration.KeyboardShortcut(UnityEngine.KeyCode.Tab), new ConfigDescription("Will unpause the simulation and then pause it after one frame", null, new ConfigurationManagerAttributes {Order = -1}));
            _autoPauseFrame = Config.Bind(autoPauseFrameDef, -1, new ConfigDescription("The simulation will automatically pause when it reaches this frame", null, new ConfigurationManagerAttributes {Order = -2}));
            _showFrameNumber = Config.Bind(showFrameNumberDef, false, new ConfigDescription("The current frame number will be shown on the screen during simulation", null, new ConfigurationManagerAttributes {Order = -3}));
        }
        void Awake(){
            
            this.repositoryUrl = null;
            this.isCheat = false;
            PolyTechMain.registerMod(this);
            Logger.LogInfo("FrameManip Registered");
            Harmony.CreateAndPatchAll(typeof(FrameManip));
            Logger.LogInfo("FrameManip Methods Patched");
        }
        void Update(){
            
            nextFrameBindJustPressed = nextFrameBind.IsDown();
            if (nextFrameBind.IsDown()){
                nextFrameBindDown = true;
                if(Enabled){
                    // Code For When Key is Pressed
                    pauseNextFrame = true;
                    topbar.OnUnPauseSim();
                }
            }
            if (nextFrameBind.IsUp()){
                nextFrameBindDown = false;
                if(Enabled){
                    // Code For When Key is Released
                }
            }
        }

        public static void skipToFrame(int frameNumber){
            if(frameNumber <= Main.m_Instance.m_World.frameCount) return;
            /*foreach (IWorldListener worldListener in Main.m_Instance.m_World.worldListeners)
			{
			    worldListener.AfterWorldFrameUpdate();
			}*/
            while(frameNumber > Main.m_Instance.m_World.frameCount){
                Common.Base2.Singleton<PolyPhysics.Utils.Timer, int>.instance.Clear();
                TriggerCallbackManager.SortAndProcessTriggerEvents();
                EventTimelines.FixedUpdate_Manual();
                Main.m_Instance.m_World.FixedUpdate_Manual();
                Vehicles.FixedUpdateManual();
                CustomShapes.FixedUpdateManual();
                ZedAxisVehicles.FixedUpdateManual();
                /*if (!GameStateSim.m_LevelPassed && !GameStateSim.m_LevelFailed && Bridge.IsSimulating())
		        {
			        GameStateSim.EvalulateIfLevelPassedOrFailed();
		        }
                if (GameStateSim.m_LevelPassed || GameStateSim.m_LevelFailed)
		        {
                    return;
                }*/
                /*foreach (IWorldListener worldListener in Main.m_Instance.m_World.worldListeners)
			    {
				    worldListener.AfterWorldFrameUpdate();
			    }*/
            }
        }
        
        [HarmonyPatch(typeof(World), "FixedUpdate_Manual")]
        [HarmonyPostfix]
        private static void WorldFixedUpdate_ManualPostfixPatch(ref World __instance){
            if(Enabled){
                //skipToFrame(autoPauseFrame);
                if(__instance.frameCount == autoPauseFrame || pauseNextFrame){
                    topbar.OnPauseSim();
                    pauseNextFrame = false;
                }
            }
            
        }
        [HarmonyPatch(typeof(Panel_TopBar), "Start")]
        [HarmonyPostfix]
        private static void PanelTopBarStartPatch(ref Panel_TopBar __instance){
            topbar = __instance;
        }
    }
}