using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace WyrmDragonHydra.STSAnalytics;


[HarmonyPatch(typeof(NCardRewardSelectionScreen))]
public static class NCardRewardSelectionScreenPatch
{

    [HarmonyPatch("RefreshOptions")]
    [HarmonyPostfix]
    public static void RefreshOptionsPostfix(NCardRewardSelectionScreen __instance,
        IReadOnlyList<CardCreationResult> options,
        IReadOnlyList<CardRewardAlternative> extraOptions
    )
    {
        var cardRow = __instance.GetNode<Control>("UI/CardRow");

        int i = 1;

        foreach (NCardHolder holder in cardRow.GetChildren().OfType<NGridCardHolder>())
        {
            ModelId? id = holder.CardModel?.Id;
            if (id == null)
            {
                continue;
            }

            CardStatistic statistic = CardStatisticStore.Instance.GetCardStatistic(id);

            Log.Info($"WDH - {id}");
            var label = new MegaRichTextLabel();
            label.CustomMinimumSize = new Vector2(300, 80);
            label.BbcodeEnabled = true;
            label.FitContent = true;
            label.ScrollActive = false;
            label.AutoSizeEnabled = true;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.Position = new Vector2(0, -225) - (label.CustomMinimumSize / 2);

            int runs_ago = (CardStatisticStore.Instance.LatestRunCount - 1) - statistic.RunLastSeen;
            string last_picked =
                    statistic.RunLastSeen < 0 ? $"Last picked: [jitter]never[/jitter]." :
                    runs_ago > 1 ? $"Last picked: [{runs_ago}] runs ago." :
                    runs_ago == 1 ? $"Last picked: [{runs_ago}] run ago." :
                    runs_ago == 0 ? $"Last picked: this run." :
                    "Error?";

            Log.Info($"Latest: {CardStatisticStore.Instance.LatestRunCount}, Last seen: {statistic.RunLastSeen} ");
            string pick_rate = statistic.TimesSeen == 0 ? "0" : ((float)statistic.TimesPicked / statistic.TimesSeen * 100).ToString("F2");
            string win_rate = statistic.Runs == 0 ? "0" : ((float)statistic.TimesWon / statistic.Runs * 100).ToString("F2");

            label.Text = $"{last_picked}\n[gold]Pick Rate: {pick_rate}%[/gold] [green]Win Rate: {win_rate}%[/green]";
            label.SetTextAutoSize(label.Text);
            holder.AddChild(label);

            i++;
        }
    }

}

