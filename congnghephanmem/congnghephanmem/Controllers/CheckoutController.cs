using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    // Yêu cầu đăng nhập mới được vào trang này
    [Authorize]
    public class CheckoutController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: /Checkout
        public ActionResult Index()
        {
            int userId = (int)Session["UserID"];
            var cart = db.carts.FirstOrDefault(c => c.user_id == userId);

            // Nếu giỏ hàng trống -> Đá về trang chủ
            if (cart == null || cart.total_items == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy danh sách sản phẩm trong giỏ để hiển thị
            var cartItems = (from ci in db.cart_items
                             join p in db.products on ci.product_id equals p.id
                             where ci.cart_id == cart.id
                             select new CartItemViewModel
                             {
                                 ProductId = ci.product_id,
                                 ProductName = ci.product_name,
                                 ProductImage = ci.image,
                                 Price = ci.sale_price,
                                 Quantity = ci.quantity
                             }).ToList();

            var model = new CheckoutViewModel
            {
                // Điền sẵn thông tin người dùng nếu có
                FullName = Session["UserName"] as string,
                PhoneNumber = db.users.Find(userId)?.phone_number,

                CartItems = cartItems,
                SubTotal = cartItems.Sum(x => x.Total),
                ShippingFee = 20000 // Mặc định phí ship
            };

            // Logic tính tổng tiền
            model.TotalAmount = model.SubTotal + model.ShippingFee;

            return View(model);
        }

        // POST: /Checkout/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(CheckoutViewModel model)
        {
            // ... (Code lấy userId và cart giữ nguyên) ...
            int userId = (int)Session["UserID"];
            var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
            var dbItems = db.cart_items.Where(ci => ci.cart_id == cart.id).ToList();

            if (dbItems.Count == 0) return RedirectToAction("Index", "Home");

            try
            {
                var newOrder = new order
                {
                    user_id = userId,
                    full_name = model.FullName ?? "", // Nếu tên null thì lưu rỗng
                    phone_number = model.PhoneNumber ?? "",

                    // --- FIX LỖI TẠI ĐÂY: Thêm fallback nếu địa chỉ bị null ---
                    shipping_address = model.Address ?? "Nhận tại quầy",

                    note = model.Note ?? "",
                    shipping_method = model.ShippingMethod,
                    payment_method = model.PaymentMethod,
                    subtotal_money = dbItems.Sum(x => x.sale_price * x.quantity),
                    shipping_fee = (model.ShippingMethod == "EXPRESS") ? 40000 : 20000,
                    discount_amount = 0,
                    status = "PENDING_CONFIRMATION",
                    payment_status = "UNPAID",
                    order_date = DateTime.Now,
                    created_at = DateTime.Now,

                    // Các trường bắt buộc khác
                    updated_at = DateTime.Now,
                    tracking_number = "",
                    created_by = Session["UserName"] as string ?? "Customer",
                    updated_by = ""
                };

                db.orders.Add(newOrder);
                db.SaveChanges();

                // ... (Đoạn code lưu Order Items và Xóa giỏ hàng bên dưới giữ nguyên) ...
                foreach (var item in dbItems)
                {
                    var orderItem = new order_items
                    {
                        order_id = newOrder.id,
                        product_id = item.product_id,
                        quantity = item.quantity,
                        price = item.sale_price,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        created_by = "",
                        updated_by = ""
                    };
                    db.order_items.Add(orderItem);
                }
                db.SaveChanges();

                foreach (var item in dbItems) { db.cart_items.Remove(item); }
                cart.total_items = 0;
                cart.total_price = 0;
                cart.updated_at = DateTime.Now;
                db.SaveChanges();

                return RedirectToAction("Success", new { id = newOrder.id });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " Lỗi chi tiết: ", fullErrorMessage);
                throw new Exception(exceptionMessage);
            }
        }

        // Trang thông báo đặt hàng thành công
        // GET: /Checkout/Success/5
        public ActionResult Success(int id)
        {
            // 1. Tìm đơn hàng theo ID
            var order = db.orders.Find(id);

            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 2. Bảo mật: Kiểm tra xem đơn hàng có đúng của người dùng đang đăng nhập không
            // (Tránh trường hợp user A gõ ID đơn hàng của user B để xem)
            if (Session["UserID"] != null)
            {
                int currentUserId = (int)Session["UserID"];
                if (order.user_id != currentUserId)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(order);
        }
    }
}