using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopManager.Model.DataModels
{
	public enum RepairOrderStatusValue
	{
        Created = 0,              // Utworzone - zlecenie utworzone przez klienta
        PendingApproval = 1,      // Do akceptacji - przydzielony mechanik i wycena wysłana do klienta
        Approved = 2,             // Wycena zaakceptowana - wycena została zaakceptowana przez klienta
        InProgress = 3,           // W realizacji - mechanik rozpoczął realizację zlecenia
        ReadyForPickup = 4,       // Gotowy do odbioru - zlecenie wykonane, czeka na odbiór klienta
        Completed = 5,            // Zakończone - klient odebrał zlecenie
        Cancelled = 6             // Anulowane - zlecenie jest anulowane
    }
}
