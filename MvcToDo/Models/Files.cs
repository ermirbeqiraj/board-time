namespace MvcToDo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Files
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Files()
        {
            TaskFiles = new HashSet<TaskFiles>(); // per mos me leju null
        }

        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        [Required]
        public byte[] FileContent { get; set; }

        [Required]
        [StringLength(36)]
        public string Author { get; set; }

        public bool Active { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Created { get; set; }

        public virtual ICollection<TaskFiles> TaskFiles { get; set; }
    }
}
