using System.Collections.Generic;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StageFilter.Lobby;

public static class ExpansionIconLoader
{
    public static readonly List<Sprite> ExpansionIconsList = [];

    public static void SaveIcons(On.RoR2.ExpansionManagement.ExpansionCatalog.orig_SetExpansions orig, ExpansionDef[] newExpansionsDefs)
    {
        orig(newExpansionsDefs);

        foreach (ExpansionDef expansion in newExpansionsDefs)
        {
            AssetReferenceT<Sprite> iconReference = expansion.iconSpriteReference;

            if (iconReference == null)
                continue;

            iconReference.LoadAssetAsync().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ExpansionIconsList.Add(handle.Result);
                }
                else
                {
                    StageFilter.Logger.LogWarning($"Failed to load icon for expansion {expansion.name}.");
                }
            };
        }

    }
}
