using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: Comments
        public ActionResult Index()
        {
            var comments = context.Comments.Include(c => c.Author);
            return View(comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = context.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        // id == task id
        public ActionResult Create(int id)
        {
            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.TaskId = id;
            return View(comment);
        }

        // POST: Comments/Create
        [HttpPost]
        public ActionResult Create(Comment comment)
        {
            if (ModelState.IsValid)
            {
                var task = context.Tasks.Find(comment.TaskId);
                var author = context.Users.Find(comment.AuthorId);
                comment.Task = task;
                comment.Author = author;
                comment.PostedAt = DateTime.Now;
                context.Comments.Add(comment);
                context.SaveChanges();
                return RedirectToAction("Show", "ProjectTask", new { id = comment.TaskId });
            }
            return View(comment);
        }

        // GET: Comments/Edit/5
        // id == comment id
        public ActionResult Edit(int id)
        {
            var comment = context.Comments.Find(id);
            if (User.Identity.GetUserId() == comment.AuthorId || User.IsInRole("Administrator"))
            {
                return View(comment);
            }
            TempData["message"] = "No authoriez to edit comment";
            return RedirectToAction("Show", "ProjectTask", new { id = comment.TaskId });
        }

        // POST: Comments/Edit/5
        [HttpPut]
        public ActionResult Edit(int id, Comment editedComment)
        {
            try
            {
               
                var comment = context.Comments.Find(id);
                var task = context.Tasks.Find(comment.TaskId);
                if (User.Identity.GetUserId() == comment.AuthorId || User.IsInRole("Administrator"))
                {
                    if (TryUpdateModel(comment))
                    {
                        comment.Message = editedComment.Message;
                        context.SaveChanges();
                    }
                    return RedirectToAction("Show", "ProjectTask", new { id = comment.TaskId });
                }
                else
                {
                    TempData["message"] = "Not authorized to edit project";
                    return RedirectToAction("Show", "ProjectTask", new { id = comment.TaskId });
                }
            }
            catch (Exception e)
            {
                return View(editedComment);
            }
        }

        // POST: Comments/Delete/5
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comment = context.Comments.Find(id);
            var taskId = comment.TaskId;
            if (User.Identity.GetUserId() == comment.AuthorId || User.IsInRole("Administrator"))
            {
                context.Comments.Remove(comment);
                context.SaveChanges();
                return RedirectToAction("Show", "ProjectTask", new { id = taskId });
            }
            else
            {
                TempData["message"] = "Not allowed to delete comment";
                return RedirectToAction("Show", "ProjectTask", new { id = taskId });
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
