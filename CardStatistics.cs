using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Text.Json;

namespace WyrmDragonHydra.STSAnalytics;

public class CardStatistic
{
    public int TimesSeen;
    public int TimesPicked;
    public int TimesEnchanted;
    public int TimesRemoved;
    public int TimesTransformed;
    public int TimesUpgraded;
    public int TimesDowngraded;
    public int TimesWon;
    public int Runs;
    public int RunLastSeen;
}

/// <summary>
/// Easy access to card statistics.
/// </summary>
public class CardStatisticStore
{
    private static CardStatisticStore _instance;
    public static CardStatisticStore Instance
    {
        get
        {
            _instance ??= new CardStatisticStore();
            return _instance;
        }
        private set { _instance = value; }
    }

    public Dictionary<ModelId, CardStatistic> Statistics;
    public int LatestRunCount { get; private set; }

    private CardStatisticStore()
    {
        Statistics = new Dictionary<ModelId, CardStatistic>();
        _instance = this;
    }

    public void LoadStatistics()
    {
        SaveManager sm = SaveManager.Instance;
        Statistics.Clear();

        //  History
        int run = 0;
        foreach (string file_name in sm.GetAllRunHistoryNames())
        {
            Log.Info(file_name);
            ReadSaveResult<RunHistory> history_rs = sm.LoadRunHistory(file_name);

            if (history_rs.Success && history_rs.SaveData != null)
            {
                RunHistory history = history_rs.SaveData;

                ProcessMapPoints(
                    history.MapPointHistory
                        .SelectMany(act => act)
                        .SelectMany(map_point_history_entry => map_point_history_entry.PlayerStats)
                        .Where(player_history_entry => player_history_entry != null),
                    history.Players.Where(player => player != null).Select(player => player.Deck),
                    history.Win,
                    run
                );
            }
            else
            {
                Log.Warn($"{file_name} couldn't be loaded.");
            }

            run += 1;
        }

        //  Current Run, if able
        if (sm.HasRunSave)
        {
            Log.Info("KSL - Has run save.");
            ReadSaveResult<SerializableRun> current_run = sm.LoadRunSave();
            if (current_run.Success && current_run.SaveData != null)
            {
                Log.Info("KSL - reading run save.");
                SerializableRun serializable_run = current_run.SaveData;
                ProcessMapPoints(
                    serializable_run.MapPointHistory
                        .SelectMany(act => act)
                        .SelectMany(map_point_history_entry => map_point_history_entry.PlayerStats)
                        .Where(player_history_entry => player_history_entry != null),
                    serializable_run.Players.Where(player => player != null).Select(player => player.Deck),
                    false,
                    run
                );
            }
            run += 1;
        }

        LatestRunCount = run;
        foreach (var kv in Statistics.Take(10))
        {
            Log.Info($"{kv.Key}: {JsonSerializer.Serialize(kv.Value)}");
        }
    }

    /// <summary>
    /// Returns a card statistic from the store.
    /// </summary>
    /// <param name="modelId"></param>
    /// <returns></returns>
    public CardStatistic GetCardStatistic(ModelId modelId)
    {
        if (!Statistics.ContainsKey(modelId))
        {
            Statistics.Add(modelId, new CardStatistic() { RunLastSeen = -1 });
        }
        return Statistics[modelId];
    }
    private void ProcessMapPoints(IEnumerable<PlayerMapPointHistoryEntry?> player_map_point_entries,
        IEnumerable<IEnumerable<SerializableCard>> decks, bool win, int run)
    {
        if (player_map_point_entries != null)
        {
            foreach (var player in player_map_point_entries)
            {
                if (player == null)
                {
                    continue;
                }
                ProcessItems(player.CardChoices.Select(x => x.Card?.Id), s => s.TimesSeen++, run);
                ProcessItems(player.CardsEnchanted.Select(x => x.Card?.Id), s => s.TimesEnchanted++, run);
                ProcessItems(player.CardsGained.Select(x => x.Id), s => s.TimesPicked++, run);
                ProcessItems(player.CardsRemoved.Select(x => x.Id), s => s.TimesRemoved++, run);
                ProcessItems(player.CardsTransformed.Select(x => x.OriginalCard.Id), s => s.TimesTransformed++, run);
                ProcessItems(player.CardsTransformed.Select(x => x.FinalCard.Id), s => s.TimesSeen++, run);
                ProcessItems(player.UpgradedCards, s => s.TimesUpgraded++, run);
                ProcessItems(player.DowngradedCards, s => s.TimesDowngraded++, run);
            }
        }
        if (decks != null)
        {
            foreach (var player_deck in decks)
            {
                if (player_deck == null)
                {
                    continue;
                }
                ProcessItems(player_deck.Select(card => card.Id).Distinct(),
                    s =>
                    {
                        s.Runs++;
                        if (win)
                        {
                            s.TimesWon++;
                        }
                    },
                    run
                );
            }
        }
    }

    private void ProcessItems(IEnumerable<ModelId?> items, Action<CardStatistic> updateAction, int run)
    {
        var validIds = items
            .Where(id => id != null);

        foreach (var id in validIds)
        {
            if (id == null)
            {
                continue;
            }
            var statistic = GetCardStatistic(id);
            updateAction(statistic);
            statistic.RunLastSeen = run;
            Statistics[id] = statistic;
        }
    }
}