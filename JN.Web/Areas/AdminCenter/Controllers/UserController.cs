using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Data.Extensions;
using JN.Data.Enum;
using JN.Services.Manager;
using System.Text.RegularExpressions;
using JN.Services.Tool;
using System.Data.Entity.SqlServer;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService UserService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly ISysSettingService SysSettingService;
        private static List<Data.SysParam> cacheSysParam = null;
        public UserController(
            ISysDBTool SysDBTool,
            IUserService UserService,
            ISysSettingService SysSettingService,
            IActLogService ActLogService,
            ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.SysDBTool = SysDBTool;
            this.SysSettingService = SysSettingService;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            cacheSysParam = MvcCore.Unity.Get<ISysParamService>().ListCache("sysparams", x => x.ID < 10000).ToList();
        }
        //用户管理首页
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult _PartialAddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult _PartialAddUser(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = new Data.User();
                TryUpdateModel(entity, fc.AllKeys);
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$")) throw new Exception("用户名只能为字母、数字和下划线");
                if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.RealName) || string.IsNullOrEmpty(entity.Mobile)) throw new Exception("会员编号、真实姓名、手机号码不能为空");
                if (string.IsNullOrEmpty(entity.Email)) throw new Exception("电子邮箱不能为空");

                if (string.IsNullOrEmpty(entity.Password) || string.IsNullOrEmpty(entity.Password2)) throw new Exception("一级密码、二级密码不能为空");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0) throw new Exception("用户名已被使用");

                entity.AccountState = (int)AccountState.Normal;
                entity.UpdateAccountStateTime = DateTime.Now;
                entity.UpdateStateReason = "后台添加";
                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                entity.CreateTime = DateTime.Now;

                UserService.Add(entity);
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

        /// <summary>
        /// 用户list
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult List(int? page)
        {
            //动态构建查询
            var list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.CreateTime).ToList();
            list = UserService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.CreateTime).ToList();
            string sign = "";
            foreach (var item in list)
            {
                sign = (item.UserName + item.StudentNumber + item.Password + item.Password2 + item.UserWallet + item.AddupTakeWallet + item.AddupIncomeWallet + item.AddupPurchaseCount + item.AddupSelloutCount+ item.Mobile).ToMD5();
                if (sign!=item.Sign)
                {
                    item.AccountState =(int) AccountState.Exceptiona;
                    item.UpdateAccountStateTime = DateTime.Now;
                    item.UpdateStateReason="账号涉及非法修改";
                    UserService.Update(item);
                    SysDBTool.Commit();
                }
            }
            string isactivation = Request["AccountSate"];
            if (isactivation=="all")
            {
                list = list.ToList();
            }
            else
            {
                int state = Convert.ToInt32(isactivation);
                list = list.Where(x => x.AccountState == state).ToList();
            }
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToList().ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 修改用户资料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Modify(string id)
        {
            ActMessage = "编辑会员资料";
            var user = UserService.Single(x=>x.ID==id);
            return View(user);
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <returns></returns>
        public ActionResult AddUser(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = new User();
                TryUpdateModel(entity, fc.AllKeys);

                if (string.IsNullOrEmpty(entity.UserName))
                    throw new Exception("用户名不能为空");
                if (string.IsNullOrEmpty(entity.StudentNumber))
                    throw new Exception("学号不能为空");
                if (string.IsNullOrEmpty(entity.Mobile))
                    throw new Exception("联系电话不能为空");
                string resetpwd = fc["resetpassowrd"];
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$"))
                    throw new Exception("用户名只能为字母、数字和下划线");
                if (string.IsNullOrEmpty(entity.Password))
                    throw new Exception("登录密码不能为空");
                if (string.IsNullOrEmpty(entity.Password2))
                    throw new Exception("支付密码不能为空");
                if (UserService.List(x => x.UserName == entity.UserName.Trim()).Count() > 0)
                    throw new Exception("用户名已被使用");
                if (UserService.List(x => x.StudentNumber == entity.StudentNumber.Trim()).Count() > 0)
                    throw new Exception("学号已被使用");
                entity.AccountState = (int)AccountState.Normal;
                entity.LastUpdateTime = DateTime.Now;
                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password2 = entity.Password2.ToMD5().ToMD5();
                //加密串
                entity.CreateSign();
                UserService.Add(entity);
                #region 写入日志
                var wlog2 = new Data.WarningLog();
                wlog2.CreateTime = DateTime.Now;
                wlog2.IP = Request.UserHostAddress;
                if (Request.UrlReferrer != null)
                    wlog2.Location = Request.UrlReferrer.ToString();
                wlog2.Platform = "后台";
                wlog2.WarningMsg = "管理员“" + Amodel.AdminName + "”添加会员（"+ entity .UserName+ "）";
                wlog2.WarningLevel = "一般";
                wlog2.ResultMsg = "通过";
                wlog2.UserName = entity.UserName;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                LogDBTool.Commit();
                #endregion
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

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string userid = fc["ID"];
                var entity = UserService.Single(x=>x.ID== userid);
                var onUser = entity.ToModel<Data.User>();
                TryUpdateModel(entity, fc.AllKeys);
                if (string.IsNullOrEmpty(entity.UserName))
                    throw new Exception("用户名不能为空");
                if (string.IsNullOrEmpty(entity.StudentNumber))
                    throw new Exception("学号不能为空");
                if (string.IsNullOrEmpty(entity.Mobile))
                    throw new Exception("联系电话不能为空");
                if (!Regex.IsMatch(entity.UserName, @"^[A-Za-z0-9_]+$"))
                    throw new Exception("用户名只能为字母、数字和下划线");
                if (UserService.List(x => x.UserName == entity.UserName&&x.ID!=entity.ID).Count() > 0)
                    throw new Exception("用户名已被使用");
                if (UserService.List(x => x.StudentNumber == entity.StudentNumber && x.ID != entity.ID).Count() > 0)
                    throw new Exception("学号已被使用");
                string passowrd = fc["resetpassowrd"];
                if (!string.IsNullOrEmpty(passowrd))
                {
                    entity.Password = passowrd.ToMD5().ToMD5();
                }
                string passowrd2 = fc["resetpassowrd2"];
                if (!string.IsNullOrEmpty(passowrd2))
                {
                    entity.Password2 = passowrd2.ToMD5().ToMD5();
                }

                string msg = "";
                if (onUser.Mobile != entity.Mobile) msg += "手机变更：" + onUser.Mobile + " => " + entity.Mobile;
                if (onUser.RealName != entity.RealName) msg += "　姓名变更：" + onUser.RealName + " => " + entity.RealName;
                if (onUser.AliPay != entity.AliPay) msg += "　支付宝变更：" + onUser.AliPay + " => " + entity.AliPay;
                #region 写入日志
                var wlog2 = new Data.WarningLog();
                wlog2.CreateTime = DateTime.Now;
                wlog2.IP = Request.UserHostAddress;
                if (Request.UrlReferrer != null)
                    wlog2.Location = Request.UrlReferrer.ToString();
                wlog2.Platform = "后台";
                wlog2.WarningMsg = "由管理员“" + Amodel.AdminName + "”修改会员资料" + (!string.IsNullOrEmpty(msg) ? ",涉及改动信息：" + msg : "");
                wlog2.WarningLevel = "一般";
                wlog2.ResultMsg = "通过";
                wlog2.UserName = entity.UserName;
                MvcCore.Unity.Get<IWarningLogService>().Add(wlog2);
                LogDBTool.Commit();
                #endregion
                entity.CreateSign();
                entity.LastUpdateTime = DateTime.Now;
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

        /// <summary>
        /// 删除用户，未激活时才可以
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = UserService.Single(x => x.ID == id);
                if (model == null)
                {
                    throw new Exception("数据不存在");
                }
                UserService.Delete(model.ID);
                result.Status = 200;
                result.Message = "操作成功";
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = "操作失败:"+ ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 修改账号状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commandtype"></param>
        /// <returns></returns>
        public ActionResult UnLock(string id, string State)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = UserService.Single(x => x.ID == id);
                if (model == null)
                {
                    throw new Exception("数据不存在");
                }
                model.AccountState = (int)AccountState.Normal;
                model.UpdateAccountStateTime = DateTime.Now;
                UserService.Update(model);
                result.Status = 200;
                result.Message = "操作成功";
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = "操作失败:"+ ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 冻结用户账号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionResult MakeLock (string id,string reason)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = UserService.Single(x => x.ID == id);
                if (model == null)
                {
                    throw new Exception("数据不存在");
                }
                model.AccountState = (int)AccountState.Lock;
                model.UpdateStateReason = reason;
                model.UpdateAccountStateTime = DateTime.Now;
                UserService.Update(model);
                result.Status = 200;
                result.Message = "操作成功";
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = "操作失败:"+ ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 解除登陆限制
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MakeFreeLogin(string id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = UserService.Single(x => x.ID == id);
                if (model == null)
                {
                    throw new Exception("数据不存在");
                }
                model.AccountState = (int)AccountState.Lock;
                model.InputWrongPwdTimes = 0;//重置登录次数
                UserService.Update(model);
                result.Status = 200;
                result.Message = "操作成功";
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = "操作失败:" + ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 解除异常
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UnExceptiona(string id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = UserService.Single(x => x.ID == id);
                if (model == null)
                {
                    throw new Exception("数据不存在");
                }
                model.AccountState = (int)AccountState.Normal;
               
                model.UpdateStateReason = Amodel.AdminName+"解除异常";
                model.UpdateAccountStateTime = DateTime.Now;
                model.CreateSign();//重新生成秘钥
                UserService.Update(model);
                result.Status = 200;
                result.Message = "操作成功";
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = "操作失败:" + ex.Message;
            }
            return Json(result);
        }
    }
}