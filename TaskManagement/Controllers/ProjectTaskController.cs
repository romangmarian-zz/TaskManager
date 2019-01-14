using Microsoft.AspNet.Identity;
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
        [Authorize(Roles = "Administrator,Organizator,User")]
        public ActionResult Index(int id)
        {
     
            ViewBag.ProjectId = id;
            var project = context.Projects.Find(id);
            ViewBag.Tasks = project.Tasks;
            var projectTitle = context.Projects.Find(id).Title;
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator") ||
                project.Members.Select(m => m.Id).ToList().Contains(User.Identity.GetUserId()))
            {
                return View();
            }
            else
            {
                TempData["Message"] = "Not authorized to access this project";
                return RedirectToAction("Index");
            }
        }

        //id = project id
        [Authorize(Roles = "Administrator,Organizator")]
        public ActionResult Create(int id)
        {
            ProjectTask task = new ProjectTask();
            task.ProjectId = id;
            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Organizator")]
        public ActionResult Create(ProjectTask task)
        {
            task.Project = context.Projects.Find(task.ProjectId);
            task.StartDate = DateTime.Now;
            task.Status = "Unassigned";
            try
            {
                if (ModelState.IsValid)
                {
                    context.Tasks.Add(task);
                    context.SaveChanges();
                    TempData["message"] = "Task created";
                    return RedirectToAction("Show", "Project", new { id = task.ProjectId});
                }
                else
                {
                    return View(task);
                }
            }
            catch (Exception e)
            {
                return View(task);
            }
        }

        //id = task id
        [Authorize(Roles = "Administrator,Organizator")]
        public ActionResult Edit(int id)
        {
            ProjectTask task = context.Tasks.Find(id);
            if (task.Project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(task);
            }
            else
            {
                TempData["message"] = "Not authorized to modify project";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator,Organizator")]
        public ActionResult Edit(int id, ProjectTask editedTask)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    ProjectTask task = context.Tasks.Find(id);
                    if (task.Project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(task))
                        {
                            task.Title = editedTask.Title;
                            task.Description = editedTask.Description;
                            task.Deadline = editedTask.Deadline;
                            context.SaveChanges();
                            TempData["message"] = "Task edited successfuly";
                        }

                        return RedirectToAction("Show","Project", new { id = task.Project.Id });
                    }
                    else
                    {
                        TempData["message"] = "Not authorized to edit tasks";
                        return RedirectToAction("Show","Project" , new { id = task.Project.Id });
                    }
                }
                else
                {
                    return View();
                }
               
            }
            catch(Exception e)
            {
                return View(editedTask);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator,Organizator")]
        public ActionResult Delete(int id)
        {
            var task = context.Tasks.Find(id);
            var projId = task.Project.Id;
            if (task.Project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                context.Tasks.Remove(task);
                context.SaveChanges();
                TempData["message"] = "Task deleted";
            }
            else
            {
                TempData["message"] = "Not authorized to modify project";
            }
            return RedirectToAction("Show", "Project", new { id = projId });
        }

        public ActionResult Show(int id)
        {
            var task = context.Tasks.Find(id);
            ViewBag.ProjectTitle = task.Project.Title;
            ViewBag.ProjectId = task.Project.Id;
            return View(task);
        }

        public ActionResult Assign(int id)
        {
            var task = context.Tasks.Find(id);
            var members = task.Project.Members.Select(x =>
                                new SelectListItem
                                {
                                    Value = x.Id,
                                    Text = x.UserName
                                });
            var membersSelect = new SelectList(members, "Value", "Text");
            var model = new TaskAssigneeViewModel
            {
                TaskId = id,
                MembersSelect = membersSelect
            };
            return View(model);
        }

        [HttpPut]
        public ActionResult Assign(TaskAssigneeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var task = context.Tasks.Find(model.TaskId);
                    var assignee = context.Users.Find(model.AssigneeId);

                    if (task.Project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(task))
                        {
                            task.AssigneeID = assignee.Id;
                            task.Assignee = assignee;
                            task.Status = "Assigned";
                            context.SaveChanges();
                            TempData["message"] = "Assigned successfuly";
                        }
                        return RedirectToAction("Show", new { id = model.TaskId });
                    }
                    else
                    {
                        TempData["message"] = "Not authorized to assign tasks for this project";
                        return RedirectToAction("Index", new { id = task.Project.Id });
                    }
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception e)
            {
                return View(model);
            }
        }

        public ActionResult UpdateStatus(int id)
        {
            var task = context.Tasks.Find(id);
            var userId = User.Identity.GetUserId();
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "In Progress",
                Value = "In Progress"
            });
            listItems.Add(new SelectListItem
            {
                Text = "In Review",
                Value = "In Review"
            });
            listItems.Add(new SelectListItem
            {
                Text = "In Deployment",
                Value = "In Deployment"
            });
            listItems.Add(new SelectListItem
            {
                Text = "Done",
                Value = "Done"
            });
            var taskStatus = new TaskStatusViewModel
            {
                TaskId = task.Id,
                Statuses = listItems
            };
            if (User.IsInRole("Administrator") || task.AssigneeID == User.Identity.GetUserId())
            {
                return View(taskStatus);
            }
            else
            {
                return RedirectToAction("Index", "Project");
            }
        }

        [HttpPut]
        public ActionResult UpdateStatus(TaskStatusViewModel statusEdit)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            try
            {
                if (ModelState.IsValid)
                {
                    var task = context.Tasks.Find(statusEdit.TaskId);

                    if (task.AssigneeID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(task))
                        {
                            task.Status = statusEdit.Status;
                            context.SaveChanges();
                            TempData["message"] = "Status Updated";
                        }
                        return RedirectToAction("Show", new { id = statusEdit.TaskId });
                    }
                    else
                    {
                        TempData["message"] = "Not authorized to update tasks for this project";
                        return RedirectToAction("Index", new { id = task.Project.Id });
                    }
                }
                else
                {
                    return View(statusEdit);
                }
            }
            catch (Exception e)
            {
                return View(statusEdit);
            }
        }
    }
}