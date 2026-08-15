using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using StageFilter.Common;
using StageFilter.Stage.Models;
using UnityEngine;
using static StageFilter.Common.AssetLoader;
using static StageFilter.Stage.StageScanner;

namespace StageFilter.Stage;

public static class StageRegistry
{
    public static readonly Sprite noPreviewOn = LoadSpriteFromResource("StageFilter.Assets.no_preview_on.png");
    public static readonly Sprite noPreviewOff = LoadSpriteFromResource("StageFilter.Assets.no_preview_off.png");

    public static void ModInit()
    {
        On.RoR2.SceneCatalog.SetSceneDefs += RegisterStages;
    }

    public static IEnumerator RegisterStages(On.RoR2.SceneCatalog.orig_SetSceneDefs orig, SceneDef[] newSceneDefs)
    {
        HashSet<string> vanillaScenes = GetVanillaScenes();

        foreach (SceneDef scene in GetAvailableStages(newSceneDefs))
        {
            if (StageDatabase.StageList.Any(x => x.ID == scene.baseSceneName))
                continue;

            bool isVanilla = vanillaScenes.Contains(scene.baseSceneName);
            AddToStageList(scene, isVanilla);
        }

        StageDatabase.StageList.Sort((x, y) =>
        {
            int result = x.StageSet.CompareTo(y.StageSet);
            return result != 0 ? result : x.ID.CompareTo(y.ID);
        });

        foreach (var stage in StageDatabase.StageList)
        {
            StageDatabase.StageDictionary.Add(stage.ID, stage);
        }

        return orig(newSceneDefs);
    }

    private static void AddToStageList(SceneDef scene, bool isVanilla)
    {
        StageDatabase.StageList.Add(new()
        {
            ID = scene.baseSceneName,
            NameToken = scene.nameToken,
            StageSet = (StageSet)scene.stageOrder,
            IsModded = !isVanilla,
            RequiredExpansion = scene.requiredExpansion,
        });

        RegisterStageSprites(scene);
    }

    private static void RegisterStageSprites(SceneDef scene)
    {
        Texture stagePreview = scene.previewTexture;

        if (stagePreview is null)
        {
            StageFilter.Logger.LogWarning($"The preview texture for the stage '{scene.cachedName}' is null.");

            StageDatabase.EnabledSpriteList.Add(scene.baseSceneName, noPreviewOn);
            StageDatabase.DisabledSpriteList.Add(scene.baseSceneName, noPreviewOff);
            return;
        }

        Texture2D preview = TextureUtil.MakeReadableTexture(stagePreview, 70, 0, 256, 256);

        StageDatabase.EnabledSpriteList.Add(
            scene.baseSceneName,
            LoadSpriteFromTexture(preview, true));

        StageDatabase.DisabledSpriteList.Add(
            scene.baseSceneName,
            LoadSpriteFromTexture(preview, false));
    }

}

