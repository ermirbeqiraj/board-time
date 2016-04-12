using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MvcToDo.Models;

namespace MvcToDo.ModelsView
{
    public class TaskItemView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TaskItemView(List<Comments> comments, string assigned, List<Files> files, TaskItem T)
        {
            this.Comments = comments;
            this.TaskAssigned = assigned;
            this.UploadedFiles = files;

            Id = T.Id;
            Name = T.Name;
            Description = T.Description;
            DueDate = T.DueDate;
            Priority = T.Priority;
            StartDate = T.StartDate.HasValue ? T.StartDate.Value : new DateTime();
            TimeEstimated = T.TimeEstimated;
            Mark = T.Mark;
            Status = T.Status ? "Active" : "In Active";
            Created = T.Created;
            LastModified = T.LastModified;
            ParentId = T.ParentId.HasValue ? T.ParentId.Value : -1;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name="Project Name")]
        public string ProjectName { get; set; }
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DueDate { get; set; }
        public byte Priority { get; set; }
        [Display(Name="Parent Task")]
        public int? ParentId { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime StartDate { get; set; }
        [Display(Name = "Time Estimated")]
        public byte? TimeEstimated { get; set; }
        public byte Mark { get; set; }
        public string Status { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime Created { get; set; }
        [Display(Name = "Last Modified")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime LastModified { get; set; }
        public string Author { get; set; }
        [Display(Name = "Task Mark")]
        public string TaskMark{ get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        [Display(Name = "Task Assigned")]
        public string TaskAssigned { get; set; }
        public virtual ICollection<Files> UploadedFiles { get; set; }
    }
}