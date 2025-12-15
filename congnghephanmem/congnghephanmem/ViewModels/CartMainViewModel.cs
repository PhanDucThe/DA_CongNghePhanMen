using congnghephanmem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class CartMainViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal { get; set; } // Tạm tính
        public decimal ShippingFee { get; set; } // Phí ship
        public decimal Total { get; set; } // Tổng cộng
        public decimal FreeShipThreshold { get; set; } = 150000; // Mốc Freeship (150k)

        // Danh sách gợi ý "Có thể bạn cần thêm"
        public List<product> Recommendations { get; set; }
    }
}