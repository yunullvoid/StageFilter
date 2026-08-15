namespace StageFilter.Common;

internal static class ModDetectionUtil
{
    public static bool IsThisModActive(string modGUID)
    {
        return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(modGUID);
    }
}