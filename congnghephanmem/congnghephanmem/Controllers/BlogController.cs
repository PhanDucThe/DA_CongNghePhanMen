using System;
using System.Linq;
using System.Web.Mvc;
using congnghephanmem.Models;

namespace congnghephanmem.Controllers
{
    public class BlogController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        public ActionResult Index(int? categoryId)
        {
            ViewBag.Categories = db.post_categories.ToList();

            var posts = db.posts.Where(p => p.status == "PUBLISHED");

            if (categoryId.HasValue)
            {
                posts = posts.Where(p => p.category_id == categoryId.Value);
                var category = db.post_categories.Find(categoryId);
                ViewBag.CurrentCategory = category != null ? category.name : "";
            }

            var result = posts.OrderByDescending(p => p.published_at).ToList();

            return View(result);
        }

        public ActionResult Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return RedirectToAction("Index");

            var post = db.posts.FirstOrDefault(p => p.slug == slug && p.status == "PUBLISHED");

            if (post == null) return RedirectToAction("Index");
            post.view_count = (post.view_count ?? 0) + 1;
            db.SaveChanges(); 
            ViewBag.RelatedPosts = db.posts
                .Where(p => p.category_id == post.category_id && p.id != post.id && p.status == "PUBLISHED")
                .OrderByDescending(p => p.published_at)
                .Take(4)
                .ToList();
            ViewBag.Categories = db.post_categories.ToList();

            return View(post);
        }
    }
}