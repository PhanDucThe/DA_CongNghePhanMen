using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using Newtonsoft.Json; // Cần thư viện này
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


        // GET: Cart (Trang giỏ hàng)
        public ActionResult Index()
        {
            var model = new CartMainViewModel();
            List<CartItemViewModel> items = new List<CartItemViewModel>();

            // 1. LẤY DỮ LIỆU GIỎ HÀNG
            if (Session["User"] != null)
            {
                // -- ĐÃ ĐĂNG NHẬP (Lấy từ DB) --
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart != null)
                {
                    var dbItems = db.cart_items.Where(ci => ci.cart_id == cart.id).ToList();
                    foreach (var dbItem in dbItems)
                    {
                        // Lấy thêm thông tin slug từ bảng product
                        var product = db.products.Find(dbItem.product_id);
                        items.Add(new CartItemViewModel
                        {
                            ProductId = dbItem.product_id,
                            ProductName = dbItem.product_name,
                            ProductImage = dbItem.image,
                            Price = dbItem.sale_price,
                            Quantity = dbItem.quantity,
                            Slug = product?.slug
                        });
                    }
                }
            }
            else
            {
                // -- KHÁCH VÃNG LAI (Lấy từ Cookie) --
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

            model.Items = items;

            // 2. TÍNH TOÁN TIỀN
            model.SubTotal = items.Sum(x => x.Total);

            // Logic Freeship: Nếu Tạm tính >= 150k thì Ship = 0, ngược lại 20k
            if (model.SubTotal >= model.FreeShipThreshold || model.SubTotal == 0)
            {
                model.ShippingFee = 0;
            }
            else
            {
                model.ShippingFee = 20000;
            }

            model.Total = model.SubTotal + model.ShippingFee;

            // 3. LẤY SẢN PHẨM GỢI Ý (Lấy 5 sản phẩm rẻ dưới 50k để user mua thêm cho đủ freeship)
            model.Recommendations = db.products
                .Where(p => p.is_active == true && p.sale_price < 50000)
                .OrderBy(x => Guid.NewGuid()) // Random
                .Take(5)
                .ToList();

            return View(model);
        }


        // Action cập nhật số lượng (Gọi bằng AJAX hoặc Reload)
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            if (Session["User"] != null)
            {
                // Update DB
                int userId = (int)Session["UserID"];
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart != null)
                {
                    var item = db.cart_items.FirstOrDefault(ci => ci.cart_id == cart.id && ci.product_id == productId);
                    if (item != null)
                    {
                        item.quantity = quantity;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                // Update Cookie
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



        // Action xóa sản phẩm
        public ActionResult Remove(int productId)
        {
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



        // POST: /Cart/AddToCart
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            // 1. TRƯỜNG HỢP: ĐÃ ĐĂNG NHẬP (Lưu vào DB)
            if (Session["User"] != null)
            {
                int userId = (int)Session["UserID"];

                // Tìm giỏ hàng của user
                var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
                if (cart == null)
                {
                    cart = new cart { user_id = userId, created_at = DateTime.Now, total_items = 0, total_price = 0 };
                    db.carts.Add(cart);
                    db.SaveChanges();
                }

                // Tìm sản phẩm trong giỏ
                var cartItem = db.cart_items.FirstOrDefault(ci => ci.cart_id == cart.id && ci.product_id == productId);
                var product = db.products.Find(productId);

                if (cartItem != null)
                {
                    // Đã có -> Tăng số lượng
                    cartItem.quantity += quantity;
                    cartItem.updated_at = DateTime.Now;
                }
                else
                {
                    // Chưa có -> Thêm mới
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
                UpdateCartTotals(cart.id); // Hàm tính lại tổng tiền (viết ở dưới)
            }
            // 2. TRƯỜNG HỢP: KHÁCH VÃNG LAI (Lưu vào Cookie)
            else
            {
                List<CartItemCookie> cartList = GetCartFromCookie();

                var item = cartList.FirstOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    item.Quantity += quantity;
                }
                else
                {
                    cartList.Add(new CartItemCookie { ProductId = productId, Quantity = quantity });
                }

                SaveCartToCookie(cartList);
            }

            // Quay lại trang trước đó
            return Redirect(Request.UrlReferrer.ToString());
        }

        // --- CÁC HÀM HELPER (HỖ TRỢ) ---

        // Helper 1: Lấy danh sách từ Cookie
        private List<CartItemCookie> GetCartFromCookie()
        {
            var cookie = Request.Cookies[CartCookieName];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                return JsonConvert.DeserializeObject<List<CartItemCookie>>(Server.UrlDecode(cookie.Value));
            }
            return new List<CartItemCookie>();
        }

        // Helper 2: Lưu danh sách vào Cookie
        private void SaveCartToCookie(List<CartItemCookie> cartList)
        {
            var json = JsonConvert.SerializeObject(cartList);
            var cookie = new HttpCookie(CartCookieName, Server.UrlEncode(json));
            cookie.Expires = DateTime.Now.AddDays(30); // Lưu 30 ngày
            Response.Cookies.Add(cookie);
        }

        // Helper 3: Cập nhật tổng số lượng/tiền cho DB Cart
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

        // Helper 4: Action trả về số lượng để hiện lên Header (Quan trọng)
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