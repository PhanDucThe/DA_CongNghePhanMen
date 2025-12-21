using congnghephanmem.Models;
using congnghephanmem.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace congnghephanmem.Controllers
{
    public class DashboardController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            var model = new DashboardViewModel();
            var today = DateTime.Today;

            model.TotalRevenue = db.orders
                .Where(o => o.status == "DELIVERED")
                .Sum(o => (decimal?)o.total_money) ?? 0;


            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var revThisMonth = db.orders
                .Where(o => o.status == "DELIVERED" && o.created_at >= thisMonth)
                .Sum(o => (decimal?)o.total_money) ?? 0;

            var revLastMonth = db.orders
                .Where(o => o.status == "DELIVERED" && o.created_at >= lastMonth && o.created_at < thisMonth)
                .Sum(o => (decimal?)o.total_money) ?? 0;

            if (revLastMonth > 0)
                model.RevenueGrowth = ((revThisMonth - revLastMonth) / revLastMonth) * 100;
            else
                model.RevenueGrowth = 100; 

            var yesterday = DateTime.Now.AddHours(-24);
            model.NewOrdersToday = db.orders.Count(o => o.created_at >= yesterday);

            model.ProductsOutOfStock = db.products.Count(p => p.stock_quantity < 10);

            var lastWeek = today.AddDays(-7);
            model.NewCustomersThisWeek = db.users
                    .Count(u => u.created_at >= lastWeek &&
                u.user_roles.Any(ur => ur.role.name == "CUSTOMER"));

            var labels = new List<string>();
            var values = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                var nextDay = day.AddDays(1); 

                labels.Add(day.ToString("dd/MM"));


                var revenue = db.orders
                    .Where(o => o.status == "DELIVERED"
                             && o.created_at >= day
                             && o.created_at < nextDay) 
                    .Sum(o => (decimal?)o.total_money) ?? 0;

                values.Add(revenue / 1000000);
            }
            model.RevenueLabels = labels.ToArray();
            model.RevenueData = values.ToArray();


            var allOrders = db.orders.ToList(); 

            model.OrderCompleted = allOrders.Count(o => o.status == "DELIVERED" || o.status == "Hoàn thành");
            model.OrderCancelled = allOrders.Count(o => o.status == "CANCELLED" || o.status == "Đã hủy");
            model.OrderShipping = allOrders.Count - model.OrderCompleted - model.OrderCancelled;
            model.RecentOrders = db.orders
                .OrderByDescending(o => o.created_at)
                .Take(5)
                .ToList();

            return View(model);
        }
    }
}