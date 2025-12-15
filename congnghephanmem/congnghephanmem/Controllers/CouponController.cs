using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using congnghephanmem.Models;
using System.Transactions;

namespace congnghephanmem.Controllers
{
    public class CouponController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: Admin/Coupon
        public ActionResult Index()
        {
            // Lấy danh sách Coupon kèm điều kiện đầu tiên để hiển thị thông tin giảm giá
            // Sử dụng ViewModel hoặc Query trực tiếp tùy bạn. Ở đây mình query trực tiếp để nhanh.
            var coupons = db.coupons.OrderByDescending(c => c.created_at).ToList();
            return View(coupons);
        }

        // GET: Admin/Coupon/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Coupon/Create
        // POST: Admin/Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CouponViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra trùng mã
                if (db.coupons.Any(c => c.code == model.Code))
                {
                    ModelState.AddModelError("Code", "Mã khuyến mãi này đã tồn tại.");
                    return View(model);
                }

                // --- SỬA TẠI ĐÂY: Dùng TransactionScope thay vì db.Database.BeginTransaction() ---
                using (var scope = new TransactionScope())
                {
                    try
                    {
                        // 2. Lưu vào bảng coupons
                        var newCoupon = new coupon
                        {
                            code = model.Code.ToUpper(),
                            is_active = model.IsActive,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            created_by = "Admin"
                        };
                        db.coupons.Add(newCoupon);
                        db.SaveChanges(); // Lưu lần 1 để lấy ID

                        // 3. Lưu vào bảng coupon_conditions
                        var newCondition = new coupon_conditions
                        {
                            coupon_id = newCoupon.id,
                            attribute = "total_money",
                            @operator = ">=",
                            value = model.MinOrderValue.ToString(),
                            discount_amount = model.DiscountAmount,
                            discount_type = model.DiscountType,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now
                        };
                        db.coupon_conditions.Add(newCondition);
                        db.SaveChanges(); // Lưu lần 2

                        // Hoàn tất giao dịch (Tương đương Commit)
                        scope.Complete();

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        // TransactionScope tự động Rollback nếu không chạy đến lệnh scope.Complete()
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
            }
            return View(model);
        }

        // Xóa / Khóa Coupon
        public ActionResult Delete(int id)
        {
            var coupon = db.coupons.Find(id);
            if (coupon != null)
            {
                // 1. Tìm danh sách điều kiện con
                var conditions = db.coupon_conditions.Where(c => c.coupon_id == id).ToList();

                // 2. SỬA LỖI TẠI ĐÂY: Dùng vòng lặp xóa từng cái thay vì RemoveRange
                foreach (var item in conditions)
                {
                    db.coupon_conditions.Remove(item);
                }

                // 3. Xóa coupon cha
                db.coupons.Remove(coupon);

                // 4. Lưu thay đổi
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Toggle Active status
        public ActionResult ToggleStatus(int id)
        {
            var coupon = db.coupons.Find(id);
            if (coupon != null)
            {
                coupon.is_active = !coupon.is_active;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}