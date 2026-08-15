using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.UI;
using StageFilter.Rule;
using StageFilter.Stage.Models;
using UnityEngine;
using UnityEngine.UI;
using static StageFilter.Common.AssetLoader;
using OnRuleCategoryController = On.RoR2.UI.RuleCategoryController;
using OnRuleChoiceController = On.RoR2.UI.RuleChoiceController;

namespace StageFilter.Lobby.UI;

internal static class Category
{
    public static readonly Sprite ThunderStoreIcon = LoadSpriteFromResource("StageFilter.Assets.thunderstore_icon.png");
    public const string HeaderToken = "RULE_HEADER_STAGES_TITLE";

    // Adapted from ExpansionManager by Groove Salad
    // https://github.com/Priscillalala/ExpansionManager/blob/master/plugins/ContentBlockers/ItemBlockers.cs#L60
    public static void RecalculateStageAvailability(ILContext il)
    {
        var c = new ILCursor(il);
        int locChoiceDefIndex = -1;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out locChoiceDefIndex),
            x => x.MatchLdfld<RuleChoiceDef>(nameof(RuleChoiceDef.requiredExpansionDef)),
            x => x.MatchLdfld<ExpansionDef>(nameof(ExpansionDef.enabledChoice)),
            x => x.MatchCallOrCallvirt<RuleBook>(nameof(RuleBook.IsChoiceActive)))
            )
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, locChoiceDefIndex);
            c.EmitDelegate<Func<bool, PreGameController, RuleChoiceDef, bool>>((result, preGameController, choiceDef) =>
            {
                if (!result) return result;

                ExpansionDef expansion = choiceDef?.requiredExpansionDef;

                if (expansion is null || !RuleManager.IsStageRule(choiceDef.ruleDef))
                    return result;

                string expansionManagerRuleName = "Expansions." + expansion.name + ".Stages";
                foreach (var choice in preGameController.readOnlyRuleBook.choices)
                {
                    if (choice.ruleDef.globalName == expansionManagerRuleName)
                    {
                        return choice.localIndex == 0;
                    }
                }

                return result;
            });
        }
        else StageFilter.Logger.LogError("Failed to hook PreGameController.RecalculateModifierAvailability!");
    }

    /// <summary>
    /// Without this, choices that become unavailable due to a disabled DLC 
    /// won't refresh until another GUI interaction occurs.
    /// </summary>
    public static void UpdateChoicesVoteIcons(OnRuleCategoryController.orig_SetData orig, RuleCategoryController self, RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook ruleBook)
    {
        orig(self, categoryDef, availability, ruleBook);

        if (self.currentCategory.displayToken != HeaderToken)
            return;

        if (self.rulesToDisplay is not null && self.popoutButtonIconAllocator?.elements is not null)
        {
            for (int i = 0; i < self.rulesToDisplay.Count; i++)
            {
                self.popoutButtonIconAllocator.elements[i].UpdateFromVotes();
            }
        }
    }

    public static void RemoveRandomChoicesButton(OnRuleCategoryController.orig_SetData orig, RuleCategoryController self, RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook ruleBook)
    {
        orig(self, categoryDef, availability, ruleBook);

        if (self.currentCategory.displayToken == HeaderToken)
        {
            self.popoutRandomButtonContainer.SetActive(false);
        }
    }

    // Adapted from ExpansionManager by Groove Salad
    // https://github.com/Priscillalala/ExpansionManager/blob/master/plugins/ExpansionManagerUI.cs#L79
    public static void AddExpansionIcons(OnRuleChoiceController.orig_UpdateChoiceDisplay orig, RuleChoiceController self, RuleChoiceDef displayChoiceDef)
    {
        orig(self, displayChoiceDef);

        bool iconsAreDisabled = !ConfigManager.AreDLCIconsEnabled.Value;
        bool isNotStageOption = !RuleManager.IsStageRule(displayChoiceDef.ruleDef);

        if (iconsAreDisabled || isNotStageOption || !self.image)
            return;

        Transform subIconTransform = self.image.transform.parent.Find("SubIcon");
        Image subIcon = subIconTransform ? subIconTransform.GetComponent<Image>() : null;

        // Hide the icon by default
        if (subIcon) subIcon.gameObject.SetActive(false);

        bool hasNoExpansion = displayChoiceDef.requiredExpansionDef is null;
        bool isVanillaStage = !((StageInfo)displayChoiceDef.extraData).IsModded;

        if (hasNoExpansion && isVanillaStage)
            return;

        // Create a subIcon if there's none
        if (!subIcon)
        {
            subIcon = new GameObject(
                "SubIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            ).GetComponent<Image>();

            subIcon.transform.SetParent(self.image.transform.parent, false);

            Transform hoverOutline = subIcon.transform.parent.Find("HoverOutline");
            if (hoverOutline)
            {
                subIcon.transform.SetSiblingIndex(hoverOutline.GetSiblingIndex());
            }

            subIcon.gameObject.layer = LayerIndex.ui.intVal;

            RectTransform rect = subIcon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(32f, 32f);
            rect.localPosition = new Vector3(-20f, 20f);
        }

        // Show the icon
        subIcon.gameObject.SetActive(displayChoiceDef.localName != "Off");
        subIcon.sprite = (!isVanillaStage && hasNoExpansion)
            ? ThunderStoreIcon
            : displayChoiceDef.requiredExpansionDef.iconSprite;
    }
}

