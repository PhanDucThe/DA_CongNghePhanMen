using CloudinaryDotNet.Core;
using congnghephanmem.Helpers;
using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: /Checkout
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }

            int userId = (int)Session["UserID"];

            var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
            if (cart == null || cart.total_items == 0)
            {
                return RedirectToAction("Index", "Home");
            }

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

            decimal subTotal = cartItems.Sum(x => x.Total);
            decimal shippingFee = ShippingHelper.CalculateFee(subTotal, "STANDARD");

            decimal discountAmount = 0;
            if (Session["DiscountAmount"] != null)
            {
                discountAmount = (decimal)Session["DiscountAmount"];
                if (discountAmount > subTotal) discountAmount = subTotal;
            }

            var user = db.users.Find(userId);
            var defaultAddress = db.user_addresses
                                   .FirstOrDefault(a => a.user_id == userId && a.is_default == true);

            var model = new CheckoutViewModel
            {
                FullName = defaultAddress != null ? defaultAddress.recipient_name : user.full_name,
                PhoneNumber = defaultAddress != null ? defaultAddress.phone_number : user.phone_number,
                Address = defaultAddress != null ? defaultAddress.address_line : "",
                CartItems = cartItems,
                SubTotal = subTotal,
                ShippingMethod = "STANDARD",
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                TotalAmount = subTotal + shippingFee - discountAmount
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(CheckoutViewModel model)
        {
            int userId = (int)Session["UserID"];

            var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
            if (cart == null) return RedirectToAction("Index", "Home");

            var dbItems = db.cart_items.Where(ci => ci.cart_id == cart.id).ToList();
            if (dbItems.Count == 0) return RedirectToAction("Index", "Home");

            decimal subTotal = dbItems.Sum(x => x.sale_price * x.quantity);
            decimal shippingFee = ShippingHelper.CalculateFee(subTotal, model.ShippingMethod);

            decimal discountAmount = 0;
            if (Session["DiscountAmount"] != null)
            {
                discountAmount = Convert.ToDecimal(Session["DiscountAmount"]);
            }

            decimal totalMoney = subTotal + shippingFee - discountAmount;
            if (totalMoney < 0) totalMoney = 0;

   
            using (var scope = new TransactionScope())
            {
                try
                {
                    var newOrder = new order
                    {
                        user_id = userId,
                        full_name = model.FullName ?? "",
                        phone_number = model.PhoneNumber ?? "",
                        shipping_address = model.Address ?? "Nhận tại quầy",
                        note = model.Note ?? "",
                        shipping_method = model.ShippingMethod,
                        payment_method = model.PaymentMethod,

                        subtotal_money = subTotal,
                        shipping_fee = shippingFee,
                        discount_amount = discountAmount,
                        total_money = totalMoney,

                        status = "PENDING_CONFIRMATION",
                        payment_status = "UNPAID",
                        order_date = DateTime.Now,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        tracking_number = "",
                        created_by = Session["UserName"] as string ?? "Customer",
                        updated_by = ""
                    };

                    db.orders.Add(newOrder);
                    db.SaveChanges(); 

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

                    foreach (var item in dbItems)
                    {
                        db.cart_items.Remove(item);
                    }

                    cart.total_items = 0;
                    cart.total_price = 0;
                    cart.updated_at = DateTime.Now;

                    Session["DiscountAmount"] = null;
                    Session["DiscountCode"] = null;

                    db.SaveChanges();

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi xử lý đơn hàng: " + ex.Message);
                    return View("Index", model);
                }
            } 



            var createdOrder = db.orders.Find(db.orders.Local.Last().id);

            if (model.PaymentMethod == "VNPAY")
            {
                return RedirectToAction("VnPayPayment", "Payment", new { orderId = createdOrder.id });
            }
            else
            {
                try
                {
                    var userEmail = db.users.Find(userId)?.email;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var orderForMail = db.orders
                            .Include(o => o.order_items.Select(oi => oi.product))
                            .FirstOrDefault(o => o.id == createdOrder.id);

                        if (orderForMail != null)
                        {
                            string body = EmailContentBuilder.BuildOrderSuccessEmail(orderForMail);
                            string subject = $"Xác nhận đơn hàng #{createdOrder.id} - Nhà Thuốc An Tâm";

                            Task.Run(() =>
                            {
                                var emailService = new EmailService();
                                emailService.SendEmail(userEmail, subject, body);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi gửi mail: " + ex.Message);
                }

                return RedirectToAction("Success", new { id = createdOrder.id });
            }
        }


        public ActionResult Success(int id)
        {
            var order = db.orders.Find(id);

            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

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