using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace congnghephanmem.Hubs
{
    public class ChatHub : Hub
    {
        public void SendMessage(string user, string message)
        {
            // 1. Gửi tin nhắn của người dùng lên giao diện (để hiện bên phải)
            Clients.All.addNewMessageToPage(user, message, "user");

            // 2. Logic CHATBOT tự động trả lời (Rule-based)
            string botAnswer = GetBotResponse(message.ToLower());

            // Giả lập độ trễ 1 chút cho giống người thật (500ms)
            System.Threading.Thread.Sleep(500);

            // 3. Bot trả lời (hiện bên trái)
            Clients.All.addNewMessageToPage("Nhà Thuốc Bot", botAnswer, "bot");
        }

        // Hàm xử lý kịch bản trả lời
        private string GetBotResponse(string input)
        {
            if (input.Contains("xin chào") || input.Contains("hi") || input.Contains("chào"))
                return "Xin chào! Nhà thuốc Online có thể giúp gì cho bạn?";

            if (input.Contains("đau đầu") || input.Contains("cảm cúm"))
                return "Bạn có thể tham khảo Panadol hoặc các loại thuốc giảm đau <a href='/Product' style='color: white; text-decoration: underline;'>tại đây</a>.";

            if (input.Contains("giao hàng") || input.Contains("ship"))
                return "Chúng tôi miễn phí vận chuyển cho đơn hàng trên 500k. Thời gian giao hàng từ 1-3 ngày.";

            if (input.Contains("tư vấn") || input.Contains("bác sĩ"))
                return "Bạn vui lòng để lại số điện thoại, dược sĩ sẽ gọi lại tư vấn trực tiếp ạ.";

            return "Cảm ơn bạn đã nhắn tin. Nhân viên sẽ phản hồi sớm nhất!";
        }
    }
}