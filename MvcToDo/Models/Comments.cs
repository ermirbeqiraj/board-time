namespace MvcToDo.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Comments
    {
        public int Id { get; set; }

        [StringLength(36)]
        public string Author { get; set; }

        public int? TaskId { get; set; }

        [StringLength(3500)]
        public string Comment { get; set; }

        public bool? Active { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Created { get; set; }

        public virtual TaskItem TaskItem { get; set; }
    }
}
