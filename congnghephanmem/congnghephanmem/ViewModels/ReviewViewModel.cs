using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class ReviewViewModel
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public List<HttpPostedFileBase> Images { get; set; } // Danh sách ảnh upload
    }
}