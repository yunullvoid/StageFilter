using System.Collections.Generic;
using StageFilter.Stage.Models;
using UnityEngine;

namespace StageFilter.Stage;

public static class StageDatabase
{
    public static readonly Dictionary<string, Sprite> EnabledSpriteList = [];
    public static readonly Dictionary<string, Sprite> DisabledSpriteList = [];
    public static readonly List<StageInfo> StageList = [];
    public static readonly Dictionary<string, StageInfo> StageDictionary = [];
}