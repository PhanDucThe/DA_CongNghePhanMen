using congnghephanmem.Helpers;
using congnghephanmem.Models;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class PaymentController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        public ActionResult VnPayPayment(int orderId)
        {
            var order = db.orders.Find(orderId);
            if (order == null) return HttpNotFound();

            string vnp_Returnurl = string.Format("{0}://{1}{2}",
                                    Request.Url.Scheme,
                                    Request.Url.Authority,
                                    Url.Action("PaymentCallback", "Payment"));

            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            if (!string.IsNullOrEmpty(vnp_HashSecret))
            {
                vnp_HashSecret = vnp_HashSecret.Trim();
            }

            decimal totalAmount = (order.total_money ?? 0m)
                                + ((decimal?)order.shipping_fee ?? 0m)
                                - (order.discount_amount ?? 0m);
            long vnp_Amount = (long)(totalAmount * 100);

            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", vnp_Amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");

            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ipAddress)) ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + order.id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);

            vnpay.AddRequestData("vnp_TxnRef", order.id.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(paymentUrl);
        }

        public ActionResult PaymentCallback()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef"); 
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                long orderId = Convert.ToInt64(vnp_TxnRef);

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (checkSignature)
                {
                    var order = db.orders.Find((int)orderId);

                    if (vnp_ResponseCode == "00")
                    {
                        if (order != null)
                        {
                            order.payment_status = "PAID"; 
                            order.status = "PROCESSING";   
                            db.SaveChanges();

                            try
                            {
                                var userEmail = db.users.Find(order.user_id)?.email;

                                if (!string.IsNullOrEmpty(userEmail))
                                {
                                    string body = EmailContentBuilder.BuildOrderSuccessEmail(order);
                                    string subject = $"Thanh toán thành công đơn hàng #{order.id} - Nhà Thuốc An Tâm";

                                    Task.Run(() =>
                                    {
                                        var emailService = new EmailService();
                                        emailService.SendEmail(userEmail, subject, body);
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error sending email: " + ex.Message);
                            }
                        }

                        return RedirectToAction("Success", "Checkout", new { id = orderId });
                    }
                    else
                    {
                        if (order != null)
                        {
                            order.payment_status = "FAILED"; 
                            order.status = "CANCELLED";     
                            db.SaveChanges();
                        }

                        ViewBag.Message = "Thanh toán thất bại hoặc bị hủy. Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Cảnh báo: Có lỗi xảy ra trong quá trình xử lý (Sai chữ ký bảo mật)";
                }
            }

            return View("PaymentFailure");
        }
    }
}