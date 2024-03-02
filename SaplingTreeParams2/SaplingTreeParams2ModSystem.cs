using HarmonyLib;
using System.Reflection;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Linq;

[assembly: ModInfo("SaplingTreeParams2",
                    Authors = new string[] { "Dooters" },
                    Description = "This is a sample mod",
                    Version = "1.0.0")]
namespace SaplingTreeParams2
{
    public class SaplingTreeParams2ModSystem : ModSystem
    {
        public ICoreAPI api;
        public Harmony harmony;
        public static SaplingTreeParamConfig config = SaplingTreeParamConfig.Instance;

        public string configFileName = "saplingtreeparam_config.json";

        // pick only one because it starts for both.
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public void SetupConfig()
        {
            try
            {
                config.saplingParameters = api.LoadModConfig<List<SaplingParameters>>(configFileName);
            }
            catch (System.Exception e)
            {
                // if the file doesn't exist it'll return null
                // but if it does and there's a typo in the json or it's just wrong
                // we'll lof the error and 
                api.Logger.Error(e);              
            }
            if (config.saplingParameters == null)
            {
                this.api.Logger.Warning("[" + Mod.Info.ModID + "]: config didn't load, generating default config.");
                this.api.Logger.Warning("[" + Mod.Info.ModID + "]: default config will only contain settings for pine.");
                config.saplingParameters = new List<SaplingParameters>() { new SaplingParameters() };
                api.StoreModConfig(config.saplingParameters, configFileName);
            }
            api.Logger.Event("config loaded: " + String.Join(",\n", config.saplingParameters.Select(sap => sap.prettyString())));
            
        }

        public void SetupCommand()
        {
            SaplingCommandHandler handler = new SaplingCommandHandler(api, configFileName);
            api.ChatCommands.GetOrCreate("sapconfig")
                .RequiresPrivilege(Privilege.commandplayer)
                .BeginSubCommand("add")
                .WithDescription("add/change a tree sapling configuration in the existing config(this also affects the file!)")
                .WithAdditionalInformation("parameters are:" +
                    "\n\ttype(tree type) (required) - only trees that can grow from seeds." +
                    "\n\tsff(skipForestFloor) - true by default." +
                    "\n\tsize - 1 by default, sizes between 0.1 and 1 are recommended." +
                    "\n\tobc(otherBlockChance - 1 by default, sizes between 0.1 and 1 are recommended. this affects resin for trees that can spawn it.)" +
                    "\n\tvgc(vinesGrowthChance) - 0.01 by default." +
                    "\n\tmgc(mossGrowChance) - 0.02 by default." +
                    "\n\tic(ignoreColdTemp) - true by default." + 
                    "\nomitted parameters will be set to their default values.")
                    .WithExamples(new string[]
                    {
                        "/sapconfig add [type=birch,sff=true,size=0.3,obc=1,vgc=0.01,mgc=0.02,ic=true]",
                        "/sapconfig add [type=pine]",
                        "/sapconfig add [type=acacia,sff=false,ic=true]"
                    })
                    .WithArgs(
                        api.ChatCommands.Parsers.Word("treeconfig", new string[] {"[type=pine]", "[type=acacia,sff=false,ic=true]"})
                    )
                    .HandleWith((args) => handler.addSaplingConfig(args))
                .EndSubCommand()
                .BeginSubCommand("remove")
                .WithDescription("remove a tree sapling configuration from the existing file using the tree name")
                .WithExamples("/sapconfig remove acacia")
                    .WithArgs(
                        api.ChatCommands.Parsers.Word("type", new string[] {"pine", "birch", "acacia", "oak", "larch", "baldcypress", "purpleheart"})
                    )
                    .HandleWith((args) => handler.removeSaplingConfig(args))
                .EndSubCommand()
                .BeginSubCommand("show")
                .WithDescription("show all sapling configurations")
                    .HandleWith((args) => handler.printConfig())
                .EndSubCommand();

        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            SetupConfig();
            SetupCommand();
            // Apply patches with harmony
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll(); // Applies all harmony patches
                api.Logger.Event("Patched Mod " + Mod.Info.ModID);
            }
            base.StartServerSide(api);
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
            api.Logger.Event("Unpatched Mod" + Mod.Info.ModID);
            base.Dispose();
        }
    }

}

