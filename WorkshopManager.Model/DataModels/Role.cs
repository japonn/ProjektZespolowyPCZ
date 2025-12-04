using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WorkshopManager.Model.DataModels
{
    public class Role : IdentityRole<int>
    {
        public RoleValue RoleValue { get; set; }
        public Role()
        {

        }
        public Role(string name, RoleValue roleValue)
        {
            RoleValue = roleValue;
            Name = name;
        }
        public Role(int id, string name, RoleValue roleValue)
        {
            Id = id;
            RoleValue = roleValue;
            Name = name;
            NormalizedName = name.ToUpper();
        }
    }
}
