using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractTests.Model
{
    public class Configuration
    {
        public string SandboxAcc1Mnemonic { get; set; }
        public string SandboxAcc2Mnemonic { get; set; }
        public ulong? AssetSell { get; set; }
        public ulong? AssetBuy { get; set; }
    }
}
