using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MvcToDo.Models;

namespace MvcToDo.ModelsView
{
    public class TaskMovementsView
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string Actor { get; set; }
        public string TaskName { get; set; }
        public string FromMode { get; set; }
        public string ToMode { get; set; }
        public string Created { get; set; }
    }

    public class DropDownItems
    {
        public short Id { get; set; }
        public string Name { get; set; }
    }

    public class SimpleList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StringList
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class AtachUserToTask
    {
        public int Id { get; set; }
        public List<UserChecks> UsersToAdd { get; set; }
    }

    public class UserChecks
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }

    /// <summary>
    /// Model to represent a single task item in Board view
    /// </summary>
    public class TaskBoard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public byte MarkId { get; set; }
        public string Category { get; set; }
        public string CssClass { get; set; }
    }

    /// <summary>
    /// Model to represent the Board view
    /// </summary>
    public class BoardView
    {
        public List<TaskBoard> Board { get; set; }
        public List<DbModel.TaskMark> BoardColumns { get; set; }
        public System.Web.Mvc.SelectList TCategories { get; set; }
        public System.Web.Mvc.SelectList TUsers { get; set; }
    }

    /// <summary>
    /// To represent the view model of the task list
    /// </summary>
    public class TaskList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mode { get; set; }
        public string Priority { get; set; }
    }

    public class DublicateTask
    {
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public int TaskId { get; set; }
        [Display(Name="Task Name")]
        public string TaskName { get; set; }
        [Display(Name="Include Files")]
        public bool IncludeFiles { get; set; }
        [Display(Name = "Include Comments")]
        public bool IncludeComments { get; set; }
        [Display(Name = "Include Users")]
        public bool IncludeUsers { get; set; }
    }

    public class ConversationView
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Created { get; set; } /*string datatype:save some converting headace in javascript from ticks*/
        public string Message { get; set; }
    }

    /// <summary>
    /// Tasks per user
    /// </summary>
    public class Stats 
    {
        public int Count { get; set; }
        public string User { get; set; }
    }

    /// <summary>
    /// /Home/TaskModePerUser
    /// to show how many tasks has each user in each board-column
    /// </summary>
    public class TaskModePerUser
    {
        public int Count { get; set; }
        public string User { get; set; }
        public string Mode { get; set; }
    }

}