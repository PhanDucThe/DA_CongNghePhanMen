using System;
using System.Linq;
using System.Web.Mvc;
using congnghephanmem.Models;

namespace congnghephanmem.Controllers
{
    public class AddressController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            var addresses = db.user_addresses.Where(a => a.user_id == userId)
                                             .OrderByDescending(a => a.is_default) // Mặc định lên đầu
                                             .ThenByDescending(a => a.created_at)
                                             .ToList();
            return View(addresses);
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            var newModel = new user_addresses();
            newModel.is_default = false; 

            return View(newModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(user_addresses model)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            if (ModelState.IsValid)
            {

                bool isFirst = !db.user_addresses.Any(a => a.user_id == userId);

                var address = new user_addresses
                {
                    user_id = userId,
                    recipient_name = model.recipient_name,
                    phone_number = model.phone_number,
                    address_line = model.address_line,
                    is_default = isFirst || (model.is_default ?? false),

                    created_at = DateTime.Now
                };

                if (address.is_default == true)
                {
                    var oldDefaults = db.user_addresses.Where(a => a.user_id == userId && a.is_default == true);
                    foreach (var item in oldDefaults)
                    {
                        item.is_default = false;
                    }
                }

                db.user_addresses.Add(address);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult SetDefault(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            var address = db.user_addresses.FirstOrDefault(a => a.id == id && a.user_id == userId);
            if (address != null)
            {
                var all = db.user_addresses.Where(a => a.user_id == userId);
                foreach (var item in all) item.is_default = false;

                address.is_default = true;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            var address = db.user_addresses.FirstOrDefault(a => a.id == id && a.user_id == userId);
            if (address != null)
            {
                if (address.is_default == true)
                {
                    TempData["Error"] = "Bạn không thể xóa địa chỉ mặc định.";
                }
                else
                {
                    db.user_addresses.Remove(address);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
    }
}