using congnghephanmem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class UserDetailViewModel
    {
        public user UserInfo { get; set; } // Thông tin cá nhân
        public List<order> OrderHistory { get; set; } // Lịch sử đơn hàng

        // Các chỉ số thống kê
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string CustomerGroup { get; set; } // VIP, Thân thiết...
    }
}