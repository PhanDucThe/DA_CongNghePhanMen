using congnghephanmem.Helpers; // Để gửi mail
using System;
using System.Linq;
using System.Web.Mvc;
using congnghephanmem.Models;

namespace congnghephanmem.Controllers
{
    public class AdminPrescriptionController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();
        public ActionResult Index()
        {
            var list = db.prescriptions.OrderByDescending(p => p.created_at).ToList();
            return View(list);
        }

        public ActionResult Process(int id)
        {
            var p = db.prescriptions.Find(id);
            if (p == null) return RedirectToAction("Index");
            return View(p);
        }

        [HttpPost]
        [ValidateInput(false)] 
        public ActionResult SubmitProcess(int id, string status, string response)
        {
            var p = db.prescriptions.Find(id);
            if (p != null)
            {
                p.status = status; 
                p.pharmacist_response = response;
                p.updated_at = DateTime.Now;
                db.SaveChanges();

                var user = db.users.Find(p.user_id);
                string subject = status == "APPROVED" ? "Đơn thuốc đã được duyệt" : "Đơn thuốc bị từ chối";
                string body = $"<p>Chào {user.full_name},</p>" +
                              $"<p>Kết quả duyệt đơn thuốc của bạn: <b>{(status == "APPROVED" ? "Đồng ý" : "Từ chối")}</b></p>" +
                              $"<p><b>Lời nhắn từ dược sĩ:</b></p>" +
                              $"<div style='background:#f1f1f1; padding:10px;'>{response}</div>";

                new EmailService().SendEmail(user.email, subject, body);
            }
            return RedirectToAction("Index");
        }
    }
}