# Stage Filter

This mod allows you to filter which stages can appear during your runs through an in-game menu. Something similar to [ExpansionManager](https://thunderstore.io/c/riskofrain2/p/006/ExpansionManager/).

<div align="center" style="margin-top: 30px; margin-bottom: 40px">
    <img src="https://raw.githubusercontent.com/yunullvoid/StageFilter/refs/heads/main/media/category.png" width="75%"></img>
</div>

In single-player mode, the mod prevents you from blocking every stage in a single set with a popup, to avoid softlocks.

<div align="center" style="margin-top: 30px; margin-bottom: 30px">
    <img src="https://raw.githubusercontent.com/yunullvoid/StageFilter/refs/heads/main/media/warning.png" width="75%"></img>
</div>

In multiplayer mode, the mod bans all the most-voted stages. If a set of stages runs out of remaining options, the mod will randomly select one of the least-voted stages and unban it to prevent softlocks.

## Limitations

- This mod only works on **Classic** and **Eclipse** Runs.
- To prevent softlocks, stages that are part of a specific route (such as the Path of the Colossus stages and Conduit Canyon) cannot be banned.

> _**This may change in future versions.**_

## Compatibility

Mods that add custom stages using [R2API](https://thunderstore.io/c/riskofrain2/p/tristanmcpherson/R2API/) should work as intended. Aside from that, some compatible mods are:

- [ExpansionManager](https://thunderstore.io/c/riskofrain2/p/006/ExpansionManager/)
- [QuickRestart](https://thunderstore.io/package/AceOfShades/QuickRestart/)
- [CENI](https://thunderstore.io/c/riskofrain2/p/Jaosnake/CENI/) / [ProperSave](https://thunderstore.io/c/riskofrain2/p/KingEnderBrine/ProperSave/)

## Bugs and Issues

- Mods that change the order of a stage **after** the Stage Catalog has been built will not update the category in the lobby, causing problems with the banning logic. For example: [ForlornWreckageStage5](https://thunderstore.io/c/riskofrain2/p/viliger/ForlornWreckageStage5/).
- Modded map variants that do not follow the vanilla naming convention (that don't end with a number or "night") may bypass the filter even if the original map is banned.
