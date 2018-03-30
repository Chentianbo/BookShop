using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data.Service;
using MvcCore.Controls;
using Webdiyer.WebControls.Mvc;
using JN.Data.Extensions;
using JN.Services.Tool;
using JN.Services.Manager;
using System.IO;
using System.Text.RegularExpressions;
using JN.Data.Enum;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class UserController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;
      
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;

        public UserController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService, ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 10000).ToList();

        }


        #region 修改个人资料
        public ActionResult Modify()
        {
            ViewBag.Title = "修改个人资料";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }

        [HttpPost]
        public JsonResult Modify(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = UserService.SingleAndInit(Umodel.ID);
                var onUser = entity.ToModel<Data.User>();

                //20160711安全更新 ---------------- start
                if (!string.IsNullOrEmpty(fc["username"]) || !string.IsNullOrEmpty(fc["wallet2001"]) || !string.IsNullOrEmpty(fc["wallet2002"]) || !string.IsNullOrEmpty(fc["wallet2003"]) || !string.IsNullOrEmpty(fc["wallet2004"]) || !string.IsNullOrEmpty(fc["wallet2005"]) || !string.IsNullOrEmpty(fc["IsAgent"]))
                {
                    var wlog = new Data.WarningLog();
                    wlog.CreateTime = DateTime.Now;
                    wlog.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        wlog.Location = Request.UrlReferrer.ToString();
                    wlog.Platform = "会员";
                    wlog.WarningMsg = "试图在修改资料时篡改数据(试图篡改钱包等敏感数据)";
                    wlog.WarningLevel = "严重";
                    wlog.ResultMsg = "拒绝";
                    wlog.UserName = Umodel.UserName;
                    MvcCore.Unity.Get<IWarningLogService>().Add(wlog);
                    LogDBTool.Commit();

                    Umodel.AccountState = (int)AccountState.Lock;
                    Umodel.LastUpdateTime = DateTime.Now;
                    Umodel.UpdateStateReason = "试图在修改资料时篡改数据(详情查看日志)";
                    MvcCore.Unity.Get<IUserService>().Update(Umodel);
                    MvcCore.Unity.Get<ISysDBTool>().Commit();
                    throw new Exception("非法数据请求，您的帐号已被冻结");
                }
                //20160711安全更新  --------------- end

                string vierypassword = fc["vierypassword"];
                if (Umodel.Password2 != vierypassword.ToMD5().ToMD5()) throw new Exception("安全密码不正确");
                if (Umodel.AccountState == (int)AccountState.Lock) throw new Exception("您的帐号受限，无法进行相关操作");
                TryUpdateModel(entity, fc.AllKeys);
                UserService.Update(entity);
                SysDBTool.Commit();

                string msg = "";
                if (onUser.Mobile != entity.Mobile) msg += "手机变更：" + onUser.Mobile + " => " + entity.Mobile;
                if (onUser.RealName != entity.RealName) msg += "　姓名变更：" + onUser.RealName + " => " + entity.RealName;
                if (onUser.AliPay != entity.AliPay) msg += "　支付宝变更：" + onUser.AliPay + " => " + entity.AliPay;

                var wlog2 = new Data.WarningLog();
                wlog2.CreateTime = DateTime.Now;
                wlog2.IP = Request.UserHostAddress;
                if (Request.UrlReferrer != null)
                    wlog2.Location = Request.UrlReferrer.ToString();
                wlog2.Platform = "会员";
                wlog2.WarningMsg = "会员自主修改资料，验证成功" + (!string.IsNullOrEmpty(msg) ? ",涉及改动信息：" + msg : "");
                wlog2.WarningLevel = "一般";
                wlog2.ResultMsg = "通过";
                wlog2.UserName = entity.UserName;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                LogDBTool.Commit();

                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult ChangePassword(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string vierypassword = form["vierypassword"];
                string newpassword = form["newpassword"];
                string connewpassword = form["connewpassword"];
                var entity = UserService.SingleAndInit(Umodel.ID);
                if (string.IsNullOrEmpty(newpassword.Trim()) || string.IsNullOrEmpty(connewpassword.Trim())) throw new Exception("新登录密码与确认新登录密码不能为空");
                if (newpassword != connewpassword) throw new Exception("新登录密码与确认新登录密码不相符");
                if (Umodel.Password2 != vierypassword.ToMD5().ToMD5()) throw new Exception("安全密码不正确");

                entity.Password = newpassword.ToMD5().ToMD5();
                UserService.Update(entity);
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

        [HttpPost]
        public JsonResult ChangePassword2(FormCollection form)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string vierypassword = form["safepwd"];
                string newpassword2 = form["newpassword2"];
                string connewpassword2 = form["connewpassword2"];
                var entity = UserService.SingleAndInit(Umodel.ID);
                if (string.IsNullOrEmpty(newpassword2.Trim()) || string.IsNullOrEmpty(connewpassword2.Trim())) throw new Exception("新安全密码与确认新安全密码不能为空");
                if (newpassword2 != connewpassword2) throw new Exception("新安全密码与确认新安全密码不相符");
                if (Umodel.Password2 != vierypassword.ToMD5().ToMD5()) throw new Exception("旧安全密码不正确");

                entity.Password2 = newpassword2.ToMD5().ToMD5();
                UserService.Update(entity);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
        #endregion


        #region 个人信息
        public ActionResult MyInfo()
        {
            ViewBag.Title = "个人信息";
            ActMessage = ViewBag.Title;
            return View(Umodel);
        }
        #endregion

        #region 验证会员信息(返回JSON)
        /// <summary>
        /// 验证会员信息(返回JSON)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult CheckUserInfo(string username)
        {
            var model = UserService.Single(x => x.UserName == username);
            if (model != null)
            {
                string okmsg = "";// "会员编号：" + model.UserName + "";
                //if (model.RealName.Length > 0)
                //    okmsg += "，姓名：*" + model.RealName.Substring(1, model.RealName.Length - 1);

                // if (model.Mobile.Length > 7)
                //     okmsg += "，电话：" + model.Mobile.Substring(0,3) + "****" + model.Mobile.Substring(7, model.Mobile.Length - 7);

                return Json(new {result = "ok", refMsg = "会员验证成功" + okmsg}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "err", refMsg = "该用户不存在" }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult CheckUserName(string username)
        {
            var model = UserService.Single(x => x.UserName == username);
            if (model != null)
            {
                string okmsg = "会员编号已被使用";
                return Json(new { result = "err", refMsg = okmsg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = "ok", refMsg = "恭喜您，该会员编号可以使用" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion
    }
}
