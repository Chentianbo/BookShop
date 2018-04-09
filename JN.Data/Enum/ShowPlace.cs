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
        [Description("中区")]
        MidAera = 0,
        [Description("热卖区")]
        HotAera = 1,
        [Description("左侧区")]
        LSideAera = 2,
        [Description("右侧区")]
        RSideAera = 3,
        [Description("幻灯片区")]
        SwichAera = 4,
    }
}
