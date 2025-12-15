using System.Web.Mvc;

namespace congnghephanmem.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            // Sau này bạn sẽ query database để lấy số liệu thật đưa vào View
            // Hiện tại mình trả về View rỗng để dựng giao diện trước
            return View();
        }
    }
}