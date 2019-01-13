using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskManagement.Models
{
    public class TaskStatusViewModel
    {
        [Display(Name = "Status")]
        public int TaskId { get; set; }
        public string Status { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
    }
}