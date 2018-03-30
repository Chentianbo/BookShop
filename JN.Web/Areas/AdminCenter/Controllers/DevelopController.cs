using JN.Data;
using JN.Data.Service;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class DevelopController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;

        public DevelopController(ISysDBTool SysDBTool, ILogDBTool LogDBTool,
            IActLogService ActLogService)
        {
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Icons()
        {
            return View();
        }

        public ActionResult UserIcons()
        {
            return View();
        }

        public ActionResult UI()
        {
            return View();
        }

        public ActionResult Widgets()
        {
            return View();
        }
        public ActionResult AdvancedProfile()
        {
            return View();
        }

        public ActionResult SysState()
        {
            return View();
        }

   



        public ActionResult Theme(int? page)
        {
            ViewBag.Title = "会员平台主题";
            ActMessage = ViewBag.Title;

            int pageIndex = page ?? 1;
            DataSet ds = XmlDataSet.GetDataSetByXml(Server.MapPath("/Theme/Theme.xml"));

            Webdiyer.WebControls.Mvc.PagedList<DataRow> pagedlist = new Webdiyer.WebControls.Mvc.PagedList<DataRow>(ds.Tables[0].Select(), 1, 15, ds.Tables[0].Rows.Count);
            return View(pagedlist);
        }

        //public ActionResult SetTheme(string themename)
        //{
        //    ConfigHelper.UpdatetConfig("Theme", themename);

        //    ViewBag.SuccessMsg = "设置新主题“" + themename + "”";
        //    ActMessage = ViewBag.SuccessMsg;
        //    return View("Success");
        //}

        //public ActionResult LoginStyle(int? page)
        //{
        //    ViewBag.Title = "会员登录页面样式";
        //    ActMessage = ViewBag.Title;

        //    int pageIndex = page ?? 1;

        //    DataSet ds = XmlDataSet.GetDataSetByXml(Server.MapPath("/LoginStyle/Login.xml"));
        //    PagedList<DataRow> pagedlist = new PagedList<DataRow>(ds.Tables[0].Select(), 1, 15, ds.Tables[0].Rows.Count);
        //    return View(pagedlist);
        //}

        //public ActionResult SetLoginStyle(string stylename)
        //{
        //    ConfigHelper.UpdatetConfig("LoginStyle", stylename);

        //    ViewBag.SuccessMsg = "设置新登录样式“" + stylename + "”";
        //    ActMessage = ViewBag.SuccessMsg;
        //    return View("Success");
        //}

        public ActionResult Setting()
        {
            ViewBag.Title = "系统设置";
            ActMessage = ViewBag.Title;

            return View();
        }

        [HttpPost]
        public ActionResult Setting(FormCollection form)
        {
            string isdevelopmode = form["isdevelopmode"];
            string ismultilanguage = form["ismultilanguage"];
            string memberatlas = form["memberatlas"];
            string dtbonus = form["dtbonus"];
            string dtsettlementway = form["dtsettlementway"];
            string staticbonus = form["staticbonus"];
            string unoperatetime = form["unoperatetime"];
            string startunoperatetime = form["startunoperatetime"];
            string userlevel = form["userlevel"];
            string isapplyagent = form["isapplyagent"];
            string agentlevel = form["agentlevel"];
            string issubaccount = form["issubaccount"];
            string adminpath = form["adminpath"];

            string pattern = @"^[a-zA-Z0-9]+$";
            if (!Regex.Match(adminpath, pattern).Success)
            {
                ViewBag.ErrorMsg = "后台路径只能是数字和字母组成。";
                return View("Error");
            }


            //ConfigHelper.UpdatetConfig("DevelopMode", TypeConverter.StrToBool(isdevelopmode, false).ToString());
            //ConfigHelper.UpdatetConfig("AdminPath", adminpath);
            //ConfigHelper.UpdatetConfig("MultiLanguage", TypeConverter.StrToBool(ismultilanguage, false).ToString());
            //ConfigHelper.UpdatetConfig("MemberAtlas", memberatlas);
            //ConfigHelper.UpdatetConfig("DtBonus", dtbonus);
            //ConfigHelper.UpdatetConfig("DtSettlementWay", dtsettlementway);
            //ConfigHelper.UpdatetConfig("StaticBonus", staticbonus);
            //ConfigHelper.UpdatetConfig("UnOperateTime", unoperatetime);
            //ConfigHelper.UpdatetConfig("StartUnOperateTime", startunoperatetime);
            //ConfigHelper.UpdatetConfig("UserLevel", TypeConverter.StrToInt(userlevel).ToString());
            //ConfigHelper.UpdatetConfig("ApplyAgent", TypeConverter.StrToBool(isapplyagent, false).ToString());
            //ConfigHelper.UpdatetConfig("AgentLevel", TypeConverter.StrToInt(agentlevel).ToString());
            //ConfigHelper.UpdatetConfig("SubAccount", TypeConverter.StrToBool(issubaccount, false).ToString());

            ActMessage = ViewBag.SuccessMsg;
            return View("Success");
        }

        //public ActionResult Theme()
        //{
        //    ViewBag.Title = "主题";
        //    ActMessage = ViewBag.Title;

        //    return View();
        //}

        public ActionResult Err()
        {
            ViewBag.Title = "主题";
            ActMessage = ViewBag.Title;

            return View();
        }
        public ActionResult Chart()
        {
            ViewBag.Title = "主题";
            ActMessage = ViewBag.Title;

            return View();
        }
    }
}
