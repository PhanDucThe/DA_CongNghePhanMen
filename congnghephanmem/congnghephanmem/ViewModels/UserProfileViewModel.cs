using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } // Email thường không cho sửa

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } // Giả sử bảng users có cột address (hoặc bạn tự thêm)

        public string CurrentAvatar { get; set; } // Link ảnh cũ

        [Display(Name = "Thay đổi ảnh đại diện")]
        public HttpPostedFileBase AvatarFile { get; set; } // File ảnh mới upload
    }
}