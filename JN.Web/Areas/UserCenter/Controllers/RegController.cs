using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System.Web.Security;
using JN.Data.Enum;
using JN.Services;
using JN.Services.Tool;
using System.Text.RegularExpressions;
using JN.Services.Manager;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class RegController : Controller
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;

        public RegController(ISysDBTool SysDBTool, IUserService UserService, IActLogService ActLogService, ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 10000).ToList();
        }
        public ActionResult Index()
        {
            string username = Request["r"];
            var sysEntity = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sysEntity.IsRegisterOpen)
            {
                Response.Write(sysEntity.CloseRegisterHint);
                Response.End();
            }

            if (!string.IsNullOrEmpty(username))
                ViewBag.username = username;
            return View();
        }


        #region 添加会员

        /// <summary>
        /// 添加会员
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult index(FormCollection fc)
        {
            string vcode = (Session["UserValidateCode"] ?? "").ToString();
            Session.Remove("UserValidateCode");
            string code = fc["code"];
            ReturnResult result = new ReturnResult();
            try
            {
                //20160711安全更新  --------------- end
                var sysEntity = MvcCore.Unity.Get<ISysSettingService>().Single(1);
                if (string.IsNullOrEmpty(vcode) || string.IsNullOrEmpty(code) || !vcode.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("验证码错误");

                var entity = new Data.User();
                TryUpdateModel(entity, fc.AllKeys);
                if (sysEntity.RegistNeedSMSVerification ?? false)
                {
                    string ageUser = fc["ageUser"];
                    if (Session["GetSMSUser"] != null && Session["SMSValidateCode"] != null)
                    {
                        if (Session["GetSMSUser"].ToString() != entity.Mobile.Trim() || Session["SMSValidateCode"].ToString() != ageUser)
                        {
                            throw new Exception("手机验证码错误");
                        }
                    }
                    else
                    {
                        throw new Exception("手机验证码错误");
                    }
                }
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new Exception("用户名只能为字母、数字和下划线");
                if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2)) throw new Exception("登录密码、二级密码不能为空");
                if (fc["password"] != fc["confirmpassword"]) throw new Exception("登录密码与确认密码不相符");
                if (fc["password2"] != fc["confirmpassword2"]) throw new Exception("二级密码与确认二级密码不相符");
                if (string.IsNullOrEmpty(entity.UserName)) throw new Exception("会员编号不能为空");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new Exception("用户名已被使用");


                if (string.IsNullOrEmpty(entity.NickName) && sysEntity.RegistNotNullItems.Contains(",NickName,")) throw new Exception("昵称不能为空");
                if (string.IsNullOrEmpty(entity.RealName) && sysEntity.RegistNotNullItems.Contains(",RealName,")) throw new Exception("真实姓名不能为空");
                if (string.IsNullOrEmpty(entity.Mobile) && sysEntity.RegistNotNullItems.Contains(",Mobile,")) throw new Exception("手机号码不能为空");
                if (string.IsNullOrEmpty(entity.Email) && sysEntity.RegistNotNullItems.Contains(",Email,")) throw new Exception("邮箱不能为空");
                if (string.IsNullOrEmpty(entity.IDCard) && sysEntity.RegistNotNullItems.Contains(",IDCard,")) throw new Exception("身份证号码不能为空");
                if (string.IsNullOrEmpty(entity.AliPay) && sysEntity.RegistNotNullItems.Contains(",AliPay,")) throw new Exception("昵称不能为空");
                if (string.IsNullOrEmpty(entity.Question) && sysEntity.RegistNotNullItems.Contains(",Question,")) throw new Exception("取回密码问题不能为空");
                if (string.IsNullOrEmpty(entity.Answer) && sysEntity.RegistNotNullItems.Contains(",Answer,")) throw new Exception("取回密码答案不能为空");

                if (entity.Mobile.Length != 11) throw new Exception("请输入11位数手机号码");  

                if (UserService.List(x => x.NickName == entity.NickName.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",NickName,")) throw new Exception("昵称已被使用");
                if (UserService.List(x => x.Mobile == entity.Mobile.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",Mobile,")) throw new Exception("手机号码已被使用");
                
                
                ////mrc 17-04-22添加
                if (UserService.List(x => x.Mobile == entity.UserName.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",Mobile,")) throw new Exception("当前用户名不可使用");
                /////////////////

                if (UserService.List(x => x.Email == entity.Email.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",Email,")) throw new Exception("邮箱已被使用");
                if (UserService.List(x => x.IDCard == entity.IDCard.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",IDCard,")) throw new Exception("身份证号码已被使用");
                if (UserService.List(x => x.AliPay == entity.AliPay.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",AliPay,")) throw new Exception("支付宝帐号已被使用");
                if (UserService.List(x => x.WeiXin == entity.WeiXin.Trim()).Count() > 0 && sysEntity.RegistOnlyOneItems.Contains(",WeiXin,")) throw new Exception("微信帐号已被使用");
                entity.AccountState=(int)AccountState.Normal;
                entity.UpdateAccountStateTime = DateTime.Now;
                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                entity.CreateTime = DateTime.Now;
                entity.UpdateStateReason = "用户注册";
                UserService.Add(entity);
                SysDBTool.Commit();

                if (MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4502).Value == "1")
                {
                    SMSHelper.CYmsm(entity.Mobile, MvcCore.Unity.Get<ISysParamService>().SingleAndInit(x => x.ID == 4502).Value2.Replace("{SYSNAME}", sysEntity.SysName).Replace("{USERNAME}", entity.UserName));
                }
                    

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


        public ActionResult SendMobileMsm()
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string phone = Request["myphone"];
                if (string.IsNullOrEmpty(phone)) throw new Exception("请您填写手机号码");
                if (!StringHelp.IsPhone(phone)) throw new Exception("请输入正确的手机号码");

                if (Session["SMSSendTime"] != null)
                {
                    if (!DateTimeDiff.DateDiff_minu(DateTime.Parse(Session["SMSSendTime"].ToString())))
                        throw new Exception("每次发送短信间隔不能少于1分钟");
                }

                //ValidateCode vEmailCode = new ValidateCode();
                //string smscode = vEmailCode.CreateRandomCode(5);
                CodeRend vEmailCode = new CodeRend();
                string smscode = vEmailCode.CreateVerifyCode(5);
                Session["SMSValidateCode"] = smscode;
                Session["GetSMSUser"] = phone;
                Session["SMSSendTime"] = DateTime.Now;
                bool b = Services.Tool.SMSHelper.CYmsm(phone, "您本次操作的验证码为：" + smscode + "");
                if (b)  
                    result.Status = 200;
                else
                {
                    Session["SMSValidateCode"] = null;
                    Session["GetSMSUser"] = null;
                    result.Message = "短信发送失败,详情请查阅发送记录";
                    result.Status = 500;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
    }
}
