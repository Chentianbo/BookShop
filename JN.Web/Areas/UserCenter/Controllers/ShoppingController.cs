using JN.Data;
using JN.Data.Common;
using JN.Data.Service;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class ShoppingController : BasicsController
    {
        private readonly IUserService UserService;
        private readonly IBookInfoService BookInfoService;
        private readonly IShopOrderService ShopOrderService;
        private readonly IShopCarService ShopCarService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
        private readonly ILogDBTool LogDBTool;
        private readonly IBookCategoryService BookCategoryService;

        public ShoppingController(ISysDBTool SysDBTool,
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
        /// <summary>
        /// 商城首页 不需要登陆
        /// </summary>
        /// <returns></returns>
        // GET: UserCenter/Shopping
        public ActionResult Index()
        {
            var list = BookInfoService.List(x => x.BookState==0).OrderByDescending(x => x.CreateTime).ToList();
            ////订单数据
            //ViewBag.UserShopCarData = ShopOrderService.List(x => x.UID == Umodel.ID).OrderByDescending(x=>x.CreateTime).ToList();
            //图书数据
            ViewBag.BookInfoData = BookInfoService.List().OrderByDescending(x => x.CreateTime).ToList();

            return View(list.ToPagedList(1, 20));
        }
        
        //列表分页
        public ActionResult List(DataPager page)
        {
            var list = BookInfoService.List(x => x.BookState == 0 || x.BookName.Contains(page.KeyWord)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            page.Total = list.Count();
            return View(list.ToPagedList(page.PageIndex, page.PageSize));
        }

        [HttpGet]
        //商品明细
        public ActionResult BookDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ShopError", "Hone");
            }
            var bookProduct = BookInfoService.Single(id);
            if (bookProduct == null)
            {
                return RedirectToAction("ShopError", "Hone");
            }
            return View(bookProduct);
        }

        [HttpGet]
        public ActionResult SearchBook(string categoryId,string KeyWord,int?page=1)
        {
            var list = BookInfoService.List(x => x.BookState == 0).OrderByDescending(x => x.CreateTime).ToList();
            if (!string.IsNullOrEmpty(categoryId))
            {
                list = list.Where(x => x.BookCategoryId == categoryId).ToList();
            }
            if (!string.IsNullOrEmpty(KeyWord))
            {
                list = list.Where(x => x.BookName.Contains(KeyWord)).ToList();
            }
            return View(list);
        }
    }
}