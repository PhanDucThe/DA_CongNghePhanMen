using congnghephanmem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class DashboardViewModel
    {
        // 1. Số liệu cho 4 thẻ Stat Cards
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; } // % Tăng trưởng so với tháng trước
        public int NewOrdersToday { get; set; }
        public int ProductsOutOfStock { get; set; } // Sản phẩm sắp hết hàng
        public int NewCustomersThisWeek { get; set; }

        // 2. Dữ liệu biểu đồ Doanh thu (Line Chart)
        public string[] RevenueLabels { get; set; } // Nhãn ngày (T2, T3...)
        public decimal[] RevenueData { get; set; }  // Số tiền tương ứng

        // 3. Dữ liệu biểu đồ Trạng thái (Donut Chart)
        public int OrderCompleted { get; set; }
        public int OrderShipping { get; set; } // Bao gồm cả Đang xử lý
        public int OrderCancelled { get; set; }

        // 4. Bảng đơn hàng gần đây
        public List<order> RecentOrders { get; set; }
    }
}