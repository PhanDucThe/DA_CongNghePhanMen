using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; } // Thumbnail
        public string Slug { get; set; } // Để click vào xem chi tiết
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; } // Để hiện giá gốc nếu cần
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}