using System;
using System.Net.Mail;
using System.Configuration; // Dùng để đọc Web.config
using System.Net.Configuration; // Dùng để lấy thông tin SMTP section

namespace congnghephanmem.Helpers
{
    public class EmailService
    {
        /// <summary>
        /// Hàm gửi email dùng chung cho toàn hệ thống
        /// </summary>
        /// <param name="toEmail">Địa chỉ người nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="body">Nội dung email (Hỗ trợ HTML)</param>
        /// <returns>True nếu gửi thành công, False nếu thất bại</returns>
        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {

                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                string fromEmail = smtpSection.From;

                MailMessage message = new MailMessage();

                message.From = new MailAddress(fromEmail, "Nhà Thuốc An Tâm");

                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true; 

                using (var client = new SmtpClient())
                {
                    client.Send(message);
                }

                return true; 
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}