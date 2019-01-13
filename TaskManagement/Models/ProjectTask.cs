using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagement.Models
{
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public string Status { get; set; }

        public DateTime StartDate { get; set; }
        [Required]
        public DateTime Deadline { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        
        public string AssigneeID { get; set; }
        public virtual ApplicationUser Assignee { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}