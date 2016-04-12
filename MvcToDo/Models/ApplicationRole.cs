using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MvcToDo.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        //public ApplicationRole(string name, string description)
        //    : base(name)
        //{
        //    this.Description = description;
        //}

        //public string Description { get; set; }
    }
}