using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

namespace SaplingTreeParams2
{
    [HarmonyPatch(typeof(BlockEntitySapling), "CheckGrow")]
    public class Sapling
    {

        private static NormalRandom normalRandom;

        static void Prefix(float dt, ref BlockEntitySapling __instance)
        {
            String instTreeType = __instance.Block.Variant["wood"];
            float temperature = __instance.Api.World.BlockAccessor.GetClimateAt(__instance.Pos, EnumGetClimateMode.NowValues).Temperature;
            
            if (__instance.Api is ICoreServerAPI && normalRandom == null)
            {
                normalRandom = new NormalRandom(__instance.Api.World.Seed);
            }

            // doesn't affect naturally(?) occurring saplings or ones that aren't in the config.
            if (!__instance.plantedFromSeed || !SaplingTreeParamConfig.Instance.saplingParameters.Exists(sapP => sapP.treeType == instTreeType))
            {
                return;
            }
            
            ICoreServerAPI sapi = __instance.Api as ICoreServerAPI;
            SaplingParameters rcc = SaplingTreeParamConfig.Instance.saplingParameters.Find(saplingParameters => saplingParameters.treeType == instTreeType);
            
            if (!rcc.ignoreColdTemp && temperature < 5f) return;

            // Access private fields not normally available through reflection
            Type typ = typeof(BlockEntitySapling);

            // Retrieve private field totalHoursTillGrowth
            FieldInfo fieldTotalHoursTillGrowth = typ.GetField("totalHoursTillGrowth", BindingFlags.NonPublic | BindingFlags.Instance);
            double totalHoursTillGrowth = (double)fieldTotalHoursTillGrowth.GetValue(__instance);
            // this field became public(?)
            EnumTreeGrowthStage stage = __instance.stage;

            // Retrieve private field growListenerId
            FieldInfo fieldGrowListenerId = typ.GetField("growListenerId", BindingFlags.NonPublic | BindingFlags.Instance);
            long growListenerId = (long)fieldGrowListenerId.GetValue(__instance);

            // Retrieve property GrowthRateMod
            PropertyInfo propGrowthRateMod = typ.GetProperty("GrowthRateMod", BindingFlags.NonPublic | BindingFlags.Instance);
            float growthRateMod = (float)propGrowthRateMod.GetValue(__instance);

            // Retrieve property nextStageDaysRnd
            PropertyInfo propNextStageDaysRnd = typ.GetProperty("nextStageDaysRnd", BindingFlags.NonPublic | BindingFlags.Instance);
            NatFloat nextStageDaysRnd = (NatFloat)propNextStageDaysRnd.GetValue(__instance);

            if (stage == EnumTreeGrowthStage.Seed || __instance.Api.World.Calendar.TotalHours < totalHoursTillGrowth) return;

            /* 
             * this section is left for future features if possible 
             * can't access sapling's totalHoursTillGrowth, unfortunately.
             */
            // if (stage == EnumTreeGrowthStage.Seed)
            //{
            //    __instance.stage = EnumTreeGrowthStage.Sapling;
            //    fieldTotalHoursTillGrowth.SetValue(__instance, 
            //        (long)(__instance.Api.World.Calendar.TotalHours + (double)nextStageDaysRnd.nextFloat(1f, __instance.Api.World.Rand) * 24f * growthRateMod));
            //    __instance.MarkDirty(redrawOnClient: true);
            //    return;
            //}

            BlockFacing[] hORIZONTALS = BlockFacing.HORIZONTALS;
            for (int i = 0; i < hORIZONTALS.Length; i++)
            {
                Vec3i normali = hORIZONTALS[i].Normali;
                int posX = __instance.Pos.X + normali.X * 32;
                int posZ = __instance.Pos.Z + normali.Z * 32;

                // Not at world edge and chunk is not loaded? We must be at the edge of loaded chunks. Wait until more chunks are generated
                if (__instance.Api.World.BlockAccessor.IsValidPos(posX, __instance.Pos.InternalY, posZ) && __instance.Api.World.BlockAccessor.GetChunkAtBlockPos(posX, __instance.Pos.InternalY, posZ) == null)
                    return;
            }

            Block block = __instance.Api.World.BlockAccessor.GetBlock(__instance.Pos);
            string treeGenCode = block.Attributes?["treeGen"].AsString(null);

            if (treeGenCode == null)
            {
                __instance.Api.Event.UnregisterGameTickListener(growListenerId);
                return;
            }

            AssetLocation code = new AssetLocation(treeGenCode);
            //ICoreServerAPI sapi = __instance.Api as ICoreServerAPI;

            ITreeGenerator gen;
            if (!sapi.World.TreeGenerators.TryGetValue(code, out gen))
            {
                __instance.Api.Event.UnregisterGameTickListener(growListenerId);
                return;
            }

            __instance.Api.World.BlockAccessor.SetBlock(0, __instance.Pos);
            __instance.Api.World.BulkBlockAccessor.ReadFromStagedByDefault = true;
            //float size = 0.6f + (float)__instance.Api.World.Rand.Next(0, (int) rcc.size) * 0.5f;
            
            float size = 0.6f + (float)__instance.Api.World.Rand.NextDouble() * rcc.size;

            TreeGenParams pa = new TreeGenParams()
            {
                skipForestFloor = rcc.skipForestFloor,
                size = size,
                otherBlockChance = rcc.otherBlockChance,
                vinesGrowthChance = rcc.vinesGrowthChance,
                mossGrowthChance = rcc.mossGrowthChance
            };

            sapi.World.TreeGenerators[code].GrowTree(__instance.Api.World.BulkBlockAccessor, __instance.Pos.DownCopy(), pa, normalRandom);
            __instance.Api.World.BulkBlockAccessor.Commit();

            return;
        }
    }
}
