using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 跳转到商城首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return RedirectToAction("Shopping", "UserCenter");
        }

        /// <summary>
        /// 错误页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// 商城错误页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopError()
        {
            return View();
        }
    }
}