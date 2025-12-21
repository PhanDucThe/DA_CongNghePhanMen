using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using congnghephanmem.Models; 

namespace congnghephanmem.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            Clients.All.addNewMessageToPage(user, message, "user");

            string botAnswer = GetSmartResponse(message);

            await Task.Delay(800);

            Clients.All.addNewMessageToPage("Nhà Thuốc An Tâm", botAnswer, "bot");
        }

        private string GetSmartResponse(string input)
        {
            input = input.ToLower().Trim();

            
            using (var db = new db_cnpmEntities())
            {
                // --- KỊCH BẢN 1: TÌM KIẾM SẢN PHẨM ---
                if (input.Contains("có bán") || input.Contains("tìm") || input.Contains("mua") || input.Contains("giá"))
                {
                    return SearchProductInDb(db, input);
                }

                // --- KỊCH BẢN 2: TRA CỨU ĐƠN HÀNG ---
                if (input.Contains("đơn hàng") || input.Contains("tra cứu"))
                {
                    // Giả sử user nhập: "Tra cứu đơn hàng #12" hoặc "đơn hàng 12"
                    string orderIdStr = System.Text.RegularExpressions.Regex.Match(input, @"\d+").Value;
                    if (!string.IsNullOrEmpty(orderIdStr))
                    {
                        return CheckOrderStatus(db, int.Parse(orderIdStr));
                    }
                    return "Vui lòng nhập mã đơn hàng bạn muốn tra cứu (Ví dụ: Đơn hàng 12)";
                }

                // --- KỊCH BẢN 3: CÁC CÂU HỎI THƯỜNG GẶP (FAQ) ---
                if (input.Contains("xin chào") || input.Contains("hi ") || input == "hi" || input.Contains("chào"))
                    return "Chào bạn! Tôi là trợ lý ảo AI. Tôi có thể giúp bạn tìm thuốc, tra cứu đơn hàng hoặc tư vấn sức khỏe cơ bản.";

                if (input.Contains("địa chỉ") || input.Contains("ở đâu"))
                    return "Nhà thuốc có địa chỉ tại: 123 Đường ABC, Quận XYZ, TP.HCM. Mở cửa từ 7:00 - 22:00 hàng ngày.";

                if (input.Contains("đau đầu") || input.Contains("cảm cúm") || input.Contains("sốt"))
                    return "Nếu bạn bị đau đầu hoặc sốt, bạn có thể tham khảo các loại thuốc giảm đau hạ sốt như Panadol hoặc Efferalgan. <br/> <a href='/Category?id=5' style='color: #00C092; font-weight:bold;'>Xem danh mục Thuốc cảm sốt tại đây</a>";

                if (input.Contains("tư vấn") || input.Contains("dược sĩ"))
                    return "Để được tư vấn kỹ hơn, bạn vui lòng gọi Hotline: <b>1900 1234</b> hoặc để lại SĐT, dược sĩ sẽ liên hệ lại ngay.";

                // --- KỊCH BẢN 4: KHÔNG HIỂU ---
                return "Xin lỗi, tôi chưa hiểu ý bạn lắm. Bạn thử hỏi về tên thuốc, triệu chứng hoặc mã đơn hàng xem sao?";
            }
        }

        // Hàm tìm sản phẩm trong Database
        private string SearchProductInDb(db_cnpmEntities db, string keyword)
        {
            // Lọc bớt các từ khóa rác để lấy tên sản phẩm chính
            string[] stopWords = { "có bán", "tìm", "mua", "thuốc", "giá", "bao nhiêu", "không", "ơi", "cho", "mình" };
            foreach (var word in stopWords)
            {
                keyword = keyword.Replace(word, "").Trim();
            }

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 2)
                return "Bạn muốn tìm thuốc gì ạ? Hãy gõ tên thuốc cụ thể nhé.";

            // Tìm trong DB (So khớp tên thuốc)
            var products = db.products
                             .Where(p => p.name.Contains(keyword) && p.is_active == true)
                             .Take(3) 
                             .ToList();

            if (products.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"Mình tìm thấy {products.Count} sản phẩm phù hợp:<br/>");
                foreach (var p in products)
                {
                    string price = string.Format("{0:0,0}đ", p.sale_price);
                    sb.Append($"- <a href='/Product/Detail/{p.id}' target='_blank' style='color: #007bff; text-decoration: none; font-weight: bold;'>{p.name}</a>: <span style='color:red'>{price}</span><br/>");
                }
                return sb.ToString();
            }
            else
            {
                return $"Tiếc quá, nhà thuốc hiện chưa có sản phẩm tên là '{keyword}'. Bạn có thể thử tìm từ khóa khác xem sao.";
            }
        }


        private string CheckOrderStatus(db_cnpmEntities db, int orderId)
        {
            var order = db.orders.Find(orderId);
            if (order != null)
            {
                string statusVi = "";
                switch (order.status)
                {
                    case "PENDING_CONFIRMATION": statusVi = "Chờ xác nhận"; break;
                    case "PROCESSING": statusVi = "Đang xử lý / Đã thanh toán"; break;
                    case "SHIPPING": statusVi = "Đang giao hàng"; break;
                    case "COMPLETED": statusVi = "Đã giao thành công"; break;
                    case "CANCELLED": statusVi = "Đã hủy"; break;
                    default: statusVi = order.status; break;
                }
                return $"Đơn hàng <b>#{orderId}</b> của bạn đang ở trạng thái: <b style='color: #00C092'>{statusVi}</b>.<br/>Tổng tiền: {string.Format("{0:0,0}đ", order.total_money)}.";
            }
            return $"Không tìm thấy đơn hàng nào có mã #{orderId}. Bạn kiểm tra lại nhé.";
        }
    }
}