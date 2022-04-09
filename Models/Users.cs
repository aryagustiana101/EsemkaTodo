using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EsemkaTodo.Models
{
    public partial class Users
    {
        /*public Users()
        {
            TodoItems = new HashSet<TodoItems>();
        }*/

        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; }

        //public virtual ICollection<TodoItems> TodoItems { get; set; }
    }
}
