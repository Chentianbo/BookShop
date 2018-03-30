using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using Webdiyer.WebControls.Mvc;
using JN.Services.Manager;
using System.Data.Entity.SqlServer;
using JN.Web.Areas.UserCenter.Models;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class FinanceController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null; 

        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly IWalletLogService WalletLogService;
        private readonly ILogDBTool LogDBTool;

        public FinanceController(ISysDBTool SysDBTool,
            IUserService UserService,
            IActLogService ActLogService,
            IWalletLogService WalletLogService,
            ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.WalletLogService = WalletLogService;
            this.LogDBTool = LogDBTool;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 10000).ToList();

        }

        public ActionResult PINCode()
        {
            ActMessage = "激活码";
            return View();
        }

        [HttpPost]
        public ActionResult BuyPINCode(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                //string buyNum = form["buyNum"];
                //if (Umodel.IsLock) throw new Exception("您的帐号受限，无法进行相关操作");
                //if (buyNum.ToInt() <= 0) throw new Exception("请填写正确的数量");
                //decimal totalmoney = buyNum.ToInt() * cacheSysParam.SingleAndInit(x => x.ID == 1001).Value.ToDecimal();
                //if (Umodel.Wallet2002 < totalmoney) throw new Exception("您的帐户余额不足");
                //for (int i = 0; i < buyNum.ToInt(); i++)
                //{
                //    //生成PIN码
                //    string keycode = GetPINNumber(15);
                //    var model = new Data.PINCode
                //    {
                //        CreateTime = DateTime.Now,
                //        OriginDescribe = "用户制作",
                //        Investment = 0,
                //        IsUsed = false,
                //        KeyCode = keycode,
                //        UID = Umodel.ID,
                //        UserName = Umodel.UserName,
                //        OriginUID = Umodel.ID,
                //        OriginUserName = Umodel.UserName
                //    };
                //    MvcCore.Unity.Get<IPINCodeService>().Add(model);
                //    SysDBTool.Commit();
                //    Wallets.changeWallet(Umodel.ID, 0 - totalmoney, 2002, "制作PIN码");
                //}
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #region 帐户明细列表
        public ActionResult AccountDetail(int? page)
        {

            ActMessage = "个人钱包";
            var list = WalletLogService.List(x =>x.UID == Umodel.ID.ToString()).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion
    }
}
