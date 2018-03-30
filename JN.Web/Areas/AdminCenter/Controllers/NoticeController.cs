using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    [ValidateInput(false)]
    public class NoticeController : BaseController
    {
        private readonly INoticeService NoticeService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;

        public NoticeController(ISysDBTool SysDBTool,
            INoticeService NoticeService,
            IActLogService ActLogService,
            ILogDBTool LogDBTool)
        {
            this.NoticeService = NoticeService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
        }

        public ActionResult Index(int? page)
        {
            ActMessage = "公告列表";
            var list = NoticeService.List().OrderByDescending(x => x.ID).ToList();
            return View(list.ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 添加或修改公告
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Modify(string id)
        {
            ActMessage = "发布公告";
            var model = new Data.Notice();
            if (!string.IsNullOrEmpty(id))
            {
                ActMessage = "修改公告";
                model = NoticeService.Single(id);
            }
            return View(model);
        }

        /// <summary>
        /// 修改公告
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commandtype"></param>
        /// <returns></returns>
        public ActionResult NoticeCommand(int id, string commandtype)
        {
            var model = NoticeService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            NoticeService.Update(model);
            SysDBTool.Commit();
            return RedirectToAction("Index", "Notice");
        }

        public ActionResult Delete(int id)
        {
            var model = NoticeService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                NoticeService.Delete(id);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "“" + model.Title + "”已被删除！";
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        /// <summary>
        /// 修改或添加公告
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            var result = new ReturnResult();
            try
            {
                string id = fc["ID"];
                var entity = NoticeService.Single(x=>x.ID== id);
                if (entity!=null)
                {
                    TryUpdateModel(entity, fc.AllKeys);
                    NoticeService.Update(entity);
                }
                else
                {
                    var addentity = new Notice();
                    TryUpdateModel(addentity, fc.AllKeys);
                    NoticeService.Add(addentity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }
    }
}
