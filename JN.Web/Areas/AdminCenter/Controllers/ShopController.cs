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
using JN.Data.Common;
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

        /// <summary>
        /// 图书列表 获取不同状态的图书
        /// </summary>
        /// <param name="page"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult BookList(int? page, int? state)
        {

            ActMessage = "图书管理";
            var list = BookInfoService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).ToList();
            if (state != null)
            {
                list = list.Where(x => x.BookState == state).ToList();
            }
            list = list.OrderByDescending(x => x.CreateTime).ToList();
            //数据验证
            foreach (var item in list)
            {
                string sign = (item.BookName + item.Author + item.ISBN + item.BookCategoryId + (Convert.ToInt32(item.CurrentPrice)).ToString() + (Convert.ToInt32(item.OlaPrice)).ToString() + item.UID
                    + item.BookState.ToString() + (Convert.ToInt32(item.FreightPrice)).ToString()).ToLower().ToMD5();
                if (sign != item.Sign)
                {

                    if (item.BookState != (int)BookState.Bbnormal)
                    {
                        item.BookState = (int)BookState.Bbnormal;
                        item.Description = "数据被非法修改";
                        BookInfoService.Update(item);
                        SysDBTool.Commit();
                    }

                }
            }
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            //枚举数据
            ViewBag.EnumData = EnumExtension.GetOptions(BookState.Wait);
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 修改图书状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult ChangeBookSate(string id, string state)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("提交出错");
                }
                if (string.IsNullOrEmpty(state))
                {
                    throw new Exception("提交出错");
                }
                if (!EnumExtension.ExitEnum((BookState)Convert.ToInt32(state)))
                {
                    throw new Exception("提交出错");
                }
                var model = BookInfoService.Single(id);
                if (model==null)
                {
                    throw new Exception("该数据不存在");
                }
                model.BookState = Convert.ToInt32(state);
                model.Description = "";
                model.CreateSign();
                BookInfoService.Update(model);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "操作成功";
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = ex.Message;
            }
            return Json(result);
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
                entity.UID = Amodel.ID;
                entity.UserName = "(管理员)"+ Amodel.AdminName;
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
        /// 编辑图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Modify(string id)
        {
            var model = BookInfoService.Single(id);
            if (model==null)
            {
                ViewBag.ErrorMsg = "抱歉，暂时找不该该数据";
                return View("Error");
            }
            ViewData["BookCategory"] = new SelectList(BookCategoryService.List().OrderBy(x => x.Sort).ToList(), "ID", "Name");
            ActMessage = "编辑图书";
            return View(model);
        }

        /// <summary>
        /// 修改保存图书
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                string id = fc["ID"];
                string bookName = fc["BookName"];
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("找不到数据");
                }
                if (string.IsNullOrEmpty(fc["BookCategoryId"]))
                {
                    throw new Exception("请选择图书分类");
                }
                if (string.IsNullOrEmpty(fc["BookName"]))
                {
                    throw new Exception("请输入图书名称");
                }
                if (string.IsNullOrEmpty(fc["Author"]))
                {
                    throw new Exception("请输入作者");
                }
                if (string.IsNullOrEmpty(fc["ISBN"]))
                {
                    throw new Exception("请输入图书ISBN码");
                }
               
                var entity = BookInfoService.Single(id);
                if (BookInfoService.List(x => x.BookName == bookName&&x.ID!=entity.ID).Count()>0)
                {
                    throw new Exception("该名称已存在");
                }
                TryUpdateModel(entity, fc.AllKeys);
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
                //加密
                entity.CreateSign();
                BookInfoService.Update(entity);
                SysDBTool.Commit();

                result.Message = "修改成功";
                result.Status = 200;
            }
            catch (Exception ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "修改失败！";
                result.Message = ex.Message;
                result.Status = 500;
            }
            return Json(result);
        }


        /// <summary>
        /// 删除图书
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {
            ReturnResult result = new ReturnResult();
            var model = BookInfoService.Single(id);
            if (model == null)
            {
                result.Status = 500;
                throw new Exception("找不到数据");
            }
            else
            {
                BookInfoService.Delete(id);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "“" + model.BookName + "”已被删除！";
            }
            return Json(result);
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

        #region 图书分类添加
        /// <summary>
        /// 图书分类添加
        /// </summary>
        /// <returns></returns>
        public ActionResult AddBookClass()
        {
            return View();
        }

        /// <summary>
        /// 保存图书分类
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

        #region 图书分类编辑
        /// <summary>
        /// 图书分类编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyBookClass(string id)
        {
            var PCategory = BookCategoryService.Single(id);
            return View(PCategory);
        }

        /// <summary>
        /// 修改图书分类
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
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

        #region 获得图书分类

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
