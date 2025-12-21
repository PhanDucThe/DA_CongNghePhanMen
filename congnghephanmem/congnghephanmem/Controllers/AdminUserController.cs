using congnghephanmem.Helpers;
using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class AdminUserController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        public ActionResult Index(string keyword, int? page)
        {
            var query = from u in db.users
                        join ur in db.user_roles on u.id equals ur.user_id
                        join r in db.roles on ur.role_id equals r.id
                        where r.code == "CUSTOMER" 
                        select new UserViewModel
                        {
                            Id = u.id,
                            FullName = u.full_name,
                            Email = u.email,
                            Phone = u.phone_number,
                            Avatar = u.avatar,
                            CreatedAt = u.created_at,
                            IsActive = u.is_active,
                            TotalSpent = db.orders.Where(o => o.user_id == u.id && o.status == "DELIVERED").Sum(o => (decimal?)o.total_money) ?? 0
                        };


            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword) || u.Phone.Contains(keyword));
            }


            var listUsers = query.OrderByDescending(u => u.TotalSpent).ToList();

            return View(listUsers);
        }


        public ActionResult ToggleStatus(int id)
        {
            var user = db.users.Find(id);
            if (user != null)
            {
                user.is_active = !user.is_active; 
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        public ActionResult Create()
        {
            return View();
        }


        public ActionResult Details(int id)
        {
            var user = db.users.Find(id);

            if (user == null)
            {
                return RedirectToAction("Index");
            }


            var orders = db.orders.Where(o => o.user_id == id)
                                  .OrderByDescending(o => o.created_at)
                                  .ToList();


            decimal totalSpent = orders.Where(o => o.status == "DELIVERED").Sum(o => (decimal?)o.total_money) ?? 0;

            string group = "Mới";
            if (totalSpent > 10000000) group = "VIP";
            else if (totalSpent > 2000000) group = "Thân thiết";
            var model = new UserDetailViewModel
            {
                UserInfo = user,
                OrderHistory = orders,
                TotalOrders = orders.Count,
                TotalSpent = totalSpent,
                CustomerGroup = group
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (db.users.Any(u => u.email == model.Email || u.phone_number == model.PhoneNumber))
                {
                    ModelState.AddModelError("", "Email hoặc Số điện thoại đã được sử dụng.");
                    return View(model);
                }


                string avatarUrl = ""; 
                if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
                {
                    var cloudinary = new CloudinaryService();
                    avatarUrl = cloudinary.UploadImage(model.AvatarFile);
                }


                var newUser = new user
                {
                    full_name = model.FullName,
                    email = model.Email,
                    phone_number = model.PhoneNumber,
                    password = model.Password, 
                    avatar = avatarUrl, 
                    is_active = model.IsActive,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    created_by = "Admin",
                    updated_by = ""
                };

                db.users.Add(newUser);
                db.SaveChanges(); 

               
                var roleCustomer = db.roles.FirstOrDefault(r => r.code == "CUSTOMER");
                if (roleCustomer != null)
                {
                    var userRole = new user_roles
                    {
                        user_id = newUser.id,
                        role_id = roleCustomer.id,
                        created_at = DateTime.Now
                    };
                    db.user_roles.Add(userRole);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        public decimal TotalSpent { get; set; }

        public string CustomerGroup
        {
            get
            {
                if (TotalSpent > 10000000) return "VIP";
                if (TotalSpent > 0) return "Thân thiết";
                return "Mới";
            }
        }
    }
}