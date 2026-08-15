using System.Collections.Generic;
using System.Linq;
using RoR2;
using StageFilter.Common;
using StageFilter.Lobby.UI;
using StageFilter.Lobby.Voting;
using StageFilter.Stage.Models;
using UnityEngine.Networking;
using static StageFilter.Common.ModDetectionUtil;
using static StageFilter.Rule.RuleManager;

namespace StageFilter.Lobby;

public static class LobbyManager
{
    public static LocalUser HostUser;
    public static bool PlayersCanVote;
    public static bool StageFilterIsAvailable = false;
    public static ILookup<StageSet, string> BannedStages;

    public static void ModInit()
    {
        On.RoR2.PreGameController.Start += PreGameController_OnStart;
        On.RoR2.UI.RuleChoiceController.OnClick += SingleplayerValidators.HandleChoiceControllerClick;
        On.RoR2.PreGameRuleVoteController.SetVote += SingleplayerValidators.ValidateSingleplayerVotes;
        On.RoR2.PreGameController.StartRun += MultiplayerValidators.ValidateMultiplayerVotes;
        On.RoR2.Run.Start += Run_Start;
        On.RoR2.Run.BeginStage += Run_BeginStage;

        if (IsThisModActive("groovesalad.ExpansionManager"))
        {
            StageFilter.Logger.LogInfo("ExpansionManager detected.");
            StageFilter.Logger.LogInfo("Hooking PreGameController.RecalculateModifierAvailability...");
            IL.RoR2.PreGameController.RecalculateModifierAvailability += Category.RecalculateStageAvailability;
        }
    }

    public static void PreGameController_OnStart(On.RoR2.PreGameController.orig_Start orig, PreGameController self)
    {
        orig(self);

        if (!NetworkServer.active)
            return;

        HostUser = LocalUserManager.GetFirstLocalUser();
        PlayersCanVote = PreGameController.cvSvAllowRuleVoting.value;
        StageFilterIsAvailable = IsValidGameMode((int)self.gameModeIndex);

#if DEBUG
        StageFilter.Logger.LogDebug("\n" +
            $"[Lobby Info]\n" +
            $"- Host User: {HostUser.userProfile.name}\n" +
            $"- Multiplayer: {RoR2Application.isInMultiPlayer}\n" +
            $"- Game Mode: {GameModeCatalog.indexToName[(int)self.gameModeIndex]}\n" +
            $"- Valid Lobby: {StageFilterIsAvailable}");
#endif
    }

    public static void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
        orig(self);

        if (!IsValidGameMode((int)self.gameModeIndex) || !NetworkServer.active)
            return;

        UpdateBannedStages(self);

        if (BannedStages.Count() > 0)
        {
            foreach (var stageSet in BannedStages)
            {
                int stageSetNumber = (int)stageSet.Key;
                string stageNames = string.Join(", ", stageSet);

                MessageUtil.SendBanMessage(
                    "STAGEFILTER_CHAT_STAGESET_BANS",
                    [$"{stageSetNumber}", stageNames]
                );
            }
        }

#if DEBUG
        foreach (RuleChoiceDef choice in self.ruleBook.choices)
        {
            if (choice.globalName.StartsWith("Expansions.") || IsStageRule(choice.ruleDef))
            {
                StageFilter.Logger.LogDebug($"{choice.ruleDef.globalName} ({choice.globalIndex}, {choice.localIndex}) -> {choice.globalName}");
            }
        }
#endif
    }

    public static void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
    {
        orig(self);

        if (!IsValidGameMode((int)self.gameModeIndex) || !NetworkServer.active)
            return;

        SceneDef currentScene = SceneCatalog.GetSceneDefForCurrentScene();
        SceneType sceneType = currentScene.sceneType;

        if (sceneType != SceneType.Stage || currentScene.isFinalStage)
            return;

        UpdateBannedStages(self);

        int stageSetNumber = currentScene.stageOrder;
        IEnumerable<string> stages = BannedStages[(StageSet)stageSetNumber];

        if (stages.Count() > 0)
        {
            string stageNames = string.Join(", ", stages);

            MessageUtil.SendBanMessage(
                "STAGEFILTER_CHAT_STAGESET_BANS",
                [$"{stageSetNumber}", stageNames]
            );
        }
    }

    public static bool IsValidGameMode(int gameModeIndex)
    {
        // GameMode Catalog
        // BaseDefenseRun: 0
        // ClassicRun: 1
        // EclipseRun: 2
        // InfiniteTowerRun: 3 (Simulacrum)
        // WeeklyRun: 4        (Prismatic Trials)
        bool isClassicRun = gameModeIndex == 1;
        bool isEclipseRun = gameModeIndex == 2;
        return (isClassicRun || isEclipseRun);
    }

    public static void UpdateBannedStages(Run run)
    {
        BannedStages = run.ruleBook.choices
        .Where(x => IsStageRule(x.ruleDef) && x.localName == "Off")
        .ToLookup(
            x => ((StageInfo)x.extraData).StageSet,
            x => Language.GetString(((StageInfo)x.extraData).NameToken)
        );
    }

}