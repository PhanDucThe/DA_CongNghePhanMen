using congnghephanmem.Helpers; 
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using congnghephanmem.Models;

namespace congnghephanmem.Controllers
{
    public class PrescriptionController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();


        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            var list = db.prescriptions.Where(p => p.user_id == userId)
                                       .OrderByDescending(p => p.created_at).ToList();
            return View(list);
        }


        public ActionResult Upload()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(string note, HttpPostedFileBase file)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            if (file != null && file.ContentLength > 0)
            {

                var cloud = new CloudinaryService();
                string imgUrl = cloud.UploadImage(file);

                var p = new prescription
                {
                    user_id = (int)Session["UserID"],
                    image_url = imgUrl,
                    note = note,
                    status = "PENDING", 
                    created_at = DateTime.Now
                };

                db.prescriptions.Add(p);
                db.SaveChanges();

                TempData["Success"] = "Đã gửi đơn thuốc! Dược sĩ sẽ phản hồi sớm.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Vui lòng chọn ảnh đơn thuốc.");
            return View();
        }

        public ActionResult Details(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];
            var p = db.prescriptions.FirstOrDefault(x => x.id == id && x.user_id == userId);
            if (p == null) return RedirectToAction("Index");
            return View(p);
        }
    }
}