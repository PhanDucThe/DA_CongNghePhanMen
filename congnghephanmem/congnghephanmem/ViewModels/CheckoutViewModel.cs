using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class CheckoutViewModel
    {
        // 1. Thông tin người nhận
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        public string Note { get; set; }

        // 2. Phương thức thanh toán & vận chuyển
        public string ShippingMethod { get; set; } = "STANDARD"; // 'STANDARD', 'EXPRESS'
        public string PaymentMethod { get; set; } = "COD"; // 'COD', 'BANK', 'MOMO'

        // 3. Thông tin đơn hàng (Chỉ hiển thị, không post ngược lại)
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
    }
}