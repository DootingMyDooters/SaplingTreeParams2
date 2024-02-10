using HarmonyLib;
using System.Reflection;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using static System.Net.Mime.MediaTypeNames;

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
        public static SaplingTreeParamConfig config;

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
                config = api.LoadModConfig<SaplingTreeParamConfig>(configFileName);
            }
            catch (System.Exception e)
            {
                // if the file doesn't exist it'll return null
                // but if it does and there's a typo in the json
                api.Logger.Error(e);
            }
            if (config == null)
            {
                this.api.Logger.Warning("[" + Mod.Info.ModID + "]: config didn't load, generating default config.");
                config = new SaplingTreeParamConfig();
                api.StoreModConfig<SaplingTreeParamConfig>(config, configFileName);
            }

            api.Logger.Event(
                "config loaded (\n" +
                    "\tskipForestFloor: " + config.skipForestFloor + ",\n" +
                    "\tsize: " + config.size + ",\n" +
                    "\totherBlockChance: " + config.otherBlockChance + "\n" +
                    "\tvinesGrowthChance: " + config.vinesGrowthChance + "\n" +
                    "\tmossGrowthChance: " + config.mossGrowthChance + "\n" +
                ")"
             );
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            SetupConfig();

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

