using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MvcToDo.Models
{
    [Table("CustomerUser")]
    public class CustomerUser
    {
        public CustomerUser()
        {
            
        }

        public int Id { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(36)]
        public string UserId { get; set; }

        public bool Active { get; set; }

        public virtual Customer Customer { get; set; }

    }
}