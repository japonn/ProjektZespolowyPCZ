using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.ViewModels
{
    public class ClientListItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string? FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string? LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }
    }
}
