using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using System.Reflection;

namespace WyrmDragonHydra.STSAnalytics;

public static class CardRewardLogger
{
    public static void LogCardOptions(IReadOnlyList<CardRewardAlternative> options)
    {
        foreach (var option in options)
        {
            Log.Info($"{option}");
        }
    }
}

[HarmonyPatch]
public static class CardRewardPatch
{
    // Use a method that returns MethodInfo to locate the target
    public static MethodBase TargetMethod()
    {
        Type? rewardType = typeof(CardReward);
        return rewardType.GetMethod("OnSelect", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    [HarmonyPrefix]
    public static void OnSelectPrefix(CardReward __instance)
    {
        Log.Warn("KSL - onSelect");

        foreach (CardModel card in __instance.Cards)
        {
            Log.Info($"{card}");
        }
    }
}

//[HarmonyPatch(typeof(CardReward))]
//[HarmonyPatch("OnSelect")]
//public static class CardRewardTranspiler
//{
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//    {
//        var codes = new List<CodeInstruction>(instructions);

//        // We'll find the place after "cardRewardOption" is generated
//        for (int i = 0; i < codes.Count; i++)
//        {
//            yield return codes[i];

//            // This is a simplistic way: look for call to "Generate" method
//            if (codes[i].opcode == OpCodes.Call
//                && codes[i].operand is System.Reflection.MethodInfo mi
//                && mi.Name == "Generate")
//            {
//                // The previous instruction leaves the result (cardRewardOption) on the stack
//                // We need to call our logger with it
//                yield return new CodeInstruction(OpCodes.Dup); // duplicate cardRewardOption on stack
//                yield return new CodeInstruction(OpCodes.Call,
//                    AccessTools.Method(typeof(CardRewardLogger), "LogCardOptions"));
//            }
//        }
//    }
//}