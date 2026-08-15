using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RoR2;
using StageFilter.Stage.Models;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace StageFilter.Lobby.Voting;

internal class MultiplayerValidators
{
    public static void ValidateMultiplayerVotes(On.RoR2.PreGameController.orig_StartRun orig, PreGameController self)
    {
        if (!LobbyManager.StageFilterIsAvailable ||
             RoR2Application.isInSinglePlayer ||
            !NetworkServer.active ||
            !LobbyManager.PlayersCanVote)
        {
            orig(self);
            return;
        }

        int[] votes = PreGameRuleVoteController.votesForEachChoice;

        var stageGroups = RuleCatalog.allChoicesDefs
                        .Where(x => x.extraData is StageInfo)
                        .ToLookup(x => (StageInfo)x.extraData);

        var stageSetGroups = stageGroups.ToLookup(x => x.Key.StageSet);

        var expansionRules = self.readOnlyRuleBook.choices
                            .Where(x => Regex.IsMatch(x.ruleDef.globalName, @"^Expansions\.DLC[0-9]+(\.Stages)?$"))
                            .ToLookup(x => x.globalName.Split(".")[1]);

        foreach (var stageSet in stageSetGroups)
        {
            var results = new List<(RuleDef rule, StageInfo stage, int score, bool banned)>();

            foreach (var stage in stageSet)
            {
                RuleChoiceDef onChoice = stage.First(x => x.localName == "On");
                RuleChoiceDef offChoice = stage.First(x => x.localName == "Off");

                int onVotes = votes[onChoice.globalIndex];
                int offVotes = votes[offChoice.globalIndex];

                int score = offVotes - onVotes;

                StageInfo stageInfo = stage.Key;

                if (stageInfo.RequiredExpansion is not null)
                {
                    bool hasExpansionRulesDisabled = expansionRules[stageInfo.RequiredExpansion.name]
                        .Any(x => x.localName == "Off");

                    stageInfo.IsDisabledByExpansion = hasExpansionRulesDisabled;
                }

                results.Add((onChoice.ruleDef, stageInfo, score, score > 0));
            }

            var votableResults = results
                .Where(x => !x.stage.IsDisabledByExpansion)
                .ToList();

            bool allBanned = votableResults.All(x => x.banned);
            StageInfo chosenStage = null;

            if (allBanned)
            {
                int lowestScore = votableResults.Min(x => x.score);

                StageInfo[] draws = votableResults
                    .Where(x => x.score == lowestScore)
                    .Select(x => x.stage)
                    .ToArray();

                chosenStage = draws[Random.Range(0, draws.Length)];
            }

            foreach (var (rule, stage, score, banned) in results)
            {
                if (stage.IsDisabledByExpansion)
                {
                    stage.IsBanned = false;
                }
                else
                {
                    stage.IsBanned = allBanned
                      ? stage != chosenStage
                      : banned;
                }

                self.ApplyChoice(rule.choices[stage.IsDisabled ? 1 : 0].globalIndex);
            }

            if (chosenStage is not null)
            {
                StageFilter.Logger.LogDebug($"Restored Stage: {Language.GetString(chosenStage.NameToken)}");
            }
        }

        orig(self);
    }
}
