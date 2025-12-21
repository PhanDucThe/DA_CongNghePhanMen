using congnghephanmem.Helpers;
using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace congnghephanmem.Controllers
{
    public class AccountController : Controller
    {

        private db_cnpmEntities db = new db_cnpmEntities();


        [HttpGet]
        public ActionResult Login()
        {

            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = db.users.FirstOrDefault(u =>
                    (u.email == model.Username || u.phone_number == model.Username)
                    && u.password == model.Password
                    && u.is_active == true);

                if (user != null)
                {

                    Session["User"] = user;
                    Session["UserID"] = user.id;
                    Session["UserName"] = user.full_name;
                    Session["UserEmail"] = user.email;
                    Session["UserAvatar"] = user.avatar;
                    FormsAuthentication.SetAuthCookie(user.email, false);


                    var userRole = db.user_roles.FirstOrDefault(ur => ur.user_id == user.id);
                    if (userRole != null)
                    {
                        var role = db.roles.Find(userRole.role_id);
                        Session["UserRole"] = role.code; 


                        if (role.code == "ADMIN")
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                    }
                    MergeCookieCartToDatabase(user.id);


                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa.");
                }
            }

            return View(model);
        }

        private void MergeCookieCartToDatabase(int userId)
        {

            var cookie = Request.Cookies["ShoppingCart"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return; 

            var cookieItems = JsonConvert.DeserializeObject<List<CartItemCookie>>(Server.UrlDecode(cookie.Value));
            if (!cookieItems.Any()) return;

            var cart = db.carts.FirstOrDefault(c => c.user_id == userId);
            if (cart == null)
            {
                cart = new cart { user_id = userId, created_at = DateTime.Now, total_items = 0, total_price = 0 };
                db.carts.Add(cart);
                db.SaveChanges();
            }


            foreach (var item in cookieItems)
            {
                var dbItem = db.cart_items.FirstOrDefault(ci => ci.cart_id == cart.id && ci.product_id == item.ProductId);
                if (dbItem != null)
                {
                    dbItem.quantity += item.Quantity; 
                }
                else
                {
                    var product = db.products.Find(item.ProductId);
                    if (product != null)
                    {
                        var newItem = new cart_items
                        {
                            cart_id = cart.id,
                            product_id = item.ProductId,
                            quantity = item.Quantity,
                            product_name = product.name,
                            image = product.thumbnail_url,
                            original_price = product.original_price,
                            sale_price = product.sale_price,
                            created_at = DateTime.Now
                        };
                        db.cart_items.Add(newItem);
                    }
                }
            }
            db.SaveChanges();


            var allItems = db.cart_items.Where(ci => ci.cart_id == cart.id).ToList();
            cart.total_items = allItems.Sum(x => x.quantity);
            cart.total_price = allItems.Sum(x => x.quantity * x.sale_price);
            db.SaveChanges();


            var expiredCookie = new HttpCookie("ShoppingCart") { Expires = DateTime.Now.AddDays(-1) };
            Response.Cookies.Add(expiredCookie);
        }


        public ActionResult Logout()
        {

            Session.Clear();
            Session.Abandon();

            FormsAuthentication.SignOut();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (db.users.Any(x => x.email == model.Email || x.phone_number == model.PhoneNumber))
                {
                    ModelState.AddModelError("", "Email hoặc Số điện thoại đã được đăng ký.");
                    return View(model);
                }


                var newUser = new user(); 
                newUser.full_name = model.FullName;
                newUser.email = model.Email;
                newUser.phone_number = model.PhoneNumber;
                newUser.password = model.Password; 
                newUser.is_active = true;
                newUser.created_at = DateTime.Now;
                newUser.updated_at = DateTime.Now;


                newUser.avatar = "/Content/images/default-avatar.png";

                db.users.Add(newUser);
                db.SaveChanges(); 

                var customerRole = db.roles.FirstOrDefault(r => r.code == "CUSTOMER");

                if (customerRole != null)
                {
                    var newRoleRelation = new user_roles();
                    newRoleRelation.user_id = newUser.id;
                    newRoleRelation.role_id = customerRole.id;
                    newRoleRelation.created_at = DateTime.Now;

                    db.user_roles.Add(newRoleRelation);
                    db.SaveChanges();
                }

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }



        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.users.FirstOrDefault(u => u.email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "Email này chưa được đăng ký trong hệ thống.");
                    return View(model);
                }

                string newPassword = GenerateRandomPassword(8);

                user.password = newPassword;
                user.updated_at = DateTime.Now;
                db.SaveChanges();


                bool sendResult = SendEmail(user.email, "Cấp lại mật khẩu mới - Nhà Thuốc Online",
                    $"<p>Xin chào <b>{user.full_name}</b>,</p>" +
                    $"<p>Bạn vừa yêu cầu cấp lại mật khẩu. Mật khẩu mới của bạn là:</p>" +
                    $"<h2 style='color: #00C092;'>{newPassword}</h2>" +
                    $"<p>Vui lòng đăng nhập và đổi lại mật khẩu ngay để bảo mật thông tin.</p>");

                if (sendResult)
                {
                    TempData["SuccessMessage"] = "Mật khẩu mới đã được gửi vào email của bạn. Vui lòng kiểm tra hộp thư.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Gửi email thất bại. Vui lòng thử lại sau.");
                }
            }
            return View(model);
        }


        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MailMessage();

                var smtpSection = (System.Net.Configuration.SmtpSection)System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                string fromEmail = smtpSection.From;

                message.From = new MailAddress(fromEmail, "Nhà Thuốc An Tâm");

                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient())
                {
                    client.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserID"];
            var user = db.users.Find(userId);

            if (user == null) return RedirectToAction("Login");

            var model = new UserProfileViewModel
            {
                Id = user.id,
                FullName = user.full_name,
                Email = user.email,
                PhoneNumber = user.phone_number,
                Address = "", 
                CurrentAvatar = string.IsNullOrEmpty(user.avatar) ? "/Content/images/default-user.png" : user.avatar
            };

            return View(model);
        }


        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                int userId = (int)Session["UserID"];
                var user = db.users.Find(userId);
                if (user.password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                user.password = model.NewPassword; 
                user.updated_at = DateTime.Now;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

                return RedirectToAction("ChangePassword");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UserProfileViewModel model)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                int userId = (int)Session["UserID"];
                var user = db.users.Find(userId);

                if (user != null)
                {
                    user.full_name = model.FullName;
                    user.phone_number = model.PhoneNumber;
                    user.updated_at = DateTime.Now;

                    if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
                    {
                        var cloud = new CloudinaryService();
                        string newAvatarUrl = cloud.UploadImage(model.AvatarFile);
                        user.avatar = newAvatarUrl;
                        Session["UserAvatar"] = newAvatarUrl;
                    }

                    Session["UserName"] = user.full_name;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction("Profile");
                }
            }

            return View("Profile", model);
        }
    }
}