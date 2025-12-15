using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.Model.DataModels;

namespace WorkshopManager.ViewModels.RepairOrders
{
    public class AssignMechanicVM
    {
        public int OrderId { get; set; }

        [Display(Name = "Mechanik")]
        [Required(ErrorMessage = "Musisz wybrać mechanika.")]
        public int? SelectedMechanicId { get; set; }

        public string ClientFullName { get; set; } = string.Empty;

        [Display(Name = "Numer rejestracyjny")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Opis usterki")]
        public string IssueDescription { get; set; } = string.Empty;

        [Display(Name = "Status zlecenia")]
        public RepairOrderStatusValue Status { get; set; }

        public IList<MechanicListItemVM> Mechanics { get; set; } = new List<MechanicListItemVM>();
    }

    public class MechanicListItemVM
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
    }
}
