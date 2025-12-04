using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopManager.Model.DataModels
{
    public class EmployeeReport
    {
        public int Id { get; set; }
        public int MechanicId { get; set; }
        public DateTime ReportDate { get; set; }
        public int? NumberOfTasks { get; set; }
        public decimal TotalCost { get; set; }

    }
}
