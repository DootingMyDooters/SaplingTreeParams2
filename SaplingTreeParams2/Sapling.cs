﻿using HarmonyLib;
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

namespace SaplingTreeParams2
{
    [HarmonyPatch(typeof(BlockEntitySapling), "CheckGrow")]
    public class Sapling
    {

        static void Prefix(float dt, ref BlockEntitySapling __instance)
        {
            ICoreServerAPI sapi = __instance.Api as ICoreServerAPI;
            SaplingTreeParamConfig rcc = sapi.LoadModConfig<SaplingTreeParamConfig>("saplingtreeparam_config.json");
            if (!__instance.plantedFromSeed || (!__instance.Block.Variant["wood"].Equals("pine") && !__instance.Block.Variant["wood"].Equals("acacia")))
            {
                // basically will not affect other trees
                return;
            }
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


            // Original code
            if (__instance.Api.World.Calendar.TotalHours < totalHoursTillGrowth)
                return;

            if (stage == EnumTreeGrowthStage.Seed)
            {
                //fieldStage.SetValue(__instance, EnumTreeGrowthStage.Sapling);
                __instance.stage = EnumTreeGrowthStage.Sapling;
                fieldTotalHoursTillGrowth.SetValue(__instance, __instance.Api.World.Calendar.TotalHours + nextStageDaysRnd.nextFloat(1, __instance.Api.World.Rand) * 24 * growthRateMod);
                __instance.MarkDirty(true);
                return;
            }

            int chunksize = __instance.Api.World.BlockAccessor.ChunkSize;
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                Vec3i dir = facing.Normali;
                int x = __instance.Pos.X + dir.X * chunksize;
                int z = __instance.Pos.Z + dir.Z * chunksize;

                // Not at world edge and chunk is not loaded? We must be at the edge of loaded chunks. Wait until more chunks are generated
                if (__instance.Api.World.BlockAccessor.IsValidPos(x, __instance.Pos.Y, z) && __instance.Api.World.BlockAccessor.GetChunkAtBlockPos(x, __instance.Pos.Y, z) == null)
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

            sapi.LoadModConfig<SaplingTreeParamConfig>("saplingtreeparam_config.json");

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

            sapi.World.TreeGenerators[code].GrowTree(__instance.Api.World.BulkBlockAccessor, __instance.Pos.DownCopy(), pa);
            __instance.Api.World.BulkBlockAccessor.Commit();

            return;
        }
    }
}
