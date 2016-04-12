namespace MvcToDo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Project")]
    public partial class Project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Project()
        {
            TaskItem = new HashSet<TaskItem>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(3500)]
        public string Description { get; set; }

        [Required]
        [StringLength(36)]
        public string Author { get; set; }

        public bool Active { get; set; }

        [Display(Name="Customer")]
        public int? CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual ICollection<TaskItem> TaskItem { get; set; }
    }
}
