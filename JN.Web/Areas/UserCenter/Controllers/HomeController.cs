using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using JN.Services.Tool;
using System.Data.Entity.SqlServer;
using JN.Services.Manager;
using System.IO;
using Webdiyer.WebControls.Mvc;
using MvcCore.Controls;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISysSettingService SysSettingService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;

        private readonly IWalletLogService WalletLogService;


        private static List<Data.SysParam> cacheSysParam = null;
        public HomeController(ISysDBTool SysDBTool,
            IUserService UserService,
            ISysSettingService SysSettingService,
            IActLogService ActLogService,
             IWalletLogService WalletLogService,
             ILogDBTool LogDBTool
            
            )
        {
            this.UserService = UserService;
            this.SysSettingService = SysSettingService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            this.WalletLogService = WalletLogService;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 10000).ToList();
        }


        //网站首页
        public ActionResult Index()
        {
            return View();
        }


        #region 留言评论
        /// <summary>
        /// 留言评论
        /// </summary>
        /// <returns></returns>
        public ActionResult SendLeavword()
        {
            string matchingno = Request["matchingno"];
            string msgcontent = Request["msgcontent"];
            if (string.IsNullOrEmpty(msgcontent))
                return Json(new { result = "error", msg = "对不起，请填写内容！" });

            var entity = new Data.LeaveWord();
            entity.CreateTime = DateTime.Now;
            entity.Content = msgcontent;
            entity.UID = Umodel.ID.ToString();
            entity.UserName = Umodel.UserName;
            entity.BookId = matchingno;
            entity.MsgType = "咨询";

            MvcCore.Unity.Get<ILeaveWordService>().Add(entity);
            SysDBTool.Commit();
            return Json(new { result = "ok", msg = "留言成功！" });
        }

        #endregion

        public ActionResult Wait()
        {
            return View();
        }

        public ActionResult Logout()
        {
            ActMessage = "会员退出";
            Services.UserLoginHelper.UserLogout();
            return Redirect("/UserCenter/Login");
        }

        #region 修改密码(登录及二级密码一起修改)
        public ActionResult ChangePassword()
        {
            ViewBag.Title = "修改密码";
            ActMessage = ViewBag.Title;
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string oldpassword = form["oldpassword"];
                string newpassword = form["newpassword"];
                string newpassword2 = form["newpassword2"];
                string connewpassword = form["connewpassword"];
                string connewpassword2 = form["connewpassword2"];

                if (oldpassword.Trim().Length == 0 || newpassword.Trim().Length == 0 || newpassword2.Trim().Length == 0)
                    throw new Exception("原二级密码、新一级密码、新二级密码不能为空");
                if (newpassword != connewpassword) throw new Exception("新一级密码与确认密码不相符");
                if (newpassword2 != connewpassword2) throw new Exception("新二级密码与确认密码不相符");
                if (Umodel.Password2 != oldpassword.ToMD5().ToMD5()) throw new Exception("原二级密码不正确");

                Umodel.Password = newpassword.ToMD5().ToMD5();
                Umodel.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(Umodel);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion
    }
}
