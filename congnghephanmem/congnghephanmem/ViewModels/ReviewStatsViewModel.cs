using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.ViewModels
{
    public class ReviewStatsViewModel
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStar { get; set; }
        public int FourStar { get; set; }
        public int ThreeStar { get; set; }
        public int TwoStar { get; set; }
        public int OneStar { get; set; }
    }
}