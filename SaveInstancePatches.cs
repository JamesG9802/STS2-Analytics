using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace WyrmDragonHydra.STSAnalytics;


[HarmonyPatch(typeof(SaveManager))]
public static class SaveManagerPatches
{
    private static bool _is_loading_run_save_postfix = false;
    [HarmonyPatch("LoadRunSave")]
    [HarmonyPostfix]
    public static void LoadRunSavePostfix()
    {
        if (_is_loading_run_save_postfix)
        {
            Log.Info("KSL - Still loading...");
            return;
        }

        try
        {
            _is_loading_run_save_postfix = true;
            Log.Info("|\nKSL - LoadRunSave Postfix\n|");
            CardStatisticStore.Instance.LoadStatistics();
        }
        finally
        {
            _is_loading_run_save_postfix = false;
        }
    }

    [HarmonyPatch("SaveRun")]
    [HarmonyPostfix]
    public static void SaveRunPostfix()
    {
        Log.Info("|\nKSL - SaveRunSave Postfix\n|");
        CardStatisticStore.Instance.LoadStatistics();
    }
}