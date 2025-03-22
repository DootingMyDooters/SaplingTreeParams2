using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace SaplingTreeParams2
{
    public class SaplingCommandHandler
    {
        private SaplingTreeParamConfig config = SaplingTreeParamConfig.Instance;
        private ICoreAPI api;
        private String configFileName;
        private String[] paramNames = new string[] { "type", "sff", "size", "obc", "vgc", "mgc", "ic" };
        private List<string> treeTypeList = new List<string>();

        public SaplingCommandHandler(ICoreAPI api, string configFileName)
        {
            this.api = api;
            this.configFileName = configFileName;
            LoadTreeTypeList();
        }

        private void LoadTreeTypeList()
        {
            Lang.GetAllEntries().Keys.Foreach((string key) => {
                if (key.Contains("treeseed-planted-")) treeTypeList.Add(key.Split("-")[2]);
            });
        }

        public TextCommandResult addSaplingConfig(TextCommandCallingArgs args)
        {
            SaplingParameters currSapParams = new SaplingParameters();
            bool changeExistingConfig = false;
            if (args.Parsers != null && args.Parsers.Count > 0) {
                ICommandArgumentParser commandArgumentParser = args.Parsers[0];
                String commandValue = ((String)commandArgumentParser.GetValue()).Trim();

                if (!commandValue.StartsWith("[") || !commandValue.EndsWith("]")) 
                    return TextCommandResult.Error("parameter list should start with [ and end with ]");

                String strippedParams = commandValue.Substring(1, commandValue.Length - 2);
                if (strippedParams.Split(",").Length < 1) return TextCommandResult.Error("no parameters provided!");

                foreach (String paramName in paramNames)
                {
                    String searchValue = paramName + "=";
                    int foundIndex = strippedParams.IndexOf(searchValue);
                    if (foundIndex < 0)
                    {
                        if (paramName != "type") continue;
                        else return TextCommandResult.Error("missing \"type\" parameter.");
                    }
                    int startIndex = foundIndex + searchValue.Length;
                    int endIndex = strippedParams.IndexOf(",", startIndex);
                    if (endIndex < 0) {
                        endIndex = strippedParams.Length;
                    }
                    int valueLength = endIndex - startIndex;
                    String paramValue = strippedParams.Substring(startIndex, valueLength);
                    try
                    {
                        switch (paramName)
                        {
                            case "type":
                                if (config.saplingParameters.Exists(sap => sap.treeType.Equals(paramValue)))
                                {
                                    currSapParams.SetSaplingParameters(config.saplingParameters.Find(sap => sap.treeType.Equals(paramValue)));
                                    changeExistingConfig = true;
                                }
                                else
                                {
                                    currSapParams.treeType = paramValue;
                                }
                                break;
                            case "sff":
                                currSapParams.skipForestFloor = paramValue.ToBool();
                                break;
                            case "size":
                                currSapParams.size = paramValue.ToFloat();
                                break;
                            case "obc":
                                currSapParams.otherBlockChance = paramValue.ToFloat();
                                break;
                            case "vgc":
                                currSapParams.vinesGrowthChance = paramValue.ToFloat();
                                break;
                            case "mgc":
                                currSapParams.mossGrowthChance = paramValue.ToFloat();
                                break;
                            case "ic":
                                currSapParams.ignoreColdTemp = paramValue.ToBool();
                                break;
                            default:
                                return TextCommandResult.Error("how did you get here???");
                        }
                    }
                    catch (Exception ex)
                    {
                        api.Logger.Error(ex);
                        return TextCommandResult.Error("Could not set " + paramName + " to " +  paramValue + ".\n" +
                            "Please check the logs.");
                    }
                }
            }
            else
            {
                return TextCommandResult.Success("nothing changed.");
            }
            if (changeExistingConfig)
            {
                config.saplingParameters.Find(sap => sap.treeType == currSapParams.treeType).SetSaplingParameters(currSapParams);
                api.StoreModConfig(config.saplingParameters, configFileName);
                return TextCommandResult.Success("config for tree type \"" + currSapParams.treeType + "\" changed to: " + currSapParams.prettyString());
            }
            else
            {
                config.saplingParameters.Add(currSapParams);
                api.StoreModConfig(config.saplingParameters, configFileName);
                return TextCommandResult.Success("config for tree type " + currSapParams.treeType + " added: " + currSapParams.prettyString());
            }
        }

        public TextCommandResult removeSaplingConfig(TextCommandCallingArgs args)
        {
            if (config.saplingParameters.Exists(sap => sap.treeType.Equals((String)args.Parsers.First().GetValue())))
            {
                config.saplingParameters.Remove(
                    config.saplingParameters.Find(sap => sap.treeType.Equals((String)args.Parsers.First().GetValue()))
                );
                api.StoreModConfig(config.saplingParameters, configFileName);
                return TextCommandResult.Success("removed config for tree type: " + (String)args.Parsers.First().GetValue());
            }
            else
            {
                return TextCommandResult.Error("this tree type doesn't exist in the config: " + (String)args.Parsers.First().GetValue());
            }
        }

        public TextCommandResult printConfig()
        {
            return TextCommandResult.Success("full config:\n" + String.Join(",\n", config.saplingParameters.Select(sap => sap.prettyString())));
        }

        public TextCommandResult printTreeTypeList()
        {
            return TextCommandResult.Success(String.Join(",", treeTypeList));
        }
    }
}
