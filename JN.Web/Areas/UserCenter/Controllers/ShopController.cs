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
using JN.Data.Enum;
using JN.Services.Tool;
using System.IO;

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
        private readonly IShopCarService ShopCarService;
        private readonly IBookCategoryService BookCategoryService;
        public ShopController(ISysDBTool SysDBTool, 
            IUserService UserService,
            IBookInfoService bookInfoService,
            IShopOrderService ShopOrderService,
            IShopCarService shopCarService,
            IBookCategoryService BookCategoryService,
        IActLogService ActLogService, ILogDBTool LogDBTool)
        {
            this.UserService = UserService;
            this.BookInfoService =bookInfoService;
            this.ShopOrderService = ShopOrderService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
            this.BookCategoryService = BookCategoryService;
            this.ShopCarService = shopCarService;
        }


        public ActionResult Index()
        {
            ActMessage = "商城首页";
            return View();
        }


        /// <summary>
        /// 添加图书
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewData["BookCategory"] = new SelectList(BookCategoryService.List().OrderBy(x => x.Sort).ToList(), "ID", "Name");
            ActMessage = "添加图书";
            return View();
        }

        /// <summary>
        /// 添加图书
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddBookInfo(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {

                var entity = new JN.Data.BookInfo();
                TryUpdateModel(entity, fc.AllKeys);
                if (BookInfoService.List(x => x.BookName == entity.BookName).Count() > 0)
                {
                    throw new Exception("该名称已存在");
                }
                if (string.IsNullOrEmpty(entity.BookCategoryId))
                {
                    throw new Exception("请选择图书分类");
                }
                if (string.IsNullOrEmpty(entity.BookName))
                {
                    throw new Exception("请输入图书名称");
                }
                if (string.IsNullOrEmpty(entity.Author))
                {
                    throw new Exception("请输入作者");
                }
                if (string.IsNullOrEmpty(entity.ISBN))
                {
                    throw new Exception("请输入图书ISBN码");
                }
                if (entity.PrintDate == null)
                {
                    throw new Exception("请输入印刷日期");
                }
                if (entity.OlaPrice <= 0)
                {
                    throw new Exception("请正确输入原价");
                }
                if (entity.CurrentPrice <= 0)
                {
                    throw new Exception("请正确输入售价");
                }
                if (entity.CurrentPrice < 0)
                {
                    throw new Exception("请正确输入运费");
                }
                HttpPostedFileBase file = Request.Files["ImageUrl"];
                string imgurl = "";
                if (file != null)
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                        return View("Error");
                    }
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    var fileName = Path.Combine(Request.MapPath("~/upfile"), newfilename);
                    try
                    {
                        file.SaveAs(fileName);
                        imgurl = "/upfile/" + newfilename;
                    }
                    catch
                    {
                        throw new Exception("封面上传失败");
                    }
                    entity.ImageUrl = imgurl;
                }
                entity.BookState = (int)BookState.Wait;
                entity.UID = Umodel.ID;
                entity.UserName = Umodel.UserName;
                //加密
                entity.CreateSign();
                BookInfoService.Add(entity);
                SysDBTool.Commit();

                result.Message = "发布成功";
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "发布失败！";
                result.Message = ex.Message;
                result.Status = 500;
            }
            return Json(result);
        }

        /// <summary>
        /// 个人购物车
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopCar(int ?page=1)
        {
            var list = ShopCarService.List(x => x.UID == Umodel.ID).ToList();
            return View(list);
        }

        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
       public ActionResult AddShopCar(string id,int amount=1)
       {
            ReturnResult result = new ReturnResult();
            try
            {
                if(string.IsNullOrEmpty(id))
                {
                    throw new Exception("添加失败");
                }
                var book = BookInfoService.Single(id);
                if (book==null)
                {
                    throw new Exception("数据不存在");
                }
                if (book.BookCount< amount)
                {
                    throw new Exception("图书数量不足 "+ amount);
                }
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //已存在 相同图书
                    var shopcar = ShopCarService.Single(x=>x.BookID==id);
                    if (shopcar!=null)
                    {
                        shopcar.Amount += amount;
                        ShopCarService.Update(shopcar);
                    }
                    else
                    {
                        var order = new Data.ShopCar();
                        order.Amount = amount;
                        order.BookID = book.ID;
                        order.UID = Umodel.ID;
                        order.BookName = book.BookName;
                        order.BookPrice = book.CurrentPrice;
                        order.CreateTime = DateTime.Now;
                        order.UserName = Umodel.UserName;
                        order.TotalPrice = order.Amount * order.BookPrice;
                        ShopCarService.Add(order);
                    }
                    SysDBTool.Commit();
                    ts.Complete();
                    result.Status = 200;
                    result.Message = "加入成功";

                }
            }
            catch (Exception ex)
            {

                result.Status=500;
                result.Message = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 购买
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
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
                        //ViewBag.Province = lastOrder.Province;
                        //ViewBag.City = lastOrder.City;
                        //ViewBag.County = lastOrder.Town;
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
        
        /// <summary>
        /// 购买图书
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
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
                            //order.City = city;
                            order.OrderNumber = orderNumber;
                            //order.Province = province;
                            order.RecAddress = RecAddress;
                            order.RecLinkMan = RecLinkMan;
                            order.RecPhone = RecPhone;
                            order.BookID = product.ID;
                            order.BookName = product.BookName;
                            order.Status = (int)Data.Enum.OrderStatus.AlreadyPaid;
                            order.ShipFreight = product.FreightPrice;
                            order.TotalCount = Convert.ToInt32(BuyNumber);
                            order.TotalPrice = (product.CurrentPrice + (product.FreightPrice ?? 0));
                            //order.Town = county;
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

        /// <summary>
        /// 生成真实订单号
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 检查订单号是否重复
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
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
                if (model.Status == (int)Data.Enum.OrderStatus.AlreadyShipped)
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
                                model.Status = (int)JN.Data.Enum.OrderStatus.Completed;
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
