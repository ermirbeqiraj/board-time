using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MvcToDo.Models
{
    [Table("TaskLifecycle")]
    public partial class TaskLifecycle
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        [Required]
        [StringLength(36)]
        public string Actor { get; set; }

        public byte MarkFromId { get; set; }

        public byte MarkToId { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Created { get; set; }


        public virtual TaskItem TaskItem { get; set; }

        public virtual TaskMark TaskMark { get; set; }

        public virtual TaskMark TaskMark1 { get; set; }
    }
}