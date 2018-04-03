using JN.Data.Service;
using JN.Services.Manager;
using JN.Services.Tool;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        private static int[] Chartdate = new int[] { 1, 2, 3,4,5,6,7,8,9,10,11,12};

        private readonly IUserService UserService;
        private readonly IBookInfoService BookInfoService;
        private readonly IShopOrderService ShopOrderService;
        private readonly IShopCarService ShopCarService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly IBookCategoryService BookCategoryService;

        public HomeController(ISysDBTool SysDBTool,
            IUserService UserService,
            IBookInfoService bookInfoService,
            IShopOrderService ShopOrderService,
            IShopCarService ShopCarService,
            IBookCategoryService bookCategoryService,
        IActLogService ActLogService, ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.BookInfoService = bookInfoService;
            this.ShopOrderService = ShopOrderService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            this.BookCategoryService = bookCategoryService;
            this.ShopCarService = ShopCarService;
        }
        //
        // GET: /AdminCenter/Index/
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult NoAuthority()
        {
            return View();
        }

        //退出
        public ActionResult Logout()
        {
            ActMessage = "退出系统";
            Services.AdminLoginHelper.AdminUserLogout();
            return Redirect(Url.Action("Index", "Login"));
        }

        public ActionResult ClearAllCache()
        {
            List<String> caches = MvcCore.Extensions.CacheExtensions.GetAllCache();
            foreach (var cachename in caches)
                MvcCore.Extensions.CacheExtensions.ClearCache(cachename);
            List<String> caches2 = Services.Tool.DataCache.GetAllCache();
            foreach (var cachename in caches2)
                Services.Tool.DataCache.ClearCache(cachename);
            Services.AdminLoginHelper.AdminUserLogout();
            return Redirect(Url.Action("Index", "Home"));
        }
        public ActionResult ChangePassword()
        {
            ActMessage = "修改密码";
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string oldpassword = fc["oldpassword"];
                string newpassword = fc["newpassword"];
                string confirmpassword = fc["confirmpassword"];
                string newpassword2 = fc["newpassword2"];
                string confirmpassword2 = fc["confirmpassword2"];

                if (oldpassword.Trim().Length == 0 || newpassword.Trim().Length == 0 || newpassword2.Trim().Length == 0)
                    throw new Exception("所填项不能为空");
                if (newpassword != confirmpassword) throw new Exception("新登录密码与确认登录密码不相符");
                if (newpassword != confirmpassword) throw new Exception("新二级密码与确认二级密码不相符");
                if (Amodel.Password2 != oldpassword.ToMD5().ToMD5()) throw new Exception("原二级密码不正确");

                Amodel.Password = newpassword.ToMD5().ToMD5();
                Amodel.Password2 = newpassword2.ToMD5().ToMD5();
                MvcCore.Unity.Get<Data.Service.IAdminUserService>().Update(Amodel);
                MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
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
        /// 获取用户统计数据
        /// </summary>
        /// <returns></returns>
        public JsonResult getcharUserdata()
        {
            List<int> data = new List<int>();
            List<string> date = new List<string>();
            //全部用户
            var users = UserService.List().ToList();
            for (int i = 0; i < Chartdate.Count(); i++)
            {
                date.Add(Chartdate[i]+"月");
                data.Add(users.Where(x => x.CreateTime.Month == Chartdate[i]).Count());
            }
            return Json(new { datas = data, dates = date}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取图书统计数据
        /// </summary>
        /// <returns></returns>
        public JsonResult getcharBookdata()
        {
            List<int> data = new List<int>();
            List<string> date = new List<string>();
            //全部用户
            var Data = BookInfoService.List().ToList();
            for (int i = 0; i < Chartdate.Count(); i++)
            {
                date.Add(Chartdate[i] + "月");
                data.Add(Data.Where(x => x.CreateTime.Month == Chartdate[i]).Count());
            }
            return Json(new { datas = data, dates = date }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        /// <returns></returns>
        public JsonResult getcharOrderdata()
        {
            List<int> data = new List<int>();
            List<string> date = new List<string>();
            //全部用户
            var Data = ShopOrderService.List().ToList();
            for (int i = 0; i < Chartdate.Count(); i++)
            {
                date.Add(Chartdate[i] + "月");
                data.Add(Data.Where(x => x.CreateTime.Month == Chartdate[i]).Count());
            }
            return Json(new { datas = data, dates = date }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取购物车统计数据
        /// </summary>
        /// <returns></returns>
        public JsonResult getshopCardata()
        {
            List<int> data = new List<int>();
            List<string> date = new List<string>();
            //全部用户
            var Data = ShopCarService.List().ToList();
            for (int i = 0; i < Chartdate.Count(); i++)
            {
                date.Add(Chartdate[i] + "月");
                data.Add(Data.Where(x => x.CreateTime.Month == Chartdate[i]).Count());
            }
            return Json(new { datas = data, dates = date }, JsonRequestBehavior.AllowGet);
        }
    }
}