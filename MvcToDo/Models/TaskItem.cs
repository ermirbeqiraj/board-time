namespace MvcToDo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskItem")]
    public partial class TaskItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TaskItem()
        {
            Comments = new HashSet<Comments>();
            TaskAssigned = new HashSet<TaskAssigned>();
            TaskFiles = new HashSet<TaskFiles>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        [Display(Name="Parent")]
        public int? ParentId { get; set; }

        [Display(Name="Project")]
        public int ProjectId { get; set; }

        [Column(TypeName = "date")]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yy}", ApplyFormatInEditMode = true)]
        [Display(Name="Due Date")]
        public DateTime? DueDate { get; set; }

        public byte Priority { get; set; }

        [Column(TypeName = "smalldatetime")]
        [Display(Name="Start Date")]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Time Estimated")]
        public byte? TimeEstimated { get; set; }

        public byte Mark { get; set; }

        public bool Status { get; set; }

        [Column(TypeName = "smalldatetime")]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; }

        [Column(TypeName = "smalldatetime")]
        [Display(Name = "Last Modified")]
        public DateTime LastModified { get; set; }

        [Required]
        [StringLength(36)]
        public string Author { get; set; }

        public bool Active { get; set; }

        [Required]
        public int Category { get; set; }

        public virtual ICollection<Comments> Comments { get; set; }

        public virtual Project Project { get; set; }

        public virtual ICollection<TaskAssigned> TaskAssigned { get; set; }

        public virtual ICollection<TaskFiles> TaskFiles { get; set; }

        public virtual TaskMark TaskMark { get; set; }

        public virtual TaskCategory TaskCategory { get; set; }

        public virtual ICollection<TaskLifecycle> TaskLifecycle { get; set; }
    }
}
