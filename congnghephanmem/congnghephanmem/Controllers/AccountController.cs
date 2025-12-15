using congnghephanmem.Models; 
using System;
using System.Linq;
using System.Web.Mvc;


namespace congnghephanmem.Controllers
{
    public class AccountController : Controller
    {
        // Khởi tạo DbContext (Thay Db_CNPMEntities bằng tên Context thực tế trong file Models của bạn)
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm user trong Database (Kiểm tra cả Email hoặc Số điện thoại)
                // Lưu ý: Mật khẩu ở đây đang so sánh text thường. 
                // Thực tế bạn nên mã hóa MD5 hoặc SHA256 trước khi so sánh.
                var user = db.users.FirstOrDefault(u =>
                    (u.email == model.Username || u.phone_number == model.Username)
                    && u.password == model.Password
                    && u.is_active == true);

                if (user != null)
                {
                    // 2. Đăng nhập thành công -> Lưu Session
                    Session["User"] = user;
                    Session["UserID"] = user.id;
                    Session["UserName"] = user.full_name;
                    Session["UserEmail"] = user.email;
                    Session["UserAvatar"] = user.avatar;

                    // Lấy Role của user (Giả sử user có 1 role chính)
                    var userRole = db.user_roles.FirstOrDefault(ur => ur.user_id == user.id);
                    if (userRole != null)
                    {
                        var role = db.roles.Find(userRole.role_id);
                        Session["UserRole"] = role.code; // Ví dụ: 'ADMIN' hoặc 'CUSTOMER'

                        // 3. Điều hướng dựa trên quyền
                        if (role.code == "ADMIN")
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                    }

                    // Mặc định về trang chủ khách hàng
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Đăng nhập thất bại
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa.");
                }
            }

            // Nếu dữ liệu không hợp lệ hoặc sai pass, trả lại View để nhập lại
            return View(model);
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa hết session
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Email hoặc SĐT đã tồn tại chưa
                if (db.users.Any(x => x.email == model.Email || x.phone_number == model.PhoneNumber))
                {
                    ModelState.AddModelError("", "Email hoặc Số điện thoại đã được đăng ký.");
                    return View(model);
                }

                // 2. Tạo User mới
                var newUser = new user(); 
                newUser.full_name = model.FullName;
                newUser.email = model.Email;
                newUser.phone_number = model.PhoneNumber;
                newUser.password = model.Password; 
                newUser.is_active = true;
                newUser.created_at = DateTime.Now;
                newUser.updated_at = DateTime.Now;

                // Ảnh đại diện mặc định (nếu cần)
                newUser.avatar = "/Content/images/default-avatar.png";

                db.users.Add(newUser);
                db.SaveChanges(); // Lưu để lấy ID vừa tạo

                // 3. Gán quyền mặc định là 'CUSTOMER' (Khách hàng)
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
    }
}