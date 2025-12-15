using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.ViewModels.RepairOrders
{
    public class CreateOrderVM
    {
        [Required(ErrorMessage = "Podanie opisu jest wymagane.")]
        [Display(Name = "Opis usterki")]
        public string IssueDescription { get; set; } = null!;

        [Required(ErrorMessage = "Numer rejestracyjny musi zostać uzupełniony.")]
        [Display(Name = "Numer rejestracyjny pojazdu")]
        public string PlateNumber { get; set; } = null!;
    }
}
