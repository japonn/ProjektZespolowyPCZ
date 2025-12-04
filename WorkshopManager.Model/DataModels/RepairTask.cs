using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopManager.Model.DataModels
{
    public class RepairTask
    {
        public int Id { get; set; }
        public RepairOrder RepairOrder { get; set; } = null!;
        public int RepairOrderId { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public bool? AcceptedByCustomer { get; set; }
        public bool IsCompleted { get; set; } = false;

        public RepairTask() { }
    }
}
