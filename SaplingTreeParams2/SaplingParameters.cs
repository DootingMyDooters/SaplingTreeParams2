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
        public SaplingParameters() {
            treeType = "pine";
            skipForestFloor = true;
            size = 1;
            otherBlockChance = 1;
            vinesGrowthChance = 0.01f;
            mossGrowthChance = 0.02f;
            ignoreColdTemp = true;
        }
        
    }
}
