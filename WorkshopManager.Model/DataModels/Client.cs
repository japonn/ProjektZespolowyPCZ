using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WorkshopManager.Model.DataModels
{
    public class Client : User
    {
        public IList<RepairOrder> RepairOrders { get; set; } = null!;
    }
}
