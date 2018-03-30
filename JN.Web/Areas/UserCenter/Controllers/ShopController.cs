using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using PagedList;
using JN.Services.Manager;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class ShopController : BaseController
    {
        object obj = new object();
        private readonly IUserService UserService;


        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly IShopOrderService ShopOrderService;
        private readonly IBookInfoService BookInfoService;
        public ShopController(ISysDBTool SysDBTool, 
            IUserService UserService,
            IBookInfoService bookInfoService,
            IShopOrderService ShopOrderService,
            IActLogService ActLogService, ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.BookInfoService =bookInfoService;
            this.ShopOrderService = ShopOrderService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
        }

        public ActionResult Index()
        {
            ActMessage = "商城首页";
            return View();
        }

        public ActionResult Product(int? page, string c)
        {
            ActMessage = "所有商品";
            var list = new List<BookInfo>();
            if (string.IsNullOrEmpty(c))
            {
                list = BookInfoService.List(x => x.BookState==0).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.CreateTime).ToList();
            }
            else
            {
                list = BookInfoService.List(x => x.BookState == 0 && x.ProductCategoryId == c).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            }

            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ShopList(int? page)
        {
            ActMessage = "营业中的店铺";
            var list = BookInfoService.List(x=> x.BookState == 0).OrderByDescending(x => x.CreateTime).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult Detail(int id)
        {
            ActMessage = "商品详情";
            var entity = BookInfoService.Single(id);
            if (entity != null)
                return View(entity);
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        public ActionResult ShopDetail(int id)
        {
            ActMessage = "店铺详情";
            var entity = BookInfoService.Single(id);
            if (entity != null)
                return View(entity);
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }


        [HttpPost]
        public ActionResult Buy(FormCollection form)
        {
            string buynumber = form["txtCount"];
            string goodsid = form["goodsId"];
            var product = BookInfoService.Single(goodsid.ToInt());
            string strErr = "";
            if (buynumber.ToInt() <= 0)
                strErr += "交易数量正确 <br />";
            if (buynumber.ToInt() > product.BookCount)
                strErr += "商品库存不足 <br />";
            if (strErr != "")
            {
                ViewBag.ErrorMsg = "抱赚您填写的信息有误： <br />" + strErr + "请核实后重新提交！";
                return View("Error");
            }

            ViewBag.Title = "确认订单并支付";
            ViewBag.Path = "在线商城";
     
            if (product != null)
            {
                ViewBag.BuyNumber = buynumber.ToInt();
                ViewBag.TotalPrice = ViewBag.BuyNumber * product.CurrentPrice;
                ViewBag.Fhsm = "";// shopinfos.GetModel(product.ShopID).ReserveStr1;

                //有过购买记录
                string userid = Umodel.ID.ToString();
                if (ShopOrderService.List(x => x.UID == userid).Count() > 0)
                {
                    var lastOrder = ShopOrderService.List(x => x.UID == userid).OrderByDescending(x => x.ID).FirstOrDefault();
                    if (lastOrder != null)
                    {
                        ViewBag.Province = lastOrder.Province;
                        ViewBag.City = lastOrder.City;
                        ViewBag.County = lastOrder.Town;
                        ViewBag.RecAddress = lastOrder.RecAddress;
                        ViewBag.RecLinkMan = lastOrder.RecLinkMan;
                        ViewBag.RecPhone = lastOrder.RecPhone;
                    }
                }

                return View(product);
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        /// <summary>
        /// 我卖出的交易
        /// </summary>
        /// <param name="df"></param>
        /// <param name="dr"></param>
        /// <param name="kf"></param>
        /// <param name="kv"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Order(int? page)
        {
            ActMessage = "我的订单";
            string userid = Umodel.ID.ToString();
            var list = ShopOrderService.List(x => x.UID == userid).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }
        

        [HttpPost]
        public ActionResult Pay(FormCollection form)
        {
            string GoodsId = form["GoodsId"];
            string BuyNumber = form["BuyNumber"];
            string province = form["hidprovince"];
            string city = form["hidcity"];
            string county = form["hidcounty"];
            string RecAddress = form["RecAddress"];
            string RecLinkMan = form["RecLinkMan"];
            string RecPhone = form["RecPhone"];
            var product = BookInfoService.Single(GoodsId.ToInt());
            string strErr = "";
            decimal acceptWallet = 0;

            if (BuyNumber.ToInt() <= 0)
                strErr += "购买数量正确 <br />";

            if (String.IsNullOrEmpty(RecAddress))
                strErr += "收货地址不能为空 <br />";

            if (String.IsNullOrEmpty(RecPhone))
                strErr += "联系电话不能为空 <br />";

            if (BuyNumber.ToInt() > product.BookCount)
                strErr += "商品库存不足 <br />";

            if (strErr != "")
            {
                ViewBag.ErrorMsg = "抱赚您填写的信息有误： <br />" + strErr + "请核实后重新提交！";
                return View("Error");
            }
            acceptWallet = Umodel.UserWallet.ToDecimal();
            if (product != null)
            {
                if (acceptWallet >= product.CurrentPrice * BuyNumber.ToInt())
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        lock (obj)
                        {
                            string orderNumber = GetOrderNumber();//创建临时单号
                            var order = new Data.ShopOrder();
                            order.BuyMsg = "";
                            order.CreateTime = DateTime.Now;
                            order.City = city;
                            order.OrderNumber = orderNumber;
                            order.Province = province;
                            order.RecAddress = RecAddress;
                            order.RecLinkMan = RecLinkMan;
                            order.RecPhone = RecPhone;
                            order.BookID = product.ID;
                            order.BookName = product.BookName;
                            order.Status = (int)Data.Enum.OrderStatus.Sales;
                            order.ShipFreight = product.FreightPrice;
                            order.TotalCount = Convert.ToInt32(BuyNumber);
                            order.TotalPrice = (product.CurrentPrice + (product.FreightPrice ?? 0));
                            order.Town = county;
                            order.UID = Umodel.ID.ToString();
                            order.UserName = Umodel.UserName;
                            ShopOrderService.Add(order);

                            product.BookCount = product.BookCount-1;
                            BookInfoService.Update(product);
                            SysDBTool.Commit();

                            Wallets.changeWallet(Umodel.ID, 0 - order.TotalPrice, "购买商品");
                            ts.Complete();

                            ViewBag.mFormUrl = "/UserCenter/Shop/Order";
                            ViewBag.SuccessMsg = "交易订单“" + orderNumber + "”支付成功！";
                            ActMessage = ViewBag.SuccessMsg;
                            return View("Success");
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "您的帐号余额不足。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在或已被删除";
                return View("Error");
            }
        }

        //生成真实订单号
        public string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "M";
            result += dateTime.Year.ToString().Substring(2, 2) + dateTime.Month + dateTime.Day;//年月日
            int hour = dateTime.Hour;
            int minu = dateTime.Minute;
            int secon = dateTime.Second;
            int count = hour * 20 + minu * 10 + secon * 5;
            int lengh = count.ToString().Length;
            string temp = count.ToString();
            string bu = "";
            if (lengh < 5)//不足5位前面补0
            {
                for (int i = 1; i < 5 - lengh; i++)
                {
                    bu += "0";
                }
            }
            result += bu + temp;
            result += Services.Tool.StringHelp.GetRandomNumber(4);//4位随机数字
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private bool IsHave(string number)
        {
            return ShopOrderService.List(x => x.OrderNumber == number).Count() > 0;
        }

        /// <summary>
        /// 确认收款
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult FinishBuy(int id)
        {
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Transaction)
                {
                    if (model.UID == Umodel.ID.ToString())
                    {
                        //TShopInfo shop = shopinfos.GetModel(model.ShopID);
                        //TUser Smodel = users.GetModel(shop.UID);
                        //users.doDelivery(onUser, (float)model.BuyAmount, 2002, description);
                        using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                        {
                            lock (obj)
                            {
                                model.Status = (int)JN.Data.Enum.OrderStatus.Deal;
                                ShopOrderService.Update(model);
                                SysDBTool.Commit();
                                //给卖家打款
                                //users.doDelivery(Smodel, (float)model.TotalPrice, 2002, "商品销售收入");
                                //执行返利
                                //users.CalculateMallBonus((int)model.ID);
                                ts.Complete();
                            }
                        }

                        ActPacket = model;

                        if (Request.UrlReferrer != null)
                        {
                            ViewBag.FormUrl = Request.UrlReferrer.ToString();
                        }
                        ViewBag.SuccessMsg = "交易收货成功！";
                        ActMessage = ViewBag.SuccessMsg;
                        return View("Success");
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "当前交易状态无法进行确认。";
                    return View("Error");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }
    }
}
