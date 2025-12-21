using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using congnghephanmem.Models;
using congnghephanmem.ViewModels; 

namespace congnghephanmem.Controllers
{
    public class PostController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();


        public ActionResult Index()
        {

            if (Session["UserID"] == null) return RedirectToAction("Login", "Account"); 

            var posts = db.posts.OrderByDescending(p => p.created_at).ToList();
            return View(posts);
        }

        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            ViewBag.Categories = new SelectList(db.post_categories.ToList(), "id", "name");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(PostViewModel model)
        {
            if (ModelState.IsValid)
            {
                string thumbnailUrl = "/Content/images/no-image.png";

                if (model.ThumbnailImage != null && model.ThumbnailImage.ContentLength > 0)
                {
                    thumbnailUrl = UploadImage(model.ThumbnailImage);
                }


                var newPost = new post
                {
                    title = model.Title,
                    slug = GenerateSlug(model.Title),
                    excerpt = model.Excerpt,
                    content = model.Content,
                    thumbnail_url = thumbnailUrl,
                    category_id = model.CategoryId,
                    author_id = (int)Session["UserID"], 
                    status = model.Status,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    view_count = 0
                };

                if (model.Status == "PUBLISHED")
                {
                    newPost.published_at = DateTime.Now;
                }

                db.posts.Add(newPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.Categories = new SelectList(db.post_categories.ToList(), "id", "name", model.CategoryId);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            var post = db.posts.Find(id);
            if (post == null) return HttpNotFound();

            var model = new PostViewModel
            {
                Id = post.id,
                Title = post.title,
                Excerpt = post.excerpt,
                Content = post.content,
                CurrentThumbnailUrl = post.thumbnail_url,
                CategoryId = post.category_id ?? 0,
                Status = post.status
            };

            ViewBag.Categories = new SelectList(db.post_categories.ToList(), "id", "name", post.category_id);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(PostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var post = db.posts.Find(model.Id);
                if (post == null) return HttpNotFound();

                post.title = model.Title;
                post.excerpt = model.Excerpt;
                post.content = model.Content;
                post.category_id = model.CategoryId;
                post.status = model.Status;
                post.updated_at = DateTime.Now;

                if (model.Status == "PUBLISHED" && post.published_at == null)
                {
                    post.published_at = DateTime.Now;
                }


                if (model.ThumbnailImage != null && model.ThumbnailImage.ContentLength > 0)
                {
                    post.thumbnail_url = UploadImage(model.ThumbnailImage);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(db.post_categories.ToList(), "id", "name", model.CategoryId);
            return View(model);
        }


        public ActionResult Delete(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            var post = db.posts.Find(id);
            if (post != null)
            {
                db.posts.Remove(post);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private string UploadImage(HttpPostedFileBase file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;

            string relativePath = "~/Content/images/posts/";
            string absolutePath = Server.MapPath(relativePath);

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            string savePath = Path.Combine(absolutePath, fileName);
            file.SaveAs(savePath);

            return "/Content/images/posts/" + fileName;
        }

        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";

            string str = phrase.ToLower();
            str = Regex.Replace(str, @"[áàạảãâấầậẩẫăắằặẳẵ]", "a");
            str = Regex.Replace(str, @"[éèẹẻẽêếềệểễ]", "e");
            str = Regex.Replace(str, @"[óòọỏõôốồộổỗơớờợởỡ]", "o");
            str = Regex.Replace(str, @"[úùụủũưứừựửữ]", "u");
            str = Regex.Replace(str, @"[íìịỉĩ]", "i");
            str = Regex.Replace(str, @"[đ]", "d");
            str = Regex.Replace(str, @"[ýỳỵỷỹ]", "y");

            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-").Trim();

            str = Regex.Replace(str, @"-+", "-");

            return str;
        }
    }
}