using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationDbContext context = ApplicationDbContext.Create();

        // GET: Project
        public ActionResult Index()
        {
            ViewBag.Projects = context.Projects;
            return View();
        }

        // Get: Project/Create
        public ActionResult Create()
        {
            Project project = new Project();
            project.OrganizerId = User.Identity.GetUserId();
            return View(project);
        }

        [HttpPost]
        public ActionResult Create(Project project)
        {
            project.OrganizerId = User.Identity.GetUserId();
            project.Organizer = context.Users.Find(project.OrganizerId);
            try
            {
                if (ModelState.IsValid)
                {
                    context.Projects.Add(project);
                    context.SaveChanges();
                    TempData["message"] = "Proiectul a fost creat";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(project);
                }
            }
            catch (Exception e)
            {
                return View(project);
            }
        }

        public ActionResult Edit(int id)
        {
            Project project = context.Projects.Find(id);
            if(project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "Not authorized to modify project";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        public ActionResult Edit(int id,Project editedProject)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    Project project = context.Projects.Find(id);

                    if(project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if(TryUpdateModel(project))
                        {
                            project.Title = editedProject.Title;
                            project.Description = editedProject.Description;
                            context.SaveChanges();
                            TempData["message"] = "Project edited successfuly";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Not authorized to edit project";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View();
                }
            }
            catch(Exception e)
            {
                return View();
            }
        }
        
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Project project = context.Projects.Find(id);

            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                context.Projects.Remove(project);
                context.SaveChanges();
                TempData["message"] = "Project deleted";
            }
            else
            {
                TempData["message"] = "Not authorized to modify project";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Show(int id)
        {
            var project = context.Projects.Find(id);
            return View(project);
        }

        public ActionResult AddMember(int id)
        {
            var model = new ProjectUsersViewModel
            {
                ProjectId = id,
                UsersSelect = GetUsers(id)
            };
            return View(model);
        }

        [HttpPut]
        public ActionResult AddMember(ProjectUsersViewModel projectUser)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var project = context.Projects.Find(projectUser.ProjectId);
                    var member = context.Users.Find(projectUser.SelectedUserId);

                    if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(project))
                        {
                            project.Members.Add(member);
                            context.SaveChanges();
                            TempData["message"] = "Member added successfuly";
                        }
                        return RedirectToAction("Show", new {  id = projectUser.ProjectId });
                    }
                    else
                    {
                        TempData["message"] = "Not authorized to add members to project";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(projectUser);
                }
            }
            catch (Exception e)
            {
                return View(projectUser);
            }
        }

        private IEnumerable<SelectListItem> GetUsers(int projectId)
        {
            var project = context.Projects.Find(projectId);
            var users = context.Users.ToList().Except(project.Members.ToList()).ToList();
            var usersSelect = users
                        .Select(x =>
                                new SelectListItem
                                {
                                    Value = x.Id,
                                    Text = x.UserName
                                });

            return new SelectList(usersSelect, "Value", "Text");
        }
    }
}