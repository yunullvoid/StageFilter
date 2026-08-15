using BepInEx;
using BepInEx.Logging;
using RoR2;
using StageFilter.Rule;
using StageFilter.Stage;
using LobbyManager = StageFilter.Lobby.LobbyManager;
using Path = System.IO.Path;

namespace StageFilter;

[BepInDependency("com.Jaosnake.CENI", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.Wolfo.WolfoFixes", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.KingEnderBrine.ProperSave", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("groovesalad.ExpansionManager", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class StageFilter : BaseUnityPlugin
{
    public const string
        PluginAuthor = "Yunull",
        PluginName = "StageFilter",
        PluginGUID = "com." + PluginAuthor + "." + PluginName,
        PluginVersion = "1.0.0";

    public static new ManualLogSource Logger { get; private set; }

    public void Awake()
    {
        Logger = base.Logger;
        SetupLanguage();
        StageRegistry.ModInit();
        ConfigManager.ModInit(Config);
        RuleManager.ModInit();
        StageBlockers.ModInit();
        LobbyManager.ModInit();
    }

    private void SetupLanguage()
    {
        string directoryName = Path.GetDirectoryName(Info.Location);
        Language.collectLanguageRootFolders += list => list.Add(Path.Combine(directoryName, "Language"));
    }

}
