namespace MvcToDo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskAssigned")]
    public partial class TaskAssigned
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        [Required]
        [StringLength(36)]
        public string Author { get; set; }

        [Required]
        [StringLength(36)]
        public string AssignedTo { get; set; }

        public bool Active { get; set; }

        public virtual TaskItem TaskItem { get; set; }
    }
}
