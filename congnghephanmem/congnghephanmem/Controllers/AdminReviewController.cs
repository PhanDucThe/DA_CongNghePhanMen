using System;
using System.Linq;
using System.Web.Mvc;
using congnghephanmem.Models;
using congnghephanmem.ViewModels;

namespace congnghephanmem.Controllers
{
    public class AdminReviewController : Controller
    {
        private db_cnpmEntities db = new db_cnpmEntities();

        // GET: Admin/Review
        public ActionResult Index()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");


            var reviews = db.comments.Where(c => c.parent_id == null).OrderByDescending(c => c.created_at).ToList();


            var stats = new ReviewManageViewModel
            {
                Reviews = reviews,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? Math.Round((double)reviews.Average(c => c.rating), 1) : 0,
                UnrepliedCount = reviews.Count(c => !db.comments.Any(r => r.parent_id == c.id))
            };

            return View(stats);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int id, string status)
        {
            var review = db.comments.Find(id);
            if (review != null)
            {
                review.status = status; 
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var review = db.comments.Find(id);
            if (review != null)
            {
                var medias = db.comment_medias.Where(m => m.comment_id == id).ToList();
                if (medias.Any())
                {
                    foreach (var item in medias)
                    {
                        db.comment_medias.Remove(item);
                    }
                }

                var replies = db.comments.Where(r => r.parent_id == id).ToList();
                if (replies.Any())
                {
                    foreach (var item in replies)
                    {
                        db.comments.Remove(item);
                    }
                }

                db.comments.Remove(review);

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Reply(int id, string replyContent)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int adminId = (int)Session["UserID"];

            if (!string.IsNullOrEmpty(replyContent))
            {
                var existingReply = db.comments.FirstOrDefault(c => c.parent_id == id);

                if (existingReply != null)
                {
                    existingReply.content = replyContent;
                    existingReply.updated_at = DateTime.Now;
                }
                else
                {
                    var parentReview = db.comments.Find(id);

                    var reply = new comment
                    {
                        product_id = parentReview.product_id,
                        user_id = adminId,
                        parent_id = id, 
                        content = replyContent,
                        rating = 5, 
                        status = "approved",
                        created_at = DateTime.Now
                    };
                    db.comments.Add(reply);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}