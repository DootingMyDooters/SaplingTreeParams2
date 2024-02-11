# SaplingTreeParams2
SaplingTreeParams stopped working in 1.19.3 and I got some time to update this mod.

## Updated Framework
I had to make a new repo,
because I didn't know how to update the original project to the new framework.
Framework was updated to .net7 like the game.

## Config Loading
previous mod allowed the config to be loaded mutiple times during the game.
current mod loads config once at game startup.

## Mod/Config Changes
1. you can now decide on what trees to modify, default is "pine".
2. config can't be empty, either load the mod or disable it.
3. you can now set ignoreColdTemp so that trees could grow in extreme weather conditions.
4. any tree that isn't in the list will be unaffected by this mod.

## Config Schema
```
[
  {
    "treeType": "pine",
    "skipForestFloor": true,
    "size": 0.5,
    "otherBlockChance": 1.0,
    "vinesGrowthChance": 0.01,
    "mossGrowthChance": 0.02,
    "ignoreColdTemp": true
  },
  {
    "treeType": "acacia",
    "skipForestFloor": true,
    "size": 0.5,
    "otherBlockChance": 1.0,
    "vinesGrowthChance": 0.01,
    "mossGrowthChance": 0.02,
    "ignoreColdTemp": false
  }, ...
]
```
