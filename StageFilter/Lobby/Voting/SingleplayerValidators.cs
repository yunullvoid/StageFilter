using System;
using System.Linq;
using System.Text.RegularExpressions;
using RoR2;
using RoR2.UI;
using StageFilter.Common;
using StageFilter.Lobby.UI;
using StageFilter.Rule;
using StageFilter.Stage;
using StageFilter.Stage.Models;

namespace StageFilter.Lobby.Voting;

public static class SingleplayerValidators
{
    public static void HandleChoiceControllerClick(On.RoR2.UI.RuleChoiceController.orig_OnClick orig, RuleChoiceController self)
    {
        if (!LobbyManager.StageFilterIsAvailable ||
             RoR2Application.isInMultiPlayer &&
             LobbyManager.PlayersCanVote)
        {
            orig(self);
            return;
        }

        RuleDef rule = self.choiceDef.ruleDef;
        string globalName = rule.globalName;

        var preGameRuleVoteController = PreGameRuleVoteController.FindForUser(self.FindNetworkUser());
        bool willDisable = preGameRuleVoteController.votes[rule.globalIndex].choiceValue == 0;

        if (willDisable)
        {
            if (RuleManager.IsStageRule(rule))
            {
                string stageID = globalName.Split('.')[^1];
                StageInfo stage = StageDatabase.StageDictionary[stageID];

                if (!CanBanStage(stage))
                {
                    Popup.OpenPopUp();
                    return;
                }
            }
            else if (Regex.IsMatch(globalName, @"^Expansions\.DLC[0-9]+(\.Stages)?$"))
            {
                string expansionID = globalName.Split('.')[1];

                if (!CanDisableExpansion(expansionID))
                {
                    Popup.OpenPopUp();
                    return;
                }
            }
        }

        orig(self);
    }

    public static void ValidateSingleplayerVotes(On.RoR2.PreGameRuleVoteController.orig_SetVote orig, PreGameRuleVoteController self, int ruleIndex, int choiceValue)
    {
        if (!LobbyManager.StageFilterIsAvailable ||
             RoR2Application.isInMultiPlayer &&
             LobbyManager.PlayersCanVote)
        {
            orig(self, ruleIndex, choiceValue);
            return;
        }

        RuleDef rule = RuleCatalog.GetRuleDef(ruleIndex);

        // Choice Values:
        // -1 = Unselected
        //  0 = Enabled
        //  1 = Disabled
        if (Regex.IsMatch(rule.globalName, @"^Expansions\.DLC[0-9]+(\.Stages)?$"))
        {
            string expansionID = rule.globalName.Split('.')[1];
            bool isDisabled = choiceValue == 1;

            if (isDisabled && !CanDisableExpansion(expansionID))
            {
                orig(self, ruleIndex, 0);
                return;
            }

            foreach (StageInfo stage in StageDatabase.StageList.Where(m => m.RequiredExpansion?.name == expansionID))
            {
                stage.IsDisabledByExpansion = isDisabled;
            }
        }

        if (RuleManager.IsStageRule(rule))
        {
            string stageID = rule.globalName.Split('.')[^1];
            StageInfo stage = StageDatabase.StageDictionary[stageID];

            bool isBanned = choiceValue == 1;

            if (isBanned && !CanBanStage(stage))
            {
                orig(self, ruleIndex, 0);
                return;
            }

            stage.IsBanned = isBanned;
        }

        orig(self, ruleIndex, choiceValue);
    }

    public static bool CanDisableExpansion(string expansionID)
    {
        foreach (StageSet stageSet in Enum.GetValues(typeof(StageSet)))
        {
            bool hasAnyStage = StageDatabase.StageList.Any(stage =>
            {
                if (stage.StageSet != stageSet)
                    return false;

                bool disabled =
                    stage.IsDisabled ||
                    !LobbyManager.HostUser.HasRequiredEntitlement(stage.RequiredExpansion?.requiredEntitlement) ||
                    stage.RequiredExpansion?.name == expansionID;

                return !disabled;
            });

            if (!hasAnyStage)
                return false;
        }

        return true;
    }

    public static bool CanBanStage(StageInfo targetStage)
    {
        return StageDatabase.StageList.Any(stage =>
            stage.StageSet == targetStage.StageSet &&
            stage.ID != targetStage.ID &&
            LobbyManager.HostUser.HasRequiredEntitlement(stage.RequiredExpansion?.requiredEntitlement) &&
            !stage.IsDisabled);
    }
}
