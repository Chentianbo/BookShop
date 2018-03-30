using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.IO;
using JN.Services.Manager;
using JN.Data.Enum;
//using System.Data.Entity.Validation;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    [ValidateInput(false)]
    public class ShopController : BaseController
    {
        private readonly IUserService UserService;
        private readonly IBookInfoService BookInfoService;
        private readonly IShopOrderService ShopOrderService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly IBookCategoryService BookCategoryService;

        public ShopController(ISysDBTool SysDBTool,
            IUserService UserService,
            IBookInfoService bookInfoService,
            IShopOrderService ShopOrderService,
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
        }

        public ActionResult ModifyShop(int? id)
        {
            ActMessage = "编辑店铺";
            if (id.HasValue)
                return View(BookInfoService.Single(id));
            else
            {
                ActMessage = "添加店铺";
                return View(new Data.User());
            }
        }

        [HttpPost]
        public ActionResult ModifyShop(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var entity = BookInfoService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);

                BookInfoService.Update(entity);
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }

        public ActionResult ShopList(int? active, int? page)
        {
            ActMessage = "店铺列表";
            int theActive = active ?? 0;
            var list = BookInfoService.List(x => x.BookState==0).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ShopStatistics(int? page)
        {
            ActMessage = "店铺销售统计";
            var list = BookInfoService.List(x => x.BookState == 0).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult ShopLock(int? islock, int? page)
        {
            int theLock = islock ?? 0;
            var list = BookInfoService.List(x => x.BookState == (int)BookState.Prohibit).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }


        public ActionResult ShopCommand(string id, string state)
        {
            try
            {
                var model = BookInfoService.Single(id);
                model.BookState = Convert.ToInt32(state);
                BookInfoService.Update(model);
                SysDBTool.Commit();
            }
            catch (Exception)
            {

                throw;
            }
         
            return RedirectToAction("ShopList", "Shop");
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
        /// 编辑商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Modify(string id)
        {
            var model = BookInfoService.Single(id);
            return View(model);
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddBookInfo(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
              
                var entity =new  JN.Data.BookInfo();
                TryUpdateModel(entity, fc.AllKeys);
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
                if (entity.PrintDate==null)
                {
                    throw new Exception("请输入印刷日期");
                }
                if (entity.OlaPrice<=0)
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
                if (file!=null)
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
                }
                entity.BookState = (int)BookState.Wait;
                entity.UId = Amodel.ID;
                entity.ImageUrl = imgurl;
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
        /// 保存商品
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            try
            {
                string id = fc["ID"];
                var entity = BookInfoService.Single(x=>x.ID== id);
                TryUpdateModel(entity, fc.AllKeys);
                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (!string.IsNullOrEmpty(file.FileName))
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
                        //
                    }
                }
                entity.ImageUrl = imgurl;
                BookInfoService.Update(entity);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "商品修改/发布成功！";
                return View("Success");
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
                return View("Error");
            }
        }

        public ActionResult Product(int? page)
        {
            ActMessage = "商品管理";
            var list = BookInfoService.List(x => x.BookState==(int)BookState.Grounding).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }

        public ActionResult OffSales(int? page)
        {
            ActMessage = "下架商品管理";
            var list = BookInfoService.List(x => x.BookState == (int)BookState.Prohibit).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            return View("Product", list.ToPagedList(page ?? 1, 20));
        }


        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {
            var model = BookInfoService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                BookInfoService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "“" + model.BookName + "”已被删除！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        /// <summary>
        /// 订单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult Order(int? page, int? status)
        {
            ActMessage = "订单管理";
            var list = ShopOrderService.List(x => x.Status == (status ?? 1)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
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
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Transaction)
                {

                    //TShopInfo shop = shopinfos.GetModel(model.ShopID);
                    //TUser Smodel = users.GetModel(shop.UID);
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        model.Status = (int)JN.Data.Enum.OrderStatus.Deal;
                        ShopOrderService.Update(model);
                        SysDBTool.Commit();
                        //给卖家打款
                        //users.changeWallet(Smodel, (float)model.TotalPrice, 2006, "商品销售收入");
                        //执行返利
                        //users.CalculateMallBonus((int)model.ID);
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
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                if (model.Status == (int)Data.Enum.OrderStatus.Sales)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        var model2 = ShopOrderService.Single(x => x.OrderNumber == model.OrderNumber);
                        if (model2 != null)
                        {
                            var book = BookInfoService.Single(model2.BookID);
                            book.BookCount += model.TotalCount;
                            BookInfoService.Update(book);
                        }
                        model.Status = (int)Data.Enum.OrderStatus.Cancel;
                        ShopOrderService.Update(model);
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
            var model = ShopOrderService.Single(id);
            if (model != null)
            {
                model.Logistics = logistics;
                model.Status = (int)Data.Enum.OrderStatus.Transaction;
                ShopOrderService.Update(model);
                SysDBTool.Commit();
                ActMessage = "订单发货成功！";
                return Content("ok");
            }
            return Content("Error");
        }


        #region 书籍分类

        #region 书籍分类列表
        /// <summary>
        /// 书记分类列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult CategoryList(int? page)
        {
            ActMessage = "图书分类管理";
            var list = BookCategoryService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderBy(x => x.Sort).ThenByDescending(x => x.CreateTime);
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }
        #endregion

        #region 删除图书分类
        /// <summary>
        /// 删除图书分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteCategory(string id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = BookCategoryService.Single(id);

                var submodel = BookCategoryService.List(x => x.ParentId == id);
                //改变下级分类 父id为0
                foreach (var item in submodel)
                {
                    item.ParentId ="0";
                    item.parentName = "";
                    BookCategoryService.Update(item);
                }
                if (model == null)
                {
                    throw new Exception("该数据不存在");
                }
                ActPacket = model;
                BookCategoryService.Delete(id);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "删除成功";
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = ex.Message;
            }
            return Json(result);
        }
        #endregion

        #region 商品分类添加
        /// <summary>
        /// 商品分类添加
        /// </summary>
        /// <returns></returns>
        public ActionResult AddBookClass()
        {
            return View();
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddBookClass(FormContext form)
        {
            try
            {
                string cateName = Request["aName"];
                string catePid = Request["catePid"];
                if (string.IsNullOrEmpty(cateName))
                {
                    throw new Exception("请输入分类名称！");
                }
                if (BookCategoryService.List(x => x.Name == cateName).Count() > 0)
                {
                    throw new Exception("分类名称已经重复！");
                }
                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (!string.IsNullOrEmpty(file.FileName))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                        return View("Error");
                    }
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    //缩略图 ------------开始
                    var fileName = Path.Combine(Request.MapPath("~/upfile/category/"), newfilename);
                    try
                    {
                        if (!Directory.Exists(Request.MapPath("~/upfile/category")))
                        {
                            Directory.CreateDirectory(Request.MapPath("~/upfile/category"));
                        }
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/upfile/category/"), 1024, 768, "EQU");
                        System.IO.File.Delete(fileName); //删除原文件
                        imgurl = "/upfile/category/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("上传失败：" + ex.Message);
                    }
                    //缩略图 ------------结束
                }
                JN.Data.BookCategory cate = new JN.Data.BookCategory();
                var pModel = BookCategoryService.Single(catePid);
                if (pModel != null)
                {
                    cate.Ppacth = pModel.Ppacth + pModel.ID + ",";
                    cate.parentName = pModel.Name;
                }
                else
                {
                    cate.Ppacth = "0,";
                }
                int sort = 1;
                var categoryList = BookCategoryService.List().ToList();
                if (categoryList != null && categoryList.Count() > 0)
                {
                    sort = categoryList.Max(x => x.Sort) + 1;
                }

                cate.Sort = sort;
                cate.CateImg = imgurl;

                cate.ParentId = catePid;
                cate.Name = cateName;
                cate.CreateTime = DateTime.Now;

                BookCategoryService.Add(cate);
                SysDBTool.Commit();

                ViewBag.SuccessMsg = "操作成功！";
                ActPacket = cate;
                return View("Success");

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View("Error");
            }
        }
        #endregion

        #region 商品分类编辑
        /// <summary>
        /// 商品分类编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyBookClass(string id)
        {
            var PCategory = BookCategoryService.Single(id);
            return View(PCategory);
        }
        [HttpPost]
        public ActionResult ModifyBookClass(FormCollection form)
        {
            try
            {
                string id = form["Id"];
                var entity = BookCategoryService.Single(x=>x.ID==id);
                var Cate = entity.ToModel<Data.BookCategory>();
                TryUpdateModel(entity, form.AllKeys);
                if (string.IsNullOrEmpty(entity.Name)) throw new Exception("分类名称不能为空！");
                string msg = "";
                if (Cate.Name != entity.Name) msg += "分类名称变更：" + entity.Name + " => " + entity.Name;

                HttpPostedFileBase file = Request.Files["imgurl"];
                string imgurl = "";
                if (!string.IsNullOrEmpty(file.FileName))
                {
                    if (!FileValidation.IsAllowedExtension(file, new FileExtension[] { FileExtension.PNG, FileExtension.JPG, FileExtension.BMP }))
                    {
                        ViewBag.ErrorMsg = "非法上传，您只可以上传图片格式的文件！";
                        return View("Error");
                    }
                    var newfilename = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
                    //缩略图 ------------开始
                    var fileName = Path.Combine(Request.MapPath("~/upfile/category/"), newfilename);
                    try
                    {
                        if (!Directory.Exists(Request.MapPath("~/upfile/category")))
                        {
                            Directory.CreateDirectory(Request.MapPath("~/upfile/category"));
                        }
                        file.SaveAs(fileName);
                        var thumbnailfilename = UploadPic.MakeThumbnail(fileName, Request.MapPath("~/upfile/category/"), 30, 30, "EQU");
                        System.IO.File.Delete(fileName); //删除原文件
                        System.IO.File.Delete(Request.MapPath(Cate.CateImg)); //删除原分类图片
                        imgurl = "/upfile/category/" + thumbnailfilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("上传失败：" + ex.Message);
                    }
                    //缩略图 ------------结束
                }
                if (imgurl != "")
                {
                    entity.CateImg = imgurl;
                }

                #region 父级分类
                string catePid = form["catePid"];
                //如果没有选中自己
                if (catePid != Cate.ID)
                {
                    var model = BookCategoryService.Single(catePid);
                    entity.Ppacth = model.Ppacth + model.ID + ",";
                    entity.parentName = model.Name;
                    entity.ParentId = catePid;
                }

                #endregion

                BookCategoryService.Update(entity);
                SysDBTool.Commit();
                //result.Status = 200;
                ViewBag.SuccessMsg = "商品分类编辑成功！";
                return View("Success");
            }
            catch (Exception ex)
            {
                //result.Message = "操作失败!";
                ViewBag.ErrorMsg = ex.Message;
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                return View("Error");
            }
            //return Json(result);
        }
        #endregion

        #region 获得商品分类

        [HttpPost]
        public JsonResult GetCategory()
        {
            string spid = Request["pId"];
            var model = BookCategoryService.List(x => x.ParentId == spid).ToList();
            var data = from a in model
                       select new
                       {
                           id = a.ID,
                           name = a.Name
                       };
            return Json(data);
        }
        #endregion

        #endregion
    }
}
