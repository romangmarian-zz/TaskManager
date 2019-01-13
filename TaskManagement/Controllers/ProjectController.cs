using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
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
        private ApplicationDbContext context;
        private ApplicationUserManager _userManager;

        

        public ProjectController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ProjectController()
        {
            context = new ApplicationDbContext();

        }

        protected override void Dispose(bool disposing)
        {
            context.Dispose();
        }

        // GET: Project
        [Authorize(Roles = "Administrator,Organizator,User")]
        public ActionResult Index()
        {

            ViewBag.UserId = User.Identity.GetUserId();
            var _list = context.Projects.ToList().Where(p =>
            p.Members.Select(m => m.Id).ToList().Contains(ViewBag.UserId) || p.OrganizerId == ViewBag.UserId);
            ViewBag.Projects = _list;
            _list = _list.ToList();

            ViewBag.ProjectsIsEmpty = _list.Any();
            return View();
        }

        // Get: Project/Create
        [Authorize(Roles = "Administrator,Organizator,User")]
        public ActionResult Create()
        {
            Project project = new Project
            {
                OrganizerId = User.Identity.GetUserId()
            };
            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Organizator,User")]
        public ActionResult Create(Project project)
        {
            project.OrganizerId = User.Identity.GetUserId();
            project.Organizer = context.Users.Find(project.OrganizerId);
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var _user = context.Users.Find(User.Identity.GetUserId());
            try
            {
                if (ModelState.IsValid)
                {
                    context.Projects.Add(project);
                    TempData["message"] = "Proiectul a fost creat";
                    context.SaveChanges();

                    if (User.IsInRole("User"))
                    {
                        UserManager.RemoveFromRole(_user.Id, "User");
                        UserManager.AddToRole(_user.Id, "Organizator");
                        context.SaveChanges();
                        Request.GetOwinContext().Authentication.SignOut();
                    }

                    

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

        [Authorize(Roles = "Administrator,Organizator")]
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
        [Authorize(Roles = "Administrator,Organizator")]
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
        [Authorize(Roles = "Administrator,Organizator")]
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

        [Authorize(Roles = "Administrator,Organizator,User")]
        public ActionResult Show(int id)
        {
            var project = context.Projects.Find(id);
            return View(project);
        }

        [Authorize(Roles = "Administrator,Organizator")]
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
        [Authorize(Roles = "Administrator,Organizator")]
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