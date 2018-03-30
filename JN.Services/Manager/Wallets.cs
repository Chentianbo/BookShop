using System;
using System.Collections.Generic;
using System.Linq;
namespace JN.Services.Manager
{
    /// <summary>
    ///钱包管理
    /// </summary>
    public partial class Wallets
    {

        #region 用户钱包操作操作
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onUserID">当前用户id</param>
        /// <param name="changeMoney"></param>
        /// <param name="description"></param>
        public static void changeWallet(string onUserID, decimal changeMoney, string description)
        {
            Data.User onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(x=>x.ID== onUserID);
            decimal changeWallet = 0;
            changeWallet = onUser.UserWallet.ToDecimal();
            onUser.UserWallet = onUser.UserWallet + changeMoney;

            //写入明细
            MvcCore.Unity.Get<JN.Data.Service.IWalletLogService>().Add(new Data.WalletLog
            {
                UID= onUser.ID,
                ChangeMoney = changeMoney,
                Balance = changeWallet + changeMoney,
                CoinName = "用户钱包",
                CreateTime = DateTime.Now,
                Description = description,
                UserName = onUser.UserName
            });
            MvcCore.Unity.Get<Data.Service.ILogDBTool>().Commit();
            //更新用户钱包
            onUser.CreateSign();//更新安全秘钥
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }
        #endregion
    }
}