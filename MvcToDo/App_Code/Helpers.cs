using MvcToDo.ModelsView;
using System.Collections.Generic;
namespace MvcToDo.App_Code
{
    public class Helpers
    {  
        /// <summary>
        /// Task Priority is not represented in the db as a table, instead it's just a task property as number.
        /// Based on the number from 1 to 5 the method will return a key-value list that is more human readable
        /// </summary>
        /// <returns>Key-Value list of TaskPriority</returns>
        public List<DropDownItems> GetTaskPriority()
        {
            var priority = new List<DropDownItems>();
            priority.Add(new DropDownItems { Id = 0, Name = "Free Time" });
            priority.Add(new DropDownItems { Id = 1, Name = "Low" });
            priority.Add(new DropDownItems { Id = 2, Name = "Below Normal" });
            priority.Add(new DropDownItems { Id = 3, Name = "Normal" });
            priority.Add(new DropDownItems { Id = 4, Name = "Above Normal" });
            priority.Add(new DropDownItems { Id = 5, Name = "High" });
            return priority;
        }

        public List<DropDownItems> GetCategoryColor()
        {
            var category = new List<DropDownItems>();
            category.Add(new DropDownItems { Id = 1, Name = "label label-default" });
            category.Add(new DropDownItems { Id = 2, Name = "label label-primary" });
            category.Add(new DropDownItems { Id = 3, Name = "label label-success" });
            category.Add(new DropDownItems { Id = 4, Name = "label label-info" });
            category.Add(new DropDownItems { Id = 5, Name = "label label-warning" });
            category.Add(new DropDownItems { Id = 6, Name = "label label-danger" });
            return category;
        }
    }
}