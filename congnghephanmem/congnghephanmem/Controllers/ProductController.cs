using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using Newtonsoft.Json; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class ProductController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();


        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        

        public ActionResult Detail(int id)
        {
            var product = db.products.FirstOrDefault(p => p.id == id && p.is_active == true);

            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.");
            }

            ViewBag.Images = db.product_images.Where(img => img.product_id == id)
                                              .OrderBy(img => img.display_order)
                                              .ToList();

            var relatedProducts = db.products.Where(p => p.category_id == product.category_id && p.id != id && p.is_active == true)
                                             .OrderByDescending(p => p.created_at)
                                             .Take(4)
                                             .ToList();
            ViewBag.RelatedProducts = relatedProducts;
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
            var reviews = db.comments.Where(c => c.product_id == id).ToList();

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
            ViewBag.CategoryName = db.categories.Find(product.category_id)?.name;
            ViewBag.BrandName = db.brands.Find(product.brand_id)?.name;

            return View(product);
        }



        [HttpPost]
        public ActionResult SubmitReview(ReviewViewModel model)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá!" });
            }

            if (model.Rating < 1 || model.Rating > 5)
            {
                return Json(new { success = false, message = "Vui lòng chọn số sao đánh giá!" });
            }

            if (string.IsNullOrEmpty(model.Content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung đánh giá!" });
            }

            int userId = (int)Session["UserID"];

            try
            {
                bool hasPurchased = (from od in db.order_items
                                     join o in db.orders on od.order_id equals o.id
                                     where o.user_id == userId
                                        && od.product_id == model.ProductId
                                        && (o.status == "DELIVERED" || o.status == "Giao thành công")
                                     select od).Any();


                var comment = new comment
                {
                    product_id = model.ProductId,
                    user_id = userId,
                    content = model.Content,
                    rating = (byte)model.Rating,
                    is_verified_purchase = hasPurchased,
                    status = "approved", 
                    created_at = DateTime.Now
                };

                db.comments.Add(comment);
                db.SaveChanges(); 

                if (model.Images != null && model.Images.Count > 0)
                {
                    foreach (var file in model.Images)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            string path = Path.Combine(Server.MapPath("~/Content/images/reviews/"), fileName);

                            Directory.CreateDirectory(Server.MapPath("~/Content/images/reviews/"));

                            file.SaveAs(path);

                            var media = new comment_medias
                            {
                                comment_id = comment.id,
                                media_type = "image",
                                media_url = "/Content/images/reviews/" + fileName,
                                created_at = DateTime.Now
                            };
                            db.comment_medias.Add(media);
                        }
                    }
                    db.SaveChanges();
                }

                return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá sản phẩm!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public ActionResult GetReviews(int productId)
        {
            var reviews = db.comments
                            .Where(c => c.product_id == productId && c.status == "approved")
                            .OrderByDescending(c => c.created_at)
                            .ToList();

            return PartialView("_PartialListReviews", reviews);
        }


        public ActionResult Category(int id, string sort, string priceRange, int[] brandIds, int? page)
        {

            var category = db.categories.Find(id);
            if (category == null) return HttpNotFound();

            string displayTitle = category.name;
            if (category.parent_id != null)
            {
                var parentCategory = db.categories.Find(category.parent_id);
                if (parentCategory != null)
                {
                    displayTitle = $"{category.name} ({parentCategory.name})";
                }
            }

            ViewBag.CategoryName = displayTitle;
            ViewBag.CategoryID = id;
            ViewBag.Brands = db.brands.ToList();
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.CurrentBrandIds = brandIds;
            var categoryIds = new List<int> { id };
            var childIds = db.categories
                             .Where(c => c.parent_id == id)
                             .Select(c => c.id)
                             .ToList();
            categoryIds.AddRange(childIds);
            var query = db.products.Where(p => categoryIds.Contains((int)p.category_id) && p.is_active == true);

            if (brandIds != null && brandIds.Length > 0)
            {
                query = query.Where(p => p.brand_id.HasValue && brandIds.Contains(p.brand_id.Value));
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under100":
                        query = query.Where(p => p.sale_price < 100000);
                        break;
                    case "100-300":
                        query = query.Where(p => p.sale_price >= 100000 && p.sale_price <= 300000);
                        break;
                    case "300-500":
                        query = query.Where(p => p.sale_price >= 300000 && p.sale_price <= 500000);
                        break;
                    case "above500":
                        query = query.Where(p => p.sale_price > 500000);
                        break;
                }
            }
            switch (sort)
            {
                case "price_asc": 
                    query = query.OrderBy(p => p.sale_price);
                    break;
                case "price_desc": 
                    query = query.OrderByDescending(p => p.sale_price);
                    break;
                case "name_az": 
                    query = query.OrderBy(p => p.name);
                    break;
                default: 
                    query = query.OrderByDescending(p => p.created_at);
                    break;
            }
            var products = query.ToList();

            return View(products);
        }
    }
}