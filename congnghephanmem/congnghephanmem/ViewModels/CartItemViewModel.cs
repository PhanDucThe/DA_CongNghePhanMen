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
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Slug { get; set; }

        // --- SỬA DÒNG NÀY ---
        // Tự động tính tiền khi Price hoặc Quantity thay đổi
        // Không cần set thủ công
        public decimal Total => Price * Quantity;
    }
}