using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopManager.Model.DataModels
{
	public enum RepairOrderStatusValue
	{
        PendingEstimate = 0,   // oczekiwanie na wycenę (przez wlasciciela)
        ClientApproval = 1, // wyceniony(przez wlasciciela), niezaakceptowane przez klienta (oczekuje na ackeptacje)
        Accepted = 2,   // zaakcpetowane przez klienta 
        InProgress = 3,
        Completed = 4,
    }
}
