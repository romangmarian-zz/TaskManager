using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagement.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string Message { get; set; }
        public DateTime PostedAt { get; set; }

        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public string TaskId { get; set; }
        public virtual ProjectTask Task { get; set; }
    }
}