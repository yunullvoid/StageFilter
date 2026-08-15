using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using StageFilter.Common;
using UnityEngine;

namespace StageFilter;

public static class ConfigManager
{
    public static ConfigEntry<bool> AreDLCIconsEnabled;

    public static void ModInit(ConfigFile config)
    {
        SetupConfigs(config);
    }

    private static void SetupConfigs(ConfigFile config)
    {
        string GUID = StageFilter.PluginGUID;
        string pluginName = StageFilter.PluginName;

        Sprite icon = AssetLoader.LoadSpriteFromResource("StageFilter.Assets.mod_icon.png");
        ModSettingsManager.SetModIcon(icon);

        ModSettingsManager.SetModDescription("desc", GUID, pluginName);
        ModSettingsManager.SetModDescriptionToken("STAGEFILTER_SETTINGS_DESCRIPTION", GUID, pluginName);

        AreDLCIconsEnabled = config.Bind(
            "General",
            "Expansion Icons",
            true,
            "If true, displays the icons of the expansions the maps belong to."
        );

        BaseOption option = new CheckBoxOption(AreDLCIconsEnabled);
        ModSettingsManager.AddOption(option, GUID, pluginName, "STAGEFILTER_SETTINGS_EXPANSIONICONS_TITLE", "STAGEFILTER_SETTINGS_EXPANSIONICONS_DESCRIPTION");
        ModSettingsManager.SetCategoryNameToken(GUID, option, "STAGEFILTER_SETTINGS_GENERAL_CATEGORY");
    }
}