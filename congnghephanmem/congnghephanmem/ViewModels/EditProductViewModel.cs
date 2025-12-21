using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class EditProductViewModel : CreateProductViewModel
    {
        public int Id { get; set; }

        // Đường dẫn ảnh hiện tại (để hiển thị cho admin xem)
        public string CurrentThumbnailUrl { get; set; }

        // Thêm 2 trường mới vào đây nếu bên CreateProductViewModel chưa có
        [Display(Name = "Dạng bào chế")]
        public string DosageForm { get; set; }

        [Display(Name = "Đối tượng sử dụng")]
        public string TargetAudience { get; set; }
    }
}