using congnghephanmem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using congnghephanmem.Models;

namespace congnghephanmem.Helpers
{
    public class EmailContentBuilder
    {
        public static string BuildOrderSuccessEmail(order order)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden;'>");
            sb.Append("<div style='background-color: #13ecc8; padding: 20px; text-align: center;'>");
            sb.Append($"<h2 style='color: white; margin: 0;'>Xác nhận đơn hàng #{order.id}</h2>");
            sb.Append("</div>");
            sb.Append("<div style='padding: 20px;'>");
            sb.Append($"<p>Xin chào <b>{order.full_name}</b>,</p>");
            sb.Append("<p>Cảm ơn bạn đã đặt hàng tại Nhà Thuốc An Tâm. Đơn hàng của bạn đang được xử lý.</p>");

            sb.Append("<h3 style='border-bottom: 2px solid #13ecc8; padding-bottom: 5px;'>Thông tin đơn hàng</h3>");
            sb.Append($"<p><b>Ngày đặt:</b> {order.created_at:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p><b>Địa chỉ giao:</b> {order.shipping_address}</p>");
            sb.Append($"<p><b>Phương thức thanh toán:</b> {order.payment_method}</p>");

            sb.Append("<h3 style='border-bottom: 2px solid #13ecc8; padding-bottom: 5px; margin-top: 20px;'>Chi tiết sản phẩm</h3>");
            sb.Append("<table style='width: 100%; border-collapse: collapse;'>");
            sb.Append("<thead><tr style='background-color: #f9fafb;'>");
            sb.Append("<th style='padding: 10px; text-align: left;'>Sản phẩm</th>");
            sb.Append("<th style='padding: 10px; text-align: center;'>SL</th>");
            sb.Append("<th style='padding: 10px; text-align: right;'>Đơn giá</th>");
            sb.Append("<th style='padding: 10px; text-align: right;'>Thành tiền</th>");
            sb.Append("</tr></thead><tbody>");

            // Note: You need to ensure order.order_items is populated. 
            // If using EF lazy loading, it might be null if not included.
            if (order.order_items != null)
            {
                foreach (var item in order.order_items)
                {
                    // Handle potential null product reference
                    string productName = item.product != null ? item.product.name : "Sản phẩm #" + item.product_id;
                    // Cách 1: Dùng Convert.ToDecimal (Khuyên dùng)
                    decimal total = Convert.ToDecimal(item.price) * Convert.ToDecimal(item.quantity);

                    sb.Append("<tr>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #ddd;'>{productName}</td>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.quantity}</td>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{string.Format("{0:0,0}đ", item.price)}</td>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{string.Format("{0:0,0}đ", total)}</td>");
                    sb.Append("</tr>");
                }
            }
            
            sb.Append("</tbody><tfoot><tr>");
            sb.Append("<td colspan='3' style='padding: 10px; text-align: right; font-weight: bold;'>Tổng cộng:</td>");
            // Assuming total_money is the final amount (subtotal + shipping - discount)
            decimal finalTotal = (order.total_money ?? 0) + Convert.ToDecimal(order.shipping_fee) - (order.discount_amount ?? 0);
            sb.Append($"<td style='padding: 10px; text-align: right; font-weight: bold; color: #d32f2f;'>{string.Format("{0:0,0}đ", finalTotal)}</td>");
            sb.Append("</tr></tfoot></table>");
            sb.Append("</div></div>");

            return sb.ToString();
        }
    }
}