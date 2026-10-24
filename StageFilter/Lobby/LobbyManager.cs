using System.Linq;
using RoR2;
using RoR2.UI;
using StageFilter.Lobby.UI;
using StageFilter.Lobby.Voting;
using StageFilter.Stage.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static StageFilter.Common.ModDetectionUtil;
using static StageFilter.Rule.RuleManager;

namespace StageFilter.Lobby;

public static class LobbyManager
{
    public static LocalUser HostUser;
    public static bool PlayersCanVote;
    public static bool StageFilterIsAvailable = false;
    public static StageInfo[] BannedStages;

    public static void ModInit()
    {
        On.RoR2.PreGameController.Start += PreGameController_OnStart;
        On.RoR2.UI.RuleChoiceController.OnClick += SingleplayerValidators.HandleChoiceControllerClick;
        On.RoR2.PreGameRuleVoteController.SetVote += SingleplayerValidators.ValidateSingleplayerVotes;
        On.RoR2.PreGameController.StartRun += MultiplayerValidators.ValidateMultiplayerVotes;
        On.RoR2.Run.Start += Run_Start;
        On.RoR2.Run.BeginStage += Run_BeginStage;
        On.RoR2.ExpansionManagement.ExpansionCatalog.SetExpansions += ExpansionIconLoader.SaveIcons;
        On.RoR2.UI.PauseScreenController.Awake += AddBanListButtonToPauseScreen;

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

        BanList.EmptyPages();

        UpdateBannedStages(self);

        BanList.FillPages(BannedStages);

        StageFilter.Logger.LogDebug(BanList.pages.Count());

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

    }

    // Adapted from QuickRestart by AceOfShades
    // https://github.com/Maceris/quick-restart/blob/f1fdc4ec65c3b3d02f9f22e1c7be355269b4e1b5/QuickRestart.cs#L188
    public static void AddBanListButtonToPauseScreen(On.RoR2.UI.PauseScreenController.orig_Awake orig, PauseScreenController self)
    {
        orig(self);

        // Only show during runs
        if (Run.instance is null || PreGameController.instance) return;

        //Add button to the pause screen
        Transform firstButton = self.mainPanel.GetChild(0).GetChild(1);
        Transform banListButton = GameObject.Instantiate(firstButton, self.mainPanel.GetChild(0));

        TextMeshProUGUI buttonText = banListButton.GetComponentInChildren<HGTextMeshProUGUI>();
        buttonText.text = Language.GetStringFormatted("PAUSE_STAGE_BAN_LIST");
        //The GUI refuses to update without literally replacing the component
        Transform buttonTextParent = buttonText.transform.parent;
        TextMeshProUGUI newButtonText = GameObject.Instantiate(buttonText, buttonTextParent);
        GameObject.Destroy(buttonText);

        HGButton restartHGButton = banListButton.GetComponent<HGButton>();
        restartHGButton.onClick = new Button.ButtonClickedEvent();
        restartHGButton.onClick.AddListener(() => ListUI.ShowBanListDialog(BanList.pages));

        //banListButton.transform.SetAsLastSibling();
        banListButton.transform.SetSiblingIndex(3);
    }

    public static bool IsValidGameMode(int gameModeIndex)
    {
        // GameMode Catalog
        // ClassicRun: 0
        // EclipseRun: 1
        // WeeklyRun: 2 (Prismatic Trials)
        // BaseDefenseRun: 3
        // InfiniteTowerRun: 4 (Simulacrum)
        bool isClassicRun = gameModeIndex == 0;
        bool isEclipseRun = gameModeIndex == 1;

        return (isClassicRun || isEclipseRun);
    }

    public static void UpdateBannedStages(Run run)
    {
        BannedStages = run.ruleBook.choices
        .Where(x => IsStageRule(x.ruleDef) && x.localName == "Off")
        .Select(x => (StageInfo)x.extraData)
        .OrderBy(x => x.StageSet)
        .ThenBy(x => x.ID)
        .ToArray();
    }

}