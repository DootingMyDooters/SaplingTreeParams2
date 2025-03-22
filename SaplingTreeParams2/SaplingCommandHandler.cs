using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
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
            this.initCommand();
        }

        private void initCommand()
        {
            this.api.ChatCommands.GetOrCreate("sapconfig")
                .WithDescription("add/remove/show/listtreetypes sapling growth configurations.")
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
                        api.ChatCommands.Parsers.Word("treeconfig", new string[] { "[type=pine]", "[type=acacia,sff=false,ic=true]" })
                    )
                    .HandleWith((args) => this.addSaplingConfig(args))
                .EndSubCommand()
                .BeginSubCommand("remove")
                .WithDescription("remove a tree sapling configuration from the existing file using the tree name")
                .WithExamples("/sapconfig remove acacia")
                    .WithArgs(
                        api.ChatCommands.Parsers.Word("type", new string[] { "pine", "birch", "acacia", "oak", "larch", "baldcypress", "purpleheart" })
                    )
                    .HandleWith((args) => this.removeSaplingConfig(args))
                .EndSubCommand()
                .BeginSubCommand("show")
                .WithDescription("show current sapling configurations")
                    .HandleWith((args) => this.printConfig())
                .EndSubCommand()
                .BeginSubCommand("listtreetypes")
                .WithDescription("list tree types that can be planted")
                .HandleWith((args) => this.printTreeTypeList())
                .EndSubCommand();
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
            return TextCommandResult.Success(String.Join(", ", treeTypeList));
        }
    }
}
