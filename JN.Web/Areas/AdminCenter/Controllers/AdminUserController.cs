using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class AdminUserController : BaseController
    {
        private readonly IAdminUserService AdminUserService;
        private readonly IAdminRoleService AdminRoleService;
        private readonly IAdminAuthorityService AdminAuthorityService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;

        public AdminUserController(ISysDBTool SysDBTool, 
            IAdminUserService AdminUserService, 
            IAdminRoleService AdminRoleService, 
            IAdminAuthorityService AdminAuthorityService, 
            IActLogService ActLogService, 
            ILogDBTool LogDBTool)
        {
            this.AdminUserService = AdminUserService;
            this.AdminRoleService = AdminRoleService;
            this.AdminAuthorityService = AdminAuthorityService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.LogDBTool = LogDBTool;
        }

        public ActionResult Index()
        {
            var list = AdminUserService.List().OrderByDescending(x => x.CreateTime).ToList();
            ViewBag.Title = "管理员管理";
            ActMessage = ViewBag.Title;
            return View(list);
        }

        public ActionResult Modify(string id)
        {
            ViewBag.Title = "修改管理员";
            ActMessage = ViewBag.Title;
            var model = AdminUserService.Single(x=>x.ID== id);
            var role = AdminRoleService.Single(x => x.ID == model.ID);
            ViewData["AdminRoleList"] = new SelectList(AdminRoleService.List().ToList(), "ID", "RoleName", model.RoleID);
             return View(model);
        }

        /// <summary>
        /// 添加页面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddAdminUser()
        {
            ViewData["AdminRoleList"] = new SelectList(AdminRoleService.List().ToList(), "ID", "RoleName");
            return View();
        }

        /// <summary>
        /// 修改管理员信息
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAdminUser(FormCollection fc)
        {
            var result = new ReturnResult();
            try
            {
                var entity = new JN.Data.AdminUser();
                TryUpdateModel(entity, fc.AllKeys);
                var roleid = fc["roleid"];
                var role = AdminRoleService.Single(x => x.ID == roleid);
                if (role.RoleName == "超管" || role.RoleName == "超级管理员")
                {
                    throw new Exception("不可以添加超级管理员，它是唯一的");
                }
                if (AdminUserService.List(x => x.AdminName.Equals(entity.AdminName)).Count() > 0)
                    throw new Exception("管理员帐号已被使用");
                entity.CreateTime = DateTime.Now;
                entity.Password = entity.Password.ToMD5().ToMD5();
                entity.Password = entity.Password2.ToMD5().ToMD5();
                entity.IsPassed = true;
                AdminUserService.Add(entity);
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

        /// <summary>
        /// 修改管理员信息
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
                var adminname = fc["adminname"];
                var realname = fc["realname"];
                var phone = fc["phone"];
                var roleid = fc["roleid"];
                var entity = AdminUserService.Single(x=>x.ID== id);
                var entityrole = AdminRoleService.Single(x => x.ID == entity.RoleID);
                if (entityrole.RoleName == "超管" || entityrole.RoleName == "超级管理员")
                {
                    throw new Exception("超级管理员数据不可修改");
                }
                var role = AdminRoleService.Single(x => x.ID == roleid);
                //超管角色不能修改
                if (role.RoleName=="超管"|| role.RoleName == "超级管理员")
                {
                    throw new Exception("超级管理员数据不可修改");
                }


                if (fc["resetpwd"] == "true")
                {
                    entity.Password = ("111111").ToMD5().ToMD5();
                    entity.Password = ("222222").ToMD5().ToMD5();
                }
                entity.AdminName = adminname;
                entity.RealName = realname;
                entity.Phone = phone;
                entity.RoleID = roleid;
                AdminUserService.Update(entity);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "修改成功";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }


        /// <summary>
        ///删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {

            ReturnResult result = new ReturnResult();
            try
            {
                var model = AdminUserService.Single(x => x.ID == id);
                if (model != null)
                {
                    var role = AdminRoleService.Single(x => x.ID == model.RoleID);
                    if (role.RoleName.Equals("超管") || role.RoleName.Equals("超级管理员"))
                    {
                        throw new Exception("超级管理员帐号不能被删除");
                    }
                    else
                    {
                        AdminUserService.Delete(x=>x.ID==model.ID);
                        SysDBTool.Commit();

                        result.Status = 200;
                        result.Message = "管理员“" + model.AdminName + "”帐号已被删除";
                    }
                }
                else
                {
                    throw new Exception("数据不存在");
                }
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 冻结
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MakeLock(string id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = AdminUserService.Single(x => x.ID == id);
                if (model != null)
                {
                    var role = AdminRoleService.Single(x => x.ID == model.RoleID);
                    if (role.RoleName.Equals("超管")|| role.RoleName.Equals("超级管理员"))
                    {
                        throw new Exception("超级管理员帐号不能被冻结");
                    }
                    else
                    {
                        model.IsPassed = false;
                        AdminUserService.Update(model);
                        SysDBTool.Commit();

                        result.Status = 200;
                        result.Message = "管理员“" + model.AdminName + "”帐号已被冻结";
                    }
                }
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message=ex.Message;
            }
           
            return Json(result);
        }

        /// <summary>
        /// 解冻
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MakeUnLock(string id)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var model = AdminUserService.Single(x => x.ID == id);
                if (model == null)
                {
                    throw new Exception("找不到数据");
                }
                model.IsPassed = true;
                AdminUserService.Update(model);
                SysDBTool.Commit();
                result.Status = 200;
                result.Message = "管理员“" + model.AdminName + "”帐号已解冻";
            }
            catch (Exception ex)
            {
                result.Status = 500;
                result.Message = ex.Message;
            }
            return Json(result);
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Role(int? page)
        {
            ViewBag.Title = "角色管理";
            ActMessage = ViewBag.Title;
            var list = AdminRoleService.List().ToList();
            return View(list);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Role(FormCollection fc)
        {
            try
            {
                var entity = AdminRoleService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID.ToString()!="1")
                    AdminRoleService.Update(entity);
                else
                    AdminRoleService.Add(entity);
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                return Json(new { result = "err", refMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ModifyRole(string id)
        {
            var model = AdminRoleService.Single(id);
            if (model != null)
                return Json(new { result = "ok", data = model }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = "err", refMsg = "记录不存在或已被删除" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 配置权限
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        public ActionResult Authority(string rid)
        {
            ActMessage = "配置角色权限";
            var role = AdminRoleService.Single(rid);
            if (role != null)
            {
                ViewBag.AuthorityIds = role.AuthorityIds + ",";
                var list = AdminAuthorityService.List().OrderByDescending(x => x.ControllerName).ThenByDescending(x => x.ActionName).ToList();
                ViewBag.Title = "配置角色权限（" + role.RoleName + "）";
                return View(list);
            }
            else
            {
                ViewBag.ErrorMsg = "错误的参数。";
                return View("Error");
            }
        }

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public ActionResult MakeAuthority(string ids, string rid)
        {
            var role = AdminRoleService.Single(rid);
            if (role != null)
            {
                role.AuthorityIds = ids;
                AdminRoleService.Update(role);
                SysDBTool.Commit();
                return Content("ok");
            }
            return Content("err");
        }


    }
}
