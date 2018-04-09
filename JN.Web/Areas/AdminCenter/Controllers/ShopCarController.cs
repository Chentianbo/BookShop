using JN.Data.Service;
using MvcCore.Controls;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    /// <summary>
    /// 购物车管理
    /// </summary>
    public class ShopCarController : BaseController
    {
        private readonly IUserService UserService;
        private readonly IBookInfoService BookInfoService;
        private readonly IShopOrderService ShopOrderService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly IShopCarService ShopCarService;

        public ShopCarController(ISysDBTool SysDBTool,
            IUserService UserService,
            IBookInfoService bookInfoService,
            IShopOrderService ShopOrderService,
            IBookCategoryService bookCategoryService,
        IActLogService ActLogService, ILogDBTool LogDBTool, IShopCarService ShopCarService)
        {
            this.UserService = UserService;
            this.BookInfoService = bookInfoService;
            this.ShopOrderService = ShopOrderService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            this.ShopCarService = ShopCarService;
        }
        // GET: AdminCenter/ShopCar
        public ActionResult Index(int? page)
        {
            ActMessage = "用户购物车管理";
            var list = ShopCarService.List().WhereDynamic(FormatQueryString(System.Web.HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }

            //枚举数据
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Delete(string id)
        {
            ReturnResult result = new ReturnResult();
            var model = ShopCarService.Single(id);
            if (model == null)
            {
                result.Status = 500;
                throw new Exception("找不到数据");
            }
            else
            {
                ShopCarService.Delete(id);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "“" + model.BookName + "”已被删除！";
            }
            return Json(result);
        }
    }
}