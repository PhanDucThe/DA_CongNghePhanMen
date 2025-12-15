using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using congnghephanmem.Models;

namespace congnghephanmem.Controllers
{
    public class PostController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: Admin/Post
        public ActionResult Index()
        {
            // Lấy danh sách bài viết, sắp xếp mới nhất
            var posts = db.posts.OrderByDescending(p => p.created_at).ToList();
            return View(posts);
        }

        // GET: Admin/Post/Create
        public ActionResult Create()
        {
            // Load danh sách chuyên mục vào Dropdown
            ViewBag.Categories = new SelectList(db.post_categories.ToList(), "id", "name");
            return View();
        }

        // POST: Admin/Post/Create
        [HttpPost]
        [ValidateInput(false)] // Tắt kiểm tra bảo mật mặc định để nhận HTML từ CKEditor
        public ActionResult Create(PostViewModel model)
        {
            if (ModelState.IsValid)
            {
                string thumbnailUrl = "/Content/images/no-image.png"; // Ảnh mặc định

                // 1. Xử lý Upload ảnh
                if (model.ThumbnailImage != null && model.ThumbnailImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(model.ThumbnailImage.FileName);
                    string extension = Path.GetExtension(model.ThumbnailImage.FileName);
                    fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + extension;

                    // Lưu vào thư mục /Content/images/posts/
                    string path = Path.Combine(Server.MapPath("~/Content/images/posts/"), fileName);

                    // Tạo thư mục nếu chưa có
                    Directory.CreateDirectory(Server.MapPath("~/Content/images/posts/"));

                    model.ThumbnailImage.SaveAs(path);
                    thumbnailUrl = "/Content/images/posts/" + fileName;
                }

                // 2. Tạo đối tượng Post
                var newPost = new post
                {
                    title = model.Title,
                    slug = GenerateSlug(model.Title), // Hàm tạo slug ở dưới
                    excerpt = model.Excerpt,
                    content = model.Content,
                    thumbnail_url = thumbnailUrl,
                    category_id = model.CategoryId,
                    author_id = 1, // Tạm thời set cứng ID admin, sau này lấy Session["UserID"]
                    status = model.Status, // PUBLISHED hoặc DRAFT
                    published_at = model.Status == "PUBLISHED" ? DateTime.Now : (DateTime?)null,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    view_count = 0
                };

                db.posts.Add(newPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.post_categories.ToList(), "id", "name", model.CategoryId);
            return View(model);
        }

        // Xóa bài viết
        public ActionResult Delete(int id)
        {
            var post = db.posts.Find(id);
            if (post != null)
            {
                db.posts.Remove(post);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // --- HELPER: Hàm tạo Slug (Tiếng Việt có dấu -> Không dấu) ---
        public string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            // Xóa dấu tiếng Việt
            str = Regex.Replace(str, @"[áàạảãâấầậẩẫăắằặẳẵ]", "a");
            str = Regex.Replace(str, @"[éèẹẻẽêếềệểễ]", "e");
            str = Regex.Replace(str, @"[óòọỏõôốồộổỗơớờợởỡ]", "o");
            str = Regex.Replace(str, @"[úùụủũưứừựửữ]", "u");
            str = Regex.Replace(str, @"[íìịỉĩ]", "i");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[ýỳỵỷỹ]", "y");

            // Xóa ký tự đặc biệt
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Chuyển khoảng trắng thành gạch ngang
            str = Regex.Replace(str, @"\s+", "-").Trim();

            return str;
        }
    }
}