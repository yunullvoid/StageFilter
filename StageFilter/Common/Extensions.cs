using System.Linq;
using System.Text.RegularExpressions;
using RoR2;
using RoR2.EntitlementManagement;
using StageFilter.Stage.Models;
using static StageFilter.Stage.StageDatabase;

namespace StageFilter.Common;

public static class Extensions
{
    public static bool HasRequiredEntitlement(this LocalUser localUser, EntitlementDef requiredEntitlement)
    {
        if (requiredEntitlement is null) return true;

        var tracker = EntitlementManager.localUserEntitlementTracker;
        return tracker.UserHasEntitlement(localUser, requiredEntitlement);
    }

    public static bool IsThisStageDisabled(this Run run, SceneDef scene)
    {
        if (run is null)
        {
            StageFilter.Logger.LogError("[IsThisStageDisabled] Null Run Instance!");
            return false;
        }

        if (string.IsNullOrEmpty(scene?.cachedName))
        {
            StageFilter.Logger.LogError("[IsThisStageDisabled] Null Scene or Empty Cached Name!");
            return false;
        }

        // Prevents variants like "blackbeach2" and "villagenight" from bypassing the ban
        string stageName = Regex.Replace(scene.cachedName, @"(night)?\d*$", "");

        if (!StageDictionary.TryGetValue(stageName, out StageInfo stage))
        {
            StageFilter.Logger.LogWarning($"[IsThisStageDisabled] Stage not found on Dictionary: {stageName}!");
            return false;
        }

        var choice = run.ruleBook.choices.FirstOrDefault(x => ReferenceEquals(x.extraData, stage));

        if (choice is null)
        {
            StageFilter.Logger.LogError($"[IsThisStageDisabled] Stage Choice not found: {stageName}!");
            return false;
        }

#if DEBUG
        StageFilter.Logger.LogDebug("\n" +
            $"[Ran IsThisStageDisabled]\n" +
            $"- Name: {stageName}\n" +
            $"- Choice: {choice.globalName}\n" +
            $"- Index: {choice.localIndex}");
#endif
        return choice.localIndex == 1;
    }
}

