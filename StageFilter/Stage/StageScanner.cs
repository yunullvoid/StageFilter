using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.ContentManagement;

namespace StageFilter.Stage;

public static class StageScanner
{
    public static IEnumerable<SceneDef> GetAvailableStages(SceneDef[] sceneDefs)
    {
        // TODO: MAYBE remove this and make a system that prevents the portal from spawning if one of these stages is banned.
        // Ex.1: If Reformed Altar is banned, the Green Portal doesn't spawn on Stage 1.
        //       But if Treeborn Colony is active, the Green Portal can spawn on Stage 2.
        // Ex.2: If Conduit Canyon is banned, the Access Node doesn't spawn.
        string[] blackList =
        [
            "lemuriantemple",
            "habitat",
            "habitatfall",
            "conduitcanyon"
        ];

        foreach (SceneDef scene in sceneDefs)
        {
            if (scene.sceneType != SceneType.Stage)
                continue;
            // 6 = Moon; 97 = Simulacrum; 99 = Void
            if (scene.stageOrder >= 6)
                continue;

            if (scene.isLockedBeforeLooping)
                continue;

            if (blackList.Contains(scene.baseSceneName))
                continue;

            yield return scene;
        }
    }

    public static HashSet<string> GetVanillaScenes()
    {
        var packs = ContentManager.allLoadedContentPacks;
        var vanillaPacks = packs.Where(pack => pack.identifier.StartsWith("RoR2."));
        HashSet<string> vanillaScenes = vanillaPacks
            .SelectMany(pack => pack.sceneDefs)
            .Select(scene => scene.baseSceneName)
            .ToHashSet();

#if DEBUG
        SceneDef[] allStages = packs.SelectMany(pack => pack.sceneDefs)
            .Where(x => x.sceneType == SceneType.Stage)
            .ToArray();

        foreach (var stage in allStages)
        {
            StageFilter.Logger.LogDebug("\n" +
                $"[Stage Info]\n" +
                $"- Name: {Language.GetString(stage.nameToken)}\n" +
                $"- ID: {stage.cachedName}\n" +
                $"- Order: {stage.stageOrder}\n" +
                $"- Loop: {stage.isLockedBeforeLooping}\n" +
                $"- Loop Variant: {stage.loopedSceneDef?.cachedName}"
            );
        }
#endif
        return vanillaScenes;
    }
}