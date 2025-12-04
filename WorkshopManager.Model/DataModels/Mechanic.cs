using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopManager.Model.DataModels
{
    public class Mechanic
    {
        public int Id { get; set; }
        public IList<RepairOrder> RepairOrders { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string BankAccountNumber { get; set; } = null!;
        public DateTime EmploymentDate { get; set; }
    }
}
