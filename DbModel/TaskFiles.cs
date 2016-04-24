namespace DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TaskFiles
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public int FileId { get; set; }

        public bool Active { get; set; }

        public virtual Files Files { get; set; }

        public virtual TaskItem TaskItem { get; set; }
    }
}
