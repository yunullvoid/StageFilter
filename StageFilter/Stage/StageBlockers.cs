using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using StageFilter.Common;
using LobbyManager = StageFilter.Lobby.LobbyManager;

namespace StageFilter.Stage;

// Adapted from ExpansionManager by Groove Salad
// https://github.com/Priscillalala/ExpansionManager/blob/master/plugins/ContentBlockers/StageBlockers.cs
public static class StageBlockers
{
    public static void ModInit()
    {
        IL.RoR2.BazaarController.SetUpSeerStations += BazaarController_SetUpSeerStations;
        On.RoR2.BazaarController.SetUpSeerStations += RevertWolfoFixes;
        On.RoR2.Run.CanPickStage += Run_CanPickStage;
    }

    private static void BazaarController_SetUpSeerStations(ILContext il)
    {
        var c = new ILCursor(il);
        int locSceneDefIndex = -1;

        if (c.TryGotoNext(
            MoveType.After,
            x => x.MatchLdloc(out locSceneDefIndex),
            x => x.MatchLdfld<SceneDef>(nameof(SceneDef.requiredExpansion)),
            x => x.MatchCallOrCallvirt<Run>(nameof(Run.IsExpansionEnabled))
        ))
        {
            c.Emit(OpCodes.Ldloc, locSceneDefIndex);
            c.EmitDelegate<Func<bool, SceneDef, bool>>((result, sceneDef) =>
            {
                bool isValidGamemode = LobbyManager.IsValidGameMode((int)Run.instance.gameModeIndex);
                bool stageIsDisabled = Run.instance.IsThisStageDisabled(sceneDef);
#if DEBUG
                StageFilter.Logger.LogDebug("\n" +
                    "[Ran SetUpSeerStations]\n" +
                    $"- Stage: {sceneDef.cachedName}\n" +
                    $"- Is Valid Gamemode: {isValidGamemode}\n" +
                    $"- Is Disabled: {stageIsDisabled}");
#endif
                return result && isValidGamemode && !stageIsDisabled;
            });
        }
        else StageFilter.Logger.LogError("Failed to hook BazaarController.SetUpSeerStations!");
    }

    private static bool Run_CanPickStage(On.RoR2.Run.orig_CanPickStage orig, Run self, SceneDef sceneDef)
    {
        bool isValidGamemode = LobbyManager.IsValidGameMode((int)self.gameModeIndex);
        bool stageIsDisabled = self.IsThisStageDisabled(sceneDef);

#if DEBUG
        StageFilter.Logger.LogDebug("\n" +
            "[Ran CanPickStage]\n" +
            $"- Stage: {sceneDef.cachedName}\n" +
            $"- Is Valid Gamemode: {isValidGamemode}\n" +
            $"- Is Disabled: {stageIsDisabled}");
#endif
        return orig(self, sceneDef) && isValidGamemode && !stageIsDisabled;
    }

    /// <summary>
    /// Without this, WolfoFixes might re-add banned stages to empty Seer Stations.
    /// </summary>
    private static void RevertWolfoFixes(On.RoR2.BazaarController.orig_SetUpSeerStations orig, BazaarController self)
    {
        orig(self);

        foreach (SeerStationController seer in self.seerStations)
        {
            if (seer.targetSceneDefIndex == -1)
                continue;

            SceneDef targetSceneDef = SceneCatalog.indexToSceneDef[seer.targetSceneDefIndex];

            if (!Run.instance.IsThisStageDisabled(targetSceneDef))
                continue;

            SceneDef replacement = FindReplacementScene(self);

            if (replacement != null)
            {
                StageFilter.Logger.LogDebug($"SEER SCENE REPLACED: {targetSceneDef.baseSceneName} -> {replacement.baseSceneName}");
                seer.SetTargetScene(replacement);
                seer.GetComponent<PurchaseInteraction>().SetAvailable(true);
            }
            else
            {
                StageFilter.Logger.LogDebug($"SEER SCENE REMOVED: {targetSceneDef.baseSceneName}");
                seer.GetComponent<PurchaseInteraction>().SetAvailable(false);
            }
        }
    }

    private static SceneDef FindReplacementScene(BazaarController bazaar)
    {
        List<string> takenScenes = [];

        foreach (SeerStationController seer in bazaar.seerStations)
        {
            if (seer.targetSceneDefIndex == -1)
                continue;

            SceneDef scene = SceneCatalog.indexToSceneDef[seer.targetSceneDefIndex];

            if (scene != null)
                takenScenes.Add(scene.baseSceneName);
        }

        List<SceneDef> candidates = [];

        foreach (SceneDef scene in SceneCatalog.allSceneDefs)
        {
            if (scene == null)
                continue;

            if (scene.filterOutOfBazaar)
                continue;

            if (scene.sceneType == SceneType.Junk)
                continue;

            if (scene.stageOrder != Run.instance.nextStageScene.stageOrder)
                continue;

            if (takenScenes.Contains(scene.baseSceneName))
                continue;

            if (Run.instance.IsThisStageDisabled(scene))
                continue;

            if (scene.requiredExpansion != null &&
                !Run.instance.IsExpansionEnabled(scene.requiredExpansion))
                continue;

            if (!bazaar.IsUnlockedBeforeLooping(scene))
                continue;

            candidates.Add(scene);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}