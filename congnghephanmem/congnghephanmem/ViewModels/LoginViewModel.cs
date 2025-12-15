using System.ComponentModel.DataAnnotations;

namespace congnghephanmem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Số điện thoại")]
        public string Username { get; set; } // Dùng chung cho cả Email và SĐT

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}