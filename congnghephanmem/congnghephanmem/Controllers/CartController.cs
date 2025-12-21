using congnghephanmem.Helpers;
using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class CartController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();
        private const string CartCookieName = "ShoppingCart";

        private List<CartItemViewModel> GetCartItems()
        {
            List<CartItemViewModel> items = new List<CartItemViewModel>();

            if (Session["User"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart != null)
                {
                    var dbItems = db.cart_items.Where(ci => ci.cart_id == cart.id).ToList();
                    foreach (var dbItem in dbItems)
                    {
                        var product = db.products.Find(dbItem.product_id);
                        if (product != null)
                        {
                            items.Add(new CartItemViewModel
                            {
                                ProductId = dbItem.product_id,
                                ProductName = dbItem.product_name,
                                ProductImage = dbItem.image,
                                Price = dbItem.sale_price,
                                Quantity = dbItem.quantity,
                                Slug = product.slug
                            });
                        }
                    }
                }
            }
            else
            {
                var cookieItems = GetCartFromCookie();
                foreach (var cookieItem in cookieItems)
                {
                    var product = db.products.Find(cookieItem.ProductId);
                    if (product != null)
                    {
                        items.Add(new CartItemViewModel
                        {
                            ProductId = product.id,
                            ProductName = product.name,
                            ProductImage = product.thumbnail_url,
                            Price = product.sale_price,
                            Quantity = cookieItem.Quantity,
                            Slug = product.slug
                        });
                    }
                }
            }
            return items;
        }

        private void ResetCouponSession()
        {
            Session["DiscountAmount"] = null;
            Session["CouponCode"] = null;
            Session["DiscountCode"] = null;
        }

        public ActionResult Index()
        {
            var model = new CartMainViewModel();
            model.Items = GetCartItems(); 

            model.SubTotal = model.Items.Sum(x => x.Total);


            model.ShippingFee = ShippingHelper.CalculateFee(model.SubTotal, "STANDARD");

            model.DiscountAmount = 0;
            if (Session["DiscountAmount"] != null)
            {
                decimal savedDiscount = (decimal)Session["DiscountAmount"];
                if (savedDiscount > model.SubTotal) savedDiscount = model.SubTotal;

                model.DiscountAmount = savedDiscount;
                model.CouponCode = Session["DiscountCode"] as string;
            }

            model.Total = model.SubTotal + model.ShippingFee - model.DiscountAmount;
            if (model.Total < 0) model.Total = 0;

            model.Recommendations = db.products
                .Where(p => p.is_active == true && p.sale_price < 50000)
                .OrderBy(x => Guid.NewGuid())
                .Take(5)
                .ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult ApplyCoupon(string code)
        {
            try
            {
                var items = GetCartItems();
                if (!items.Any()) return Json(new { success = false, message = "Giỏ hàng đang trống!" });

                decimal subTotal = items.Sum(x => x.Total);
                var coupon = db.coupons.FirstOrDefault(c => c.code == code && c.is_active == true);
                if (coupon == null)
                {
                    ResetCouponSession();
                    return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết hạn!" });
                }

                var conditions = db.coupon_conditions.Where(c => c.coupon_id == coupon.id).ToList();
                decimal discountAmount = 0;
                if (!conditions.Any())
                {
                    return Json(new { success = false, message = "Mã giảm giá này chưa được cấu hình!" });
                }

                foreach (var cond in conditions)
                {
                    if (cond.attribute == "SUBTOTAL" || cond.attribute == "TOTAL_ORDER")
                    {
                        decimal conditionValue;
                        if (!decimal.TryParse(cond.value, out conditionValue)) continue;

                        bool conditionMet = true;
                        switch (cond.@operator)
                        {
                            case ">=":
                                if (subTotal < conditionValue) conditionMet = false;
                                break;
                            case ">":
                                if (subTotal <= conditionValue) conditionMet = false;
                                break;
                            case "=":
                            case "==":
                                if (subTotal != conditionValue) conditionMet = false;
                                break;
                            case "<":
                                if (subTotal >= conditionValue) conditionMet = false;
                                break;
                            }

                            if (!conditionMet)
                            {
                                return Json(new { success = false, message = $"Đơn hàng phải từ {conditionValue:N0}đ mới được dùng mã này." });
                            }
                        }

                        if (cond.discount_type == "PERCENTAGE")
                        {
                            discountAmount += subTotal * (cond.discount_amount / 100);
                        }
                        else if (cond.discount_type == "FIXED_AMOUNT")
                        {
                            discountAmount += cond.discount_amount;
                        }
                    }

                    if (discountAmount <= 0)
                    {
                        return Json(new { success = false, message = "Mã này không áp dụng giảm giá nào cho đơn hàng của bạn." });
                    }
                    if (discountAmount > subTotal) discountAmount = subTotal;


                    decimal shippingFee = (subTotal >= 150000 || subTotal == 0) ? 0 : 20000;
                    decimal finalTotal = subTotal + shippingFee - discountAmount;


                    Session["DiscountCode"] = code;
                    Session["DiscountAmount"] = discountAmount;

                    return Json(new
                    {
                        success = true,
                        message = "Áp dụng mã thành công!",
                        discountStr = discountAmount.ToString("N0") + "đ",
                        totalStr = finalTotal.ToString("N0") + "đ"
                    });
                }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý: " + ex.Message });
            }
        }


        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            ResetCouponSession(); 

            if (Session["User"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart == null)
                {
                    cart = new cart { user_id = userId, created_at = DateTime.Now, total_items = 0, total_price = 0 };
                    db.carts.Add(cart);
                    db.SaveChanges();
                }

                var cartItem = db.cart_items.FirstOrDefault(ci => ci.cart_id == cart.id && ci.product_id == productId);
                var product = db.products.Find(productId);

                if (cartItem != null)
                {
                    cartItem.quantity += quantity;
                    cartItem.updated_at = DateTime.Now;
                }
                else
                {
                    cartItem = new cart_items
                    {
                        cart_id = cart.id,
                        product_id = productId,
                        quantity = quantity,
                        product_name = product.name,
                        image = product.thumbnail_url,
                        original_price = product.original_price,
                        sale_price = product.sale_price,
                        created_at = DateTime.Now
                    };
                    db.cart_items.Add(cartItem);
                }
                db.SaveChanges();
                UpdateCartTotals(cart.id);
            }
            else
            {
                var list = GetCartFromCookie();
                var item = list.FirstOrDefault(x => x.ProductId == productId);
                if (item != null) item.Quantity += quantity;
                else list.Add(new CartItemCookie { ProductId = productId, Quantity = quantity });
                SaveCartToCookie(list);
            }

            if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
            return RedirectToAction("Index");
        }

        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1) quantity = 1;
            ResetCouponSession(); 

            if (Session["User"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart != null)
                {
                    var item = db.cart_items.FirstOrDefault(ci => ci.cart_id == cart.id && ci.product_id == productId);
                    if (item != null)
                    {
                        item.quantity = quantity;
                        db.SaveChanges();
                        UpdateCartTotals(cart.id);
                    }
                }
            }
            else
            {
                var list = GetCartFromCookie();
                var item = list.FirstOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    SaveCartToCookie(list);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Remove(int productId)
        {
            ResetCouponSession(); 

            if (Session["User"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart != null)
                {
                    var item = db.cart_items.FirstOrDefault(ci => ci.cart_id == cart.id && ci.product_id == productId);
                    if (item != null)
                    {
                        db.cart_items.Remove(item);
                        db.SaveChanges();
                        UpdateCartTotals(cart.id);
                    }
                }
            }
            else
            {
                var list = GetCartFromCookie();
                var item = list.FirstOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    list.Remove(item);
                    SaveCartToCookie(list);
                }
            }
            return RedirectToAction("Index");
        }


        private List<CartItemCookie> GetCartFromCookie()
        {
            var cookie = Request.Cookies[CartCookieName];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                return JsonConvert.DeserializeObject<List<CartItemCookie>>(Server.UrlDecode(cookie.Value));
            }
            return new List<CartItemCookie>();
        }

        private void SaveCartToCookie(List<CartItemCookie> cartList)
        {
            var json = JsonConvert.SerializeObject(cartList);
            var cookie = new HttpCookie(CartCookieName, Server.UrlEncode(json));
            cookie.Expires = DateTime.Now.AddDays(30);
            Response.Cookies.Add(cookie);
        }

        private void UpdateCartTotals(int cartId)
        {
            var cart = db.carts.Find(cartId);
            if (cart != null)
            {
                var items = db.cart_items.Where(ci => ci.cart_id == cartId).ToList();
                cart.total_items = items.Sum(x => x.quantity);
                cart.total_price = items.Sum(x => x.quantity * x.sale_price);
                db.SaveChanges();
            }
        }

        [ChildActionOnly]
        public ActionResult GetCartCount()
        {
            int count = 0;
            if (Session["User"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                count = cart != null ? (cart.total_items ?? 0) : 0;
            }
            else
            {
                var list = GetCartFromCookie();
                count = list.Sum(x => x.Quantity);
            }
            return Content(count.ToString());
        }
    }
}