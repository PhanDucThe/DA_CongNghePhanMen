using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Display(Name = "Email đăng ký")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; }
    }
}