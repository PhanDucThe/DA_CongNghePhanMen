using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace congnghephanmem.Helpers
{
    public class ShippingHelper
    {
        public static decimal STANDARD_FEE = 20000; 
        public static decimal EXPRESS_FEE = 40000;  
        public static decimal FREE_SHIP_THRESHOLD = 150000; 

        public static decimal CalculateFee(decimal subTotal, string method = "STANDARD")
        {
            if (method == "EXPRESS")
            {
                return EXPRESS_FEE;
            }
            else
            {
                if (subTotal >= FREE_SHIP_THRESHOLD || subTotal == 0)
                    return 0;
                else
                    return STANDARD_FEE;
            }
        }
    }
}