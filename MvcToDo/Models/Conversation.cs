using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MvcToDo.Models
{
    [Table("Conversation")]
    public partial class Conversation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Conversation()
        {
            Chat = new HashSet<Chat>();
        }

        public short Id { get; set; }

        [StringLength(36)]
        public string Usr_1 { get; set; }

        [StringLength(36)]
        public string Usr_2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Chat> Chat { get; set; }
    }
}