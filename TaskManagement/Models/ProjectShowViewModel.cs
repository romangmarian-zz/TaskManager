using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskManagement.Models
{
    public class ProjectShowViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public IEnumerable<ApplicationUser> Members { get; set; }
        public IEnumerable<ProjectTask> Tasks { get; set; }
    }
}