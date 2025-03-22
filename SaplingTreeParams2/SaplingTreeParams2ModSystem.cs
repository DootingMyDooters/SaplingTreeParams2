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
                    Description = "Change a few growth parameters for tree saplings",
                    Version = "1.0.5")]
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
            api.Logger.Event("Unpatched Mod " + Mod.Info.ModID);
            base.Dispose();
        }
    }

}

