using System.Collections.Generic;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StageFilter.Lobby;

public static class ExpansionIconLoader
{
    public static readonly Dictionary<string, Sprite> ExpansionIconsList = [];

    public static void SaveIcons(On.RoR2.ExpansionManagement.ExpansionCatalog.orig_SetExpansions orig, ExpansionDef[] newExpansionsDefs)
    {
        orig(newExpansionsDefs);

        foreach (ExpansionDef expansion in newExpansionsDefs)
        {
            Sprite oldIconReference = expansion.iconSprite;
            AssetReferenceT<Sprite> newIconReference = expansion.iconSpriteReference;

            if (oldIconReference is not null)
            {
                ExpansionIconsList[expansion.name] = oldIconReference;
                continue;
            }

            if (newIconReference is null || !newIconReference.RuntimeKeyIsValid())
            {
                StageFilter.Logger.LogWarning($"Invalid icon reference for expansion {expansion.name}!");
                ExpansionIconsList[expansion.name] = null;
                continue;
            }

            newIconReference.LoadAssetAsync().Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result is null)
                {
                    StageFilter.Logger.LogWarning($"Failed to load icon for expansion {expansion.name}!");
                    ExpansionIconsList[expansion.name] = null;
                    return;
                }

                ExpansionIconsList[expansion.name] = handle.Result;
            };
        }
    }
}
