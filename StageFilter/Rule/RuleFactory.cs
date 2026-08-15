using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using StageFilter.Stage;
using StageFilter.Stage.Models;
using UnityEngine;
using LobbyManager = StageFilter.Lobby.LobbyManager;

namespace StageFilter.Rule;

internal class RuleFactory
{
    public const string StageRuleName = "Stages.";

    public static void CreateStagesCategory()
    {
        RuleCatalog.AddCategory(
            "RULE_HEADER_STAGES_TITLE",
            "RULE_HEADER_STAGES_SUBTITLE",
            new Color32(196, 143, 51, 255),
            null,
            "RULE_HEADER_STAGES_EDIT",
            () => !LobbyManager.StageFilterIsAvailable,
            RuleCatalog.RuleCategoryType.VoteResultGrid
        );

        foreach (var stage in StageDatabase.StageList)
        {
            RuleCatalog.AddRule(CreateStageRule(stage));
        }
    }

    private static RuleDef CreateStageRule(StageInfo stage)
    {
        string stageID = stage.ID;
        string stageName = stage.NameToken;
        ExpansionDef stageRequiredExpansion = stage.RequiredExpansion;
        EntitlementDef stageRequiredEntitlement = stageRequiredExpansion?.requiredEntitlement;
        RuleDef ruleDef = new(StageRuleName + stageID, stageName);
        string tooltipNameToken = $"{Language.GetString(stageName)} {Language.GetString("CHOICE_TOOLTIP_TITLE_STAGES")} {(int)stage.StageSet}";
        // On
        RuleChoiceDef enabledChoice = ruleDef.AddChoice("On", stage);
        enabledChoice.sprite = StageDatabase.EnabledSpriteList[stageID];
        enabledChoice.tooltipNameToken = tooltipNameToken;
        enabledChoice.tooltipBodyToken = "CHOICE_TOOLTIP_BODY_STAGES_ON";
        enabledChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact);
        enabledChoice.selectionUISound = "Play_UI_artifactSelect";
        enabledChoice.requiredEntitlementDef = stageRequiredEntitlement;
        enabledChoice.requiredExpansionDef = stageRequiredExpansion;
        ruleDef.MakeNewestChoiceDefault();
        // Off
        RuleChoiceDef disabledChoice = ruleDef.AddChoice("Off", stage);
        disabledChoice.sprite = StageDatabase.DisabledSpriteList[stageID];
        disabledChoice.tooltipNameToken = tooltipNameToken;
        disabledChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
        disabledChoice.tooltipBodyToken = "CHOICE_TOOLTIP_BODY_STAGES_OFF";
        disabledChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
        disabledChoice.selectionUISound = "Play_UI_artifactDeselect";

        return ruleDef;
    }

}

