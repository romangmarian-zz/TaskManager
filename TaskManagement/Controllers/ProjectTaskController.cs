using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class ProjectTaskController : Controller
    {
        private ApplicationDbContext context = ApplicationDbContext.Create();
        // GET: ProjectTask
        //int id = the id of the event whose tasks we want to view
        public ActionResult Index(int id)
        {
            ViewBag.Tasks = context.Tasks.Include("Assignee");
            ViewBag.EventId = id;
            return View();
        }
    }
}