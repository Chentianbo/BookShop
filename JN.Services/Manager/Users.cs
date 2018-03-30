using JN.Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
namespace JN.Services.Manager
{
    public partial class Users
    {
        #region 会员激活操作
        public static void UpdateAccountSate(Data.User onUser, AccountState state)
        {
            var cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparam", MvcCore.Extensions.CacheTimeType.ByHours, 24, x => x.ID < 10000);
            //更新激活标记
            onUser.AccountState =(int)state;
            onUser.UpdateAccountStateTime = DateTime.Now;
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }
        #endregion
    }
}