using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace MvcToDo.Models
{
    [Table("TaskCategory")]
    public class TaskCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TaskCategory()
        {
            TaskItem = new HashSet<TaskItem>();
        }

        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Caption { get; set; }
        [Required]
        [StringLength(50)]
        public string Color { get; set; }

        public virtual ICollection<TaskItem> TaskItem { get; set; }
    }
}