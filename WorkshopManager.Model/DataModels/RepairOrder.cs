using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopManager.Model.DataModels
{
    public class RepairOrder
    {
        public int Id { get; set; }
        public Client Client { get; set; } = null!;
        public int ClientId { get; set; }
        public Mechanic? Mechanic { get; set; } = null!;
        public int? MechanicId { get; set; }
        public RepairOrderStatusValue Status { get; set; }
        public IList<RepairTask> Tasks { get; set; } = null!;
        public DateTime SubmissionDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? EntryEstimatedCost { get; set; }
        public string EntryIssueDescription { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!;
    }
}
