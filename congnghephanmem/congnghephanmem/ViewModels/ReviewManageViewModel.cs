using congnghephanmem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class ReviewManageViewModel
    {
        public List<comment> Reviews { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int UnrepliedCount { get; set; }
    }
}