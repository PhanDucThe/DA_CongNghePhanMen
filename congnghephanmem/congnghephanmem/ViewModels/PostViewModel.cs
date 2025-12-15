using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc; // Để dùng [AllowHtml]

namespace congnghephanmem.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Tóm tắt ngắn")]
        public string Excerpt { get; set; }

        [AllowHtml] // Cho phép chứa thẻ HTML từ CKEditor
        [Display(Name = "Nội dung bài viết")]
        public string Content { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public HttpPostedFileBase ThumbnailImage { get; set; } // Hứng file upload
        public string ExistingThumbnailUrl { get; set; } // Lưu đường dẫn cũ nếu đang sửa

        [Required(ErrorMessage = "Vui lòng chọn chuyên mục")]
        public int CategoryId { get; set; }

        public string Status { get; set; } // 'PUBLISHED', 'DRAFT'
    }
}