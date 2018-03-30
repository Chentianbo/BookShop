using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Enum
{
    public enum BookState
    {
        [Description("待审核")]
        Wait = 0,
        [Description("已上架")]
        Grounding = 1,
        [Description("已下架")]
        UnderCarriage = -1,
        [Description("禁售")]
        Prohibit = -2,
        [Description("异常")]
        Bbnormal = -3,

    }
}
