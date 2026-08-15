using RoR2.ExpansionManagement;

namespace StageFilter.Stage.Models;

public enum StageSet
{
    First  = 1,
    Second = 2,
    Third  = 3,
    Fourth = 4,
    Fifth  = 5,
}

public class StageInfo
{
    public string ID { get; set; }
    public string NameToken { get; set; }
    public StageSet StageSet { get; set; }
    public ExpansionDef RequiredExpansion { get; set; }
    public bool IsModded { get; set; }
    public bool IsBanned { get; set; }
    public bool IsDisabledByExpansion { get; set; }
    public bool IsDisabled => IsBanned || IsDisabledByExpansion;
}
