using BepInEx;
using HarmonyLib;

namespace TaikoTitleFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var instance = new Harmony(PluginInfo.PLUGIN_NAME);
            instance.PatchAll(typeof(TitleFix));

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
