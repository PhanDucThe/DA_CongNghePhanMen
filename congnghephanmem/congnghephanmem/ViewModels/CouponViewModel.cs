using System;
using System.ComponentModel.DataAnnotations;

namespace congnghephanmem.Models
{
    public class CouponViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi")]
        [Display(Name = "Mã Code")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã chỉ chứa chữ hoa và số, không dấu cách")]
        public string Code { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string Description { get; set; } // Có thể lưu vào bảng coupons nếu mở rộng, hoặc chỉ để hiển thị

        // Dữ liệu cho bảng Coupon_Conditions
        [Required(ErrorMessage = "Vui lòng nhập giá trị đơn hàng tối thiểu")]
        [Display(Name = "Đơn hàng tối thiểu")]
        public decimal MinOrderValue { get; set; } // Sẽ map vào cột 'value' với attribute='total_money'

        [Required(ErrorMessage = "Vui lòng nhập mức giảm")]
        [Display(Name = "Mức giảm")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; } // 'PERCENTAGE' hoặc 'FIXED_AMOUNT'

        public bool IsActive { get; set; }
    }
}