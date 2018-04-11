using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System.Collections.Specialized;
using System.Web.Security;
using JN.Data.Enum;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class BaseController : Controller
    {
        public Data.User Umodel { get; set; }
        public object ActPacket { get; set; }
        public string ActMessage { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!sys.IsOpenUp)
            {
                Response.Write(sys.CloseHint);
                Response.End();
            }

            //设置语言版本
            string lang = Request["lang"];
            if (!string.IsNullOrEmpty(lang))
            {
                JN.Web.Framework.ResourceProvider.Culture = lang;
            }

            //超管登录
            string adminloginname = Request["adminloginname"];
            if (!string.IsNullOrEmpty(adminloginname))
            {
                var adminEntity = Services.AdminLoginHelper.CurrentAdmin();
                if (adminEntity != null && adminEntity.IsPassed)
                {
                    var userEntity = MvcCore.Unity.Get<IUserService>().Single(x => x.UserName == adminloginname);
                    if (userEntity != null)
                    {
                        var newUserEntity = Services.UserLoginHelper.GetSysLoginBy(userEntity, adminEntity);
                    }
                }
                else
                {
                    Response.Redirect("/UserCenter/Shopping");
                    filterContext.Result = new EmptyResult();
                }
            }
            //校验用户是否已经登录
            var model = Services.UserLoginHelper.CurrentUser();

            if (model != null)
            {
                Umodel = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(model.ID);

                string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
                if (Umodel.AccountState!= (int)AccountState.Normal)
                {
                    Response.Redirect("/UserCenter/Shopping");
                    filterContext.Result = new EmptyResult();
                }
            }
            else
            {
                Response.Redirect("/UserCenter/Shopping");
                filterContext.Result = new EmptyResult();
            }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            ViewBag.Title = ActMessage;
            var model = Services.UserLoginHelper.CurrentUser();
            if (model != null)
            {
                Umodel = model;
                if (!string.IsNullOrEmpty(ActMessage))
                {
                    string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
                    string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();

                    //记录操作日志，写进操作日志中
                    var log = new Data.ActLog();
                    log.ActContent = ActMessage;
                    log.CreateTime = DateTime.Now;
                    log.IP = Request.UserHostAddress;
                    if (Request.UrlReferrer != null)
                        log.Location = Request.UrlReferrer.ToString();
                    if (ActPacket != null)
                        log.PacketFile = Newtonsoft.Json.JsonConvert.SerializeObject(ActPacket);
                    else
                        log.PacketFile = "";
                    log.Platform = "会员";
                    log.Source = "";
                    log.UID = model.ID.ToString();
                    log.UserName = model.UserName;
                    MvcCore.Unity.Get<JN.Data.Service.IActLogService>().Add(log);

                    if (ViewBag.mFormUrl != null)
                        ViewBag.FormUrl = ViewBag.mFormUrl;
                    else
                    {
                        if (Request.UrlReferrer != null)
                            ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                }
            }
            else
            {
                Response.Redirect("/UserCenter/Shopping");
                filterContext.Result = new EmptyResult();
            }
        }

        protected NameValueCollection FormatQueryString(NameValueCollection nameValues)
        {
            NameValueCollection nvcollection = new NameValueCollection();
            foreach (var item in nameValues.AllKeys)
            {
                if (item.Equals("df"))
                {
                    string daterange = nameValues["dr"];
                    if (!String.IsNullOrEmpty(daterange))
                    {
                        if (daterange.Contains('-'))
                        {
                            DateTime startDate = daterange.Split('-')[0].Trim().ToDateTime().AddSeconds(-1);
                            DateTime endDate = daterange.Split('-')[1].Trim().ToDateTime().AddDays(1);
                            nvcollection.Add(nameValues[item] + ">", startDate.ToString());
                            nvcollection.Add(nameValues[item] + "<", endDate.ToString());
                        }
                    }
                }
                else if (item.Equals("kf"))
                {
                    if (!string.IsNullOrEmpty(nameValues["kv"]))
                        nvcollection.Add(nameValues[item], nameValues["kv"]);
                }
                else if (item.Equals("isasc") || !string.IsNullOrEmpty(nameValues["isasc"]))
                    nvcollection.Add(item, nameValues["isasc"]);
                else if (item.Equals("orderfiled") || !string.IsNullOrEmpty(nameValues["orderfiled"]))
                    nvcollection.Add(item, nameValues["orderfiled"]);
            }

            return nvcollection;
        }

        public ActionResult showmsg(string msg)
        {
            ViewBag.Title = "提示！";
            ViewBag.url = "/usercenter/home";
            if (Request.UrlReferrer != null)
                ViewBag.url = Request.UrlReferrer.ToString();
            ViewBag.msg = msg;
            return View("msg");
        }
    }
}
