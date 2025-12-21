using System;
using System.Linq;
using System.Web.Mvc;
using congnghephanmem.Models;
using System.Collections.Generic;

namespace congnghephanmem.Controllers
{
    public class OrderController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();


        public ActionResult Index(string status, string keyword)
        {

            var query = db.orders.OrderByDescending(o => o.created_at).AsQueryable();


            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(o => o.status == status);
            }


            if (!string.IsNullOrEmpty(keyword))
            {
                int orderId;
                bool isId = int.TryParse(keyword, out orderId);

                query = query.Where(o => o.full_name.Contains(keyword) || (isId && o.id == orderId));
            }


            ViewBag.CountAll = db.orders.Count();
            ViewBag.CountPending = db.orders.Count(o => o.status == "PENDING_CONFIRMATION");
            ViewBag.CountProcessing = db.orders.Count(o => o.status == "PROCESSING"); 
            ViewBag.CountShipping = db.orders.Count(o => o.status == "SHIPPED");
            ViewBag.CountCompleted = db.orders.Count(o => o.status == "DELIVERED"); 
            ViewBag.CountCancelled = db.orders.Count(o => o.status == "CANCELLED");
            ViewBag.CurrentStatus = status ?? "All";

            return View(query.ToList());
        }

        public ActionResult Details(int id)
        {
            var order = db.orders.Find(id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }


        [HttpPost]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.orders.Find(id);
            if (order != null)
            {
                order.status = status;
                order.updated_at = DateTime.Now;
                db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = id });
        }


        public ActionResult History()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];
            var orders = db.orders.Where(o => o.user_id == userId)
                                  .OrderByDescending(o => o.created_at)
                                  .ToList();
            return View(orders);
        }

        public ActionResult DetailsCustomer(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            var order = db.orders.FirstOrDefault(o => o.id == id && o.user_id == userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToAction("History");
            }

            return View(order);
        }
    }
}