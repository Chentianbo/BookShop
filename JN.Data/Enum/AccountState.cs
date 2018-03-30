using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Enum
{
   public enum AccountState
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 0,

        /// <summary>
        /// 锁定
        /// </summary>
        [Description("锁定")]
        Lock = 1,
        /// <summary>
        /// 未激活
        /// </summary>
        [Description("未激活")]
        UnActivation = 2,
        /// <summary>
        /// 异常
        /// </summary>
        [Description("异常")]
        Exceptiona = 3,
    }
}
