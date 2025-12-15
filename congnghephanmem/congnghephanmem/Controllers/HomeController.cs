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

        // Thêm tham số đầu vào cho Action Index
        public ActionResult Index(int[] brandIds, string priceRange, string sort)
        {
            // 1. Khởi tạo truy vấn (chưa thực thi)
            var query = db.products.Where(p => p.is_active == true);

            // 2. Xử lý Lọc theo Thương hiệu (nếu có chọn)
            if (brandIds != null && brandIds.Length > 0)
            {
                query = query.Where(p => brandIds.Contains(p.brand_id.Value));
            }

            // 3. Xử lý Lọc theo Giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under100": // Dưới 100k
                        query = query.Where(p => p.sale_price < 100000);
                        break;
                    case "100-300": // Từ 100k - 300k
                        query = query.Where(p => p.sale_price >= 100000 && p.sale_price <= 300000);
                        break;
                    case "300-500": // Từ 300k - 500k
                        query = query.Where(p => p.sale_price >= 300000 && p.sale_price <= 500000);
                        break;
                    case "above500": // Trên 500k
                        query = query.Where(p => p.sale_price > 500000);
                        break;
                }
            }

            // 4. Xử lý Sắp xếp
            switch (sort)
            {
                case "price_asc": // Giá tăng dần
                    query = query.OrderBy(p => p.sale_price);
                    break;
                case "price_desc": // Giá giảm dần
                    query = query.OrderByDescending(p => p.sale_price);
                    break;
                case "name_az": // Tên A-Z
                    query = query.OrderBy(p => p.name);
                    break;
                default: // Mặc định: Mới nhất lên đầu
                    query = query.OrderByDescending(p => p.created_at);
                    break;
            }

            // 5. Lưu lại trạng thái lọc để hiển thị lại trên View (giữ checkbox đã tích)
            ViewBag.CurrentBrandIds = brandIds;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.CurrentSort = sort;
            ViewBag.Brands = db.brands.Where(b => b.is_active == true).ToList();

            // 6. Thực thi truy vấn và trả về List
            return View(query.ToList());
        }
    }
}