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

        public ActionResult Index()
        {
            var coupons = db.coupons.OrderByDescending(c => c.created_at).ToList();
            return View(coupons);
        }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CouponViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (db.coupons.Any(c => c.code == model.Code))
                {
                    ModelState.AddModelError("Code", "Mã khuyến mãi này đã tồn tại.");
                    return View(model);
                }

                using (var scope = new TransactionScope())
                {
                    try
                    {
                        var newCoupon = new coupon
                        {
                            code = model.Code.ToUpper(),
                            is_active = model.IsActive,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            created_by = "Admin"
                        };
                        db.coupons.Add(newCoupon);
                        db.SaveChanges(); 

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
                        db.SaveChanges(); 

                        scope.Complete();

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
            }
            return View(model);
        }


        public ActionResult Delete(int id)
        {
            var coupon = db.coupons.Find(id);
            if (coupon != null)
            {

                var conditions = db.coupon_conditions.Where(c => c.coupon_id == id).ToList();
                foreach (var item in conditions)
                {
                    db.coupon_conditions.Remove(item);
                }
                db.coupons.Remove(coupon);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
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