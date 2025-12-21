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
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }

        // --- THÊM MỚI ---
        public decimal DiscountAmount { get; set; } // Số tiền được giảm
        public string CouponCode { get; set; }      // Mã đang áp dụng
                                                    // ----------------

        public decimal Total { get; set; }
        public decimal FreeShipThreshold { get; set; } = 150000; // Ví dụ mốc freeship
        public List<product> Recommendations { get; set; }
    }
}