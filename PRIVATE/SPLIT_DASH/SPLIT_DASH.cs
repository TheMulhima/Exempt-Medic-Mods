using Modding;

namespace Dash_Stuff
{
    public class Dash_Stuff : Mod
    {
        public override string GetVersion()
        {
            return "1.0";
        }
        public override void Initialize()
        {
            Log("Initializing");

            ModHooks.DashPressedHook += OnDash;
            ModHooks.GetPlayerBoolHook += OnPlayerBool;

            Log("Initialized");
        }
        private bool OnPlayerBool(string name, bool orig)
        {
            if (name == "canDash" && PlayerData.instance.hasShadowDash)
            {
                return true;
            }
            else
                return orig;
        }
        private bool OnDash()
        {
            if (PlayerData.instance.hasShadowDash && !PlayerData.instance.hasDash)
            {
                HeroController.instance.DASH_COOLDOWN = HeroController.instance.SHADOW_DASH_COOLDOWN + 0.001f;
                HeroController.instance.DASH_COOLDOWN_CH = HeroController.instance.SHADOW_DASH_COOLDOWN + 0.001f;
            }
            else
            {
                HeroController.instance.DASH_COOLDOWN = 0.6f;
                HeroController.instance.DASH_COOLDOWN_CH = 0.4f;
            }
            return false;
        }
    }
}
