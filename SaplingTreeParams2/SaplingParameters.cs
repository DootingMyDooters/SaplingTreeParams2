using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaplingTreeParams2
{
    public class SaplingParameters
    {
        public string treeType;
        public bool skipForestFloor;
        public float size;
        public float otherBlockChance;
        public float vinesGrowthChance;
        public float mossGrowthChance;
        public bool ignoreColdTemp;

        public SaplingParameters(
                string treeType = "pine", 
                bool skipForestFloor = true, 
                float size = 1, 
                float otherBlockChance = 1, 
                float vinesGrowthChance = 0.01f, 
                float mossGrowthChance = 0.02f, 
                bool ignoreColdTemp = true
            )
        {
            this.treeType = treeType;
            this.skipForestFloor = skipForestFloor;
            this.size = size;
            this.otherBlockChance = otherBlockChance;
            this.vinesGrowthChance = vinesGrowthChance;
            this.mossGrowthChance = mossGrowthChance;
            this.ignoreColdTemp = ignoreColdTemp;
        }

        public void SetSaplingParameters(SaplingParameters saplingParameters)
        {
            this.treeType = saplingParameters.treeType;
            this.skipForestFloor = saplingParameters.skipForestFloor;
            this.size = saplingParameters.size;
            this.otherBlockChance = saplingParameters.otherBlockChance;
            this.vinesGrowthChance = saplingParameters.vinesGrowthChance;
            this.mossGrowthChance = saplingParameters.mossGrowthChance;
            this.ignoreColdTemp = saplingParameters.ignoreColdTemp;
        }

        public String prettyString()
        {
            return "(\n\ttreeType: " + this.treeType +
                "\n\tskipForestFloor: " + this.skipForestFloor +
                "\n\tsize: " + this.size +
                "\n\totherBlockChance: " + this.otherBlockChance +
                "\n\tvinesGrowthChance: " + this.vinesGrowthChance +
                "\n\tmossGrowthChance: " + this.mossGrowthChance +
                "\n\tignoreColdTemp: " + this.ignoreColdTemp +
                "\n)";
        }
    }
}
