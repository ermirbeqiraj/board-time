namespace DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskCategory")]
    public partial class TaskCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TaskCategory()
        {
            TaskItem = new HashSet<TaskItem>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Caption { get; set; }

        [Required]
        [StringLength(50)]
        public string Color { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskItem> TaskItem { get; set; }
    }
}
