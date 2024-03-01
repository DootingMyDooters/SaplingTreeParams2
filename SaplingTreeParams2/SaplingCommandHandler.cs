using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace SaplingTreeParams2
{
    public static class SaplingCommandHandler
    {
        public static SaplingTreeParamConfig config;
        public static ILogger logger;
        public static ICoreAPI api;

        public static TextCommandResult addSaplingConfig(TextCommandCallingArgs args, String fileName)
        {
            SaplingParameters saplingParameters = new SaplingParameters();
            bool changeExistingConfig = false;
            if (args.Parsers != null && args.Parsers.Count > 0) {
                ICommandArgumentParser commandArgumentParser = args.Parsers[0];
                String commandValue = ((String)commandArgumentParser.GetValue()).Trim();

                // Regex reg = new Regex(@"^\[type=\w+((,sff=(on|off|true|false|on|off|0|1))?(,size=[0-9](\.[0-9]+)?)?(,obc=[0-9](\.[0-9]+)?)(,vgc=[0-9](\.[0-9]+)?)(,mgc=[0-9](\.[0-9]+)?)?)(,ic=(on|off|true|false|on|off|0|1))?\]$");

                //if (!reg.IsMatch(commandValue)) return TextCommandResult.Error("wrong parameter syntax");
                if (!commandValue.StartsWith("[") || !commandValue.EndsWith("]")) 
                    return TextCommandResult.Error("parameter list should start with [ and end with ]");

                String strippedParams = commandValue.Substring(1, commandValue.Length - 2);
                String[] splitParams = strippedParams.Split(",");


                foreach (String command in splitParams)
                {
                    String[] splitCommand = command.Trim().Split("=");
                    String ParamName = splitCommand[0].Trim();
                    String ParamValue = splitCommand[1].Trim();
                    switch (ParamName)
                    {
                        case "type":
                            if (config.saplingParameters.Exists(sap => sap.treeType.Equals(ParamValue)))
                            {
                                changeExistingConfig = true;
                            }
                            else
                            {
                                saplingParameters.treeType = ParamValue;
                            }
                            break;
                        case "sff":
                            saplingParameters.skipForestFloor = ParamValue.ToBool();
                            break;
                        case "size":
                            saplingParameters.size = ParamValue.ToFloat();
                            break;
                        case "obc":
                            saplingParameters.otherBlockChance = ParamValue.ToFloat();
                            break;
                        case "vgc":
                            saplingParameters.vinesGrowthChance = ParamValue.ToFloat();
                            break;
                        case "mgc":
                            saplingParameters.mossGrowthChance = ParamValue.ToFloat();
                            break;
                        case "ic":
                            saplingParameters.ignoreColdTemp = ParamValue.ToBool();
                            break;
                        default:
                            return TextCommandResult.Error("wrong parameter passed.");
                    }
                }
            }
            else
            {
                return TextCommandResult.Success("nothing changed.");
            }
            if (changeExistingConfig)
            {
                config.saplingParameters.Find(sap => sap.treeType == saplingParameters.treeType).SetSaplingParameters(saplingParameters);
                api.StoreModConfig(config.saplingParameters, fileName);
                return TextCommandResult.Success("config for tree type " + saplingParameters.treeType + " changed to: " + saplingParameters.prettyString());
            }
            else
            {
                config.saplingParameters.Add(saplingParameters);
                api.StoreModConfig(config.saplingParameters, fileName);
                return TextCommandResult.Success("config for tree type " + saplingParameters.treeType + " added: " + saplingParameters.prettyString());
            }
        }

        public static TextCommandResult removeSaplingConfig(TextCommandCallingArgs args)
        {
            if (config.saplingParameters.Exists(sap => sap.treeType.Equals((String)args.Parsers.First().GetValue())))
            {
                config.saplingParameters.Remove(
                    config.saplingParameters.Find(sap => sap.treeType.Equals((String)args.Parsers.First().GetValue()))
                );
                return TextCommandResult.Success("removed config for treeType: " + (String)args.Parsers.First().GetValue());
            }
            else
            {
                return TextCommandResult.Error("this treeType doesn't exist in the config: " + (String)args.Parsers.First().GetValue());
            }
        }

        public static TextCommandResult printConfig()
        {
            return TextCommandResult.Success(String.Join(",", config.saplingParameters.Select(sap => sap.prettyString())));
        }
    }
}
