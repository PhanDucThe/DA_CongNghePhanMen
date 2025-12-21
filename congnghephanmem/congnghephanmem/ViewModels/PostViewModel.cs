using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace congnghephanmem.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string Excerpt { get; set; }

        [AllowHtml] // Cho phép chứa mã HTML từ CKEditor
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public HttpPostedFileBase ThumbnailImage { get; set; }
        public string CurrentThumbnailUrl { get; set; } // Dùng khi Edit để hiển thị ảnh cũ

        [Required(ErrorMessage = "Vui lòng chọn chuyên mục")]
        [Display(Name = "Chuyên mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } // "PUBLISHED" hoặc "DRAFT"
    }
}