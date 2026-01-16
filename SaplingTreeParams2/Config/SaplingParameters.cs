
namespace SaplingTreeParams2.Config
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
            treeType = saplingParameters.treeType;
            skipForestFloor = saplingParameters.skipForestFloor;
            size = saplingParameters.size;
            otherBlockChance = saplingParameters.otherBlockChance;
            vinesGrowthChance = saplingParameters.vinesGrowthChance;
            mossGrowthChance = saplingParameters.mossGrowthChance;
            ignoreColdTemp = saplingParameters.ignoreColdTemp;
        }

        public string prettyString()
        {
            return "(\n\ttreeType: " + treeType +
                "\n\tskipForestFloor: " + skipForestFloor +
                "\n\tsize: " + size +
                "\n\totherBlockChance: " + otherBlockChance +
                "\n\tvinesGrowthChance: " + vinesGrowthChance +
                "\n\tmossGrowthChance: " + mossGrowthChance +
                "\n\tignoreColdTemp: " + ignoreColdTemp +
                "\n)";
        }
    }
}
