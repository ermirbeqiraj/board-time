namespace DbModel
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
            TaskLifecycle = new HashSet<TaskLifecycle>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        public int? ParentId { get; set; }

        public int ProjectId { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DueDate { get; set; }

        public byte Priority { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? StartDate { get; set; }

        public byte? TimeEstimated { get; set; }

        public byte Mark { get; set; }

        public bool Status { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Created { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime LastModified { get; set; }

        [Required]
        [StringLength(36)]
        public string Author { get; set; }

        public bool Active { get; set; }

        public int Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comments> Comments { get; set; }

        public virtual Project Project { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskAssigned> TaskAssigned { get; set; }

        public virtual TaskCategory TaskCategory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskFiles> TaskFiles { get; set; }

        public virtual TaskMark TaskMark { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskLifecycle> TaskLifecycle { get; set; }
    }
}
