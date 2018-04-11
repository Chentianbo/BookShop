using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Enum
{
    public enum ShowPlace
    {
      
        [Description("热卖区")]
        HotAera = 0,
        [Description("中区")]
        MidAera = 1,
        [Description("上侧区")]
        LSideAera = 2,
        [Description("下侧区")]
        RSideAera = 3,
        [Description("幻灯片区")]
        SwichAera = 4,
    }
}
