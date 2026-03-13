using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace WyrmDragonHydra.STSAnalytics;

[ModInitializer("Initialize")]
public class ModEntry
{
    public static void Initialize()
    {
        Log.Warn("KSL - Mod load");
        var harmony = new Harmony("STSAnalytics.patch");
        harmony.PatchAll();
    }
}