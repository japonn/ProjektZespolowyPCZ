using System.ComponentModel.DataAnnotations;
using WorkshopManager.Model.DataModels;

namespace WorkshopManager.ViewModels.RepairOrders
{
    public class ChangeStatusVM
    {
        public int OrderId { get; set; }

        public string ClientFullName { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Aktualny status")]
        public RepairOrderStatusValue CurrentStatus { get; set; }

        [Display(Name = "Nowy status")]
        [Required(ErrorMessage = "Nowy status jest wymagany.")]
        public RepairOrderStatusValue NewStatus { get; set; }

        [Display(Name = "Opis usterki")]
        public string IssueDescription { get; set; } = string.Empty;
    }
}
