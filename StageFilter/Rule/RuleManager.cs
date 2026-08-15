using MonoMod.Cil;
using RoR2;
using StageFilter.Lobby.UI;
using OnRuleCategoryController = On.RoR2.UI.RuleCategoryController;
using OnRuleChoiceController = On.RoR2.UI.RuleChoiceController;

namespace StageFilter.Rule;

public static class RuleManager
{
    public static void ModInit()
    {
        IL.RoR2.RuleCatalog.Init += AddCustomCategory;
        OnRuleCategoryController.SetData += Category.RemoveRandomChoicesButton;
        OnRuleCategoryController.SetData += Category.UpdateChoicesVoteIcons;
        OnRuleChoiceController.UpdateChoiceDisplay += Category.AddExpansionIcons;
    }

    private static void AddCustomCategory(ILContext il)
    {
        var c = new ILCursor(il);

        if (c.TryGotoNext(
            MoveType.After,
            x => x.MatchCall(typeof(RuleCatalog), nameof(RuleCatalog.AddRule)),
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(7)))
        {
            c.EmitDelegate(RuleFactory.CreateStagesCategory);
        }
        else StageFilter.Logger.LogError("Failed to hook RuleCatalog.Init!");
    }

    public static bool IsStageRule(RuleDef rule)
    {
        return rule.globalName.StartsWith(RuleFactory.StageRuleName);
    }
}