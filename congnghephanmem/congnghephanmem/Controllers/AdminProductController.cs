using congnghephanmem.Helpers;
using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class AdminProductController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: Admin/Product
        public ActionResult Index(string keyword, string filter)
        {
            var query = db.products.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                if (filter == "low_stock") 
                {
                    query = query.Where(p => p.stock_quantity < 10);
                }
                else if (filter == "hidden") 
                {
                    query = query.Where(p => p.is_active == false);
                }
            }


            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.name.Contains(keyword) || p.sku.Contains(keyword));
            }


            var products = query.OrderByDescending(p => p.created_at).ToList();


            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentKeyword = keyword;

            return View(products);
        }


        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            var product = db.products.Find(id);
            if (product != null)
            {
                product.is_active = !product.is_active;
                db.SaveChanges();
            }
            return RedirectToAction("Index"); 
        }


        public ActionResult Delete(int id)
        {
            var product = db.products.Find(id);
            if (product != null)
            {

                var images = db.product_images.Where(i => i.product_id == id).ToList();


                foreach (var img in images)
                {
                    db.product_images.Remove(img);
                }
                db.products.Remove(product);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(db.categories.ToList(), "id", "name");
            ViewBag.Brands = new SelectList(db.brands.ToList(), "id", "name");
            ViewBag.DosageForms = new SelectList(new List<string> { "Viên nén", "Viên nang", "Siro", "Dạng bột", "Kem bôi", "Dung dịch" });
            ViewBag.TargetAudiences = new SelectList(new List<string> { "Người lớn", "Trẻ em", "Phụ nữ mang thai", "Người cao tuổi", "Mọi đối tượng" });

            return View();
        }


        [HttpPost]
        [ValidateInput(false)] 
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (db.products.Any(p => p.sku == model.Sku))
                {
                    ModelState.AddModelError("Sku", "Mã SKU này đã tồn tại.");
                    ReloadDropdowns();
                    return View(model);
                }

                var cloud = new CloudinaryService();

                string mainThumbUrl = "/Content/images/no-image.png";
                if (model.ThumbnailFile != null && model.ThumbnailFile.ContentLength > 0)
                {
                    mainThumbUrl = cloud.UploadImage(model.ThumbnailFile);
                }

                var newProduct = new product
                {
                    name = model.Name,
                    sku = model.Sku,
                    slug = GenerateSlug(model.Name), 
                    original_price = model.OriginalPrice,
                    sale_price = model.SalePrice,
                    stock_quantity = model.StockQuantity,
                    stock = 0, 
                    description = model.Description,
                    content = model.Content,
                    ingredients = "[]", 
                    dosage = "",
                    contraindications = "",
                    packaging_details = "",
                    prescription_required = false,


                    category_id = model.CategoryId,
                    brand_id = model.BrandId,

                    thumbnail_url = mainThumbUrl,
                    is_active = model.IsActive,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    created_by = "Admin",
                    updated_by = ""
                };

                db.products.Add(newProduct);
                db.SaveChanges();


                if (model.GalleryFiles != null && model.GalleryFiles.Any())
                {
                    int order = 1;
                    foreach (var file in model.GalleryFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string url = cloud.UploadImage(file);
                            var img = new product_images 
                            {
                                product_id = newProduct.id,
                                image_url = url,
                                display_order = order++,
                                created_at = DateTime.Now
                            };
                            db.product_images.Add(img);
                        }
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ReloadDropdowns();
            return View(model);
        }


        public ActionResult Edit(int id)
        {

            var product = db.products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var model = new EditProductViewModel
            {
                Id = product.id,
                Name = product.name,
                Sku = product.sku,
                OriginalPrice = product.original_price,
                SalePrice = product.sale_price,
                StockQuantity = product.stock_quantity,
                Description = product.description,
                Content = product.content,
                CategoryId = product.category_id ?? 0,
                BrandId = product.brand_id ?? 0,
                IsActive = product.is_active ?? true,
                CurrentThumbnailUrl = product.thumbnail_url,

              
            };

            ReloadDropdowns();

            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = db.products.Find(model.Id);
                if (product == null) return HttpNotFound();

                if (product.sku != model.Sku && db.products.Any(p => p.sku == model.Sku))
                {
                    ModelState.AddModelError("Sku", "Mã SKU này đã tồn tại ở sản phẩm khác.");
                    ReloadDropdowns();
                    return View(model);
                }

                product.name = model.Name;
                product.sku = model.Sku;

                product.original_price = model.OriginalPrice;
                product.sale_price = model.SalePrice;
                product.stock_quantity = model.StockQuantity;
                product.description = model.Description;
                product.content = model.Content;
                product.category_id = model.CategoryId;
                product.brand_id = model.BrandId;
                product.is_active = model.IsActive;
                product.updated_at = DateTime.Now;

                if (model.ThumbnailFile != null && model.ThumbnailFile.ContentLength > 0)
                {
                    var cloud = new CloudinaryService();
                    string newUrl = cloud.UploadImage(model.ThumbnailFile);
                    product.thumbnail_url = newUrl;
                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ReloadDropdowns();
            return View(model);
        }

        private void ReloadDropdowns()
        {
            ViewBag.Categories = new SelectList(db.categories.ToList(), "id", "name");
            ViewBag.Brands = new SelectList(db.brands.ToList(), "id", "name");
            ViewBag.DosageForms = new SelectList(new List<string> { "Viên nén", "Viên nang", "Siro", "Dạng bột", "Kem bôi", "Dung dịch" });
            ViewBag.TargetAudiences = new SelectList(new List<string> { "Người lớn", "Trẻ em", "Phụ nữ mang thai", "Người cao tuổi", "Mọi đối tượng" });
        }

        public string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = Regex.Replace(str, @"[áàạảãâấầậẩẫăắằặẳẵ]", "a");
            str = Regex.Replace(str, @"[éèẹẻẽêếềệểễ]", "e");
            str = Regex.Replace(str, @"[óòọỏõôốồộổỗơớờợởỡ]", "o");
            str = Regex.Replace(str, @"[úùụủũưứừựửữ]", "u");
            str = Regex.Replace(str, @"[íìịỉĩ]", "i");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[ýỳỵỷỹ]", "y");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-").Trim();
            return str;
        }
    }
}