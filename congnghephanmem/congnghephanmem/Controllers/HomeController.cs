using congnghephanmem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class HomeController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();


        public ActionResult Index(int[] brandIds, string priceRange, string sort, string keyword)
        {

            ViewBag.Title = "Trang chủ";

            var query = db.products.Where(p => p.is_active == true);


            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.name.Contains(keyword));
            }

            if (brandIds != null && brandIds.Length > 0)
            {
                query = query.Where(p => brandIds.Contains(p.brand_id.Value));
            }


            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under100": query = query.Where(p => p.sale_price < 100000); break;
                    case "100-300": query = query.Where(p => p.sale_price >= 100000 && p.sale_price <= 300000); break;
                    case "300-500": query = query.Where(p => p.sale_price >= 300000 && p.sale_price <= 500000); break;
                    case "above500": query = query.Where(p => p.sale_price > 500000); break;
                }
            }

            switch (sort)
            {
                case "price_asc": query = query.OrderBy(p => p.sale_price); break;
                case "price_desc": query = query.OrderByDescending(p => p.sale_price); break;
                case "name_az": query = query.OrderBy(p => p.name); break;
                default: query = query.OrderByDescending(p => p.created_at); break;
            }


            ViewBag.CurrentBrandIds = brandIds;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.Brands = db.brands.Where(b => b.is_active == true).ToList();

            return View(query.ToList());
        }
    }
}