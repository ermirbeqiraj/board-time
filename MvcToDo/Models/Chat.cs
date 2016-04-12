using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MvcToDo.Models
{
    [Table("Chat")]
    public partial class Chat
    {
        public int Id { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Created { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public short ConversationId { get; set; }

        public bool OrderedAsc { get; set; }

        public virtual Conversation Conversation { get; set; }
    }
}