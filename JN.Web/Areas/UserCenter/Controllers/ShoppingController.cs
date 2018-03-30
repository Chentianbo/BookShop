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
        private readonly IBookInfoService BookInfoService;
        public ShoppingController(IBookInfoService bookInfoService)
        {
            this.BookInfoService = bookInfoService;
        }
        // GET: UserCenter/Shopping
        public ActionResult Index()
        {
            var list = BookInfoService.List(x => x.BookState==0).OrderByDescending(x => x.CreateTime).ToList();
            return View(list.ToPagedList(1, 20));
        }
        
        //列表分页
        public ActionResult List(DataPager page)
        {
            var list = BookInfoService.List(x => x.BookState == 0 || x.BookName.Contains(page.KeyWord)).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID);
            page.Total = list.Count();
            return View(list.ToPagedList(page.PageIndex, page.PageSize));
        }

        //商品明细
        public ActionResult ShopDetail(string id)
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
    }
}