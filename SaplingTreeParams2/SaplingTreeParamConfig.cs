using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaplingTreeParams2
{
    public sealed class SaplingTreeParamConfig
    {
        public List<SaplingParameters> saplingParameters;   
        private static SaplingTreeParamConfig instance = null;
        private static readonly object padlock = new object();
        public SaplingTreeParamConfig(){}
        public static SaplingTreeParamConfig Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SaplingTreeParamConfig();
                    }
                    return instance;
                }
            }
        }
    }
}