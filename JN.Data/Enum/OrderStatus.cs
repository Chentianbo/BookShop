using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum OrderStatus
    {
        [Description("已付款待发货")]
        AlreadyPaid = 1,

        [Description("已发货")]
        AlreadyShipped = 2,

        [Description("已收货")]
        ConfirmReception = 3,

        [Description("已完成")]
        Completed = 4,

        [Description("已取消")]
        Cancel = -1,

        [Description("退货中")]
        ReturnOut = -2,

        [Description("换货")]
        Replace = -3,
    }
}