<p align="center">
    <img src="https://raw.githubusercontent.com/yunullvoid/StageFilter/refs/heads/main/media/banner.png"></img>
</p>

<p align="center">
    <a href="https://thunderstore.io/c/riskofrain2/p/Null_Team/StageFilter/">
        <img
            src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2FNull_Team%2FStageFilter%2F&query=%24.latest_version&label=thunderstore&prefix=v&color=23fc79&style=for-the-badge"
            alt="Thunderstore version">
    </a>
    <a href="https://thunderstore.io/c/riskofrain2/p/Null_Team/StageFilter/">
        <img
            src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2FNull_Team%2FStageFilter%2F&query=%24.downloads&label=downloads&color=ff8a28&style=for-the-badge"
            alt="Thunderstore downloads"
        >
    </a>
</p>

---

This mod allows you to filter which stages can appear during your runs through an in-game menu.

<p align="center" style="margin-top: 30px; margin-bottom: 40px">
    <img src="https://raw.githubusercontent.com/yunullvoid/StageFilter/refs/heads/main/media/category.png" width="75%"></img>
</p>

In single-player mode, the mod prevents you from blocking every stage in a single set with a popup, to avoid softlocks.

<p align="center" style="margin-top: 30px; margin-bottom: 30px">
    <img src="https://raw.githubusercontent.com/yunullvoid/StageFilter/refs/heads/main/media/warning.png" width="75%"></img>
</div>

In multiplayer mode, the mod bans all the most-voted stages. If a set of stages runs out of remaining options, the mod will randomly select one of the least-voted stages and unban it to prevent softlocks.

## Limitations

- This mod only works on **Classic** and **Eclipse** Runs.
- To prevent softlocks, stages that are part of a specific route (such as the Path of the Colossus stages and Conduit Canyon) cannot be banned.

> _**This may change in future versions.**_

## Compatibility

Mods that add custom stages using [R2API](https://thunderstore.io/c/riskofrain2/p/tristanmcpherson/R2API/) should work as intended. Aside from that, some compatible mods are:

|                                                                                                                                                                                                                             |                                                                                  |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| <p align="center"><a href="https://thunderstore.io/c/riskofrain2/p/Jaosnake/CENI/"><img width="100" src="https://ccdn.thunderstore.io/live/repository/icons/Jaosnake-CENI-1.0.0.png"></img></a></p>                         | [CENI](https://thunderstore.io/c/riskofrain2/p/Jaosnake/CENI/)                   |
| <p align="center"><a href="https://thunderstore.io/c/riskofrain2/p/KingEnderBrine/ProperSave/"><img width="100" src="https://ccdn.thunderstore.io/live/repository/icons/KingEnderBrine-ProperSave-3.0.7.png"></img></a></p> | [ProperSave](https://thunderstore.io/c/riskofrain2/p/KingEnderBrine/ProperSave/) |
| <p align="center"><a href="https://thunderstore.io/package/AceOfShades/QuickRestart/"><img width="100" src="https://ccdn.thunderstore.io/live/repository/icons/AceOfShades-QuickRestart-1.6.1.png"></img></a></p>           | [QuickRestart](https://thunderstore.io/package/AceOfShades/QuickRestart/)        |

## Bugs and Issues

- Mods that change the order of a stage **after** the Stage Catalog has been built will not update the category in the lobby, causing problems with the banning logic. For example: [ForlornWreckageStage5](https://thunderstore.io/c/riskofrain2/p/viliger/ForlornWreckageStage5/).
- Modded stage variants that do not follow the vanilla naming convention (that don't end with a number or "night") may bypass the filter even if the original stage is banned.

> _**If you encounter any bugs, feel free to open an issue on GitHub or ping me on the [modding server](https://discord.com/invite/5MbXZvd).**_

## Credits

- **Developer:** Yunull
- **Testing:** Sakimi Yris, Sahbine
