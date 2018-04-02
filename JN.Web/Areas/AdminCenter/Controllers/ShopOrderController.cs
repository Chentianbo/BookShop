using JN.Data.Service;
using JN.Services.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    /// <summary>
    /// 用户订单管理
    /// </summary>
    public class ShopOrderController : BaseController
    {
        private readonly IShopOrderService _shopOrderService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly IBookInfoService BookInfoService;
        public ShopOrderController(ISysDBTool sysDBTool,ILogDBTool logDBTool,IShopOrderService shopOrderService, IBookInfoService BookInfoService, IActLogService ActLogService)
        {
            this._shopOrderService = shopOrderService;
            this.BookInfoService = BookInfoService;
            this.SysDBTool = sysDBTool;
            this.LogDBTool = logDBTool;
            this.ActLogService = ActLogService;
        }
        // GET: AdminCenter/ShopOrder
        public ActionResult Index()
        {
            return View();
        }

        #region 订单管理
        /// <summary>
        /// 订单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult Order(int? page, int? status)
        {
            ActMessage = "订单管理";
            var list = _shopOrderService.List(x => x.Status == (status ?? 1)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 确认收款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult doFinishOrder(int id)
        {
            var model = _shopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Transaction)
                {

                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        model.Status = (int)JN.Data.Enum.OrderStatus.Deal;
                        _shopOrderService.Update(model);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    //ActPacket = model;
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "确认收货成功！";
                    return View("Success");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        /// <summary>
        /// 中止交易
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CancelOrder(int id)
        {
            var model = _shopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Sales)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        var model2 = _shopOrderService.Single(x => x.OrderNumber == model.OrderNumber);
                        if (model2 != null)
                        {
                            var book = BookInfoService.Single(model2.BookID);
                            book.BookCount += model.TotalCount;
                            BookInfoService.Update(book);
                        }
                        model.Status = (int)Data.Enum.OrderStatus.Cancel;
                        _shopOrderService.Update(model);
                        SysDBTool.Commit();

                        Wallets.changeWallet(model.UID, model.TotalPrice, "商城退款");
                        ts.Complete();
                    }
                    //退款

                    ActPacket = model;
                    if (Request.UrlReferrer != null)
                    {
                        ViewBag.FormUrl = Request.UrlReferrer.ToString();
                    }
                    ViewBag.SuccessMsg = "成功取消订单！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易状态无法取消。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult doDeliver(int id, string logistics)
        {
            var model = _shopOrderService.Single(id);
            if (model != null)
            {
                model.Logistics = logistics;
                model.Status = (int)Data.Enum.OrderStatus.Transaction;
                _shopOrderService.Update(model);
                SysDBTool.Commit();
                ActMessage = "订单发货成功！";
                return Content("ok");
            }
            return Content("Error");
        }
        #endregion
    }
}