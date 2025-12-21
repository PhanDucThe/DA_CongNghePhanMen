using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace congnghephanmem.ViewModels
{
    public class CreateProductViewModel
    {
        [Display(Name = "Tên thuốc")]
        [Required(ErrorMessage = "Vui lòng nhập tên thuốc")]
        public string Name { get; set; }

        [Display(Name = "Mã SKU")]
        [Required(ErrorMessage = "Vui lòng nhập mã SKU")]
        public string Sku { get; set; }

        [Display(Name = "Giá gốc")]
        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        public decimal OriginalPrice { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Tồn kho")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int StockQuantity { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string Description { get; set; }

        [AllowHtml] // Cho phép nội dung HTML từ CKEditor
        [Display(Name = "Nội dung chi tiết")]
        public string Content { get; set; }

        // --- CÁC TRƯỜNG CHỌN ---
        [Required(ErrorMessage = "Chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Chọn thương hiệu")]
        public int BrandId { get; set; }

        [Display(Name = "Dạng bào chế")]
        public string DosageForm { get; set; } // Viên nén, Siro...

        [Display(Name = "Đối tượng sử dụng")]
        public string TargetAudience { get; set; } // Người lớn, Trẻ em...

        // --- HÌNH ẢNH ---
        [Display(Name = "Ảnh đại diện chính")]
        public HttpPostedFileBase ThumbnailFile { get; set; }

        [Display(Name = "Album ảnh phụ")]
        public IEnumerable<HttpPostedFileBase> GalleryFiles { get; set; }

        public bool IsActive { get; set; } = true;
    }
}