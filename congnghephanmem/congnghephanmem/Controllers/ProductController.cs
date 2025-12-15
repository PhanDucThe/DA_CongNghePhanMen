using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using congnghephanmem.Models;
using Newtonsoft.Json; // Cần thư viện này để đọc cột ingredients JSON
using congnghephanmem.ViewModels;

namespace congnghephanmem.Controllers
{
    public class ProductController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: Product
        public ActionResult Index()
        {
            // Trang danh sách thuốc (Sẽ làm sau hoặc dùng lại Home/Index)
            return RedirectToAction("Index", "Home");
        }

        // GET: Product/Detail/5
        public ActionResult Detail(int id)
        {
            // 1. Tìm sản phẩm theo ID
            var product = db.products.FirstOrDefault(p => p.id == id && p.is_active == true);

            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.");
            }

            // 2. Lấy danh sách ảnh phụ (Gallery)
            ViewBag.Images = db.product_images.Where(img => img.product_id == id)
                                              .OrderBy(img => img.display_order)
                                              .ToList();

            // 3. Lấy sản phẩm liên quan (Cùng danh mục, khác ID hiện tại)
            // Lấy tối đa 4 sản phẩm
            var relatedProducts = db.products.Where(p => p.category_id == product.category_id && p.id != id && p.is_active == true)
                                             .OrderByDescending(p => p.created_at)
                                             .Take(4)
                                             .ToList();
            ViewBag.RelatedProducts = relatedProducts;

            // 4. Giải mã cột Ingredients (JSON) thành List object để hiển thị
            // Cấu trúc JSON: [{"name": "Vitamin C", "amount": "500", "unit": "mg"}, ...]
            if (!string.IsNullOrEmpty(product.ingredients) && product.ingredients != "[]")
            {
                try
                {
                    ViewBag.Ingredients = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(product.ingredients);
                }
                catch
                {
                    ViewBag.Ingredients = null;
                }
            }
            // Lấy các comment đã được duyệt (status = 'approved' hoặc tùy logic bạn)
            var reviews = db.comments.Where(c => c.product_id == id).ToList();

            // SỬA ĐOẠN NÀY: Dùng class cụ thể thay vì anonymous object
            var ratingStats = new congnghephanmem.ViewModels.ReviewStatsViewModel
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(c => c.rating ?? 0), 1) : 0,
                FiveStar = reviews.Count(c => c.rating == 5),
                FourStar = reviews.Count(c => c.rating == 4),
                ThreeStar = reviews.Count(c => c.rating == 3),
                TwoStar = reviews.Count(c => c.rating == 2),
                OneStar = reviews.Count(c => c.rating == 1)
            };

            ViewBag.RatingStats = ratingStats;

            // 5. Lấy tên Danh mục và Thương hiệu để làm Breadcrumb
            ViewBag.CategoryName = db.categories.Find(product.category_id)?.name;
            ViewBag.BrandName = db.brands.Find(product.brand_id)?.name;

            return View(product);
        }
    }
}